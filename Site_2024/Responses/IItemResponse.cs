namespace Site_2024.Web.Api.Responses
{
    public interface IItemResponse
    {
        bool IsSuccessful { get; set; }

        string TransactionId { get; set; }

        object Item { get; }
    }
}