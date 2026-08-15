# WEB-ADMIN DETAILED AUDIT

## Overview

**Status:** 31% coverage  
**Endpoints Wired:** 25/80  
**Screens Implemented:** 12/40  
**Critical Missing:** 6 screens with 44+ endpoints

---

## Dashboard & Overview

### ✅ Dashboard Overview

**Status:** Implemented  
**Endpoints Used:**
- GET /tenant/dashboard/overview
- GET /tenant/dashboard/analytics
- GET /tenant/dashboard/top-products
- GET /tenant/dashboard/orders (recent)

**Current Features:**
- Total Orders
- Total Revenue
- Active Customers
- Low Stock Products
- Pending Orders
- Today's Sales

**What's Working:** Core metrics display ✅

**What's Missing:**
- ⚠️ Charts (sales trend, orders trend)
- ⚠️ Recent activity timeline
- ⚠️ Top customers by revenue
- ⚠️ Conversion funnel
- ⚠️ Device/traffic breakdown

**Recommended Additions:**
```typescript
// Enhanced dashboard should have:
1. Revenue Chart (30-day trend)
2. Orders Chart (daily breakdown)
3. Top Products by Sales (table)
4. Top Customers by LTV (table)
5. Recent Orders (activity feed)
6. Traffic Sources (pie chart)
7. Customer Acquisition (line chart)
```

---

## Products Management

### ✅ Products Screen

**Status:** Implemented  
**Endpoints Used:**
- GET /products (list)
- POST /products (create)
- PUT /products/{id} (edit)
- DELETE /products/{id} (delete)
- GET /products/{id} (detail)

**Current Features:**
- List all products
- Create product
- Edit product
- Delete product
- Pagination
- Basic filtering

**What's Missing:**
- ❌ Bulk actions (bulk edit, bulk delete, bulk publish)
- ⚠️ Quick edit (inline editing)
- ⚠️ Variant management (no dedicated UI)
- ❌ Image upload/gallery
- ❌ SEO settings edit
- ❌ Advanced filtering (by date, status, etc.)
- ❌ Export products
- ❌ Import products
- ❌ Product templates/duplication (endpoint exists but UI missing)

**Recommended Additions:**
```typescript
// Products page enhancements:
- Bulk select (checkbox column)
- Actions menu (bulk edit, bulk delete, bulk publish)
- Quick edit modal (edit name, price, stock inline)
- Variant tab (manage variants)
- Images tab (upload/reorder/delete)
- SEO tab (title, description, keywords)
- Duplicate product (action button)
- Status filter (Draft, Published, Archived)
- Sort options (name, price, date, sales)
```

---

## Categories Management

### ✅ Categories Screen

**Status:** Implemented  
**Endpoints Used:**
- GET /categories (list)
- POST /categories (create)
- PUT /categories/{id} (edit)
- DELETE /categories/{id} (delete)

**Current Features:**
- List categories
- Create category
- Edit category
- Delete category
- Basic pagination

**What's Missing:**
- ❌ Nested categories (parent/child hierarchy)
- ❌ Category image/icon upload
- ❌ Drag-to-reorder
- ❌ SEO settings
- ❌ Bulk actions

---

## Collections Management

### ⚠️ Collections (Listed but not shown)

**Status:** Sidebar link exists but NO DEDICATED SCREEN  
**Endpoints Available:** 3 endpoints

| Endpoint | Status |
|----------|--------|
| GET /collections | ✅ Exists |
| POST /collections | ⚠️ No UI |
| DELETE /collections/{id} | ⚠️ No UI |

**Issue:** Collections screen not built  
**Action Required:** Create CollectionsPage

```typescript
// Collections screen should have:
1. List all collections
2. Create collection (name, description, featured image)
3. Add/remove products from collection
4. Edit collection
5. Delete collection
6. Publish/draft status
7. Set featured collection
```

---

## Inventory Management

### ⚠️ Inventory Screen (Incomplete)

**Status:** Shows low stock only  
**Endpoints Available:**
- GET /tenant/dashboard/low-stock (used)
- GET /inventory (partial)
- POST /inventory/adjust (missing UI)

**Current Features:**
- Shows low stock alerts
- Product name, SKU, current stock

**What's Missing:**
- ❌ Full inventory list (not just low stock)
- ❌ Stock adjustment form
- ❌ Stock history/audit trail
- ❌ Reorder level configuration
- ❌ Bulk stock import/export
- ❌ Stock alerts configuration
- ❌ Warehouse management (if multi-warehouse)

