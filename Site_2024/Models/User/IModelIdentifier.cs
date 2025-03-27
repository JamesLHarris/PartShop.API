namespace Site_2024.Web.Api.Models.User
{
    /// <summary>
    /// Interface to implment on any Request Model that you would like to have hydrated by an Id parameter
    /// </summary>
    public interface IModelIdentifier
    {
        /// <summary>
        /// 
        /// </summary>
        int Id { get; set; }
    }
}
