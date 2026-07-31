# Phase 8: Customer Portal & Store Operations - Requirements & Implementation Strategy

**Phase 8 Scope**: Customer Portal + Store Operations  
**Target Test Count**: 200+ comprehensive tests  
**Estimated Duration**: Based on Phase 6-7 pattern  
**Status**: Ready to begin

---

## Part 1: Customer Portal

### 1.1 Customer Dashboard

**Requirements**:
- Display customer greeting with name/email
- Show recent orders (last 5-10)
- Quick order statistics (total orders, total spent, points/rewards if applicable)
- Direct links to profile, addresses, wishlist, orders
- Activity timeline (last 30 days)
- Promotional banner area (upcoming campaigns)

**Domain Entities**:
- No new entity needed - dashboard queries existing data (Order, Coupon, Customer)

**Queries**:
```
GetCustomerDashboardQuery
  → GetRecentOrders (Orders + OrderItems)
  → GetOrderStatistics (Order count, total spent)
  → GetActivityTimeline (OrderTimeline entries)
  → GetEligiblePromotions (Valid coupons/discounts)
```

**Database**:
- No schema changes - query Orders, OrderTimelines, Coupons, Discounts

---

### 1.2 Customer Profile Management

**Requirements**:
- View profile: name, email, phone, date of birth
- Edit profile: name, phone (email/DOB change not allowed)
- View account preferences (newsletter opt-in, notification settings)
- View account security (password last changed, login history)
- Download personal data (GDPR compliance)
- Delete account option

**Domain Entities**:
```
CustomerProfile (new)
  ├── CustomerId
  ├── FirstName
  ├── LastName
  ├── PhoneNumber
  ├── DateOfBirth
  ├── NewsletterOptIn
  ├── NotificationPreferences (JSON)
  └── Auditing fields
```

**Commands**:
```
UpdateCustomerProfileCommand (name, phone, preferences)
UpdateNotificationPreferencesCommand
RequestPersonalDataDownloadCommand
DeleteCustomerAccountCommand (soft delete)
```

**Queries**:
```
GetCustomerProfileQuery
GetNotificationPreferencesQuery
GetAccountSecurityInfoQuery
GetLoginHistoryQuery
```

**Tests**: 25-30 tests
- Profile update validations
- Permission checks
- Audit logging
- GDPR data deletion

---

### 1.3 Address Book Management

**Requirements**:
- View all saved addresses
- Add new address (name, street, city, state, postal code, country, phone)
- Edit address
- Delete address
- Set default shipping address
- Set default billing address
- Address validation (postal code format per country)

**Domain Entities**:
```
CustomerAddress (new - or extend existing)
  ├── CustomerId
  ├── AddressType (Shipping/Billing/Both)
  ├── Street
  ├── City
  ├── StateCode
  ├── PostalCode
  ├── CountryCode
  ├── PhoneNumber
  ├── Label (e.g., "Home", "Office")
  ├── IsDefault
  ├── IsActive
  └── Auditing
```

**Commands**:
```
AddCustomerAddressCommand
UpdateCustomerAddressCommand
DeleteCustomerAddressCommand
SetDefaultShippingAddressCommand
SetDefaultBillingAddressCommand
ValidateAddressCommand
```

**Queries**:
```
GetCustomerAddressesQuery
GetAddressByIdQuery
GetDefaultShippingAddressQuery
GetDefaultBillingAddressQuery
```

**Tests**: 30-35 tests
- Address CRUD operations
- Default address validation
- Postal code format validation per country
- Permission checks (only own addresses)
- Multi-address scenarios

---

### 1.4 Wishlist Management

**Note**: Wishlist entity already exists from Phase 5. This is enhancement/query API.

**Requirements**:
- View all wishlist items (product name, price, availability)
- Add to wishlist (from product page)
- Remove from wishlist
- Move wishlist item to cart
- Share wishlist (generate public link)
- Get wishlist statistics (total items, total value)

**Commands**:
```
AddToWishlistCommand (already exists - verify implementation)
RemoveFromWishlistCommand (already exists - verify)
MoveWishlistItemToCartCommand (new)
ShareWishlistCommand (generates token)
```

**Queries**:
```
GetCustomerWishlistQuery
GetWishlistItemsQuery
GetPublicWishlistQuery (by share token)
GetWishlistStatisticsQuery
```

**Tests**: 20-25 tests
- Wishlist CRUD operations
- Move to cart
- Share link generation
- Public access validation

---

### 1.5 Order History & Tracking