**Recommended Additions:**
```typescript
// Full Inventory Management:
1. Inventory List
   - Product, SKU, current qty, reorder level
   - Status (In Stock, Low Stock, Out of Stock)
   - Actions: Adjust Stock, View History

2. Adjust Stock Modal
   - Current quantity display
   - New quantity input
   - Reason (Sale, Restock, Correction, etc.)
   - Notes field

3. Stock History
   - Previous adjustments
   - Date, reason, quantity change, user

4. Low Stock Alerts
   - Set reorder level per product
   - Notification preferences
```

---

## Orders Management

### ✅ Orders Screen (Mostly complete)

**Status:** Implemented  
**Endpoints Used:**
- GET /tenant/dashboard/orders (list)
- GET /orders/{id} (detail)
- POST /orders/{id}/confirm (labeled but not wired)

**Current Features:**
- List orders with pagination
- View order details
- Order status display
- Basic filtering

**What's Missing:**
- ⚠️ Order confirmation (endpoint exists but not used)
- ❌ Order status update (PUT /orders/{id}/status)
- ❌ Order fulfillment workflow (mark as shipped, tracking)
- ❌ Refund/credit functionality
- ❌ Order notes/timeline
- ❌ Customer communication history
- ❌ Print/export order
- ❌ Bulk actions (bulk ship, bulk cancel)
- ❌ Advanced filters (by date, customer, status, amount)

**Recommended Additions:**
```typescript
// Enhanced Orders Page:
1. Order List
   - Status badges (Pending, Processing, Shipped, Delivered, Cancelled)
   - Customer name, order total
   - Order date, payment status
   - Quick actions: View, Mark Shipped, Refund

2. Order Detail Sidebar
   - Customer info
   - Items ordered (table)
   - Shipping address
   - Billing address
   - Payment method
   - Order notes (add notes ability)
   - Timeline (placed, processed, shipped, delivered)
   - Tracking number input (mark shipped)
   - Refund form (if needed)

3. Bulk Actions
   - Select orders
   - Mark as shipped
   - Print labels
   - Cancel orders
```

---

## Customers Management

### ⚠️ Customers Screen (Skeleton only)

**Status:** Screen exists but incomplete  
**Endpoints Available:** 7 endpoints

| Endpoint | Status |
|----------|--------|
| GET /customers | ✅ List view |
| GET /customers/{id} | ⚠️ Detail view |
| GET /customers/{id}/orders | ⚠️ Shown in detail |
| GET /customers/{id}/addresses | ❌ Not shown separately |
| POST /customers/{id}/addresses | ❌ No UI |
| PUT /customers/{id}/addresses/{id} | ❌ No UI |
| DELETE /customers/{id}/addresses/{id} | ❌ No UI |

**Current Features:**
- Customer list with search
- View customer details
- Show customer order history

**What's Missing:**
- ❌ Customer segment/tags
- ❌ Lifetime value display
- ❌ Address book (add, edit, delete)
- ❌ Customer preferences/notes
- ❌ Email communication history
- ❌ RFM analysis (Recency, Frequency, Monetary)
- ❌ Bulk messaging
- ❌ Import/export customers

---

## ❌ SHIPPING MANAGEMENT (Completely Missing)

**Status:** 11 ENDPOINTS - ZERO UI

| Endpoint | Purpose | Status |
|----------|---------|--------|
| POST /shipping/zones | Create zone | ❌ Missing |
| GET /shipping/zones | List zones | ❌ Missing |
| GET /shipping/zones/{id} | Zone detail | ❌ Missing |
| PUT /shipping/zones/{id} | Update zone | ❌ Missing |
| DELETE /shipping/zones/{id} | Delete zone | ❌ Missing |
| POST /shipping/zones/{id}/methods | Add method | ❌ Missing |
| POST /shipping/calculate | Calculate cost | ❌ Missing |
| POST /taxes/regions | Create tax zone | ❌ Missing |
| GET /taxes/regions | List tax zones | ❌ Missing |
| PUT /taxes/regions/{id} | Update tax zone | ❌ Missing |
| DELETE /taxes/regions/{id} | Delete tax zone | ❌ Missing |

**Impact:** Merchants cannot configure shipping or taxes at all

