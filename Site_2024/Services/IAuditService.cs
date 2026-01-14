using Site_2024.Web.Api.Constructors;
using Site_2024.Web.Api.Models;

namespace Site_2024.Web.Api.Services
{
    public interface IAuditService
    {
        PartAudit GetAuditByPartId(int partId, int maxRows);
        Paged<PartAudit> GetAuditByPartIdPaginated(int partId, int pageIndex, int pageSize);
        PartAudit GetAuditByRecent(int maxRows);
        Paged<PartAudit> GetAuditRecentPaginated(int pageIndex, int pageSize);
    }
}