**Requirements**:
- List all customer orders (paginated, sortable, filterable)
- View order details: items, totals, addresses, shipping, payment info
- View order status timeline
- Track shipment (carrier, tracking number, estimated delivery)
- Download invoice (PDF generation)
- Re-order: create cart from previous order
- Leave feedback/rating on order
- Contact support for order

**Queries**:
```
GetCustomerOrdersQuery (paginated, filtered)
GetOrderDetailsQuery
GetOrderTimelineQuery
GetShipmentTrackingQuery
GetOrderInvoiceQuery
```

**Commands**:
```
ReorderCommand (copy previous order to new cart)
LeaveOrderFeedbackCommand
CreateSupportTicketCommand (if support system exists)
DownloadInvoiceCommand
```

**Tests**: 35-40 tests
- Order filtering/pagination
- Access control (only own orders)
- Timeline verification
- Re-order functionality
- Feedback validation

---

### 1.6 Notifications & Preferences

**Requirements**:
- View notification history
- Notification types: order updates, shipment tracking, new products, promotions
- Set notification preferences (email, SMS, push, in-app)
- Unsubscribe from notifications
- Notification frequency settings (real-time, daily digest, weekly)

**Domain Entities**:
```
CustomerNotificationPreference (new)
  ├── CustomerId
  ├── NotificationType (OrderUpdate, Shipment, NewProduct, Promotion)
  ├── EmailEnabled
  ├── SMSEnabled
  ├── PushEnabled
  ├── InAppEnabled
  ├── Frequency (RealTime, Daily, Weekly)
  └── Auditing

CustomerNotificationLog (new)
  ├── CustomerId
  ├── NotificationType
  ├── Channel (Email, SMS, Push, InApp)
  ├── Message
  ├── SentOnUtc
  ├── ReadOnUtc
  └── Status
```

**Commands**:
```
UpdateNotificationPreferenceCommand
MarkNotificationAsReadCommand
UnsubscribeCommand
```

**Queries**:
```
GetNotificationHistoryQuery
GetNotificationPreferencesQuery
GetUnreadNotificationCountQuery
```

**Tests**: 20-25 tests
- Preference update validation
- Notification filtering
- Permission checks

---

## Part 2: Store Operations

### 2.1 Inventory Dashboard

**Requirements**:
- Real-time inventory status (on-hand, reserved, available)
- Low stock alerts (items below reorder point)
- Inventory by warehouse/location (if multi-location)
- Search/filter inventory
- Stock movement history
- FIFO/LIFO tracking for expirable items

**Note**: Inventory entity already exists. This is enhancements/reporting.

**Queries**:
```
GetInventoryDashboardQuery
GetLowStockItemsQuery
GetInventoryMovementHistoryQuery
GetInventoryByLocationQuery
```

**Tests**: 15-20 tests
- Inventory calculations
- Low stock threshold
- Movement history

---

### 2.2 Inventory Adjustments

**Requirements**:
- Manual stock adjustments (add/remove/correct)
- Reason tracking (damage, theft, recount, etc.)
- Adjustment approval workflow (for sensitive adjustments)
- Audit trail (who, when, why)
- Bulk adjustments (CSV upload)

**Domain Entities**:
```
InventoryAdjustment (new)
  ├── ProductVariantId
  ├── Quantity (positive or negative)
  ├── Reason (Damage, Theft, Recount, Return, etc.)
  ├── Description
  ├── Status (Pending, Approved, Rejected)
  ├── RequestedBy
  ├── ApprovedBy
  └── Auditing
```

**Commands**:
```
CreateInventoryAdjustmentCommand
ApproveInventoryAdjustmentCommand
RejectInventoryAdjustmentCommand
BulkAdjustInventoryCommand (CSV)
```

**Tests**: 25-30 tests
- Adjustment validation
- Approval workflow
- Audit tracking
- Bulk operations

---

### 2.3 Fulfillment Workflow

**Requirements**:
- Pending orders queue (awaiting fulfillment)
- Pick/pack/ship workflow
- Generate shipping labels
- Update tracking numbers
- Batch fulfillment (pack multiple orders)
- Fulfillment notes/comments
- Fulfillment date tracking

**Domain Entities**:
```
Fulfillment (new)
  ├── OrderId
  ├── Status (Pending, Picking, Packed, Shipped, Delivered, Failed)
  ├── FulfilledBy
  ├── PickedOnUtc
  ├── PackedOnUtc
  ├── ShippedOnUtc
  ├── TrackingNumber
  ├── CourierName
  ├── Notes
  └── Auditing

FulfillmentItem (new)
  ├── FulfillmentId
  ├── OrderItemId
  ├── ProductVariantId
  ├── QuantityPicked
  ├── QuantityPacked
  ├── QuantityShipped
```

