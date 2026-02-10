using Site_2024.Models.Domain.Parts;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Extensions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Site_2024.Web.Api.Services
{
    public class PartImageService : IPartImageService
    {
        private readonly IDataProvider _data;

        public PartImageService(IDataProvider data)
        {
            _data = data;
        }

        public int Add(int partId, string url, bool isPrimary, int sortOrder, int userId)
        {
            const string procName = "[dbo].[PartImages_Insert]";
            int id = 0;

            _data.ExecuteNonQuery(procName, col =>
            {
                col.AddWithValue("@PartId", partId);
                col.AddWithValue("@Url", url);
                col.AddWithValue("@IsPrimary", isPrimary);
                col.AddWithValue("@SortOrder", sortOrder);
                col.AddWithValue("@CreatedByUserId", userId);

                SqlParameter pId = col.Add("@Id", SqlDbType.Int);
                pId.Direction = ParameterDirection.Output;
            }, returnParameters: col =>
            {
                id = (int)col["@Id"].Value;
            });

            return id;
        }

        public List<PartImage> GetByPartId(int partId)
        {
            const string procName = "[dbo].[PartImages_SelectByPartId]";
            List<PartImage> list = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@PartId", partId);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    PartImage img = new PartImage();
                    img.Id = reader.GetSafeInt32(i++);
                    img.PartId = reader.GetSafeInt32(i++);
                    img.Url = reader.GetSafeString(i++);
                    img.IsPrimary = reader.GetSafeBool(i++);
                    img.SortOrder = reader.GetSafeInt32(i++);
                    img.DateCreated = reader.GetSafeDateTime(i++);
                    img.CreatedByUserId = reader.GetSafeInt32(i++);

                    list ??= new List<PartImage>();
                    list.Add(img);
                });

            return list ?? new List<PartImage>();
        }

        public bool HasPrimary(int partId)
        {
            bool has = false;
            string procName = "[dbo].[PartImages_HasPrimary]";

            _data.ExecuteCmd(procName,
                col => col.AddWithValue("@PartId", partId),
                (reader, set) =>
                {
                    has = reader.GetSafeBool(0);
                });

            return has;
        }

    }
}
