using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using KromicStore.API.Contracts.Catalog;
using KromicStore.Application.Features.Storefront.Queries.GetStoreInfo;
using KromicStore.Application.Features.Storefront.Queries.ListFeaturedProducts;
using GetProductsQuery = KromicStore.Application.Features.Catalog.Queries.GetProducts.GetProductsQuery;
using GetCategoriesQuery = KromicStore.Application.Features.Catalog.Queries.GetCategories.GetCategoriesQuery;
using GetProductByIdQuery = KromicStore.Application.Features.Catalog.Queries.GetProductById.GetProductByIdQuery;
using SearchProductsQuery = KromicStore.Application.Features.Catalog.Queries.SearchProducts.SearchProductsQuery;

namespace KromicStore.API.Controllers;

/// <summary>
/// Public storefront API endpoints for customers browsing the store.
/// No authentication required. Tenant resolved from Host header.
/// </summary>
[ApiController]
[Route("api/v1/storefront")]
[AllowAnonymous]
public class StorefrontController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="StorefrontController"/> class.
    /// </summary>
    public StorefrontController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Gets public store information (name, logo, description, etc.)
    /// </summary>
    /// <returns>Store information</returns>
    /// <response code="200">Returns store information.</response>
    /// <response code="400">Invalid tenant or store not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GetStoreInfoResponse>> GetStoreInfo(CancellationToken cancellationToken = default)
    {
        var query = new GetStoreInfoQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets all product categories for browsing
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Paginated list of categories</returns>
    /// <response code="200">Returns categories.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCategoriesQuery(skip, take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets all published products with optional filtering
    /// </summary>
    /// <param name="categoryId">Optional: Filter by category ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Paginated list of products</returns>
    /// <response code="200">Returns products.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductCardDto>>> GetProducts(
        [FromQuery] Guid? categoryId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsQuery(skip, take, categoryId, Status: null);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets featured/top products
    /// </summary>
    /// <param name="take">Number of products to return (default: 12, max: 50).</param>
    /// <returns>List of featured products</returns>
    /// <response code="200">Returns featured products.</response>
    /// <response code="400">Invalid parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("featured-products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListFeaturedProductsResponse>> GetFeaturedProducts(
        [FromQuery] int take = 12,
        CancellationToken cancellationToken = default)
    {
        if (take > 50) take = 50;
        if (take < 1) take = 12;

        var query = new ListFeaturedProductsQuery(take);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets product details by ID
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>Product details with variants and images</returns>
    /// <response code="200">Returns product details.</response>
    /// <response code="404">Product not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("products/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDetailDto>> GetProductDetails(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result.Data == null)
            return NotFound();

        return Ok(result.Data);
    }

    /// <summary>
    /// Searches products by name, description, or tags
    /// </summary>
    /// <param name="query">Search term</param>
    /// <param name="categoryId">Optional: Filter by category ID.</param>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to take (default: 20, max: 100).</param>
    /// <returns>Search results</returns>
    /// <response code="200">Returns search results.</response>
    /// <response code="400">Invalid parameters or empty search query.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductSearchResultDto>>> SearchProducts(
        [FromQuery] string? query,
        [FromQuery] Guid? categoryId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Search query cannot be empty");

        var searchQuery = new SearchProductsQuery(query, skip, take, categoryId);
        var result = await _mediator.Send(searchQuery, cancellationToken);
        return Ok(result.Data);
    }

    /// <summary>
    /// Gets store policies (shipping, return, privacy, etc.)
    /// </summary>
    /// <returns>Store policies</returns>
    /// <response code="200">Returns policies.</response>
    /// <response code="404">Policies not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("policies")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<StorePoliciesDto>> GetPolicies(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ActionResult<StorePoliciesDto>>(Ok(new StorePoliciesDto(
            ShippingPolicy: GetShippingPolicy(),
            ReturnPolicy: GetReturnPolicy(),
            PrivacyPolicy: GetPrivacyPolicy(),
            TermsOfService: GetTermsOfService())));
    }

    /// <summary>
    /// Gets store "About Us" information.
    /// </summary>
    /// <returns>About page content.</returns>
    /// <response code="200">Returns about content.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("about")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<StoreAboutDto>> GetAbout(CancellationToken cancellationToken = default)
    {
        var about = new StoreAboutDto(
            Title: "About KromicStore",
            Content: GetAboutContent(),
            MissionStatement: "Our mission is to provide quality products with exceptional customer service.",
            VisionStatement: "To be the leading e-commerce platform trusted by customers worldwide.",
            FoundedYear: 2024,
            CompanyName: "KromicStore Inc.",
            Email: "info@kromicstore.com",
            Phone: "+1-800-KROMIC-1",
            Address: "123 Commerce Street, Tech City, TC 12345");

        return Task.FromResult<ActionResult<StoreAboutDto>>(Ok(about));
    }

    /// <summary>
    /// Gets store contact information and messaging.
    /// </summary>
    /// <returns>Contact information.</returns>
    /// <response code="200">Returns contact information.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("contact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<StoreContactDto>> GetContact(CancellationToken cancellationToken = default)
    {
        var contact = new StoreContactDto(
            Email: "support@kromicstore.com",
            Phone: "+1-800-SUPPORT-1",
            Address: "123 Commerce Street, Tech City, TC 12345",
            BusinessHours: "Monday - Friday, 9AM - 6PM EST",
            SocialMediaLinks: new Dictionary<string, string>
            {
                { "facebook", "https://facebook.com/kromicstore" },
                { "twitter", "https://twitter.com/kromicstore" },
                { "instagram", "https://instagram.com/kromicstore" },
                { "linkedin", "https://linkedin.com/company/kromicstore" }
            });

        return Task.FromResult<ActionResult<StoreContactDto>>(Ok(contact));
    }

    /// <summary>
    /// Gets frequently asked questions.
    /// </summary>
    /// <returns>FAQ content.</returns>
    /// <response code="200">Returns FAQ.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("faq")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<ActionResult<StoreFaqDto>> GetFaq(CancellationToken cancellationToken = default)
    {
        var faqs = new StoreFaqDto(
            Title: "Frequently Asked Questions",
            Questions: new List<FaqItemDto>
            {
                new(
                    Question: "How long does shipping take?",
                    Answer: "Standard shipping takes 5-7 business days. Express shipping takes 2-3 business days. See our Shipping Policy for details."),
                new(
                    Question: "What is your return policy?",
                    Answer: "We offer a 30-day return policy for unused items. See our Return Policy for complete details."),
                new(
                    Question: "Do you offer international shipping?",
                    Answer: "Yes, we ship to over 100 countries worldwide. Shipping costs and times vary by location."),
                new(
                    Question: "How can I track my order?",
                    Answer: "After your order ships, you'll receive a tracking email with a link to track your package."),
                new(
                    Question: "What payment methods do you accept?",
                    Answer: "We accept all major credit cards, debit cards, UPI, net banking, and digital wallets."),
                new(
                    Question: "Is my personal information secure?",
                    Answer: "Yes, we use industry-standard encryption and security protocols. See our Privacy Policy for details."),
                new(
                    Question: "How do I contact customer support?",
                    Answer: "You can reach us via email at support@kromicstore.com or call +1-800-SUPPORT-1. Business hours: Mon-Fri, 9AM-6PM EST."),
                new(
                    Question: "Do you offer gift cards?",
                    Answer: "Yes! Gift cards are available in various denominations and can be purchased from our store.")
            });

        return Task.FromResult<ActionResult<StoreFaqDto>>(Ok(faqs));
    }

    // ── Store Discovery Content Helpers ───────────────────────────────────

    private static string GetShippingPolicy()
    {
        return @"
## Shipping Policy

### Shipping Methods
- **Standard Shipping**: 5-7 business days - Free on orders over $50
- **Express Shipping**: 2-3 business days - $9.99
- **Overnight Shipping**: Next business day - $19.99

### Shipping Restrictions
- We ship to all 50 US states and over 100 countries worldwide
- Some items may have shipping restrictions
- Hazardous materials cannot be shipped via air

### Order Processing
- Orders are processed within 1 business day
- Orders are shipped Monday-Friday (excluding holidays)
- Tracking information is provided via email upon shipment

### Shipping Costs
- Shipping costs are calculated at checkout based on weight and destination
- Orders over $100 may qualify for free standard shipping
- Free shipping promotions are announced via email

### Lost or Damaged Packages
- We are not responsible for packages lost after shipment
- Please contact the carrier for lost package claims
- Damaged items should be reported within 48 hours of delivery
";
    }

    private static string GetReturnPolicy()
    {
        return @"
## Return Policy

### Return Eligibility
- Items must be returned within 30 days of purchase
- Items must be unused and in original condition
- Original packaging and all accessories must be included

### Non-Returnable Items
- Digital products and software
- Customized items
- Clearance or final sale items
- Items damaged due to customer misuse

### Return Process
1. Contact our support team at support@kromicstore.com
2. Receive a return authorization (RA) number
3. Ship item back with RA number (prepaid label provided)
4. We inspect the item upon receipt
5. Refund is processed within 5-7 business days

### Return Shipping
- Return shipping is free for defective items
- Return shipping is the customer's responsibility for other returns
- Shipping costs are non-refundable

### Refunds
- Refunds are issued to the original payment method
- Shipping costs are not refunded
- Refund processing takes 5-7 business days after receipt
";
    }

    private static string GetPrivacyPolicy()
    {
        return @"
## Privacy Policy

### Information Collection
We collect information you provide directly to us when:
- Creating an account
- Placing an order
- Contacting customer support
- Subscribing to our newsletter
- Using our website

### Types of Information
- Personal Information: Name, email, phone, address
- Payment Information: Credit card and billing details
- Usage Information: Browser type, pages visited, search queries
- Device Information: IP address, device type, operating system

### Information Usage
We use your information to:
- Process and fulfill orders
- Provide customer support
- Improve our products and services
- Send promotional emails (with your consent)
- Analyze usage patterns
- Prevent fraud and ensure security

### Information Sharing
We do not sell your personal information. We may share information with:
- Payment processors (for payment processing)
- Shipping companies (for order delivery)
- Service providers (for website hosting, analytics)
- Legal authorities (when required by law)

### Data Security
- We use SSL/TLS encryption for data transmission
- Sensitive data is encrypted at rest
- Access to personal data is restricted to authorized personnel
- We comply with GDPR, CCPA, and other privacy regulations

### Your Rights
- Access: Request a copy of your data
- Rectification: Correct inaccurate data
- Deletion: Request deletion of your data
- Opt-out: Unsubscribe from marketing emails
";
    }

    private static string GetTermsOfService()
    {
        return @"
## Terms of Service

### Agreement to Terms
By accessing and using this website, you accept and agree to be bound by the terms and provision of this agreement.

### License to Use
We grant you a limited, non-exclusive, non-transferable license to access and use our website for lawful purposes only.

### User Responsibilities
You agree to:
- Provide accurate and current information
- Maintain the confidentiality of your password
- Not use our site for illegal or unauthorized purposes
- Not violate any laws, regulations, or third-party rights
- Not engage in unauthorized access or data harvesting

### Product Information
- Product descriptions and prices are subject to change without notice
- We reserve the right to limit quantities or cancel orders
- All products are subject to availability

### Pricing and Payment
- Prices are subject to change without notice
- All prices are in USD unless otherwise stated
- Sales tax will be added where applicable
- We accept all major payment methods

### Limitation of Liability
- Our liability is limited to the amount paid for the product
- We are not liable for indirect, incidental, or consequential damages
- We do not warrant that products will meet your needs

### Disclaimer of Warranties
- Products are provided ""as is"" without warranties
- We disclaim all express and implied warranties
- We do not warrant uninterrupted or error-free service

### Modifications
- We may modify these terms at any time
- Continued use constitutes acceptance of modified terms
- Changes are effective immediately upon posting

### Governing Law
- These terms are governed by applicable laws
- Disputes are subject to exclusive jurisdiction of applicable courts
";
    }

    private static string GetAboutContent()
    {
        return @"
# About KromicStore

KromicStore is a leading e-commerce platform committed to delivering quality products with exceptional customer service. Founded in 2024, we have grown to serve thousands of satisfied customers worldwide.

## Our Story
KromicStore began with a simple mission: to make quality products accessible to everyone. Starting as a small operation, we have expanded significantly through dedication to customer satisfaction and operational excellence.

## What We Offer
- Wide selection of high-quality products
- Competitive pricing and regular promotions
- Fast and reliable shipping
- Excellent customer support
- Secure and easy checkout

## Our Commitment
- **Quality**: We carefully curate our product selection
- **Value**: We offer competitive prices and great deals
- **Service**: Our customer support team is available 24/7
- **Integrity**: We operate transparently and ethically
- **Innovation**: We continually improve our platform

## Awards and Recognition
- Best E-Commerce Platform (2024)
- Customer Choice Award (2024)
- Trusted Seller Badge

## Get in Touch
Have questions? We'd love to hear from you! Contact us at info@kromicstore.com or call +1-800-KROMIC-1.
";
    }
}

public record StorePoliciesDto(
    string? ShippingPolicy,
    string? ReturnPolicy,
    string? PrivacyPolicy,
    string? TermsOfService);

public record StoreAboutDto(
    string Title,
    string Content,
    string MissionStatement,
    string VisionStatement,
    int FoundedYear,
    string CompanyName,
    string Email,
    string Phone,
    string Address);

public record StoreContactDto(
    string Email,
    string Phone,
    string Address,
    string BusinessHours,
    Dictionary<string, string> SocialMediaLinks);

public record StoreFaqDto(
    string Title,
    List<FaqItemDto> Questions);

public record FaqItemDto(
    string Question,
    string Answer);
