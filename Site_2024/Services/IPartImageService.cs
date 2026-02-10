using Site_2024.Models.Domain.Parts;
using System.Collections.Generic;

namespace Site_2024.Web.Api.Services
{
    public interface IPartImageService
    {
        int Add(int partId, string url, bool isPrimary, int sortOrder, int userId);
        List<PartImage> GetByPartId(int partId);
        bool HasPrimary(int partId);
    }
}
