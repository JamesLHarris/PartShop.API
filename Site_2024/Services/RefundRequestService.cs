using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Site_2024.Models;
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

        public int Add(RefundRequestAddRequest model, int userId)
        {
            int id = 0;
            string procName = "[dbo].[RefundRequests_Insert]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);

                    col.AddWithValue("@CreatedByUserId", userId);

                    SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                    idOut.Direction = ParameterDirection.Output;
                    col.Add(idOut);
                },
                returnParameters: delegate (SqlParameterCollection returnCollection)
                {
                    object oId = returnCollection["@Id"].Value;
                    int.TryParse(oId.ToString(), out id);
                });

            return id;
        }

        public RefundRequest GetById(int id)
        {
            RefundRequest refundRequest = null;
            string procName = "[dbo].[RefundRequests_GetById]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    refundRequest = MapRefundRequest(reader, ref startingIndex);
                });

            return refundRequest;
        }

        public Paged<RefundRequest> GetPaginated(int pageIndex, int pageSize, RefundRequestSearchRequest model)
        {
            Paged<RefundRequest> pagedList = null;
            List<RefundRequest> list = null;
            int totalCount = 0;

            string procName = "[dbo].[RefundRequests_GetPaginated]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);

                    if (string.IsNullOrWhiteSpace(model.Status))
                    {
                        col.AddWithValue("@Status", DBNull.Value);
                    }
                    else
                    {
                        col.AddWithValue("@Status", model.Status);
                    }

                    if (model.PartId.HasValue)
                    {
                        col.AddWithValue("@PartId", model.PartId.Value);
                    }
                    else
                    {
                        col.AddWithValue("@PartId", DBNull.Value);
                    }

                    if (model.ShopifyOrderId.HasValue)
                    {
                        col.AddWithValue("@ShopifyOrderId", model.ShopifyOrderId.Value);
                    }
                    else
                    {
                        col.AddWithValue("@ShopifyOrderId", DBNull.Value);
                    }
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    RefundRequest refundRequest = MapRefundRequestForPaged(reader, ref startingIndex);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(startingIndex++);
                    }

                    if (list == null)
                    {
                        list = new List<RefundRequest>();
                    }

                    list.Add(refundRequest);
                });

            if (list != null)
            {
                pagedList = new Paged<RefundRequest>(list, pageIndex, pageSize, totalCount);
            }

            return pagedList;
        }

        public void UpdateStatus(int id, RefundRequestUpdateStatusRequest model, int userId)
        {
            string procName = "[dbo].[RefundRequests_UpdateStatus]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                    col.AddWithValue("@Status", model.Status);

                    if (string.IsNullOrWhiteSpace(model.Notes))
                    {
                        col.AddWithValue("@Notes", DBNull.Value);
                    }
                    else
                    {
                        col.AddWithValue("@Notes", model.Notes);
                    }

                    col.AddWithValue("@ResolvedByUserId", userId);
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
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);
            model.DateModified = reader.GetSafeDateTime(startingIndex++);
            model.CreatedByUserId = reader.GetSafeInt32(startingIndex++);
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
            model.DateCreated = reader.GetSafeDateTime(startingIndex++);
            model.DateModified = reader.GetSafeDateTime(startingIndex++);
            model.CreatedByUserId = reader.GetSafeInt32(startingIndex++);
            model.CreatedByName = reader.GetSafeString(startingIndex++);
            model.ResolvedByUserId = reader.GetSafeInt32Nullable(startingIndex++);
            model.ResolvedByName = reader.GetSafeString(startingIndex++);
            model.ResolvedDate = reader.GetSafeDateTimeNullable(startingIndex++);

            return model;
        }

        private static void AddCommonParams(RefundRequestAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@PartId", model.PartId);

            if (model.ShopifyOrderId.HasValue)
            {
                col.AddWithValue("@ShopifyOrderId", model.ShopifyOrderId.Value);
            }
            else
            {
                col.AddWithValue("@ShopifyOrderId", DBNull.Value);
            }

            col.AddWithValue("@Reason", model.Reason);

            if (string.IsNullOrWhiteSpace(model.Notes))
            {
                col.AddWithValue("@Notes", DBNull.Value);
            }
            else
            {
                col.AddWithValue("@Notes", model.Notes);
            }
        }
    }
}