**Commands**:
```
CreateFulfillmentCommand
PickOrderItemCommand
PackOrderCommand
ShipOrderCommand (generate label)
UpdateTrackingCommand
CancelFulfillmentCommand
```

**Queries**:
```
GetPendingFulfillmentsQuery
GetFulfillmentDetailsQuery
GetFulfillmentHistoryQuery
```

**Tests**: 30-35 tests
- Fulfillment workflow state transitions
- Inventory deduction
- Tracking integration
- Batch operations

---

### 2.4 Returns Management

**Requirements**:
- Request returns (within return window)
- Return authorization tracking
- Return shipping label generation
- Receive returned items (inspection)
- Quality assessment (refund/restock decision)
- Refund processing
- Return statistics

**Domain Entities**:
```
ReturnRequest (new)
  ├── OrderId
  ├── OrderItemId
  ├── Quantity
  ├── Reason (DefectiveProduct, NotAsDescribed, ChangedMind, Damaged)
  ├── Status (Requested, Authorized, ShippedBack, Received, Inspected, Approved, Rejected, Refunded)
  ├── ReturnAuthorizationNumber
  ├── ReturnShippingLabel
  ├── RequestedOnUtc
  ├── AuthorizedOnUtc
  ├── ReceivedOnUtc
  ├── Notes
  └── Auditing

ReturnInspection (new)
  ├── ReturnRequestId
  ├── InspectedBy
  ├── Quality (Acceptable, Damaged, Missing, Other)
  ├── RestockDecision (Restock, Scrap, Donate)
  ├── Comments
  └── InspectedOnUtc
```

**Commands**:
```
RequestReturnCommand
AuthorizeReturnCommand (generate RMA)
GenerateReturnLabelCommand
ReceiveReturnCommand
InspectReturnCommand
ApproveReturnCommand (approve refund)
RejectReturnCommand
ProcessRefundCommand
```

**Queries**:
```
GetReturnRequestsQuery
GetReturnDetailsQuery
GetReturnStatisticsQuery
GetRefundHistoryQuery
```

**Tests**: 35-40 tests
- Return workflow state transitions
- Return window validation
- Inventory restock logic
- Refund processing
- Return statistics

---

### 2.5 Refund Tracking

**Requirements**:
- Refund status tracking (pending, processed, completed)
- Refund method (original payment, store credit, gift card)
- Partial refunds
- Refund timeline
- Failed refund retry

**Note**: Payment/Refund entities exist from Phase 6. This is enhancement/tracking.

**Queries**:
```
GetRefundHistoryQuery
GetRefundDetailsQuery
GetRefundStatisticsQuery
GetPendingRefundsQuery
```

**Commands**:
```
ProcessRefundCommand (if not in Phase 6)
RetryFailedRefundCommand
CancelRefundCommand
```

**Tests**: 15-20 tests
- Refund status tracking
- Partial refund calculations
- Multiple refund methods

---

### 2.6 Dashboard & Analytics

**Requirements**:
- Sales dashboard (daily/weekly/monthly revenue)
- Order metrics (total orders, average order value, conversion rate)
- Inventory metrics (inventory turnover, low stock items)
- Customer metrics (new customers, repeat purchase rate)
- Fulfillment metrics (fulfillment time, shipping time)
- Returns & refunds analytics
- Revenue by product/category

**Queries**:
```
GetSalesDashboardQuery (date range)
GetOrderMetricsQuery
GetInventoryMetricsQuery
GetCustomerMetricsQuery
GetFulfillmentMetricsQuery
GetReturnsAnalyticsQuery
```

**Tests**: 20-25 tests
- Metric calculations
- Date range filtering
- Data aggregation

---

### 2.7 SEO Settings

**Requirements**:
- Meta titles, descriptions for products/categories
- URL slug management
- XML sitemap generation
- Robot.txt configuration
- Canonical URLs
- Schema.org markup configuration

**Domain Entities**:
```
SEOConfiguration (new)
  ├── EntityType (Product, Category, Page)
  ├── EntityId
  ├── MetaTitle
  ├── MetaDescription
  ├── Slug
  ├── CanonicalUrl
  ├── SchemaMarkup (JSON)
  └── Auditing
```

**Commands**:
```
UpdateSEOSettingsCommand
GenerateMetaDataCommand (auto-generate from title/description)
```

**Queries**:
```
GetSEOSettingsQuery
GenerateSitemapQuery
```

**Tests**: 15-20 tests
- SEO validation
- Slug uniqueness
- Sitemap generation

---

### 2.8 Email Templates

**Requirements**:
- Template management (order, shipment, refund, etc.)
- Template personalization (variables: {{customer_name}}, {{order_number}})
- Email preview
- Send test email
- Template versioning
- A/B testing support