**Required Screen:**
```typescript
// Shipping & Taxes Settings Screen:

TAB 1: Shipping Zones
├── List zones (name, countries, active status)
├── Create zone
│   ├── Zone name
│   ├── Select countries/regions
│   ├── Add shipping methods
│   │   ├── Method name (Standard, Express, etc.)
│   │   ├── Price/rate
│   │   ├── Delivery time
│   │   └── Conditions (min order, weight range)
│   └── Save
├── Edit/Delete zone

TAB 2: Tax Settings
├── List tax regions
├── Create region
│   ├── Country/region
│   ├── Tax rate (%)
│   ├── Tax name (VAT, Sales Tax, etc.)
│   └── Apply to (products, shipping, both)
├── Edit/Delete region
```

**Time to Build:** 2-3 days

---

## ❌ PROMOTIONS/DISCOUNTS (Empty State Only)

**Status:** 12 ENDPOINTS - EMPTY UI

| Endpoint | Purpose | Status |
|----------|---------|--------|
| GET /promotions | List | ❌ No screen |
| POST /promotions | Create | ❌ No screen |
| PUT /promotions/{id} | Update | ❌ No screen |
| DELETE /promotions/{id} | Delete | ❌ No screen |
| GET /coupons | List | ❌ No screen |
| POST /coupons | Create | ❌ No screen |
| PUT /coupons/{id} | Update | ❌ No screen |
| DELETE /coupons/{id} | Delete | ❌ No screen |
| GET /discounts | List | ⚠️ Empty state |
| POST /discounts | Create | ⚠️ No form |
| PUT /discounts/{id} | Update | ❌ Missing |
| DELETE /discounts/{id} | Delete | ❌ Missing |

**Impact:** Merchants cannot create sales/promotions/discounts

**Current State:**
- "Discounts" sidebar link shows empty state
- No create form
- No list table

**Required Screen:**
```typescript
// Promotions Tab:

TAB 1: Discounts
├── List (name, type, discount amount, valid dates, status)
├── Create discount
│   ├── Name
│   ├── Type (percentage, fixed amount, buy X get Y)
│   ├── Discount value
│   ├── Min order amount
│   ├── Valid from/to dates
│   ├── Active toggle
│   └── Save
├── Edit/Delete

TAB 2: Coupons
├── List (code, discount, usage, expiry)
├── Generate coupon
│   ├── Coupon code
│   ├── Discount (link to discount or manual)
│   ├── Max uses
│   ├── Max uses per customer
│   ├── Expiry date
│   └── Save
├── Edit/Delete
├── View usage stats

TAB 3: Campaigns
├── List (name, type, recipients, status, sent date)
├── Create campaign
│   ├── Campaign name
│   ├── Type (email, discount code, etc.)
│   ├── Target audience (all, segment, etc.)
│   ├── Offer details
│   ├── Schedule (send now, schedule)
│   └── Send
├── View results (opens, clicks, conversions)
```

**Time to Build:** 3-4 days

---

## ❌ MARKETING/CAMPAIGNS (Skeleton Only)

**Status:** 10 ENDPOINTS - NO UI

| Endpoint | Purpose | Status |
|----------|---------|--------|
| GET /marketing/campaigns | List | ❌ No screen |
| POST /marketing/campaigns | Create | ❌ No screen |
| GET /marketing/campaigns/{id} | Detail | ❌ No screen |
| PUT /marketing/campaigns/{id} | Update | ❌ No screen |
| DELETE /marketing/campaigns/{id} | Delete | ❌ No screen |
| POST /marketing/campaigns/{id}/send | Send | ❌ No screen |
| All other endpoints (5) | Various | ❌ No screen |

**Impact:** Merchants cannot run marketing campaigns or email marketing

**Required Screen:**
```typescript
// Email Marketing Screen:

1. Campaign List
   - Campaign name, type, recipients count, status
   - Sent date, open rate, click rate
   - Quick actions: Edit, View Stats, Resend

2. Create Campaign
   - Campaign name
   - Email subject
   - Email body (rich editor)
   - Template selection
   - Recipient selection (all, segment, list)
   - Scheduling (send now, schedule for later)
   - A/B testing (optional)

3. Campaign Stats
   - Sent count
   - Open rate
   - Click rate
   - Conversion rate
   - Bounce rate
   - Device breakdown
```

**Time to Build:** 2-3 days

---

## ❌ REVIEWS MANAGEMENT (Not in Admin)

