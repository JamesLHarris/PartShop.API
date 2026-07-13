using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Models.Shopify;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Site_2024.Web.Api.Services
{
    public class ShopifyAdminService : IShopifyAdminService
    {
        private readonly HttpClient _httpClient;
        private readonly IShopifyTokenService _tokenService;
        private readonly ShopifySettings _settings;
        private readonly ILogger<ShopifyAdminService> _logger;

        public ShopifyAdminService(
            HttpClient httpClient,
            IShopifyTokenService tokenService,
            IOptions<ShopifySettings> settings,
            ILogger<ShopifyAdminService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _settings = settings.Value;
            _logger = logger;
        }
        public async Task<ShopifyCreateProductResult> CreateProductForPartAsync(Part part)
        {
            string mutation = @"
mutation CreateSitePartProduct($product: ProductCreateInput!) {
  productCreate(product: $product) {
    product {
      id
      title
      status
      variants(first: 1) {
        nodes {
          id
          inventoryItem {
            id
          }
        }
      }
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                product = new
                {
                    title = BuildTitle(part),
                    descriptionHtml = BuildDescriptionHtml(part),
                    vendor = _settings.DefaultVendor,
                    productType = _settings.DefaultProductType,
                    status = _settings.CreateProductsAsDraft ? "DRAFT" : "ACTIVE",
                    tags = BuildTags(part)
                }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);

            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("productCreate");

            JsonElement userErrors = payload.GetProperty("userErrors");

            if (userErrors.GetArrayLength() > 0)
            {
                string messages = string.Join("; ",
                    userErrors.EnumerateArray().Select(e => e.GetProperty("message").GetString()));

                throw new ApplicationException($"Shopify productCreate failed: {messages}");
            }

            JsonElement product = payload.GetProperty("product");
            string productGid = product.GetProperty("id").GetString() ?? string.Empty;

            JsonElement variant = product
                .GetProperty("variants")
                .GetProperty("nodes")[0];

            string variantGid = variant.GetProperty("id").GetString() ?? string.Empty;

            string inventoryItemGid = variant
                .GetProperty("inventoryItem")
                .GetProperty("id")
                .GetString() ?? string.Empty;

            return new ShopifyCreateProductResult
            {
                ProductGid = productGid,
                VariantGid = variantGid,
                InventoryItemGid = inventoryItemGid,
                ProductId = ExtractNumericId(productGid),
                VariantId = ExtractNumericId(variantGid),
                InventoryItemId = ExtractNumericId(inventoryItemGid)
            };
        }

        public async Task<List<ShopifyLocationResult>> GetLocationsAsync()
        {
            string query = @"
query GetLocations {
  locations(first: 20, includeInactive: false) {
    nodes {
      id
      name
      isActive
    }
  }
}";

            using JsonDocument doc = await SendGraphQlAsync(query, new { });

            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement nodes = root
                .GetProperty("data")
                .GetProperty("locations")
                .GetProperty("nodes");

            List<ShopifyLocationResult> locations = new List<ShopifyLocationResult>();

            foreach (JsonElement node in nodes.EnumerateArray())
            {
                string gid = node.GetProperty("id").GetString() ?? string.Empty;

                locations.Add(new ShopifyLocationResult
                {
                    Gid = gid,
                    Id = ExtractNumericId(gid),
                    Name = node.GetProperty("name").GetString() ?? string.Empty,
                    IsActive = node.GetProperty("isActive").GetBoolean()
                });
            }

            return locations;
        }

        public async Task<ShopifyProductInventorySyncResult> SyncProductDetailsForPartAsync(Part part)
        {
            if (!part.ShopifyProductId.HasValue ||
                !part.ShopifyVariantId.HasValue ||
                !part.ShopifyInventoryItemId.HasValue)
            {
                throw new ApplicationException("Part is missing Shopify product, variant, or inventory item IDs.");
            }

            if (string.IsNullOrWhiteSpace(_settings.DefaultLocationGid))
            {
                throw new ApplicationException("ShopifySettings:DefaultLocationGid is missing.");
            }

            string productGid = $"gid://shopify/Product/{part.ShopifyProductId.Value}";
            string variantGid = $"gid://shopify/ProductVariant/{part.ShopifyVariantId.Value}";
            string inventoryItemGid = $"gid://shopify/InventoryItem/{part.ShopifyInventoryItemId.Value}";
            string locationGid = _settings.DefaultLocationGid;

            string cleanPartNumber = string.IsNullOrWhiteSpace(part.PartNumber)
                ? "NO-PART-NUMBER"
                : part.PartNumber.Trim();

            string sku = $"SITE-{part.Id}-{cleanPartNumber}";

            int quantity = part.Quantity <= 0 ? 0 : part.Quantity;

            await UpdateVariantPriceAsync(productGid, variantGid, part.Price);
            await UpdateInventoryItemAsync(inventoryItemGid, sku);
            await EnsureInventoryActiveAtLocationAsync(inventoryItemGid, locationGid);
            await SetInventoryQuantityAsync(inventoryItemGid, locationGid, quantity, part.Id);

            return new ShopifyProductInventorySyncResult
            {
                ProductGid = productGid,
                VariantGid = variantGid,
                InventoryItemGid = inventoryItemGid,
                LocationGid = locationGid,
                Price = part.Price,
                Sku = sku,
                Quantity = quantity,
                VariantUpdated = true,
                InventoryItemUpdated = true,
                InventoryActivated = true
            };
        }

        public async Task<ShopifyDiscountDeactivateResult> DeactivateDiscountCodeAsync(string shopifyDiscountGid)
        {
            if (string.IsNullOrWhiteSpace(shopifyDiscountGid))
            {
                throw new ApplicationException("ShopifyDiscountGid is required to deactivate a Shopify discount.");
            }

            string mutation = @"
mutation DeactivateDiscountCode($id: ID!) {
  discountCodeDeactivate(id: $id) {
    codeDiscountNode {
      id
      codeDiscount {
        ... on DiscountCodeBasic {
          title
          status
          endsAt
        }
      }
    }
    userErrors {
      field
      code
      message
    }
  }
}";

            var variables = new
            {
                id = shopifyDiscountGid
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);

            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("discountCodeDeactivate");

            JsonElement userErrors = payload.GetProperty("userErrors");

            if (userErrors.GetArrayLength() > 0)
            {
                string messages = string.Join("; ",
                    userErrors.EnumerateArray().Select(e =>
                    {
                        string field = e.TryGetProperty("field", out JsonElement f)
                            ? f.ToString()
                            : string.Empty;

                        string code = e.TryGetProperty("code", out JsonElement c)
                            ? c.ToString()
                            : string.Empty;

                        string message = e.TryGetProperty("message", out JsonElement m)
                            ? m.GetString() ?? string.Empty
                            : string.Empty;

                        return $"{field} {code} {message}".Trim();
                    }));

                throw new ApplicationException($"Shopify discountCodeDeactivate failed: {messages}");
            }

            JsonElement node = payload.GetProperty("codeDiscountNode");
            string gid = node.GetProperty("id").GetString() ?? shopifyDiscountGid;

            JsonElement codeDiscount = node.GetProperty("codeDiscount");

            string title = codeDiscount.GetProperty("title").GetString() ?? string.Empty;
            string status = codeDiscount.GetProperty("status").GetString() ?? string.Empty;

            DateTime? endsAt = null;
            if (codeDiscount.TryGetProperty("endsAt", out JsonElement endsAtElement)
                && endsAtElement.ValueKind != JsonValueKind.Null
                && DateTime.TryParse(endsAtElement.GetString(), out DateTime parsedEndsAt))
            {
                endsAt = parsedEndsAt;
            }

            return new ShopifyDiscountDeactivateResult
            {
                DiscountGid = gid,
                Title = title,
                Status = status,
                EndsAt = endsAt
            };
        }

        public async Task<ShopifyDiscountCreateResult> CreateBasicDiscountCodeAsync(AdminDiscountCode discount)
        {
            string mutation = @"
mutation CreateDiscountCode($basicCodeDiscount: DiscountCodeBasicInput!) {
  discountCodeBasicCreate(basicCodeDiscount: $basicCodeDiscount) {
    codeDiscountNode {
      id
      codeDiscount {
        ... on DiscountCodeBasic {
          title
          status
          codes(first: 1) {
            nodes {
              code
            }
          }
        }
      }
    }
    userErrors {
      field
      code
      message
    }
  }
}";

            Dictionary<string, object?> value = BuildDiscountValue(discount);
            Dictionary<string, object?> items = BuildDiscountItems(discount);

            var variables = new
            {
                basicCodeDiscount = new
                {
                    title = discount.Title,
                    code = discount.Code,
                    startsAt = (discount.StartsAtUtc ?? DateTime.UtcNow).ToString("o"),
                    endsAt = discount.EndsAtUtc?.ToString("o"),
                    usageLimit = discount.UsageLimit > 0 ? discount.UsageLimit : (int?)null,
                    appliesOncePerCustomer = discount.OncePerCustomer,

                    // All buyers can use the code.
                    context = new
                    {
                        all = "ALL"
                    },

                    customerGets = new
                    {
                        value = value,
                        items = items
                    }
                }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);

            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("discountCodeBasicCreate");

            JsonElement userErrors = payload.GetProperty("userErrors");

            if (userErrors.GetArrayLength() > 0)
            {
                string messages = string.Join("; ",
                    userErrors.EnumerateArray().Select(e =>
                    {
                        string field = e.TryGetProperty("field", out JsonElement f)
                            ? f.ToString()
                            : string.Empty;

                        string code = e.TryGetProperty("code", out JsonElement c)
                            ? c.ToString()
                            : string.Empty;

                        string message = e.TryGetProperty("message", out JsonElement m)
                            ? m.GetString() ?? string.Empty
                            : string.Empty;

                        return $"{field} {code} {message}".Trim();
                    }));

                throw new ApplicationException($"Shopify discountCodeBasicCreate failed: {messages}");
            }

            JsonElement node = payload.GetProperty("codeDiscountNode");
            string discountGid = node.GetProperty("id").GetString() ?? string.Empty;

            JsonElement basic = node.GetProperty("codeDiscount");
            string status = basic.GetProperty("status").GetString() ?? string.Empty;

            string codeValue = basic
                .GetProperty("codes")
                .GetProperty("nodes")[0]
                .GetProperty("code")
                .GetString() ?? discount.Code;

            return new ShopifyDiscountCreateResult
            {
                DiscountGid = discountGid,
                Code = codeValue,
                Status = status
            };
        }

        public async Task<ShopifyProductPublishResult> PublishProductForPartAsync(Part part)
        {
            if (!part.ShopifyProductId.HasValue)
            {
                throw new ApplicationException("Part does not have a ShopifyProductId. Sync it to Shopify before publishing.");
            }

            string productGid = $"gid://shopify/Product/{part.ShopifyProductId.Value}";

            string status = await UpdateProductStatusAsync(productGid, "ACTIVE", "publish");
            string onlineStorePublicationGid = await GetOnlineStorePublicationGidAsync();
            bool publishedToOnlineStore = await PublishProductToPublicationAsync(productGid, onlineStorePublicationGid);

            return new ShopifyProductPublishResult
            {
                PartId = part.Id,
                ProductGid = productGid,
                ProductId = part.ShopifyProductId.Value,
                Status = status,
                OnlineStorePublicationGid = onlineStorePublicationGid,
                PublishedToOnlineStore = publishedToOnlineStore
            };
        }

        public async Task<ShopifyProductPublishResult> UnpublishProductForPartAsync(Part part)
        {
            if (!part.ShopifyProductId.HasValue)
            {
                throw new ApplicationException("Part does not have a ShopifyProductId. Sync it to Shopify before unpublishing.");
            }

            string productGid = $"gid://shopify/Product/{part.ShopifyProductId.Value}";

            string onlineStorePublicationGid = await GetOnlineStorePublicationGidAsync();
            bool publishedToOnlineStore = await UnpublishProductFromPublicationAsync(productGid, onlineStorePublicationGid);
            string status = await UpdateProductStatusAsync(productGid, "DRAFT", "unpublish");

            return new ShopifyProductPublishResult
            {
                PartId = part.Id,
                ProductGid = productGid,
                ProductId = part.ShopifyProductId.Value,
                Status = status,
                OnlineStorePublicationGid = onlineStorePublicationGid,
                PublishedToOnlineStore = publishedToOnlineStore
            };
        }

        private async Task<string> UpdateProductStatusAsync(string productGid, string status, string actionName)
        {
            string mutation = @"
mutation UpdateProductStatus($product: ProductUpdateInput!) {
  productUpdate(product: $product) {
    product {
      id
      status
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                product = new
                {
                    id = productGid,
                    status = status
                }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);

            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("productUpdate");

            ThrowIfUserErrors(payload.GetProperty("userErrors"), $"Shopify productUpdate {actionName} failed");

            JsonElement product = payload.GetProperty("product");
            return product.GetProperty("status").GetString() ?? status;
        }

        private async Task<string> GetOnlineStorePublicationGidAsync()
        {
            if (!string.IsNullOrWhiteSpace(_settings.OnlineStorePublicationGid))
            {
                return _settings.OnlineStorePublicationGid.Trim();
            }

            string query = @"
query GetPublications {
  publications(first: 20) {
    nodes {
      id
      autoPublish
      supportsFuturePublishing
      catalog {
        id
        title
      }
    }
  }
}";

            using JsonDocument doc = await SendGraphQlAsync(query, new { });
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement nodes = root
                .GetProperty("data")
                .GetProperty("publications")
                .GetProperty("nodes");

            string? firstFuturePublishingPublication = null;
            string? firstPublication = null;

            foreach (JsonElement node in nodes.EnumerateArray())
            {
                string publicationGid = node.GetProperty("id").GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(publicationGid))
                {
                    continue;
                }

                firstPublication ??= publicationGid;

                bool supportsFuturePublishing = node.TryGetProperty("supportsFuturePublishing", out JsonElement futureEl)
                    && futureEl.ValueKind == JsonValueKind.True;

                if (supportsFuturePublishing)
                {
                    firstFuturePublishingPublication ??= publicationGid;
                }

                if (node.TryGetProperty("catalog", out JsonElement catalog) &&
                    catalog.ValueKind == JsonValueKind.Object &&
                    catalog.TryGetProperty("title", out JsonElement titleEl))
                {
                    string title = titleEl.GetString() ?? string.Empty;

                    if (string.Equals(title.Trim(), "Online Store", StringComparison.OrdinalIgnoreCase))
                    {
                        return publicationGid;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(firstFuturePublishingPublication))
            {
                return firstFuturePublishingPublication;
            }

            throw new ApplicationException(
                "Could not find the Shopify Online Store publication. Add Shopify:OnlineStorePublicationGid to configuration, or make sure the app has read_publications/write_publications scopes.");
        }

        private async Task<bool> PublishProductToPublicationAsync(string productGid, string publicationGid)
        {
            string mutation = @"
mutation PublishProductToOnlineStore($id: ID!, $publicationId: ID!) {
  publishablePublish(id: $id, input: { publicationId: $publicationId }) {
    publishable {
      publishedOnPublication(publicationId: $publicationId)
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                id = productGid,
                publicationId = publicationGid
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("publishablePublish");

            ThrowIfUserErrors(payload.GetProperty("userErrors"), "Shopify publishablePublish failed");

            return payload
                .GetProperty("publishable")
                .GetProperty("publishedOnPublication")
                .GetBoolean();
        }

        private async Task<bool> UnpublishProductFromPublicationAsync(string productGid, string publicationGid)
        {
            string mutation = @"
mutation UnpublishProductFromOnlineStore($id: ID!, $publicationId: ID!) {
  publishableUnpublish(id: $id, input: { publicationId: $publicationId }) {
    publishable {
      publishedOnPublication(publicationId: $publicationId)
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                id = productGid,
                publicationId = publicationGid
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("publishableUnpublish");

            ThrowIfUserErrors(payload.GetProperty("userErrors"), "Shopify publishableUnpublish failed");

            return payload
                .GetProperty("publishable")
                .GetProperty("publishedOnPublication")
                .GetBoolean();
        }

        private static void ThrowIfUserErrors(JsonElement userErrors, string failurePrefix)
        {
            if (userErrors.GetArrayLength() <= 0)
            {
                return;
            }

            string messages = string.Join("; ",
                userErrors.EnumerateArray().Select(e =>
                {
                    string field = e.TryGetProperty("field", out JsonElement f)
                        ? f.ToString()
                        : string.Empty;

                    string message = e.TryGetProperty("message", out JsonElement m)
                        ? m.GetString() ?? string.Empty
                        : string.Empty;

                    return string.IsNullOrWhiteSpace(field)
                        ? message
                        : $"{field}: {message}";
                }));

            throw new ApplicationException($"{failurePrefix}: {messages}");
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
                _logger.LogError("Shopify GraphQL request failed. Status: {StatusCode}. Body: {Body}",
                    response.StatusCode,
                    responseText);

                throw new ApplicationException(
                    $"Shopify GraphQL request failed: {(int)response.StatusCode} {response.ReasonPhrase}. Body: {responseText}");
            }

            return JsonDocument.Parse(responseText);
        }

        private static string BuildTitle(Part part)
        {
            string partNumber = string.IsNullOrWhiteSpace(part.PartNumber)
                ? $"Part #{part.Id}"
                : part.PartNumber;

            return $"{part.Name} - {partNumber}";
        }

        private static string BuildDescriptionHtml(Part part)
        {
            string description = System.Net.WebUtility.HtmlEncode(part.Description ?? string.Empty);
            string partNumber = System.Net.WebUtility.HtmlEncode(part.PartNumber ?? string.Empty);
            string condition = System.Net.WebUtility.HtmlEncode(part.Condition?.Name ?? string.Empty);
            string make = System.Net.WebUtility.HtmlEncode(part.Make?.Company ?? string.Empty);
            string model = System.Net.WebUtility.HtmlEncode(part.Make?.Model?.Name ?? string.Empty);

            return $@"
<p>{description}</p>
<ul>
  <li><strong>Site Part ID:</strong> {part.Id}</li>
  <li><strong>Part Number:</strong> {partNumber}</li>
  <li><strong>Condition:</strong> {condition}</li>
  <li><strong>Make:</strong> {make}</li>
  <li><strong>Model:</strong> {model}</li>
</ul>";
        }

        private static string[] BuildTags(Part part)
        {
            List<string> tags = new List<string>
            {
                "Site_2024",
                $"SitePartId_{part.Id}"
            };

            if (!string.IsNullOrWhiteSpace(part.Catagory?.Name))
            {
                tags.Add(part.Catagory.Name);
            }

            if (!string.IsNullOrWhiteSpace(part.Make?.Company))
            {
                tags.Add(part.Make.Company);
            }

            if (!string.IsNullOrWhiteSpace(part.Make?.Model?.Name))
            {
                tags.Add(part.Make.Model.Name);
            }

            return tags.ToArray();
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

        private static long ExtractNumericId(string gid)
        {
            if (string.IsNullOrWhiteSpace(gid))
            {
                throw new ApplicationException("Shopify returned an empty GID.");
            }

            string idString = gid.Split('/').Last();

            if (!long.TryParse(idString, out long id))
            {
                throw new ApplicationException($"Could not parse Shopify numeric ID from GID: {gid}");
            }

            return id;
        }
        private async Task UpdateVariantPriceAsync(string productGid, string variantGid, decimal price)
        {
            string mutation = @"
mutation UpdateVariantPrice($productId: ID!, $variants: [ProductVariantsBulkInput!]!) {
  productVariantsBulkUpdate(productId: $productId, variants: $variants) {
    product {
      id
    }
    productVariants {
      id
      price
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                productId = productGid,
                variants = new[]
                {
            new
            {
                id = variantGid,
                price = price.ToString("0.00", CultureInfo.InvariantCulture),
                inventoryPolicy = "DENY"
            }
        }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);
            ThrowIfGraphQlErrors(doc, "productVariantsBulkUpdate");
        }
        private async Task UpdateInventoryItemAsync(string inventoryItemGid, string sku)
        {
            string mutation = @"
mutation UpdateInventoryItem($id: ID!, $input: InventoryItemInput!) {
  inventoryItemUpdate(id: $id, input: $input) {
    inventoryItem {
      id
      sku
      tracked
      requiresShipping
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                id = inventoryItemGid,
                input = new
                {
                    sku = sku,
                    tracked = true,
                    requiresShipping = true
                }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);
            ThrowIfGraphQlErrors(doc, "inventoryItemUpdate");
        }

        private async Task EnsureInventoryActiveAtLocationAsync(
            string inventoryItemGid,
            string locationGid)
        {
            string mutation = @"
mutation ActivateInventoryItem(
  $inventoryItemId: ID!,
  $locationId: ID!,
  $idempotencyKey: String!
) {
  inventoryActivate(
    inventoryItemId: $inventoryItemId,
    locationId: $locationId
  ) @idempotent(key: $idempotencyKey) {
    inventoryLevel {
      id
      item {
        id
      }
      location {
        id
      }
    }
    userErrors {
      field
      message
    }
  }
}";

            var variables = new
            {
                inventoryItemId = inventoryItemGid,
                locationId = locationGid,
                idempotencyKey = $"site-2024-inventory-activate-{inventoryItemGid.Split('/').Last()}-{locationGid.Split('/').Last()}"
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);

            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty("inventoryActivate");

            JsonElement userErrors = payload.GetProperty("userErrors");

            if (userErrors.GetArrayLength() == 0)
            {
                return;
            }

            string messages = string.Join("; ",
                userErrors.EnumerateArray().Select(e => e.GetProperty("message").GetString()));

            // If it is already active, that is fine for our sync flow.
            if (messages.Contains("already active", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new ApplicationException($"Shopify inventoryActivate failed: {messages}");
        }

        private async Task SetInventoryQuantityAsync(
            string inventoryItemGid,
            string locationGid,
            int quantity,
            int partId)
        {
            string mutation = @"
mutation InventorySet($input: InventorySetQuantitiesInput!, $idempotencyKey: String!) {
  inventorySetQuantities(input: $input) @idempotent(key: $idempotencyKey) {
    inventoryAdjustmentGroup {
      reason
      referenceDocumentUri
      changes {
        name
        delta
        quantityAfterChange
      }
    }
    userErrors {
      code
      field
      message
    }
  }
}";

            var variables = new
            {
                idempotencyKey = $"site-2024-inventory-set-{partId}-{quantity}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                input = new
                {
                    name = "available",
                    reason = "correction",
                    referenceDocumentUri = $"gid://site-2024/Part/{partId}",
                    quantities = new[]
                    {
                new
                {
                    inventoryItemId = inventoryItemGid,
                    locationId = locationGid,
                    quantity = quantity,

                    // 2026-04 Shopify syntax:
                    // null means skip compare-and-swap check.
                    changeFromQuantity = (int?)null
                }
            }
                }
            };

            using JsonDocument doc = await SendGraphQlAsync(mutation, variables);
            ThrowIfGraphQlErrors(doc, "inventorySetQuantities");
        }

        private static void ThrowIfGraphQlErrors(JsonDocument doc, string payloadName)
        {
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("errors", out JsonElement topErrors))
            {
                throw new ApplicationException($"Shopify GraphQL error: {topErrors}");
            }

            JsonElement payload = root
                .GetProperty("data")
                .GetProperty(payloadName);

            if (!payload.TryGetProperty("userErrors", out JsonElement userErrors))
            {
                return;
            }

            if (userErrors.GetArrayLength() > 0)
            {
                string messages = string.Join("; ",
                    userErrors.EnumerateArray().Select(e =>
                    {
                        string field = e.TryGetProperty("field", out JsonElement fieldElement)
                            ? fieldElement.ToString()
                            : string.Empty;

                        string message = e.TryGetProperty("message", out JsonElement messageElement)
                            ? messageElement.GetString() ?? string.Empty
                            : string.Empty;

                        return string.IsNullOrWhiteSpace(field)
                            ? message
                            : $"{field}: {message}";
                    }));

                throw new ApplicationException($"Shopify {payloadName} failed: {messages}");
            }
        }

        private static Dictionary<string, object?> BuildDiscountValue(AdminDiscountCode discount)
        {
            if (discount.DiscountType == "Percentage")
            {
                // Shopify expects 10% as 0.10, not 10.
                return new Dictionary<string, object?>
                {
                    ["percentage"] = decimal.ToDouble(discount.DiscountValue / 100m)
                };
            }

            if (discount.DiscountType == "FixedAmount")
            {
                return new Dictionary<string, object?>
                {
                    ["discountAmount"] = new
                    {
                        amount = discount.DiscountValue.ToString("0.00", CultureInfo.InvariantCulture),

                        // For a part-specific code, this means $X off the eligible item.
                        appliesOnEachItem = true
                    }
                };
            }

            throw new ApplicationException("DiscountType must be Percentage or FixedAmount.");
        }

        private static Dictionary<string, object?> BuildDiscountItems(AdminDiscountCode discount)
        {
            if (discount.AppliesToType == "General")
            {
                return new Dictionary<string, object?>
                {
                    ["all"] = true
                };
            }

            if (discount.AppliesToType == "Part" || discount.AppliesToType == "Variant")
            {
                if (!discount.ShopifyVariantId.HasValue)
                {
                    throw new ApplicationException("Part-specific discount is missing ShopifyVariantId.");
                }

                return new Dictionary<string, object?>
                {
                    ["products"] = new
                    {
                        productVariantsToAdd = new[]
                        {
                    $"gid://shopify/ProductVariant/{discount.ShopifyVariantId.Value}"
                }
                    }
                };
            }

            if (discount.AppliesToType == "Product")
            {
                if (!discount.ShopifyProductId.HasValue)
                {
                    throw new ApplicationException("Product-specific discount is missing ShopifyProductId.");
                }

                return new Dictionary<string, object?>
                {
                    ["products"] = new
                    {
                        productsToAdd = new[]
                        {
                    $"gid://shopify/Product/{discount.ShopifyProductId.Value}"
                }
                    }
                };
            }

            throw new ApplicationException("AppliesToType must be General, Product, Variant, or Part.");
        }
    }
}
