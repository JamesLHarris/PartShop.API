using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Site_2024.Models.Domain.RefundRequests;
using Site_2024.Models.Requests.RefundRequests;
using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Interfaces;

namespace Site_2024.Web.Api.Services
{
    public class RefundRequestService : IRefundRequestService
    {
        private readonly IDataProvider _data;

        public RefundRequestService(IDataProvider data)
        {
            _data = data;
        }

        public int Add(RefundRequestAddRequest model, int? userId)
        {
            int id = 0;
            const string procName = "[dbo].[RefundRequests_Insert]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);

                    col.AddWithValue(
                        "@CreatedByUserId",
                        userId.HasValue ? userId.Value : DBNull.Value
                    );

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output,
                        Value = 0
                    };

                    col.Add(idOut);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    if (set == 0)
                    {
                        id = Convert.ToInt32(reader["Id"]);
                    }
                });

            if (id <= 0)
            {
                throw new Exception("RefundRequests_Insert did not return a valid RefundRequest Id.");
            }

            if (model.Items != null)
            {
                foreach (RefundRequestItemAddRequest item in model.Items)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    bool isDefaultPrimaryItem = item.PartId == model.PartId
                        && item.ShopifyLineItemId == null
                        && item.Quantity == 1
                        && string.IsNullOrWhiteSpace(item.ItemNotes);

                    if (!isDefaultPrimaryItem)
                    {
                        AddItem(id, item);
                    }
                }
            }

            if (model.Photos != null)
            {
                foreach (RefundRequestPhotoAddRequest photo in model.Photos)
                {
                    if (photo != null && !string.IsNullOrWhiteSpace(photo.Url))
                    {
                        AddPhoto(id, photo);
                    }
                }
            }

            return id;
        }

        public RefundRequest? GetById(int id)
        {
            RefundRequest? refundRequest = null;
            const string procName = "[dbo].[RefundRequests_GetById]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    if (set == 0)
                    {
                        refundRequest = MapRefundRequest(reader, ref startingIndex);
                    }
                    else if (set == 1 && refundRequest != null)
                    {
                        RefundRequestItem item = MapRefundRequestItem(reader, ref startingIndex);
                        refundRequest.Items.Add(item);
                    }
                    else if (set == 2 && refundRequest != null)
                    {
                        RefundRequestPhoto photo = MapRefundRequestPhoto(reader, ref startingIndex);
                        refundRequest.Photos.Add(photo);
                    }
                });

            if (refundRequest != null)
            {
                refundRequest.ItemCount = refundRequest.Items.Count;
                refundRequest.PhotoCount = refundRequest.Photos.Count;
            }

            return refundRequest;
        }

        public Paged<RefundRequest>? GetPaginated(int pageIndex, int pageSize, RefundRequestSearchRequest model)
        {
            Paged<RefundRequest>? pagedList = null;
            List<RefundRequest>? list = null;
            int totalCount = 0;

            const string procName = "[dbo].[RefundRequests_GetPaginated]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@Status", string.IsNullOrWhiteSpace(model?.Status) ? DBNull.Value : model.Status);
                    col.AddWithValue("@PartId", model?.PartId.HasValue == true ? model.PartId.Value : DBNull.Value);
                    col.AddWithValue("@ShopifyOrderId", model?.ShopifyOrderId.HasValue == true ? model.ShopifyOrderId.Value : DBNull.Value);
                    col.AddWithValue("@OrderNumber", string.IsNullOrWhiteSpace(model?.OrderNumber) ? DBNull.Value : model.OrderNumber);
                    col.AddWithValue("@CustomerEmail", string.IsNullOrWhiteSpace(model?.CustomerEmail) ? DBNull.Value : model.CustomerEmail);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    RefundRequest refundRequest = MapRefundRequestForPaged(reader, ref startingIndex);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }

                    list ??= new List<RefundRequest>();
                    list.Add(refundRequest);
                });

            if (list != null)
            {
                pagedList = new Paged<RefundRequest>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;
        }

        public List<ReturnReason> GetReasons()
        {
            List<ReturnReason> list = new List<ReturnReason>();
            const string procName = "[dbo].[ReturnReasons_SelectAll]";

            _data.ExecuteCmd(procName,
                inputParamMapper: null,
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    list.Add(MapReturnReason(reader, ref startingIndex));
                });

            return list;
        }

        public List<ReturnStatus> GetStatuses()
        {
            List<ReturnStatus> list = new List<ReturnStatus>();
            const string procName = "[dbo].[ReturnStatuses_SelectAll]";

            _data.ExecuteCmd(procName,
                inputParamMapper: null,
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    list.Add(MapReturnStatus(reader, ref startingIndex));
                });

            return list;
        }

        public int AddItem(int refundRequestId, RefundRequestItemAddRequest model)
        {
            int id = 0;
            const string procName = "[dbo].[RefundRequestItems_Insert]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@RefundRequestId", refundRequestId);
                    col.AddWithValue("@PartId", model.PartId);
                    col.AddWithValue("@ShopifyLineItemId", model.ShopifyLineItemId.HasValue ? model.ShopifyLineItemId.Value : DBNull.Value);
                    col.AddWithValue("@Quantity", model.Quantity <= 0 ? 1 : model.Quantity);
                    col.AddWithValue("@ItemNotes", string.IsNullOrWhiteSpace(model.ItemNotes) ? DBNull.Value : model.ItemNotes);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    col.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;
                    int.TryParse(oId.ToString(), out id);
                });

            return id;
        }

        public int AddPhoto(int refundRequestId, RefundRequestPhotoAddRequest model)
        {
            int id = 0;
            const string procName = "[dbo].[RefundRequestPhotos_Insert]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@RefundRequestId", refundRequestId);
                    col.AddWithValue("@RefundRequestItemId", model.RefundRequestItemId.HasValue ? model.RefundRequestItemId.Value : DBNull.Value);
                    col.AddWithValue("@Url", model.Url);
                    col.AddWithValue("@OriginalFileName", string.IsNullOrWhiteSpace(model.OriginalFileName) ? DBNull.Value : model.OriginalFileName);
                    col.AddWithValue("@ContentType", string.IsNullOrWhiteSpace(model.ContentType) ? DBNull.Value : model.ContentType);
                    col.AddWithValue("@SortOrder", model.SortOrder);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    col.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;
                    int.TryParse(oId.ToString(), out id);
                });

            return id;
        }

        public void UpdateStatus(int id, RefundRequestUpdateStatusRequest model, int userId)
        {
            const string procName = "[dbo].[RefundRequests_UpdateStatus]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                    col.AddWithValue("@Status", model.Status);
                    col.AddWithValue("@Notes", string.IsNullOrWhiteSpace(model.Notes) ? DBNull.Value : model.Notes);
                    col.AddWithValue("@ResolvedByUserId", userId);
                    col.AddWithValue("@AdminNotes", string.IsNullOrWhiteSpace(model.AdminNotes) ? DBNull.Value : model.AdminNotes);
                    col.AddWithValue("@DenialReason", string.IsNullOrWhiteSpace(model.DenialReason) ? DBNull.Value : model.DenialReason);
                },
                returnParameters: null);
        }

        private static RefundRequest MapRefundRequest(IDataReader reader, ref int startingIndex)
        {
            RefundRequest model = new RefundRequest();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.PartId = reader.GetSafeInt32(startingIndex++);
            model.PartName = reader.GetSafeString(startingIndex++);
            model.PartNumber = reader.GetSafeString(startingIndex++);
            model.Price = reader.GetSafeDecimal(startingIndex++);
            model.PartShopifyOrderId = reader.GetSafeInt64Nullable(startingIndex++);
            model.ShopifyOrderId = reader.GetSafeInt64Nullable(startingIndex++);
            model.Reason = reader.GetSafeString(startingIndex++);
            model.Notes = reader.GetSafeString(startingIndex++);
            model.Status = reader.GetSafeString(startingIndex++);
            model.StatusId = reader.GetSafeInt32Nullable(startingIndex++);
            model.StatusName = reader.GetSafeString(startingIndex++);
            model.OrderNumber = reader.GetSafeString(startingIndex++);
            model.CustomerEmail = reader.GetSafeString(startingIndex++);
            model.ReturnReasonId = reader.GetSafeInt32Nullable(startingIndex++);
            model.ReturnReasonName = reader.GetSafeString(startingIndex++);
            model.RequiresNotes = reader.GetSafeBool(startingIndex++);
            model.RequiresPhotos = reader.GetSafeBool(startingIndex++);
            model.AdminNotes = reader.GetSafeString(startingIndex++);
            model.DenialReason = reader.GetSafeString(startingIndex++);
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);
            model.DateModified = reader.GetSafeDateTime(startingIndex++);
            model.CreatedByUserId = reader.GetSafeInt32Nullable(startingIndex++);
            model.CreatedByName = reader.GetSafeString(startingIndex++);
            model.ResolvedByUserId = reader.GetSafeInt32Nullable(startingIndex++);
            model.ResolvedByName = reader.GetSafeString(startingIndex++);
            model.ResolvedDate = reader.GetSafeDateTimeNullable(startingIndex++);

            return model;
        }

        private static RefundRequest MapRefundRequestForPaged(IDataReader reader, ref int startingIndex)
        {
            RefundRequest model = new RefundRequest();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.PartId = reader.GetSafeInt32(startingIndex++);
            model.PartName = reader.GetSafeString(startingIndex++);
            model.PartNumber = reader.GetSafeString(startingIndex++);
            model.Price = reader.GetSafeDecimal(startingIndex++);
            model.ShopifyOrderId = reader.GetSafeInt64Nullable(startingIndex++);
            model.Reason = reader.GetSafeString(startingIndex++);
            model.Status = reader.GetSafeString(startingIndex++);
            model.StatusId = reader.GetSafeInt32Nullable(startingIndex++);
            model.StatusName = reader.GetSafeString(startingIndex++);
            model.OrderNumber = reader.GetSafeString(startingIndex++);
            model.CustomerEmail = reader.GetSafeString(startingIndex++);
            model.ReturnReasonId = reader.GetSafeInt32Nullable(startingIndex++);
            model.ReturnReasonName = reader.GetSafeString(startingIndex++);
            model.ItemCount = reader.GetSafeInt32(startingIndex++);
            model.PhotoCount = reader.GetSafeInt32(startingIndex++);
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);
            model.DateModified = reader.GetSafeDateTime(startingIndex++);
            model.CreatedByUserId = reader.GetSafeInt32Nullable(startingIndex++);
            model.CreatedByName = reader.GetSafeString(startingIndex++);
            model.ResolvedByUserId = reader.GetSafeInt32Nullable(startingIndex++);
            model.ResolvedByName = reader.GetSafeString(startingIndex++);
            model.ResolvedDate = reader.GetSafeDateTimeNullable(startingIndex++);

            return model;
        }

        private static RefundRequestItem MapRefundRequestItem(IDataReader reader, ref int startingIndex)
        {
            RefundRequestItem model = new RefundRequestItem();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.RefundRequestId = reader.GetSafeInt32(startingIndex++);
            model.PartId = reader.GetSafeInt32(startingIndex++);
            model.PartName = reader.GetSafeString(startingIndex++);
            model.PartNumber = reader.GetSafeString(startingIndex++);
            model.Price = reader.GetSafeDecimal(startingIndex++);
            model.Image = reader.GetSafeString(startingIndex++);
            model.ShopifyLineItemId = reader.GetSafeInt64Nullable(startingIndex++);
            model.Quantity = reader.GetSafeInt32(startingIndex++);
            model.ItemNotes = reader.GetSafeString(startingIndex++);
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);

            return model;
        }

        private static RefundRequestPhoto MapRefundRequestPhoto(IDataReader reader, ref int startingIndex)
        {
            RefundRequestPhoto model = new RefundRequestPhoto();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.RefundRequestId = reader.GetSafeInt32(startingIndex++);
            model.RefundRequestItemId = reader.GetSafeInt32Nullable(startingIndex++);
            model.Url = reader.GetSafeString(startingIndex++);
            model.OriginalFileName = reader.GetSafeString(startingIndex++);
            model.ContentType = reader.GetSafeString(startingIndex++);
            model.SortOrder = reader.GetSafeInt32(startingIndex++);
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);

            return model;
        }

        private static ReturnReason MapReturnReason(IDataReader reader, ref int startingIndex)
        {
            ReturnReason model = new ReturnReason();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.Name = reader.GetSafeString(startingIndex++);
            model.RequiresNotes = reader.GetSafeBool(startingIndex++);
            model.RequiresPhotos = reader.GetSafeBool(startingIndex++);
            model.IsActive = reader.GetSafeBool(startingIndex++);
            model.SortOrder = reader.GetSafeInt32(startingIndex++);
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);
            model.DateModified = reader.GetSafeDateTime(startingIndex++);

            return model;
        }

        private static ReturnStatus MapReturnStatus(IDataReader reader, ref int startingIndex)
        {
            ReturnStatus model = new ReturnStatus();

            model.Id = reader.GetSafeInt32(startingIndex++);
            model.Name = reader.GetSafeString(startingIndex++);
            model.IsTerminal = reader.GetSafeBool(startingIndex++);
            model.SortOrder = reader.GetSafeInt32(startingIndex++);
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);
            model.DateModified = reader.GetSafeDateTime(startingIndex++);

            return model;
        }

        private static void AddCommonParams(RefundRequestAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@PartId", model.PartId);
            col.AddWithValue("@ShopifyOrderId", model.ShopifyOrderId.HasValue ? model.ShopifyOrderId.Value : DBNull.Value);
            col.AddWithValue("@Reason", model.Reason);
            col.AddWithValue("@Notes", string.IsNullOrWhiteSpace(model.Notes) ? DBNull.Value : model.Notes);
            col.AddWithValue("@OrderNumber", string.IsNullOrWhiteSpace(model.OrderNumber) ? DBNull.Value : model.OrderNumber);
            col.AddWithValue("@CustomerEmail", string.IsNullOrWhiteSpace(model.CustomerEmail) ? DBNull.Value : model.CustomerEmail);
            col.AddWithValue("@ReturnReasonId", model.ReturnReasonId.HasValue ? model.ReturnReasonId.Value : DBNull.Value);
        }
    }
}
