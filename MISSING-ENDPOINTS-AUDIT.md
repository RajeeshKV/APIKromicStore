# Missing Backend Endpoints - Audit & Implementation Plan

**Date:** July 31, 2026  
**Status:** 4 Missing Capabilities Identified  
**Build:** ✅ 0 errors, 0 warnings  
**Priority:** HIGH (Required for scalable multi-tenancy)

---

## 📊 Audit Summary

| Capability | Endpoint | Status | Priority | Est. Time |
|-----------|----------|--------|----------|-----------|
| Bulk Operations | POST /api/v1/products/bulk-delete | ❌ MISSING | HIGH | 1 day |
| Bulk Operations | POST /api/v1/orders/bulk-update-status | ❌ MISSING | HIGH | 1 day |
| Review Moderation | POST /api/v1/reviews/{id}/approve | ❌ MISSING | MEDIUM | 4 hours |
| Review Moderation | POST /api/v1/reviews/{id}/reject | ❌ MISSING | MEDIUM | 4 hours |
| CSV Exports | GET /api/v1/orders/export | ⚠️ STUB | HIGH | 1 day |
| CSV Exports | GET /api/v1/customers/export | ⚠️ STUB | HIGH | 1 day |
| Theme Assets | POST /api/v1/themes/{id}/assets | ❌ MISSING | MEDIUM | 2 days |

**Total Time to Implement:** 4-5 days

---

## 1️⃣ Bulk Operations API

### What's Missing

**Endpoints:**
```
POST /api/v1/products/bulk-delete
POST /api/v1/orders/bulk-update-status
```

**Current Issue:** Products and orders updated one-by-one (inefficient for bulk operations)

### Why It Matters

- ✅ Performance: Batch operations reduce API calls from N to 1
- ✅ Scalability: Essential for high-volume merchant operations
- ✅ UX: Quick bulk actions (delete 100 products in 1 request)
- ✅ Database: Reduces transaction overhead

### Implementation Plan

#### Step 1: Create CQRS Commands

**File:** `src/KromicStore.Application/Features/Catalog/Commands/BulkDeleteProducts/BulkDeleteProductsCommand.cs`

```csharp
public sealed record BulkDeleteProductsCommand(
    IEnumerable<Guid> ProductIds
) : IRequest<BulkDeleteProductsResponse>;

public sealed record BulkDeleteProductsResponse(
    int DeletedCount,
    int FailedCount,
    List<BulkOperationError> Errors
);

public sealed record BulkOperationError(
    Guid Id,
    string ErrorMessage
);
```

**File:** `src/KromicStore.Application/Features/Orders/Commands/BulkUpdateOrderStatus/BulkUpdateOrderStatusCommand.cs`

```csharp
public sealed record BulkUpdateOrderStatusCommand(
    IEnumerable<Guid> OrderIds,
    string NewStatus
) : IRequest<BulkUpdateOrderStatusResponse>;

public sealed record BulkUpdateOrderStatusResponse(
    int UpdatedCount,
    int FailedCount,
    List<BulkOperationError> Errors
);
```

#### Step 2: Create Handlers

**File:** `src/KromicStore.Application/Features/Catalog/Commands/BulkDeleteProducts/BulkDeleteProductsCommandHandler.cs`

```csharp
public sealed class BulkDeleteProductsCommandHandler 
    : IRequestHandler<BulkDeleteProductsCommand, BulkDeleteProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<BulkDeleteProductsCommandHandler> _logger;

    public async Task<BulkDeleteProductsResponse> Handle(
        BulkDeleteProductsCommand request, 
        CancellationToken cancellationToken)
    {
        var errors = new List<BulkOperationError>();
        int deletedCount = 0;

        foreach (var productId in request.ProductIds)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product == null)
                {
                    errors.Add(new BulkOperationError(productId, "Product not found"));
                    continue;
                }

                // Soft delete
                product.Delete();
                _productRepository.Update(product);
                deletedCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete product {ProductId}", productId);
                errors.Add(new BulkOperationError(productId, ex.Message));
            }
        }

        // Single SaveChanges for all operations
        if (deletedCount > 0)
        {
            await _productRepository.SaveChangesAsync(cancellationToken);
        }

        return new BulkDeleteProductsResponse(
            DeletedCount: deletedCount,
            FailedCount: errors.Count,
            Errors: errors
        );
    }
}
```

