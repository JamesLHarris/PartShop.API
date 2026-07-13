using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Configurations;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Text.Json;
using StaticFileOptions = Site_2024.Web.Api.Configurations.StaticFileOptions;

namespace Site_2024.Web.Api.Services
{
    public class ShopifyOrderService : IShopifyOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IShopifyTokenService _tokenService;
        private readonly ShopifySettings _settings;
        private readonly IDataProvider _data;
        private readonly StaticFileOptions _staticFileOptions;
        private readonly ILogger<ShopifyOrderService> _logger;

        public ShopifyOrderService(
            HttpClient httpClient,
            IShopifyTokenService tokenService,
            IOptions<ShopifySettings> settings,
            IDataProvider data,
            IOptions<StaticFileOptions> staticFileOptions,
            ILogger<ShopifyOrderService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _settings = settings.Value;
            _data = data;
            _staticFileOptions = staticFileOptions.Value;
            _logger = logger;
        }

        public async Task<List<ShopifyOrderSummary>> GetRecentOrdersAsync(int first, string? view)
        {
            first = first <= 0 ? 25 : Math.Min(first, 50);

            string? orderQuery = BuildOrderQuery(view);

            string query = @"
query GetRecentOrders($first: Int!, $query: String) {
  orders(first: $first, reverse: true, sortKey: CREATED_AT, query: $query) {
    nodes {
      id
      name
      createdAt
      email
      displayFinancialStatus
      displayFulfillmentStatus
      customer {
        displayName
        email
      }
      currentTotalPriceSet {
        shopMoney {
          amount
          currencyCode
        }
      }
      lineItems(first: 50) {
        nodes {
          id
          title
          quantity
          sku
          originalUnitPriceSet {
            shopMoney {
              amount
              currencyCode
            }
          }
          variant {
            id
            sku
            image {
              url
            }
            product {
              id
            }
          }
        }
      }
    }
  }
}";

            var variables = new
            {
                first,
                query = orderQuery
            };

            using JsonDocument doc = await SendGraphQlAsync(query, variables);
            ThrowIfTopLevelErrors(doc);

            JsonElement nodes = doc.RootElement
                .GetProperty("data")
                .GetProperty("orders")
                .GetProperty("nodes");

            List<ShopifyOrderSummary> orders = new List<ShopifyOrderSummary>();

            foreach (JsonElement node in nodes.EnumerateArray())
            {
                ShopifyOrderSummary order = MapOrder(node);

                foreach (ShopifyOrderLineItemSummary item in order.LineItems)
                {
                    if (item.ShopifyVariantId.HasValue)
                    {
                        item.LocalPart = GetLocalPartMatchByVariantId(item.ShopifyVariantId.Value);
                    }
                }

                orders.Add(order);
            }

            return orders;
        }