**Domain Entities**:
```
EmailTemplate (new)
  ├── Name
  ├── EmailType (OrderConfirmation, Shipment, Refund, etc.)
  ├── Subject
  ├── HtmlBody
  ├── TextBody
  ├── AvailableVariables (JSON)
  ├── Status (Active, Inactive, Draft)
  ├── Version
  └── Auditing
```

**Commands**:
```
CreateEmailTemplateCommand
UpdateEmailTemplateCommand
PublishEmailTemplateCommand
SendTestEmailCommand
```

**Queries**:
```
GetEmailTemplatesQuery
GetEmailTemplateDetailsQuery
```

**Tests**: 20-25 tests
- Template validation
- Variable substitution
- Test email sending

---

### 2.9 Notification Templates

**Requirements**:
- SMS template management
- Push notification templates
- In-app notification templates
- Template personalization
- Send test notification

**Similar to Email Templates above**

**Tests**: 15-20 tests

---

### 2.10 Audit Logs

**Requirements**:
- Track all admin actions (create, update, delete)
- Admin activity dashboard
- Search audit logs
- Export audit logs (compliance)
- Audit log retention policy
- Sensitive action alerts (bulk deletes, price changes, discounts)

**Domain Entities** (may already exist):
```
AuditLog (existing? - verify)
  ├── EntityType
  ├── EntityId
  ├── Action (Create, Update, Delete)
  ├── ActorId
  ├── OldValue (JSON)
  ├── NewValue (JSON)
  ├── Timestamp
```

**Queries**:
```
GetAuditLogsQuery (filterable, paginated)
GetUserActivityQuery
GetEntityChangeHistoryQuery
```

**Tests**: 20-25 tests
- Audit log capture
- Activity filtering
- Change tracking

---

## Implementation Strategy

### Phase 8 Task Breakdown

1. **Customer Portal** (100-120 tests)
   - Profile management (25-30)
   - Address management (30-35)
   - Wishlist enhancements (20-25)
   - Order history (35-40)
   - Notifications (20-25)

2. **Store Operations** (80-100 tests)
   - Inventory (25-30)
   - Fulfillment (30-35)
   - Returns (35-40)
   - Refunds (15-20)
   - Analytics (20-25)
   - Settings (20-25) - SEO, Email, Notifications, Audit

3. **Total**: 180-220 tests (target 200+)

### Architecture Pattern to Follow

**Domain Layer**:
```
Phase8/
├── CustomerPortal/
│   ├── Entities/ (CustomerProfile, CustomerAddress, etc.)
│   └── ValueObjects/
└── StoreOperations/
    ├── Entities/ (Fulfillment, ReturnRequest, etc.)
    └── ValueObjects/
```

**Application Layer**:
```
Features/
├── CustomerPortal/
│   ├── Commands/ (UpdateProfile, AddAddress, etc.)
│   ├── Queries/ (GetDashboard, GetAddresses, etc.)
│   ├── Validators/
│   └── DTOs/
└── StoreOperations/
    ├── Commands/ (CreateFulfillment, etc.)
    ├── Queries/ (GetPendingFulfillments, etc.)
    ├── Validators/
    └── DTOs/
```

**Repositories**:
```
ICustomerProfileRepository
ICustomerAddressRepository
IFulfillmentRepository
IReturnRequestRepository
IAuditLogRepository
IEmailTemplateRepository
```

### Standards to Maintain

✅ Use TenantEntity for all multi-tenant entities  
✅ Use ITenantContext for tenant resolution  
✅ Global query filters in DbContext for tenant isolation  
✅ FluentValidation for all commands  
✅ MediatR for command/query handlers  
✅ Repository pattern for data access  
✅ Soft delete (IsDeleted, DeletedOnUtc, DeletedBy)  
✅ Auditing (CreatedOnUtc, ModifiedOnUtc, CreatedBy, ModifiedBy)  
✅ Comprehensive domain tests (20-30% of total tests)  
✅ Validator tests for all commands  

### Success Criteria

- ✅ 200+ tests total (domain + application)
- ✅ 100% test pass rate
- ✅ 0 build errors
- ✅ Architecture fully compliant
- ✅ Multi-tenant isolation verified
- ✅ Production-ready code quality

---

## Handoff from Phase 7

- Database migration: `20260731030804_Phase7_Shipping_Taxes_Promotions.cs`
- All Phase 7 repositories and commands registered
- MediatR configured with validation pipeline
- Global query filters protect tenant data
- Use ITenantContext.TenantId for tenant resolution
- Follow existing command/query patterns

---

**Ready to begin Phase 8 implementation**