#### Step 3: Create Controller Endpoint

**File:** `src/KromicStore.API/Controllers/ProductsController.cs` (Add endpoint)

```csharp
/// <summary>
/// Bulk delete products.
/// Admin operation that soft-deletes multiple products in one request.
/// </summary>
/// <param name="request">Product IDs to delete.</param>
/// <response code="200">Returns operation results with success/failure counts.</response>
/// <response code="400">Validation error.</response>
/// <response code="401">Unauthorized.</response>
/// <response code="403">Forbidden.</response>
[HttpPost("bulk-delete")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<ActionResult<BulkDeleteProductsResponse>> BulkDeleteProducts(
    [FromBody] BulkDeleteProductsRequest request,
    CancellationToken cancellationToken)
{
    if (request?.ProductIds == null || !request.ProductIds.Any())
        return BadRequest("No product IDs provided");

    var command = new BulkDeleteProductsCommand(request.ProductIds);
    var result = await _mediator.Send(command, cancellationToken);
    
    return Ok(result);
}
```

**Request DTO:**
```csharp
public record BulkDeleteProductsRequest(
    IEnumerable<Guid> ProductIds
);
```

#### Step 4: Similarly for Orders

Follow the same pattern for `BulkUpdateOrderStatusCommand` with:
- Validation of new status (Draft, Confirmed, Shipped, Delivered, Cancelled)
- Authorization check (TenantAdmin only)
- Event publishing for status changes

### API Request/Response

```bash
# Request
POST /api/v1/products/bulk-delete
{
  "productIds": [
    "550e8400-e29b-41d4-a716-446655440000",
    "550e8400-e29b-41d4-a716-446655440001"
  ]
}

# Response (200 OK)
{
  "deletedCount": 2,
  "failedCount": 0,
  "errors": []
}
```

---

## 2️⃣ Review Moderation API

### What's Missing

**Endpoints:**
```
POST /api/v1/reviews/{id}/approve
POST /api/v1/reviews/{id}/reject
```

**Current Issue:** ReviewsController fetches approved reviews but has no admin moderation endpoints

### Why It Matters

- ✅ Content Moderation: Filter spam/inappropriate reviews before showing
- ✅ Admin Control: Approve/reject reviews manually
- ✅ Trust: Ensure quality reviews for customers
- ✅ Compliance: Remove harmful content

### Implementation Plan

#### Step 1: Update ProductReview Entity

**File:** `src/KromicStore.Domain/Catalog/Entities/ProductReview.cs` (Add methods)

```csharp
public void Approve()
{
    Status = ReviewStatus.Approved;
    ApprovedOnUtc = DateTime.UtcNow;
}

public void Reject(string reason)
{
    Status = ReviewStatus.Rejected;
    RejectionReason = reason;
    RejectedOnUtc = DateTime.UtcNow;
}
```

#### Step 2: Create Commands

**File:** `src/KromicStore.Application/Features/Catalog/Commands/ApproveReview/ApproveReviewCommand.cs`

```csharp
public sealed record ApproveReviewCommand(
    Guid ReviewId
) : IRequest<ReviewDto>;
```

**File:** `src/KromicStore.Application/Features/Catalog/Commands/RejectReview/RejectReviewCommand.cs`

```csharp
public sealed record RejectReviewCommand(
    Guid ReviewId,
    string RejectionReason
) : IRequest<ReviewDto>;
```

#### Step 3: Create Handlers

```csharp
public sealed class ApproveReviewCommandHandler 
    : IRequestHandler<ApproveReviewCommand, ReviewDto>
{
    private readonly IProductReviewRepository _reviewRepository;
    private readonly ILogger<ApproveReviewCommandHandler> _logger;

    public async Task<ReviewDto> Handle(ApproveReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            throw new NotFoundException("Review not found");

        review.Approve();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Review {ReviewId} approved", request.ReviewId);
        return MapToDto(review);
    }
}
```

#### Step 4: Add Controller Endpoints

**File:** `src/KromicStore.API/Controllers/ReviewsController.cs` (Add methods)