**Status:** 6 ENDPOINTS - NO ADMIN SCREEN

| Endpoint | Purpose | Status |
|----------|---------|--------|
| GET /products/{id}/reviews | List (for moderation) | ❌ No screen |
| GET /products/{id}/reviews/stats | Stats | ⚠️ Not shown |
| PUT /products/{id}/reviews/{id} | Approve/reject | ❌ No screen |
| DELETE /products/{id}/reviews/{id} | Delete spam | ❌ No screen |
| PATCH /products/{id}/reviews/{id}/status | Change status | ❌ No screen |
| POST /products/{id}/reviews/{id}/response | Respond | ❌ No screen |

**Impact:** Merchants cannot moderate or respond to reviews

**Required Screen:**
```typescript
// Reviews Moderation Screen:

1. Pending Reviews Tab
   - List unmoderated reviews
   - Product name, rating, reviewer, date
   - Review text preview
   - Actions: Approve, Reject, Delete, Respond

2. Approved Reviews Tab
   - All published reviews
   - Star rating, reviewer, date
   - Helpful count
   - Merchant response (if any)
   - Actions: Edit, Delete, Respond

3. Review Detail/Edit
   - Full review text
   - Reviewer info
   - Product info
   - Status selector (Pending, Approved, Rejected)
   - Merchant response field
   - Save

4. Response to Review
   - Modal to write merchant response
   - Preview
   - Publish
```

**Time to Build:** 1-2 days

---

## ⚠️ ANALYTICS (Partial Implementation)

**Status:** 7 endpoints - only overview used

| Endpoint | Status |
|----------|--------|
| GET /analytics/overview | ✅ Used |
| GET /analytics/sales | ❌ Not shown |
| GET /analytics/orders | ❌ Not shown |
| GET /analytics/customers | ❌ Not shown |
| GET /analytics/products | ✅ Top products used |
| GET /analytics/export | ❌ Missing |
| GET /analytics/trends | ⚠️ Listed not used |

**Current Features:**
- Overview metrics
- Top products table

**What's Missing:**
- ❌ Sales trend chart
- ❌ Order trend chart
- ❌ Customer acquisition chart
- ❌ Revenue by product chart
- ❌ Revenue by category chart
- ❌ Geographic analysis
- ❌ Custom date range (dynamic)
- ❌ Export to CSV/PDF
- ❌ Comparison (this month vs last month)

---

## ⚠️ THEME BUILDER (Limited)

**Status:** 7 endpoints - basic UI

| Endpoint | Status |
|----------|--------|
| GET /theme/current | ✅ |
| POST /theme/create | ✅ |
| PUT /theme/{id} | ✅ |
| GET /theme/{id}/preview | ✅ |
| DELETE /theme/{id} | ✅ |
| POST /theme/{id}/publish | ⚠️ No confirm |
| POST /theme/{id}/duplicate | ❌ Missing |

**Current Features:**
- Canvas editor
- Publish theme

**What's Missing:**
- ❌ Duplicate theme
- ❌ Theme marketplace/templates
- ❌ Proper preview (desktop/tablet/mobile)
- ❌ Component tree/layers
- ❌ Undo/redo history
- ❌ Collaboration (multiple editors)
- ❌ Version history
- ⚠️ Live preview (may be hard)

---

## ⚠️ CMS/PAGES (Partial)

**Status:** 6 endpoints - basic CRUD

| Endpoint | Status |
|----------|--------|
| GET /cms/pages | ✅ List |
| POST /cms/pages | ✅ Create |
| GET /cms/pages/{id} | ✅ Edit |
| PUT /cms/pages/{id} | ✅ Update |
| DELETE /cms/pages/{id} | ✅ Delete |
| POST /cms/pages/{id}/publish | ❌ Missing publish |

**Current Features:**
- List pages
- Create/edit page
- Delete page

**What's Missing:**
- ❌ Publish/draft status management
- ❌ Page preview
- ❌ Rich content editor
- ❌ SEO settings
- ❌ Page hierarchy/parent pages
- ❌ Scheduled publishing
- ❌ Revision history

---

## ⚠️ SETTINGS (Partial)

**Status:** Some implemented, needs expansion

