
namespace Site_2024.Web.Api.Services
{
    public interface IShopifyTokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}