```csharp
/// <summary>
/// Approves a review for display (admin only).
/// </summary>
[HttpPost("{reviewId:guid}/approve")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
[ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<ProductReviewDto>> ApproveReview(
    Guid productId,
    Guid reviewId,
    CancellationToken cancellationToken)
{
    var command = new ApproveReviewCommand(reviewId);
    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}

/// <summary>
/// Rejects a review (admin only).
/// </summary>
[HttpPost("{reviewId:guid}/reject")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
[ProducesResponseType(typeof(ProductReviewDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<ActionResult<ProductReviewDto>> RejectReview(
    Guid productId,
    Guid reviewId,
    [FromBody] RejectReviewRequest request,
    CancellationToken cancellationToken)
{
    var command = new RejectReviewCommand(reviewId, request.RejectionReason);
    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}
```

### API Request/Response

```bash
# Approve Review
POST /api/v1/products/{productId}/reviews/{reviewId}/approve
# Response: 200 OK with updated review

# Reject Review
POST /api/v1/products/{productId}/reviews/{reviewId}/reject
Content-Type: application/json
{
  "rejectionReason": "Inappropriate language"
}
# Response: 200 OK with updated review
```

---

## 3️⃣ CSV Export Endpoints

### What's Missing

**Endpoints:**
```
GET /api/v1/orders/export?startDate=2024-01-01&endDate=2024-12-31
GET /api/v1/customers/export?skip=0&take=1000
```

**Current Issue:** AnalyticsController has stub export that returns empty CSV with no real data

### Why It Matters

- ✅ Accounting: Export orders for accounting software (Xero, QuickBooks)
- ✅ CRM Integration: Export customers for email marketing (Mailchimp, HubSpot)
- ✅ Data Analysis: Download data for BI tools
- ✅ Reporting: Create custom reports offline

### Implementation Plan

#### Step 1: Create CSV Service

**File:** `src/KromicStore.Infrastructure/Services/Reporting/CsvExportService.cs`

```csharp
public interface ICsvExportService
{
    byte[] ExportOrders(IEnumerable<OrderExportDto> orders);
    byte[] ExportCustomers(IEnumerable<CustomerExportDto> customers);
}

public sealed class CsvExportService : ICsvExportService
{
    public byte[] ExportOrders(IEnumerable<OrderExportDto> orders)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Order ID,Order Number,Customer Email,Total,Status,Created Date");
        
        foreach (var order in orders)
        {
            csv.AppendLine(
                $"\"{order.Id}\",\"{order.OrderNumber}\",\"{order.CustomerEmail}\",{order.Total},\"{order.Status}\",{order.CreatedDate:yyyy-MM-dd}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    public byte[] ExportCustomers(IEnumerable<CustomerExportDto> customers)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Customer ID,Email,Name,Phone,Total Orders,Total Spent");
        
        foreach (var customer in customers)
        {
            csv.AppendLine(
                $"\"{customer.Id}\",\"{customer.Email}\",\"{customer.Name}\",\"{customer.Phone}\",{customer.TotalOrders},{customer.TotalSpent}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
```

#### Step 2: Create Queries

**File:** `src/KromicStore.Application/Features/Orders/Queries/ExportOrders/ExportOrdersQuery.cs`

```csharp
public sealed record ExportOrdersQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<IEnumerable<OrderExportDto>>;
```

#### Step 3: Create Handler

```csharp
public sealed class ExportOrdersQueryHandler 
    : IRequestHandler<ExportOrdersQuery, IEnumerable<OrderExportDto>>
{
    private readonly IOrderRepository _orderRepository;

    public async Task<IEnumerable<OrderExportDto>> Handle(
        ExportOrdersQuery request, 
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByDateRangeAsync(
            request.StartDate, 
            request.EndDate, 
            cancellationToken);

        return orders.Select(o => new OrderExportDto(
            Id: o.Id,
            OrderNumber: o.OrderNumber,
            CustomerEmail: o.CustomerEmail,
            Total: o.Total,
            Status: o.Status.ToString(),
            CreatedDate: o.CreatedOnUtc
        ));
    }
}
```

#### Step 4: Create Controller Endpoints

