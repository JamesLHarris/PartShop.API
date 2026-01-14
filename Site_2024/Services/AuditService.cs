using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Linq;
using Site_2024.Web.Api.Constructors;


namespace Site_2024.Web.Api.Services
{
    public class AuditService : IAuditService
    {
        private IDataProvider _data;
        private readonly Site_2024.Web.Api.Configurations.StaticFileOptions _staticFileOptions;
        public AuditService(IDataProvider data, IOptions<Site_2024.Web.Api.Configurations.StaticFileOptions> staticFileOptions)
        {
            _data = data;
            _staticFileOptions = staticFileOptions.Value;
        }

        #region ---GET---
        public PartAudit GetAuditByPartId(int partId, int maxRows)
        {
            string procName = "[dbo].[PartsAudit_GetByPartId]";
            PartAudit? audit = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PartId", partId);
                    col.AddWithValue("@MaxRows", maxRows);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    audit = MapSingleAudit(reader, ref startingIndex);
                });

            return audit;
        }

        public PartAudit GetAuditByRecent(int maxRows)
        {
            string procName = "[dbo].[PartsAudit_GetRecent]";
            PartAudit? audit = null;

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@MaxRows", maxRows);
                }, delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;
                    audit = MapSingleAudit(reader, ref startingIndex);
                });

            return audit;
        }

        public Paged<PartAudit> GetAuditByPartIdPaginated(int partId, int pageIndex, int pageSize)
        {
            string procName = "[dbo].[PartsAudit_GetByPartIdPaginated]";
            List<PartAudit> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                    col.AddWithValue("@PartId", partId);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    PartAudit audit = MapSingleAudit(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i); // Read TotalCount
                    }

                    list ??= new List<PartAudit>();
                    list.Add(audit);
                }
            );

            return list == null ? null : new Paged<PartAudit>(list, pageIndex, pageSize, totalCount);
        }
        public Paged<PartAudit> GetAuditRecentPaginated(int pageIndex, int pageSize)
        {
            string procName = "[dbo].[PartsAudit_GetRecentPaginated]";
            List<PartAudit> list = null;
            int totalCount = 0;

            _data.ExecuteCmd(
                procName,
                inputParamMapper: col =>
                {
                    col.AddWithValue("@PageIndex", pageIndex);
                    col.AddWithValue("@PageSize", pageSize);
                },
                singleRecordMapper: (reader, set) =>
                {
                    int i = 0;
                    PartAudit audit = MapSingleAudit(reader, ref i);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(i); // Read TotalCount
                    }

                    list ??= new List<PartAudit>();
                    list.Add(audit);
                }
            );

            return list == null ? null : new Paged<PartAudit>(list, pageIndex, pageSize, totalCount);
        }

        #endregion

        #region ---MAPPER---

        private PartAudit MapSingleAudit(IDataReader reader, ref int startingIndex)
        {
            PartAudit audit = new PartAudit();
            audit.Part = new PartSlim();
            audit.User = new UserSlim();

            audit.Id = reader.GetSafeInt32(startingIndex++);
            audit.Part.Id = reader.GetSafeInt32(startingIndex++);
            audit.Part.Name = reader.GetSafeString(startingIndex++);
            audit.ChangeType = reader.GetSafeString(startingIndex++);
            audit.ColumnName = reader.GetSafeString(startingIndex++);
            audit.OldValue = reader.GetSafeString(startingIndex++);
            audit.NewValue = reader.GetSafeString(startingIndex++);
            audit.User.Id = reader.GetSafeInt32(startingIndex++);
            audit.User.Name = reader.GetSafeString(startingIndex++);
            audit.ChangedOn = reader.GetSafeDateTime(startingIndex++);

            return audit;

        }
        #endregion

    }
}
