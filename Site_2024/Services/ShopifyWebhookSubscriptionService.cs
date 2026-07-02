using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Models;
using System.Text;
using System.Text.Json;

namespace Site_2024.Web.Api.Services
{
    public class ShopifyWebhookSubscriptionService : IShopifyWebhookSubscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly IShopifyTokenService _tokenService;
        private readonly ShopifySettings _settings;
        private readonly ILogger<ShopifyWebhookSubscriptionService> _logger;

        public ShopifyWebhookSubscriptionService(
            HttpClient httpClient,
            IShopifyTokenService tokenService,
            IOptions<ShopifySettings> settings,
            ILogger<ShopifyWebhookSubscriptionService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ShopifyWebhookSubscriptionResult> RegisterOrdersPaidWebhookAsync(string callbackUrl)
        {
            ValidateCallbackUrl(callbackUrl);

            string mutation = @"
mutation webhookSubscriptionCreate($topic: WebhookSubscriptionTopic!, $webhookSubscription: WebhookSubscriptionInput!) {
  webhookSubscriptionCreate(topic: $topic, webhookSubscription: $webhookSubscription) {
    webhookSubscription {
      id
      topic
      uri
      format
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                topic = "ORDERS_PAID",
                webhookSubscription = new
                {
                    uri = callbackUrl
                }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);
            ThrowIfTopLevelErrors(doc);

            JsonElement payload = doc.RootElement
                .GetProperty("data")
                .GetProperty("webhookSubscriptionCreate");

            ShopifyWebhookSubscriptionResult result = new ShopifyWebhookSubscriptionResult();

            if (payload.TryGetProperty("userErrors", out JsonElement userErrors))
            {
                foreach (JsonElement error in userErrors.EnumerateArray())
                {
                    string? message = GetString(error, "message");
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        result.UserErrors.Add(message);
                    }
                }
            }

            if (payload.TryGetProperty("webhookSubscription", out JsonElement subscription)
                && subscription.ValueKind != JsonValueKind.Null)
            {
                result.Subscription = MapSubscription(subscription);
            }

            return result;
        }

        public async Task<List<ShopifyWebhookSubscriptionInfo>> GetOrderWebhookSubscriptionsAsync()
        {
            string query = @"
query GetOrderWebhookSubscriptions {
  webhookSubscriptions(first: 50, topics: [ORDERS_PAID, ORDERS_CREATE, ORDERS_UPDATED]) {
    nodes {
      id
      topic
      uri
      format
    }
  }
}";

            using JsonDocument doc = await SendGraphQlAsync(query, new { });
            ThrowIfTopLevelErrors(doc);

            JsonElement nodes = doc.RootElement
                .GetProperty("data")
                .GetProperty("webhookSubscriptions")
                .GetProperty("nodes");

            List<ShopifyWebhookSubscriptionInfo> subscriptions = new List<ShopifyWebhookSubscriptionInfo>();

            foreach (JsonElement node in nodes.EnumerateArray())
            {
                subscriptions.Add(MapSubscription(node));
            }

            return subscriptions;
        }

        private async Task<JsonDocument> SendGraphQlAsync(string query, object variables)
        {
            string shopDomain = NormalizeShopDomain(_settings.ShopDomain);
            string endpoint = $"https://{shopDomain}/admin/api/{_settings.ApiVersion}/graphql.json";
            string token = await _tokenService.GetAccessTokenAsync();

            var body = new
            {
                query,
                variables
            };

            string json = JsonSerializer.Serialize(body);

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Add("X-Shopify-Access-Token", token);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Shopify webhook subscription GraphQL request failed. Status: {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    responseText);

                throw new ApplicationException(
                    $"Shopify webhook subscription GraphQL request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {responseText}");
            }

            return JsonDocument.Parse(responseText);
        }

        private static void ValidateCallbackUrl(string callbackUrl)
        {
            if (string.IsNullOrWhiteSpace(callbackUrl))
            {
                throw new ApplicationException("CallbackUrl is required.");
            }

            if (!Uri.TryCreate(callbackUrl, UriKind.Absolute, out Uri? uri))
            {
                throw new ApplicationException("CallbackUrl must be an absolute URL.");
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new ApplicationException("CallbackUrl must use HTTPS for Shopify webhooks.");
            }
        }

        private static ShopifyWebhookSubscriptionInfo MapSubscription(JsonElement node)
        {
            return new ShopifyWebhookSubscriptionInfo
            {
                Id = GetString(node, "id") ?? string.Empty,
                Topic = GetString(node, "topic") ?? string.Empty,
                Uri = GetString(node, "uri") ?? string.Empty,
                Format = GetString(node, "format")
            };
        }

        private static void ThrowIfTopLevelErrors(JsonDocument doc)
        {
            if (doc.RootElement.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }
        }

        private static string NormalizeShopDomain(string shopDomain)
        {
            shopDomain = shopDomain
                .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                .Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                .Trim()
                .TrimEnd('/');

            if (!shopDomain.EndsWith(".myshopify.com", StringComparison.OrdinalIgnoreCase))
            {
                shopDomain = $"{shopDomain}.myshopify.com";
            }

            return shopDomain;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value)
                || value.ValueKind == JsonValueKind.Null
                || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        }
    }
}