**File:** `src/KromicStore.API/Controllers/OrdersController.cs` (Add endpoint)

```csharp
/// <summary>
/// Exports orders to CSV file.
/// Useful for accounting, CRM integration, and data analysis.
/// </summary>
/// <param name="startDate">Export start date (format: yyyy-MM-dd)</param>
/// <param name="endDate">Export end date (format: yyyy-MM-dd)</param>
/// <response code="200">Returns CSV file download.</response>
/// <response code="400">Validation error.</response>
/// <response code="401">Unauthorized.</response>
/// <response code="403">Forbidden.</response>
[HttpGet("export")]
[Authorize(Roles = "TenantAdmin,StoreManager")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<IActionResult> ExportOrders(
    [FromQuery] DateTime startDate,
    [FromQuery] DateTime endDate,
    CancellationToken cancellationToken)
{
    if (endDate < startDate)
        return BadRequest("End date must be after start date");

    var query = new ExportOrdersQuery(startDate, endDate);
    var orders = await _mediator.Send(query, cancellationToken);
    
    var csvBytes = _csvExportService.ExportOrders(orders);
    
    return File(
        csvBytes, 
        "text/csv", 
        $"Orders_Export_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv");
}
```

### API Request/Response

```bash
# Export Orders
GET /api/v1/orders/export?startDate=2024-01-01&endDate=2024-12-31

# Response: CSV file download
# Content-Type: text/csv
# Content-Disposition: attachment; filename=Orders_Export_20240101_20241231.csv
```

**CSV Content:**
```
Order ID,Order Number,Customer Email,Total,Status,Created Date
"550e8400-e29b-41d4-a716-446655440000","ORD-001","john@example.com",150.00,"Delivered","2024-01-15"
"550e8400-e29b-41d4-a716-446655440001","ORD-002","jane@example.com",200.00,"Shipped","2024-01-16"
```

---

## 4️⃣ Theme Image Asset Storage API

### What's Missing

**Endpoint:**
```
POST /api/v1/themes/{id}/assets
```

**Current Issue:** Themes store color palettes + raw CSS strings, no file upload for logos/banners

### Why It Matters

- ✅ Complete Theme Builder: Upload store logos, hero banners
- ✅ Brand Customization: Merchants can upload custom assets
- ✅ Media Management: Integration with Cloudinary for storage
- ✅ CDN Delivery: Assets served from CDN for performance

### Implementation Plan

#### Step 1: Update Theme Entity

**File:** `src/KromicStore.Domain/CMS/Entities/Theme.cs` (Add properties)

```csharp
public class Theme : BaseEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    // ... existing properties
    
    // New properties
    public List<ThemeAsset> Assets { get; private set; } = [];
    
    public void AddAsset(ThemeAsset asset)
    {
        Assets.Add(asset);
    }
    
    public void RemoveAsset(Guid assetId)
    {
        Assets.RemoveAll(a => a.Id == assetId);
    }
}

public class ThemeAsset : BaseEntity
{
    public Guid ThemeId { get; private set; }
    public string AssetType { get; private set; } // "logo", "hero_banner", "favicon"
    public string CloudinaryUrl { get; private set; } = string.Empty;
    public string CloudinaryPublicId { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    
    public static ThemeAsset Create(
        Guid themeId,
        string assetType,
        string cloudinaryUrl,
        string publicId,
        long fileSizeBytes)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            ThemeId = themeId,
            AssetType = assetType,
            CloudinaryUrl = cloudinaryUrl,
            CloudinaryPublicId = publicId,
            FileSizeBytes = fileSizeBytes
        };
    }
}
```

#### Step 2: Create Upload Command

**File:** `src/KromicStore.Application/Features/Themes/Commands/UploadThemeAsset/UploadThemeAssetCommand.cs`

```csharp
public sealed class UploadThemeAssetCommand : IRequest<ThemeAssetDto>
{
    public Guid ThemeId { get; set; }
    public string AssetType { get; set; } // "logo", "hero_banner", "favicon"
    public IFormFile File { get; set; }
}

public sealed record ThemeAssetDto(
    Guid AssetId,
    string AssetType,
    string CloudinaryUrl,
    long FileSizeBytes,
    DateTime UploadedOn
);
```

