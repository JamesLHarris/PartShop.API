using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Configurations;
using Site_2024.Web.Api.Extensions;
using Site_2024.Web.Api.Interfaces;
using Site_2024.Web.Api.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using StaticFileOptions = Site_2024.Web.Api.Configurations.StaticFileOptions;

namespace Site_2024.Web.Api.Services
{
    public class ShopifyWebhookService : IShopifyWebhookService
    {
        private readonly ShopifySettings _settings;
        private readonly IDataProvider _data;
        private readonly StaticFileOptions _staticFileOptions;
        private readonly ILogger<ShopifyWebhookService> _logger;

        public ShopifyWebhookService(
            IOptions<ShopifySettings> settings,
            IDataProvider data,
            IOptions<StaticFileOptions> staticFileOptions,
            ILogger<ShopifyWebhookService> logger)
        {
            _settings = settings.Value;
            _data = data;
            _staticFileOptions = staticFileOptions.Value;
            _logger = logger;
        }

        public async Task<ShopifyWebhookProcessingResult> ProcessOrdersPaidWebhookAsync(byte[] rawBody, IHeaderDictionary headers)
        {
            ShopifyWebhookProcessingResult result = new ShopifyWebhookProcessingResult
            {
                WebhookId = GetHeader(headers, "X-Shopify-Webhook-Id"),
                Topic = GetHeader(headers, "X-Shopify-Topic"),
                ShopDomain = GetHeader(headers, "X-Shopify-Shop-Domain")
            };

            string? hmacHeader = GetHeader(headers, "X-Shopify-Hmac-SHA256");
            result.IsHmacValid = VerifyHmac(rawBody, hmacHeader);

            if (!result.IsHmacValid)
            {
                result.Message = "Webhook HMAC verification failed.";
                return result;
            }

            if (string.IsNullOrWhiteSpace(result.WebhookId))
            {
                // Shopify sends this header. If it is missing, something is off, so do not process.
                result.Message = "Missing X-Shopify-Webhook-Id header.";
                throw new ApplicationException(result.Message);
            }

            if (!string.IsNullOrWhiteSpace(result.Topic)
                && !string.Equals(result.Topic, "orders/paid", StringComparison.OrdinalIgnoreCase))
            {
                result.Message = $"Ignored webhook topic {result.Topic}. Expected orders/paid.";
                WebhookReceiptState ignoredReceipt = InsertWebhookReceipt(result.WebhookId, result.Topic, result.ShopDomain, null);
                result.IsDuplicate = ignoredReceipt.IsDuplicate;

                if (!ignoredReceipt.IsDuplicate || !string.Equals(ignoredReceipt.Status, "Processed", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateWebhookReceipt(result.WebhookId, null, "Processed", 0, 0, 1, result.Message);
                }

                return result;
            }

            using JsonDocument doc = JsonDocument.Parse(rawBody);
            JsonElement root = doc.RootElement;

            result.ShopifyOrderId = GetInt64(root, "id");
            result.OrderName = GetString(root, "name") ?? GetString(root, "order_number");

            WebhookReceiptState receipt = InsertWebhookReceipt(
                result.WebhookId,
                result.Topic ?? "orders/paid",
                result.ShopDomain,
                result.ShopifyOrderId <= 0 ? null : result.ShopifyOrderId);

            result.IsDuplicate = receipt.IsDuplicate;

            if (receipt.IsDuplicate && string.Equals(receipt.Status, "Processed", StringComparison.OrdinalIgnoreCase))
            {
                result.Message = "Duplicate Shopify webhook delivery. Already processed.";
                return result;
            }

            try
            {
                string? financialStatus = GetString(root, "financial_status");

                if (!string.IsNullOrWhiteSpace(financialStatus) && !IsPaidFinancialStatus(financialStatus))
                {
                    result.Message = $"Skipped because financial_status is {financialStatus}.";
                    result.SkippedCount++;
                    UpdateWebhookReceipt(result.WebhookId, result.ShopifyOrderId, "Processed", 0, 0, result.SkippedCount, result.Message);
                    return result;
                }

                if (!root.TryGetProperty("line_items", out JsonElement lineItems)
                    || lineItems.ValueKind != JsonValueKind.Array)
                {
                    result.Message = "Webhook payload did not include line_items.";
                    result.SkippedCount++;
                    UpdateWebhookReceipt(result.WebhookId, result.ShopifyOrderId, "Processed", 0, 0, result.SkippedCount, result.Message);
                    return result;
                }

                foreach (JsonElement lineItem in lineItems.EnumerateArray())
                {
                    result.LineItemsChecked++;

                    long lineItemId = GetInt64(lineItem, "id");
                    long? variantId = GetNullableInt64(lineItem, "variant_id");
                    int quantity = GetInt32(lineItem, "quantity");

                    ShopifyWebhookLineItemResult row = new ShopifyWebhookLineItemResult
                    {
                        ShopifyLineItemId = lineItemId,
                        ShopifyVariantId = variantId,
                        QuantityPurchased = quantity <= 0 ? 1 : quantity
                    };

                    if (lineItemId <= 0)
                    {
                        row.Message = "Skipped because the Shopify line item did not include a valid id.";
                        result.SkippedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    if (!variantId.HasValue || variantId.Value <= 0)
                    {
                        row.Message = "Skipped because the Shopify line item did not include a variant_id.";
                        result.SkippedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    ShopifyLocalPartMatch? localPart = GetLocalPartMatchByVariantId(variantId.Value);

                    if (localPart == null)
                    {
                        row.Message = "Skipped because no local part matched this Shopify variant.";
                        result.SkippedCount++;
                        result.Items.Add(row);
                        continue;
                    }

                    row.PartId = localPart.PartId;
                    row.PartName = localPart.PartName;

                    bool wasAlreadySynced = MarkLocalPartSoldFromOrder(
                        localPart.PartId,
                        result.ShopifyOrderId,
                        lineItemId,
                        variantId.Value,
                        row.QuantityPurchased);

                    row.WasAlreadySynced = wasAlreadySynced;
                    row.WasSynced = !wasAlreadySynced;
                    row.Message = wasAlreadySynced
                        ? "This Shopify order line was already recorded."
                        : "Recorded confirmed sale and updated local quantity.";

                    if (wasAlreadySynced)
                    {
                        result.AlreadySyncedCount++;
                    }
                    else
                    {
                        result.PartsMarkedSold++;
                    }

                    result.Items.Add(row);
                }

                result.Message = "Shopify orders/paid webhook processed.";
                UpdateWebhookReceipt(
                    result.WebhookId,
                    result.ShopifyOrderId,
                    "Processed",
                    result.PartsMarkedSold,
                    result.AlreadySyncedCount,
                    result.SkippedCount,
                    null);

                await Task.CompletedTask;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process Shopify orders/paid webhook {WebhookId} for order {ShopifyOrderId}.",
                    result.WebhookId,
                    result.ShopifyOrderId);

                UpdateWebhookReceipt(
                    result.WebhookId,
                    result.ShopifyOrderId,
                    "Error",
                    result.PartsMarkedSold,
                    result.AlreadySyncedCount,
                    result.SkippedCount,
                    ex.Message);

                throw;
            }
        }

        private bool VerifyHmac(byte[] rawBody, string? hmacHeader)
        {
            if (string.IsNullOrWhiteSpace(hmacHeader))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(_settings.ClientSecret))
            {
                throw new ApplicationException("ShopifySettings:ClientSecret is missing. Cannot verify webhook HMAC.");
            }

            try
            {
                byte[] receivedHmac = Convert.FromBase64String(hmacHeader.Trim());
                byte[] secretBytes = Encoding.UTF8.GetBytes(_settings.ClientSecret);

                using HMACSHA256 hmac = new HMACSHA256(secretBytes);
                byte[] calculatedHmac = hmac.ComputeHash(rawBody);

                return receivedHmac.Length == calculatedHmac.Length
                    && CryptographicOperations.FixedTimeEquals(receivedHmac, calculatedHmac);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private WebhookReceiptState InsertWebhookReceipt(string webhookId, string? topic, string? shopDomain, long? shopifyOrderId)
        {
            WebhookReceiptState state = new WebhookReceiptState();
            const string procName = "[dbo].[ShopifyWebhookReceipts_InsertIfNew]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@WebhookId", webhookId);
                    col.AddWithValue("@Topic", string.IsNullOrWhiteSpace(topic) ? "orders/paid" : topic);
                    col.AddWithValue("@ShopDomain", string.IsNullOrWhiteSpace(shopDomain) ? DBNull.Value : shopDomain);
                    col.AddWithValue("@ShopifyOrderId", shopifyOrderId.HasValue ? shopifyOrderId.Value : DBNull.Value);
                },
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int i = 0;
                    state.Id = reader.GetSafeInt32(i++);
                    state.WebhookId = reader.GetSafeString(i++);
                    state.IsDuplicate = reader.GetSafeBool(i++);
                    state.Status = reader.GetSafeString(i++);
                });