| Area | Endpoints | Status |
|------|-----------|--------|
| Store Settings | GET, PUT | ✅ |
| Payment Settings | GET, PUT | ⚠️ Not shown |
| General | N/A | ✅ |
| Branding | N/A | ⚠️ Logo/colors |
| SEO | N/A | ❌ Missing |
| Shipping | 11 endpoints | ❌ Missing |
| Taxes | 4 endpoints | ❌ Missing |
| Users/Roles | N/A | ❌ Missing |
| Notifications | N/A | ❌ Missing |

**Required Settings Sections:**
```typescript
SettingsPage Tabs:
1. General ✅
   - Store name, email, phone

2. Branding ⚠️
   - Logo upload
   - Brand colors
   - Font selection
   - Favicon

3. SEO
   - Store title
   - Meta description
   - Keywords
   - Site map config

4. Shipping ❌
   - [Link to Shipping Screen]

5. Taxes ❌
   - [Link to Taxes Screen]

6. Payment ⚠️
   - Payment gateway config

7. Users/Roles ❌
   - Admin users list
   - Roles management
   - Permissions

8. Notifications ❌
   - Email notifications config
   - Order notifications
   - Customer notifications
   - Alert preferences
```

---

## Admin Summary Table

| Category | Endpoints | Wired | Missing | % |
|----------|-----------|-------|---------|---|
| **Dashboard** | 4 | 4 | 0 | 100% |
| **Products** | 5 | 5 | 0 | 100% |
| **Categories** | 4 | 4 | 0 | 100% |
| **Collections** | 3 | 0 | 3 | 0% |
| **Orders** | 6 | 4 | 2 | 67% |
| **Customers** | 7 | 3 | 4 | 43% |
| **Inventory** | 2 | 1 | 1 | 50% |
| **Reviews** | 6 | 0 | 6 | 0% |
| **Shipping** | 11 | 0 | 11 | 0% |
| **Promotions** | 12 | 1 | 11 | 8% |
| **Marketing** | 10 | 0 | 10 | 0% |
| **Analytics** | 7 | 3 | 4 | 43% |
| **Theme Builder** | 7 | 6 | 1 | 86% |
| **CMS/Pages** | 6 | 5 | 1 | 83% |
| **Settings** | Various | 2 | 10+ | 15% |
| **TOTAL** | ~80 | ~25 | ~55 | **31%** |

---

## Critical Action Items for Web-Admin

### TIER 1 - BLOCKING (Must do immediately):

1. ❌ **Shipping Management Screen** (11 endpoints)
   - Estimate: 2-3 days
   - Impact: HIGH (required for storefront)

2. ❌ **Promotions/Discounts Screen** (12 endpoints)
   - Estimate: 3-4 days
   - Impact: HIGH (revenue feature)

3. ❌ **Marketing Campaigns Screen** (10 endpoints)
   - Estimate: 2-3 days
   - Impact: HIGH (retention feature)

4. ❌ **Review Moderation Screen** (6 endpoints)
   - Estimate: 1-2 days
   - Impact: HIGH (trust/engagement)

### TIER 2 - HIGH (Need for MVP):

1. ⚠️ Complete **Inventory Management** (2 endpoints, 1 existing)
   - Estimate: 1-2 days

2. ⚠️ Complete **Customer Management** (address book, etc.)
   - Estimate: 1-2 days

3. ⚠️ **Collections Screen** (3 endpoints)
   - Estimate: 1 day

4. ⚠️ **Enhanced Analytics** (3 missing endpoints)
   - Estimate: 2-3 days

### TIER 3 - MEDIUM (Should have):

1. ⚠️ Complete **Settings** (payment, users, roles, notifications)
   - Estimate: 2-3 days

2. ⚠️ **Theme Templates/Marketplace**
   - Estimate: 1-2 days

3. ⚠️ **CMS Enhancements** (publish status, preview)
   - Estimate: 1 day

4. ⚠️ **Order Fulfillment Workflow** (tracking, refunds)
   - Estimate: 2-3 days

---

## Admin Development Timeline

If building in priority order:

```
Week 1:
  - Shipping Management (2-3 days)
  - Promotions/Discounts (3-4 days) → SPILLOVER

Week 2:
  - Promotions completion (1 day)
  - Marketing Campaigns (2-3 days)
  - Review Moderation (1-2 days)

Week 3:
  - Collections (1 day)
  - Complete Inventory (1-2 days)
  - Complete Customers (1-2 days)
  - Analytics (2-3 days) → SPILLOVER

Week 4:
  - Analytics completion (1 day)
  - Settings completion (2-3 days)

Total: 4 weeks for complete admin coverage
```

---