#### Step 3: Create Handler

**File:** `src/KromicStore.Application/Features/Themes/Commands/UploadThemeAsset/UploadThemeAssetCommandHandler.cs`

```csharp
public sealed class UploadThemeAssetCommandHandler 
    : IRequestHandler<UploadThemeAssetCommand, ThemeAssetDto>
{
    private readonly IThemeRepository _themeRepository;
    private readonly IMediaService _mediaService;
    private readonly ILogger<UploadThemeAssetCommandHandler> _logger;

    public async Task<ThemeAssetDto> Handle(
        UploadThemeAssetCommand request, 
        CancellationToken cancellationToken)
    {
        // Validate theme exists
        var theme = await _themeRepository.GetByIdAsync(request.ThemeId, cancellationToken);
        if (theme == null)
            throw new NotFoundException("Theme not found");

        // Validate file
        if (request.File == null || request.File.Length == 0)
            throw new InvalidOperationException("No file provided");

        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (request.File.Length > maxFileSize)
            throw new InvalidOperationException("File size exceeds 5MB limit");

        // Upload to Cloudinary
        using var stream = request.File.OpenReadStream();
        var uploadResult = await _mediaService.UploadAsync(
            stream,
            $"themes/{theme.Id}/{request.AssetType}",
            cancellationToken);

        // Create theme asset
        var asset = ThemeAsset.Create(
            themeId: theme.Id,
            assetType: request.AssetType,
            cloudinaryUrl: uploadResult.Url,
            publicId: uploadResult.PublicId,
            fileSizeBytes: request.File.Length);

        theme.AddAsset(asset);
        _themeRepository.Update(theme);
        await _themeRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Theme asset uploaded. ThemeId: {ThemeId}, AssetType: {AssetType}",
            theme.Id, request.AssetType);

        return new ThemeAssetDto(
            AssetId: asset.Id,
            AssetType: asset.AssetType,
            CloudinaryUrl: asset.CloudinaryUrl,
            FileSizeBytes: asset.FileSizeBytes,
            UploadedOn: asset.CreatedOnUtc);
    }
}
```

#### Step 4: Create Controller Endpoint

**File:** `src/KromicStore.API/Controllers/ThemeBuilderController.cs` (Add endpoint)

```csharp
/// <summary>
/// Uploads an asset (logo, hero banner, favicon) to a theme.
/// File is stored on Cloudinary and URL is saved to theme.
/// </summary>
/// <param name="themeId">Theme ID.</param>
/// <param name="assetType">Asset type (logo, hero_banner, favicon).</param>
/// <param name="file">Image file to upload (max 5MB).</param>
/// <response code="200">Asset uploaded successfully.</response>
/// <response code="400">Validation error or invalid file.</response>
/// <response code="401">Unauthorized.</response>
/// <response code="404">Theme not found.</response>
[HttpPost("{themeId}/assets")]
[Authorize(Roles = "TenantAdmin")]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(ThemeAssetDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<ThemeAssetDto>> UploadThemeAsset(
    Guid themeId,
    [FromQuery] string assetType,
    IFormFile file,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(assetType))
        return BadRequest("Asset type is required (logo, hero_banner, favicon)");

    if (!new[] { "logo", "hero_banner", "favicon" }.Contains(assetType))
        return BadRequest("Invalid asset type");

    var command = new UploadThemeAssetCommand
    {
        ThemeId = themeId,
        AssetType = assetType,
        File = file
    };

    var result = await _mediator.Send(command, cancellationToken);
    return Ok(result);
}

/// <summary>
/// Gets all assets for a theme.
/// </summary>
[HttpGet("{themeId}/assets")]
[Authorize(Roles = "TenantAdmin")]
[ProducesResponseType(typeof(IEnumerable<ThemeAssetDto>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<IEnumerable<ThemeAssetDto>>> GetThemeAssets(
    Guid themeId,
    CancellationToken cancellationToken)
{
    var query = new GetThemeAssetsQuery(themeId);
    var assets = await _mediator.Send(query, cancellationToken);
    return Ok(assets);
}

/// <summary>
/// Deletes an asset from a theme.
/// </summary>
[HttpDelete("{themeId}/assets/{assetId}")]
[Authorize(Roles = "TenantAdmin")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> DeleteThemeAsset(
    Guid themeId,
    Guid assetId,
    CancellationToken cancellationToken)
{
    var command = new DeleteThemeAssetCommand(themeId, assetId);
    await _mediator.Send(command, cancellationToken);
    return NoContent();
}
```

