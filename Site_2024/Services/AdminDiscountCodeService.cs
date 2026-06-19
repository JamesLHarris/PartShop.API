using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using System.Data;
using System.Data.SqlClient;

namespace Site_2024.Web.Api.Services
{
    public class AdminDiscountCodeService : IAdminDiscountCodeService
    {
        private readonly IDataProvider _data;

        public AdminDiscountCodeService(IDataProvider data)
        {
            _data = data;
        }

        public int Add(AdminDiscountCodeAddRequest model, int? userId)
        {
            int id = 0;
            const string procName = "[dbo].[AdminDiscountCodes_Insert]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    AddCommonParams(model, col);

                    col.AddWithValue("@CreatedByUserId", userId.HasValue ? userId.Value : DBNull.Value);

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

            return id;
        }

        public AdminDiscountCode? GetById(int id)
        {
            AdminDiscountCode? discount = null;
            const string procName = "[dbo].[AdminDiscountCodes_GetById]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    discount = MapSingleDiscount(reader);
                });

            return discount;
        }

        public Paged<AdminDiscountCode>? GetPaginated(int pageIndex, int pageSize, AdminDiscountCodeSearchRequest model)
        {
            Paged<AdminDiscountCode>? paged = null;
            List<AdminDiscountCode> list = null;
            int totalCount = 0;

            const string procName = "[dbo].[AdminDiscountCodes_GetPaginated]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@Status", string.IsNullOrWhiteSpace(model?.Status) ? DBNull.Value : model.Status);
                    col.AddWithValue("@Code", string.IsNullOrWhiteSpace(model?.Code) ? DBNull.Value : model.Code);
                    col.AddWithValue("@CustomerEmail", string.IsNullOrWhiteSpace(model?.CustomerEmail) ? DBNull.Value : model.CustomerEmail);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    AdminDiscountCode discount = MapSingleDiscount(reader);

                    if (totalCount == 0 && reader["TotalCount"] != DBNull.Value)
                    {
                        totalCount = Convert.ToInt32(reader["TotalCount"]);
                    }

                    if (list == null)
                    {
                        list = new List<AdminDiscountCode>();
                    }

                    list.Add(discount);
                });

            if (list != null)
            {
                paged = new Paged<AdminDiscountCode>(list, pageIndex, pageSize, totalCount);
            }

            return paged;
        }

        public void MarkShopifyCreated(int id, AdminDiscountCodeShopifyCreatedRequest model)
        {
            const string procName = "[dbo].[AdminDiscountCodes_MarkShopifyCreated]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                    col.AddWithValue("@ShopifyDiscountGid", model.ShopifyDiscountGid);
                });
        }

        public void Deactivate(int id, AdminDiscountCodeDeactivateRequest model, int? userId)
        {
            const string procName = "[dbo].[AdminDiscountCodes_Deactivate]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                    col.AddWithValue("@DeactivatedByUserId", userId.HasValue ? userId.Value : DBNull.Value);
                    col.AddWithValue("@AdminNotes", string.IsNullOrWhiteSpace(model?.AdminNotes) ? DBNull.Value : model.AdminNotes);
                });
        }

        public void MarkError(int id, string adminNotes)
        {
            const string procName = "[dbo].[AdminDiscountCodes_MarkError]";

            _data.ExecuteNonQuery(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@Id", id);
                    col.AddWithValue("@AdminNotes", string.IsNullOrWhiteSpace(adminNotes)
                        ? DBNull.Value
                        : adminNotes);
                });
        }

        private static void AddCommonParams(AdminDiscountCodeAddRequest model, SqlParameterCollection col)
        {
            col.AddWithValue("@Code", model.Code);
            col.AddWithValue("@Title", string.IsNullOrWhiteSpace(model.Title) ? DBNull.Value : model.Title);
            col.AddWithValue("@DiscountType", model.DiscountType);
            col.AddWithValue("@DiscountValue", model.DiscountValue);
            col.AddWithValue("@AppliesToType", model.AppliesToType);

            col.AddWithValue("@PartId", model.PartId.HasValue ? model.PartId.Value : DBNull.Value);
            col.AddWithValue("@ShopifyProductId", model.ShopifyProductId.HasValue ? model.ShopifyProductId.Value : DBNull.Value);
            col.AddWithValue("@ShopifyVariantId", model.ShopifyVariantId.HasValue ? model.ShopifyVariantId.Value : DBNull.Value);

            col.AddWithValue("@CustomerEmail", string.IsNullOrWhiteSpace(model.CustomerEmail) ? DBNull.Value : model.CustomerEmail);

            col.AddWithValue("@StartsAtUtc", model.StartsAtUtc.HasValue ? model.StartsAtUtc.Value : DBNull.Value);
            col.AddWithValue("@EndsAtUtc", model.EndsAtUtc.HasValue ? model.EndsAtUtc.Value : DBNull.Value);

            col.AddWithValue("@UsageLimit", model.UsageLimit <= 0 ? 1 : model.UsageLimit);
            col.AddWithValue("@OncePerCustomer", model.OncePerCustomer);

            col.AddWithValue("@AdminNotes", string.IsNullOrWhiteSpace(model.AdminNotes) ? DBNull.Value : model.AdminNotes);
        }

        private static AdminDiscountCode MapSingleDiscount(IDataReader reader)
        {
            AdminDiscountCode discount = new AdminDiscountCode();

            int index = 0;

            discount.Id = reader.GetInt32(index++);
            discount.Code = reader.GetSafeString(index++);
            discount.Title = reader.GetSafeString(index++);
            discount.DiscountType = reader.GetSafeString(index++);
            discount.DiscountValue = reader.GetDecimal(index++);
            discount.AppliesToType = reader.GetSafeString(index++);

            discount.PartId = reader.GetSafeInt32Nullable(index++);
            discount.PartName = reader.GetSafeString(index++);
            discount.PartNumber = reader.GetSafeString(index++);

            discount.ShopifyProductId = reader.GetSafeInt64Nullable(index++);
            discount.ShopifyVariantId = reader.GetSafeInt64Nullable(index++);

            discount.CustomerEmail = reader.GetSafeString(index++);

            discount.StartsAtUtc = reader.GetSafeDateTimeNullable(index++);
            discount.EndsAtUtc = reader.GetSafeDateTimeNullable(index++);

            discount.UsageLimit = reader.GetInt32(index++);
            discount.OncePerCustomer = reader.GetBoolean(index++);

            discount.ShopifyDiscountGid = reader.GetSafeString(index++);

            discount.Status = reader.GetSafeString(index++);
            discount.UsageCount = reader.GetInt32(index++);

            discount.AdminNotes = reader.GetSafeString(index++);

            discount.CreatedByUserId = reader.GetSafeInt32Nullable(index++);
            discount.CreatedByName = reader.GetSafeString(index++);

            discount.DeactivatedByUserId = reader.GetSafeInt32Nullable(index++);
            discount.DeactivatedByName = reader.GetSafeString(index++);

            discount.DateCreated = reader.GetDateTime(index++);
            discount.DateModified = reader.GetDateTime(index++);
            discount.DeactivatedDateUtc = reader.GetSafeDateTimeNullable(index++);

            if (reader.FieldCount > index && reader["TotalCount"] != DBNull.Value)
            {
                discount.TotalCount = Convert.ToInt32(reader["TotalCount"]);
            }

            return discount;
        }
    }
}