        public async Task<ShopifyOrderSyncResult> SyncRecentPaidOrdersAsync(int first, int userId)
        {
            ShopifyOrderSyncResult result = new ShopifyOrderSyncResult();
            List<ShopifyOrderSummary> orders = await GetRecentOrdersAsync(first, "awaitingShipment");

            result.OrdersChecked = orders.Count;

            foreach (ShopifyOrderSummary order in orders)
            {
                bool isPaid = IsPaidFinancialStatus(order.DisplayFinancialStatus);

                foreach (ShopifyOrderLineItemSummary item in order.LineItems)
                {
                    result.LineItemsChecked++;

                    ShopifyOrderSyncLineItemResult row = new ShopifyOrderSyncLineItemResult
                    {
                        OrderName = order.Name,
                        ShopifyOrderId = order.ShopifyOrderId,
                        ShopifyLineItemId = item.ShopifyLineItemId,
                        ShopifyVariantId = item.ShopifyVariantId,
                        PartId = item.LocalPart?.PartId,
                        PartName = item.LocalPart?.PartName,
                        QuantityPurchased = item.Quantity
                    };

                    if (!isPaid)
                    {
                        row.Message = $"Skipped because financial status is {order.DisplayFinancialStatus ?? "Unknown"}.";
                        result.SkippedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    if (item.LocalPart == null)
                    {
                        row.Message = "Skipped because no local part matched this Shopify variant.";
                        result.SkippedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    if (item.LocalPart.ShopifyOrderId.HasValue && item.LocalPart.ShopifyOrderId.Value == order.ShopifyOrderId)
                    {
                        row.WasAlreadySynced = true;
                        row.Message = "Already synced to this Shopify order.";
                        result.AlreadySyncedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    if (item.LocalPart.ShopifyOrderId.HasValue && item.LocalPart.ShopifyOrderId.Value != order.ShopifyOrderId)
                    {
                        row.Message = $"Skipped because local part is already attached to Shopify order {item.LocalPart.ShopifyOrderId.Value}.";
                        result.SkippedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    try
                    {
                        bool wasAlreadySynced = MarkLocalPartSoldFromOrder(
                            item.LocalPart.PartId,
                            order.ShopifyOrderId,
                            item.Quantity,
                            userId);

                        row.WasAlreadySynced = wasAlreadySynced;
                        row.WasSynced = !wasAlreadySynced;
                        row.Message = wasAlreadySynced
                            ? "Already synced to this Shopify order."
                            : "Marked local part sold/unavailable.";

                        if (wasAlreadySynced)
                        {
                            result.AlreadySyncedCount++;
                        }
                        else
                        {
                            result.PartsMarkedSold++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to sync Shopify order {ShopifyOrderId} line item {ShopifyLineItemId} to local part {PartId}.",
                            order.ShopifyOrderId,
                            item.ShopifyLineItemId,
                            item.LocalPart.PartId);

                        row.Message = ex.Message;
                        result.SkippedCount++;
                    }

                    result.Items.Add(row);
                }
            }

            return result;
        }

        private static bool IsPaidFinancialStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            string normalized = status.Trim().ToUpperInvariant();
            return normalized == "PAID" || normalized == "PARTIALLY_PAID";
        }

        private bool MarkLocalPartSoldFromOrder(int partId, long shopifyOrderId, int quantityPurchased, int userId)
        {
            bool wasAlreadySynced = false;
            const string procName = "[dbo].[Parts_MarkSoldFromShopifyOrder]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PartId", partId);
                    col.AddWithValue("@ShopifyOrderId", shopifyOrderId);
                    col.AddWithValue("@QuantityPurchased", quantityPurchased <= 0 ? 1 : quantityPurchased);
                    col.AddWithValue("@LastMovedBy", userId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    i++; // Id
                    i++; // ShopifyOrderId
                    i++; // SoldOnUtc
                    i++; // Quantity
                    i++; // AvailableId
                    wasAlreadySynced = reader.GetSafeBool(i++);
                });

            return wasAlreadySynced;
        }

        private ShopifyOrderSummary MapOrder(JsonElement node)
        {
            string orderGid = GetString(node, "id") ?? string.Empty;

            ShopifyOrderSummary order = new ShopifyOrderSummary
            {
                ShopifyOrderGid = orderGid,
                ShopifyOrderId = ExtractNumericId(orderGid),
                Name = GetString(node, "name") ?? string.Empty,
                OrderNumber = ExtractOrderNumberFromName(GetString(node, "name")),
                CreatedAt = GetDateTime(node, "createdAt"),
                CustomerEmail = GetString(node, "email"),
                DisplayFinancialStatus = GetString(node, "displayFinancialStatus"),
                DisplayFulfillmentStatus = GetString(node, "displayFulfillmentStatus")
            };

            if (node.TryGetProperty("customer", out JsonElement customer)
                && customer.ValueKind != JsonValueKind.Null)
            {
                order.CustomerDisplayName = GetString(customer, "displayName");
                order.CustomerEmail = GetString(customer, "email") ?? order.CustomerEmail;
            }

            if (node.TryGetProperty("currentTotalPriceSet", out JsonElement totalSet)
                && totalSet.TryGetProperty("shopMoney", out JsonElement shopMoney))
            {
                order.TotalPrice = GetDecimal(shopMoney, "amount");
                order.CurrencyCode = GetString(shopMoney, "currencyCode");
            }

            if (node.TryGetProperty("lineItems", out JsonElement lineItems)
                && lineItems.TryGetProperty("nodes", out JsonElement lineItemNodes))
            {
                foreach (JsonElement lineItemNode in lineItemNodes.EnumerateArray())
                {
                    order.LineItems.Add(MapLineItem(lineItemNode));
                }
            }

            return order;
        }

        private ShopifyOrderLineItemSummary MapLineItem(JsonElement node)
        {
            string lineItemGid = GetString(node, "id") ?? string.Empty;

            ShopifyOrderLineItemSummary item = new ShopifyOrderLineItemSummary
            {
                ShopifyLineItemGid = lineItemGid,
                ShopifyLineItemId = ExtractNumericId(lineItemGid),
                Title = GetString(node, "title") ?? string.Empty,
                Sku = GetString(node, "sku"),
                Quantity = GetInt32(node, "quantity")
            };

            if (node.TryGetProperty("originalUnitPriceSet", out JsonElement priceSet)
                && priceSet.TryGetProperty("shopMoney", out JsonElement shopMoney))
            {
                item.UnitPrice = GetDecimal(shopMoney, "amount");
                item.CurrencyCode = GetString(shopMoney, "currencyCode");
            }

            if (node.TryGetProperty("variant", out JsonElement variant)
                && variant.ValueKind != JsonValueKind.Null)
            {
                string? variantGid = GetString(variant, "id");
                item.ShopifyVariantGid = variantGid;
                item.ShopifyVariantId = string.IsNullOrWhiteSpace(variantGid) ? null : ExtractNumericId(variantGid);

                item.Sku = GetString(variant, "sku") ?? item.Sku;

                if (variant.TryGetProperty("image", out JsonElement image)
                    && image.ValueKind != JsonValueKind.Null)
                {
                    item.ShopifyImageUrl = GetString(image, "url");
                }

                if (variant.TryGetProperty("product", out JsonElement product)
                    && product.ValueKind != JsonValueKind.Null)
                {
                    string? productGid = GetString(product, "id");
                    item.ShopifyProductGid = productGid;
                    item.ShopifyProductId = string.IsNullOrWhiteSpace(productGid) ? null : ExtractNumericId(productGid);
                }
            }

            return item;
        }

        private ShopifyLocalPartMatch? GetLocalPartMatchByVariantId(long shopifyVariantId)
        {
            ShopifyLocalPartMatch? match = null;
            const string procName = "[dbo].[Parts_GetOrderMatchByShopifyVariantId]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@ShopifyVariantId", shopifyVariantId);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;

                    int partId = reader.GetSafeInt32(i++);
                    string partName = reader.GetSafeString(i++);
                    string partNumber = reader.GetSafeString(i++);
                    string imagePath = reader.GetSafeString(i++);

                    match = new ShopifyLocalPartMatch
                    {
                        PartId = partId,
                        PartName = partName,
                        PartNumber = partNumber,
                        ImageUrl = string.IsNullOrWhiteSpace(imagePath)
                            ? null
                            : $"{_staticFileOptions.ImageBaseUrl}{imagePath}",
                        AvailableId = reader.GetSafeInt32(i++),
                        AvailableStatus = reader.GetSafeString(i++),
                        SiteName = reader.GetSafeString(i++),
                        AreaName = reader.GetSafeString(i++),
                        AisleName = reader.GetSafeString(i++),
                        ShelfName = reader.GetSafeString(i++),
                        SectionName = reader.GetSafeString(i++),
                        BoxName = reader.GetSafeString(i++),
                        OtherBox = reader.GetSafeString(i++),
                        ShopifyVariantId = reader.GetSafeInt64(i++),
                        ShopifyOrderId = reader.GetSafeInt64Nullable(i++),
                        SoldOnUtc = reader.GetSafeDateTimeNullable(i++),
                        Quantity = reader.GetSafeInt32(i++)
                    };
                });

            return match;
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
                _logger.LogError("Shopify orders GraphQL request failed. Status: {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    responseText);

                throw new ApplicationException(
                    $"Shopify orders GraphQL request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {responseText}");
            }

            return JsonDocument.Parse(responseText);
        }

        private static string? BuildOrderQuery(string? view)
        {
            if (string.IsNullOrWhiteSpace(view))
            {
                return null;
            }

            switch (view.Trim().ToLowerInvariant())
            {
                case "awaitingshipment":
                case "awaiting-shipment":
                    return "status:open fulfillment_status:unfulfilled";

                case "fulfilled":
                    return "fulfillment_status:fulfilled";

                case "open":
                    return "status:open";

                case "all":
                default:
                    return null;
            }
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

        private static int ExtractOrderNumberFromName(string? orderName)
        {
            if (string.IsNullOrWhiteSpace(orderName))
            {
                return 0;
            }

            StringBuilder digits = new StringBuilder();

            foreach (char c in orderName)
            {
                if (char.IsDigit(c))
                {
                    digits.Append(c);
                }
            }

            return int.TryParse(digits.ToString(), out int orderNumber) ? orderNumber : 0;
        }

        private static long ExtractNumericId(string? gid)
        {
            if (string.IsNullOrWhiteSpace(gid))
            {
                return 0;
            }

            string idString = gid.Split('/').Last();
            return long.TryParse(idString, out long id) ? id : 0;
        }

        private static string? GetString(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value)
                || value.ValueKind == JsonValueKind.Null
                || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            return value.GetString();
        }

        private static int GetInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value)
                || value.ValueKind == JsonValueKind.Null
                || value.ValueKind == JsonValueKind.Undefined)
            {
                return 0;
            }

            return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int result)
                ? result
                : 0;
        }

        private static decimal GetDecimal(JsonElement element, string propertyName)
        {
            string? raw = GetString(element, propertyName);
            return decimal.TryParse(raw, out decimal result) ? result : 0m;
        }

        private static DateTime GetDateTime(JsonElement element, string propertyName)
        {
            string? raw = GetString(element, propertyName);
            return DateTime.TryParse(raw, out DateTime result) ? result : DateTime.MinValue;
        }
    }
}