            return state;
        }

        private void UpdateWebhookReceipt(
            string webhookId,
            long? shopifyOrderId,
            string status,
            int partsMarkedSold,
            int alreadySyncedCount,
            int skippedCount,
            string? errorMessage)
        {
            const string procName = "[dbo].[ShopifyWebhookReceipts_UpdateResult]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@WebhookId", webhookId);
                    col.AddWithValue("@ShopifyOrderId", shopifyOrderId.HasValue ? shopifyOrderId.Value : DBNull.Value);
                    col.AddWithValue("@Status", status);
                    col.AddWithValue("@PartsMarkedSold", partsMarkedSold);
                    col.AddWithValue("@AlreadySyncedCount", alreadySyncedCount);
                    col.AddWithValue("@SkippedCount", skippedCount);
                    col.AddWithValue("@ErrorMessage", string.IsNullOrWhiteSpace(errorMessage) ? DBNull.Value : errorMessage);
                },
                singleRecordMapper: delegate { });
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

                    string imagePath;

                    match = new ShopifyLocalPartMatch
                    {
                        PartId = reader.GetSafeInt32(i++),
                        PartName = reader.GetSafeString(i++),
                        PartNumber = reader.GetSafeString(i++)
                    };

                    imagePath = reader.GetSafeString(i++);
                    match.ImageUrl = string.IsNullOrWhiteSpace(imagePath)
                        ? null
                        : $"{_staticFileOptions.ImageBaseUrl}{imagePath}";

                    match.AvailableId = reader.GetSafeInt32(i++);
                    match.AvailableStatus = reader.GetSafeString(i++);
                    match.SiteName = reader.GetSafeString(i++);
                    match.AreaName = reader.GetSafeString(i++);
                    match.AisleName = reader.GetSafeString(i++);
                    match.ShelfName = reader.GetSafeString(i++);
                    match.SectionName = reader.GetSafeString(i++);
                    match.BoxName = reader.GetSafeString(i++);
                    match.OtherBox = reader.GetSafeString(i++);
                    match.ShopifyVariantId = reader.GetSafeInt64(i++);
                    match.ShopifyOrderId = reader.GetSafeInt64Nullable(i++);
                    match.SoldOnUtc = reader.GetSafeDateTimeNullable(i++);
                    match.Quantity = reader.GetSafeInt32(i++);
                });

            return match;
        }

        private bool MarkLocalPartSoldFromOrder(
            int partId,
            long shopifyOrderId,
            long shopifyLineItemId,
            long shopifyVariantId,
            int quantityPurchased)
        {
            bool wasAlreadySynced = false;
            const string procName = "[dbo].[Parts_MarkSoldFromShopifyOrder]";

            _data.ExecuteCmd(procName,
                inputParamMapper: delegate (SqlParameterCollection col)
                {
                    col.AddWithValue("@PartId", partId);
                    col.AddWithValue("@ShopifyOrderId", shopifyOrderId);
                    col.AddWithValue("@ShopifyLineItemId", shopifyLineItemId);
                    col.AddWithValue("@ShopifyVariantId", shopifyVariantId);
                    col.AddWithValue("@QuantityPurchased", quantityPurchased <= 0 ? 1 : quantityPurchased);
                    col.AddWithValue("@Source", "ShopifyWebhook");
                    col.AddWithValue("@LastMovedBy", DBNull.Value);
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

        private static bool IsPaidFinancialStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            string normalized = status.Trim()
                .Replace("-", "_")
                .Replace(" ", "_")
                .ToUpperInvariant();

            return normalized == "PAID" || normalized == "PARTIALLY_PAID";
        }

        private static string? GetHeader(IHeaderDictionary headers, string name)
        {
            return headers.TryGetValue(name, out var values) ? values.FirstOrDefault() : null;
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

        private static int GetInt32(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value)
                || value.ValueKind == JsonValueKind.Null
                || value.ValueKind == JsonValueKind.Undefined)
            {
                return 0;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int intValue))
            {
                return intValue;
            }

            return int.TryParse(value.ToString(), out int parsedValue) ? parsedValue : 0;
        }

        private static long GetInt64(JsonElement element, string propertyName)
        {
            return GetNullableInt64(element, propertyName) ?? 0;
        }

        private static long? GetNullableInt64(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement value)
                || value.ValueKind == JsonValueKind.Null
                || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out long longValue))
            {
                return longValue;
            }

            return long.TryParse(value.ToString(), out long parsedValue) ? parsedValue : null;
        }

        private class WebhookReceiptState
        {
            public int Id { get; set; }
            public string WebhookId { get; set; } = string.Empty;
            public bool IsDuplicate { get; set; }
            public string Status { get; set; } = string.Empty;
        }
    }
}