### API Request/Response

```bash
# Upload Theme Asset
POST /api/v1/themes/{themeId}/assets?assetType=logo
Content-Type: multipart/form-data

[binary file data]

# Response (200 OK)
{
  "assetId": "550e8400-e29b-41d4-a716-446655440000",
  "assetType": "logo",
  "cloudinaryUrl": "https://res.cloudinary.com/...",
  "fileSizeBytes": 245600,
  "uploadedOn": "2024-07-31T10:30:00Z"
}
```

---

## 📋 Implementation Checklist

### Phase 1: Bulk Operations (1 day)
- [ ] Create BulkDeleteProducts command/handler/validator
- [ ] Create BulkUpdateOrderStatus command/handler/validator
- [ ] Add controller endpoints
- [ ] Add integration tests
- [ ] Update Swagger docs

### Phase 2: Review Moderation (1 day)
- [ ] Update ProductReview entity with Approve/Reject methods
- [ ] Create ApproveReview command/handler
- [ ] Create RejectReview command/handler
- [ ] Add controller endpoints
- [ ] Add integration tests
- [ ] Update Swagger docs

### Phase 3: CSV Exports (1 day)
- [ ] Create CsvExportService
- [ ] Create ExportOrders query/handler
- [ ] Create ExportCustomers query/handler
- [ ] Add controller endpoints
- [ ] Add integration tests
- [ ] Test with Excel/Google Sheets

### Phase 4: Theme Assets (2 days)
- [ ] Create ThemeAsset entity and DbSet
- [ ] Add database migration
- [ ] Create UploadThemeAsset command/handler
- [ ] Create GetThemeAssets query/handler
- [ ] Create DeleteThemeAsset command/handler
- [ ] Add controller endpoints
- [ ] Add Cloudinary integration validation
- [ ] Add integration tests

---

## ✅ Testing Strategy

### Unit Tests
- Command validators (empty lists, invalid IDs, etc.)
- Entity methods (Approve, Reject, AddAsset, etc.)
- Service methods (CSV generation, file validation)

### Integration Tests
```csharp
[Fact]
public async Task BulkDeleteProducts_WithValidIds_DeletesSuccessfully()
{
    // Arrange
    var productIds = new[] { product1.Id, product2.Id };
    
    // Act
    var result = await mediator.Send(new BulkDeleteProductsCommand(productIds));
    
    // Assert
    Assert.Equal(2, result.DeletedCount);
    Assert.Empty(result.Errors);
}
```

### Manual Testing
- Test each endpoint with Postman
- Verify CSV exports open in Excel
- Test file upload with different sizes
- Test authorization on admin endpoints

---

## 🚀 Deployment

**Build Command:**
```bash
dotnet build
```

**Migration Command:**
```bash
dotnet ef database update
```

**Deployment:**
```bash
# Deploy to Render (auto on git push)
git add .
git commit -m "Add bulk operations, review moderation, CSV exports, theme assets"
git push
```

---

## 📊 Summary

| Endpoint | Type | Status | Owner | Deadline |
|----------|------|--------|-------|----------|
| POST /api/v1/products/bulk-delete | CQRS | TODO | Backend | Day 1 |
| POST /api/v1/orders/bulk-update-status | CQRS | TODO | Backend | Day 1 |
| POST /api/v1/reviews/{id}/approve | CQRS | TODO | Backend | Day 1 |
| POST /api/v1/reviews/{id}/reject | CQRS | TODO | Backend | Day 1 |
| GET /api/v1/orders/export | Query | STUB→REAL | Backend | Day 2 |
| GET /api/v1/customers/export | Query | STUB→REAL | Backend | Day 2 |
| POST /api/v1/themes/{id}/assets | Command | TODO | Backend | Day 3-4 |

**Total Implementation Time:** 4-5 days  
**Priority:** HIGH  
**Complexity:** MEDIUM

