# KromicStore Frontend Integration Audit Report

**Date:** July 31, 2026  
**Status:** COMPREHENSIVE AUDIT  
**Total Backend Endpoints:** 147 across 31 controllers

---

## EXECUTIVE SUMMARY

Your frontend integration has **GOOD foundational coverage** but is **MISSING significant functionality**:

- ✅ Core authentication & storefront: Complete
- ✅ Product & order workflows: Implemented
- ⚠️ **44+ critical endpoints NOT wired to UI**
- ⚠️ **Multiple missing screens and features**
- ⚠️ **Admin functionality severely under-implemented**

---

## CRITICAL FINDINGS

### 1. Missing Admin Screens (Web-Admin)

**Current:** 12 screens implemented  
**Backend Supports:** 40+ admin features  
**Gap:** 28+ screens/features missing

#### Missing High-Priority Admin Screens:

| Feature | Endpoints | UI Status | Priority |
|---------|-----------|-----------|----------|
| **Shipping Management** | 11 endpoints | ❌ MISSING | CRITICAL |
| **Promotions/Discounts** | 12 endpoints | ❌ MISSING | CRITICAL |
| **Customer Management** | 7 endpoints | ⚠️ Incomplete | HIGH |
| **Reviews & Ratings** | 6 endpoints | ❌ MISSING | HIGH |
| **Marketing Campaigns** | 10 endpoints | ⚠️ Partial | HIGH |
| **CMS Pages** | 6 endpoints | ⚠️ Partial | MEDIUM |
| **Webhooks** | 1 endpoint | ❌ MISSING | MEDIUM |
| **Theme Builder** | 7 endpoints | ⚠️ Limited | HIGH |

### 2. Missing Storefront Features (Web-Storefront)

**Current:** 4 screens  
**Backend Supports:** 15+ customer features  
**Gap:** 11+ features missing

#### Missing Storefront Screens:

| Feature | Endpoints | UI Status | Priority |
|---------|-----------|-----------|----------|
| **Reviews & Ratings** | 6 endpoints | ❌ MISSING | HIGH |
| **Wishlist Management** | 4 endpoints | ⚠️ Incomplete | HIGH |
| **Customer Account** | 7 endpoints | ⚠️ Incomplete | HIGH |
| **Order Management** | 6 endpoints | ⚠️ Incomplete | HIGH |
| **Address Book** | Part of customer | ❌ MISSING | MEDIUM |
| **Return Requests** | Not exposed yet | ❌ MISSING | MEDIUM |

### 3. Incomplete Platform Admin (Web-Super)

**Current:** 9 screens  
**Backend Supports:** 25+ platform features  
**Gap:** 16+ features missing

#### Missing Platform Admin Screens:

| Feature | Endpoints | UI Status | Priority |
|---------|-----------|-----------|----------|
| **Platform Settings** | 0 endpoints (empty controller) | ❌ MISSING | CRITICAL |
| **Audit Logs** | 1 endpoint | ⚠️ Listed but not filtering | HIGH |
| **Feature Flags** | 4 endpoints | ❌ MISSING | MEDIUM |
| **Health Monitoring** | 0 endpoints documented | ⚠️ Incomplete | HIGH |
| **Support Tickets** | ❌ No backend | ⚠️ UI only | N/A |

### 4. Public Website Issues (Web-Public)

**Current:** 4 screens (static)  
**Backend Supports:** Dynamic data endpoints  
**Gap:** NOT consuming backend data

#### Issues:

- ❌ **Pricing page is STATIC** - Backend has SubscriptionPlan API
- ❌ **FAQ is MISSING** - Not implemented as API endpoint
- ⚠️ **Contact form works** but no contact list/admin screen
- ⚠️ **Newsletter signup** - Not wired

---

## DETAILED ENDPOINT AUDIT BY CONTROLLER

### **Auth Controller** (10 endpoints) ✅ COMPLETE

**Status:** All wired correctly

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /register | LoginPage | ✅ |
| POST /login | LoginPage | ✅ |
| POST /refresh | Auto refresh | ✅ |
| POST /logout | NavBar | ✅ |
| GET /verify-email | Email link | ✅ |
| POST /resend-verification | Auth flow | ✅ |
| POST /forgot-password | Password reset | ✅ |
| POST /reset-password | Password reset | ✅ |
| POST /change-password | Settings | ✅ |
| GET /me | CurrentUser | ✅ |

---

### **Products Controller** (5 endpoints) ✅ MOSTLY COMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /products | ProductsPage | ✅ |
| POST /products | ProductsPage (Add) | ✅ |
| GET /products/{id} | ProductDetailsPage | ✅ |
| PUT /products/{id} | ProductsPage (Edit) | ✅ |
| DELETE /products/{id} | ProductsPage | ✅ |

**Note:** Missing: RestoreProduct, DuplicateProduct endpoints

---

### **Cart Controller** (8 endpoints) ⚠️ PARTIALLY WIRED

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /cart | CheckoutPage | ✅ |
| GET /cart | CheckoutPage | ✅ |
| POST /cart/items | CheckoutPage | ✅ |
| DELETE /cart/items/{itemId} | CheckoutPage | ✅ |
| PUT /cart/items/{itemId} | CheckoutPage | ✅ |
| POST /cart/clear | CheckoutPage | ✅ |
| PUT /cart/apply-coupon | CheckoutPage | ⚠️ Not implemented |
| PUT /cart/remove-coupon | CheckoutPage | ⚠️ Not implemented |

---

### **Storefront Controller** (10 endpoints) ✅ MOSTLY WIRED

| Endpoint | Frontend | Status |
|----------|-----------|--------|
| GET /storefront/info | HomePage | ✅ |
| GET /storefront/categories | Browse | ✅ |
| GET /storefront/products | Browse | ✅ |
| GET /storefront/featured-products | HomePage | ✅ |
| GET /storefront/products/{id} | ProductDetailsPage | ✅ |
| GET /storefront/search | SearchPage | ✅ |
| GET /storefront/policies | Footer | ⚠️ Static |
| GET /storefront/about | AboutPage | ⚠️ Static |
| GET /storefront/contact-info | ContactPage | ✅ |
| GET /storefront/newsletter | Footer | ⚠️ Not wired |

---

### **Checkout Controller** (6 endpoints) ✅ IMPLEMENTED

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /checkout/sessions | CheckoutPage | ✅ |
| GET /checkout/sessions/{id} | CheckoutPage | ✅ |
| PUT /checkout/sessions/{id}/shipping-address | CheckoutPage | ✅ |
| PUT /checkout/sessions/{id}/billing-address | CheckoutPage | ✅ |
| POST /checkout/sessions/{id}/apply-coupon | CheckoutPage | ⚠️ Not implemented |
| POST /checkout/sessions/{id}/place-order | CheckoutPage | ✅ |

---

### **Orders Controller** (6 endpoints) ⚠️ PARTIAL

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /orders | OrdersPage | ✅ |
| GET /orders/{id} | OrderDetailsPage | ✅ |
| PUT /orders/{id}/status | OrdersPage (Admin) | ⚠️ Not exposed |
| POST /orders/{id}/confirm | OrdersPage | ⚠️ Labeled but not wired |
| POST /orders/{id}/cancel | OrdersPage | ❌ Missing |
| GET /orders/{id}/tracking | OrderDetailsPage | ❌ Missing |

---

### **Categories Controller** (4 endpoints) ✅ COMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /categories | CategoriesPage | ✅ |
| POST /categories | CategoriesPage | ✅ |
| PUT /categories/{id} | CategoriesPage | ✅ |
| DELETE /categories/{id} | CategoriesPage | ✅ |

---

### **Collections Controller** (3 endpoints) ⚠️ LIMITED

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /collections | CollectionsPage | ⚠️ Listed but not shown |
| POST /collections | CollectionsPage | ⚠️ Listed but not shown |
| DELETE /collections/{id} | CollectionsPage | ⚠️ Listed but not shown |

**Issue:** Collections sidebar link exists but no dedicated screen

---

### **Wishlist Controller** (4 endpoints) ⚠️ INCOMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /wishlist | WishlistPage (Admin only) | ⚠️ Admin only |
| POST /wishlist | ProductDetailsPage | ✅ |
| DELETE /wishlist/{id} | ProductDetailsPage | ✅ |
| GET /wishlist/count | Nav badge | ✅ |

**Issue:** Customer wishlist management missing from storefront

---

### **Reviews Controller** (6 endpoints) ❌ MISSING

**Status:** COMPLETELY MISSING FROM FRONTEND

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /reviews | ProductDetailsPage | ❌ Missing |
| POST /reviews | ProductDetailsPage | ❌ Missing |
| PUT /reviews/{id} | ProductDetailsPage | ❌ Missing |
| DELETE /reviews/{id} | ProductDetailsPage | ❌ Missing |
| GET /products/{id}/reviews | ProductDetailsPage | ❌ Missing |
| PATCH /reviews/{id}/helpful | ProductDetailsPage | ❌ Missing |

**Impact:** Product ratings/reviews not available to customers or admins

---

### **Promotions Controller** (12 endpoints) ❌ MISSING

**Status:** COMPLETELY MISSING FROM FRONTEND

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /promotions | ❌ NO SCREEN | ❌ Missing |
| POST /promotions | ❌ NO SCREEN | ❌ Missing |
| PUT /promotions/{id} | ❌ NO SCREEN | ❌ Missing |
| DELETE /promotions/{id} | ❌ NO SCREEN | ❌ Missing |
| GET /coupons | ❌ NO SCREEN | ❌ Missing |
| POST /coupons | ❌ NO SCREEN | ❌ Missing |
| PUT /coupons/{id} | ❌ NO SCREEN | ❌ Missing |
| DELETE /coupons/{id} | ❌ NO SCREEN | ❌ Missing |
| GET /discounts | AdminPage lists | ⚠️ Empty state |
| POST /discounts | AdminPage lists | ⚠️ No create form |
| PUT /discounts/{id} | ❌ Missing | ❌ Missing |
| DELETE /discounts/{id} | ❌ Missing | ❌ Missing |

**Impact:** Cannot create or manage promotions, coupons, discounts

---

### **Shipping Management Controller** (11 endpoints) ❌ MISSING

**Status:** COMPLETELY MISSING FROM FRONTEND

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /shipping/zones | ❌ NO SCREEN | ❌ Missing |
| POST /shipping/zones | ❌ NO SCREEN | ❌ Missing |
| PUT /shipping/zones/{id} | ❌ NO SCREEN | ❌ Missing |
| DELETE /shipping/zones/{id} | ❌ NO SCREEN | ❌ Missing |
| All Tax endpoints (7) | ❌ NO SCREEN | ❌ Missing |

**Impact:** Cannot configure shipping zones or tax regions

---

### **CMS Pages Controller** (6 endpoints) ⚠️ PARTIAL

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /cms/pages | CMSPage | ✅ |
| POST /cms/pages | CMSPage | ✅ |
| GET /cms/pages/{id} | CMSPage (Edit) | ✅ |
| PUT /cms/pages/{id} | CMSPage (Edit) | ✅ |
| DELETE /cms/pages/{id} | CMSPage | ✅ |
| PUBLISH /cms/pages/{id} | CMSPage | ⚠️ Missing publish action |

---

### **Analytics Controller** (7 endpoints) ⚠️ PARTIAL

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /analytics/overview | AdminAnalyticsPage | ✅ |
| GET /analytics/sales | AdminAnalyticsPage | ⚠️ Not shown |
| GET /analytics/orders | AdminAnalyticsPage | ⚠️ Not shown |
| GET /analytics/customers | AdminAnalyticsPage | ⚠️ Not shown |
| GET /analytics/products | AdminAnalyticsPage | ✅ (top products) |
| GET /analytics/export | ❌ Missing | ❌ Missing |
| GET /analytics/trends | ✅ Listed but not used | ⚠️ Data not visualized |

---

### **Theme Builder Controller** (7 endpoints) ⚠️ LIMITED

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /theme/current | ThemeBuilderPage | ✅ |
| POST /theme/create | ThemeBuilderPage | ✅ |
| PUT /theme/{id} | ThemeBuilderPage | ✅ |
| GET /theme/{id}/preview | ThemeBuilderPage | ✅ |
| DELETE /theme/{id} | ThemeBuilderPage | ✅ |
| POST /theme/{id}/publish | ThemeBuilderPage | ⚠️ No confirm |
| POST /theme/{id}/duplicate | ThemeBuilderPage | ❌ Missing |

---

### **Customer Management Controller** (7 endpoints) ⚠️ INCOMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /customers | CustomersPage | ✅ |
| GET /customers/{id} | CustomerDetailsPage | ✅ |
| GET /customers/{id}/orders | CustomerDetailsPage | ✅ |
| GET /customers/{id}/addresses | ⚠️ Not shown separately | ⚠️ Partial |
| POST /customers/{id}/addresses | ❌ Missing | ❌ Missing |
| PUT /customers/{id}/addresses/{addressId} | ❌ Missing | ❌ Missing |
| DELETE /customers/{id}/addresses/{addressId} | ❌ Missing | ❌ Missing |

---

### **Marketing Controller** (10 endpoints) ⚠️ SKELETON ONLY

**Status:** Endpoints defined but mostly not implemented in handler

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /marketing/campaigns | ❌ NO SCREEN | ❌ Missing |
| POST /marketing/campaigns | ❌ NO SCREEN | ❌ Missing |
| GET /marketing/campaigns/{id} | ❌ NO SCREEN | ❌ Missing |
| PUT /marketing/campaigns/{id} | ❌ NO SCREEN | ❌ Missing |
| POST /marketing/campaigns/{id}/send | ❌ NO SCREEN | ❌ Missing |
| All other marketing endpoints (5) | ❌ NO SCREEN | ❌ Missing |

---

### **Inventory Controller** (2 endpoints) ⚠️ INCOMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /inventory | InventoryPage | ✅ (low stock) |
| POST /inventory/adjust | InventoryPage | ⚠️ Not shown as form |

**Issue:** Only shows low stock, doesn't show full inventory management

---

### **Variants Controller** (2 endpoints) ⚠️ MISSING UI

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /variants | ProductsPage (assumed) | ⚠️ Not explicit |
| PUT /variants/{id} | ProductsPage (assumed) | ⚠️ Not explicit |

**Issue:** No dedicated variant management screen

---

### **Contact Request Controller** (2 endpoints) ⚠️ PARTIAL

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /contact-requests | ContactPage | ✅ |
| GET /contact-requests | ❌ NO ADMIN SCREEN | ❌ Missing |

**Issue:** No admin dashboard to view/respond to contact requests

---

### **Audit Log Controller** (1 endpoint) ⚠️ INCOMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /audit-logs | AuditLogsPage | ⚠️ Listed but no filtering/sorting |

---

### **Feature Flag Controller** (4 endpoints) ❌ MISSING

**Status:** COMPLETELY MISSING FROM FRONTEND

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /feature-flags | ❌ NO SCREEN | ❌ Missing |
| POST /feature-flags | ❌ NO SCREEN | ❌ Missing |
| PUT /feature-flags/{id} | ❌ NO SCREEN | ❌ Missing |
| DELETE /feature-flags/{id} | ❌ NO SCREEN | ❌ Missing |

**Impact:** Platform admin cannot manage feature flags

---

### **Health Controller** (0 endpoints) ⚠️ EMPTY

**Status:** Controller exists but no endpoints

**Issue:** SystemHealthPage shows static data, not real health checks

---

### **Platform Settings Controller** (0 endpoints) ❌ EMPTY

**Status:** Controller exists but no implemented endpoints

**Issue:** PlatformSettingsPage shows static form, cannot save settings

---

### **Search Controller** (1 endpoint) ✅ COMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /search | Search results | ✅ |

---

### **Payment Webhook Controller** (2 endpoints) ⚠️ BACKEND ONLY

**Status:** These are webhooks (not UI)

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /webhook/payment-success | ✅ Handled backend | ✅ |
| POST /webhook/payment-failed | ✅ Handled backend | ✅ |

---

### **Setup Controller** (2 endpoints) ⚠️ SPECIAL

| Endpoint | Frontend | Status |
|----------|----------|--------|
| POST /setup/create-superuser | ❌ NO UI | ⚠️ Backend only |
| GET /setup/status | ❌ NO UI | ⚠️ Backend only |

**Issue:** Should have initial setup wizard UI

---

### **Subscription Plan Controller** (5 endpoints) ⚠️ PARTIAL

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /subscription-plans | SubscriptionsPage | ✅ |
| POST /subscription-plans | ❌ NO ADMIN SCREEN | ❌ Missing |
| GET /subscription-plans/{id} | SubscriptionsPage | ✅ |
| PUT /subscription-plans/{id} | ❌ NO ADMIN SCREEN | ❌ Missing |
| DELETE /subscription-plans/{id} | ❌ NO ADMIN SCREEN | ❌ Missing |

**Issue:** Admins cannot create/manage subscription plans

---

### **Super User Controller** (3 endpoints) ✅ MOSTLY COMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /superuser/dashboard | DashboardPage | ✅ |
| GET /superuser/tenants | TenantsOperationsPage | ✅ |
| GET /superuser/tenants/{id} | TenantDetailsPage | ✅ |

**Missing:** Tenant impersonation, tenant creation

---

### **Tenant Dashboard Controller** (11 endpoints) ✅ MOSTLY COMPLETE

| Endpoint | Frontend | Status |
|----------|----------|--------|
| GET /tenant/dashboard/overview | DashboardPage | ✅ |
| GET /tenant/dashboard/store-settings | SettingsPage | ✅ |
| PUT /tenant/dashboard/store-settings | SettingsPage | ✅ |
| GET /tenant/dashboard/analytics | AdminAnalyticsPage | ✅ |
| GET /tenant/dashboard/orders | OrdersPage | ✅ |
| GET /tenant/dashboard/products | ProductsPage | ✅ |
| GET /tenant/dashboard/customers | CustomersPage | ✅ |
| GET /tenant/dashboard/low-stock | InventoryPage | ✅ |
| GET /tenant/dashboard/top-products | AdminAnalyticsPage | ✅ |
| GET /tenant/dashboard/payment-settings | SettingsPage | ⚠️ Not shown |
| PUT /tenant/dashboard/payment-settings | SettingsPage | ⚠️ Not shown |

---

## SUMMARY TABLE

| Application | Endpoints | Implemented | Partial | Missing | % Coverage |
|-------------|-----------|-------------|---------|---------|-----------|
| **web-public** | ~20 | 4 | 3 | 13 | 20% |
| **web-storefront** | ~30 | 15 | 10 | 5 | 50% |
| **web-admin** | ~80 | 25 | 20 | 35 | 31% |
| **web-super** | ~17 | 12 | 3 | 2 | 71% |
| **TOTAL** | 147 | 56 | 36 | 55 | 38% |

---

## CRITICAL ACTION ITEMS

### TIER 1 - BLOCKING (Must fix immediately):

1. ❌ **Shipping Management Screen** - 11 endpoints, zero UI
2. ❌ **Promotions/Discounts Screen** - 12 endpoints, empty state only
3. ❌ **Marketing Campaigns Screen** - 10 endpoints, zero UI
4. ❌ **Reviews & Ratings** - 6 endpoints, zero customer UI
5. ❌ **Platform Settings** - Controller empty, UI static

### TIER 2 - HIGH (Must implement for MVP):

1. ⚠️ **Complete Customer Management** - Address book, order history
2. ⚠️ **Complete Inventory Management** - Not just low stock alerts
3. ⚠️ **Theme Builder Enhancements** - Duplicate, proper preview
4. ⚠️ **Dynamic Public Site** - Pricing from API, FAQ, newsletters
5. ⚠️ **Feature Flags Management** - 4 endpoints, zero UI

### TIER 3 - MEDIUM (Should implement):

1. ⚠️ **Coupon Management** - In cart but not admin screen
2. ⚠️ **Contact Request Admin** - Can receive but can't manage
3. ⚠️ **Analytics Export** - Missing export functionality
4. ⚠️ **Webhook Management** - Missing UI for webhook configuration
5. ⚠️ **Audit Log Filtering** - Shows list but no filters/search

### TIER 4 - LOW (Nice to have):

1. ⚠️ **Order Tracking** - Order status tracking page
2. ⚠️ **Return Requests** - Not exposed yet
3. ⚠️ **Variant Management** - Dedicated UI
4. ⚠️ **Newsletter Signup** - Not wired

---

## RECOMMENDATIONS

### Immediate (This Week):

```
Priority 1: Build Shipping Management Screen
- Add 11 endpoints for shipping zones and taxes
- 1-2 days of work

Priority 2: Build Promotions/Discounts Screen  
- Add 12 promotion endpoints
- Add coupon/discount management
- 2-3 days of work

Priority 3: Build Reviews Screen
- Product review list/filter/moderate
- Customer review submission
- Rating display
- 1-2 days of work
```

### This Sprint:

```
1. Complete Admin section (Marketing, Features, Inventory)
2. Enhance Storefront (reviews, wishlist, addresses)
3. Make Public Site dynamic (pricing, FAQ, settings from API)
4. Build remaining utility screens (Webhooks, Settings, Logs)
```

### Before Production:

```
1. Audit every endpoint - create dedicated endpoint coverage map
2. Implement data export/reporting
3. Add bulk operations to tables
4. Complete search and filtering across all screens
5. Add role-based UI (hide admin features from staff)
```

---

## CONCLUSION

Your frontend integration has **solid foundational work** but **critical gaps remain**:

- **38% endpoint coverage** - 147 endpoints, only 56 wired
- **4 completely missing screens** with 44+ endpoints
- **12 empty/static screens** that should be dynamic
- **Admin functionality only 31% complete**

**Recommendation:** Focus on completing the three TIER 1 items (Shipping, Promotions, Reviews) before pushing to production. These represent the core commerce functionality merchants need daily.



---

# PHASE 2: WEB-STOREFRONT DETAILED AUDIT

## Overview

**Status:** 50% coverage  
**Endpoints Wired:** 15/30  
**Screens Implemented:** 4/8  
**Critical Issues:** 6

---

## Storefront Endpoint Breakdown

### ✅ IMPLEMENTED & WIRED

**1. Product Discovery & Browsing**

| Endpoint | Status | Notes |
|----------|--------|-------|
| GET /storefront/info | ✅ | Wired to HomePage |
| GET /storefront/categories | ✅ | Category filtering works |
| GET /storefront/products | ✅ | Product listing with pagination |
| GET /storefront/featured-products | ✅ | HomePage hero section |
| GET /storefront/products/{id} | ✅ | ProductDetailsPage |
| GET /storefront/search | ✅ | Search functionality |
| GET /products (list) | ✅ | Alternative product endpoint |

**2. Shopping Cart & Checkout**

| Endpoint | Status | Notes |
|----------|--------|-------|
| POST /cart | ✅ | Cart creation |
| GET /cart | ✅ | Cart retrieval |
| POST /cart/items | ✅ | Add to cart |
| DELETE /cart/items/{id} | ✅ | Remove from cart |
| PUT /cart/items/{id} | ✅ | Update cart item quantity |
| POST /checkout/sessions | ✅ | Create checkout session |
| GET /checkout/sessions/{id} | ✅ | Get checkout state |
| PUT /checkout/sessions/{id}/shipping-address | ✅ | Shipping address |
| PUT /checkout/sessions/{id}/billing-address | ✅ | Billing address |
| POST /checkout/sessions/{id}/place-order | ✅ | Place order |

**3. Order Management (Customer)**

| Endpoint | Status | Notes |
|----------|--------|-------|
| GET /orders | ✅ | Customer order list |
| GET /orders/{id} | ✅ | Order details |

---

### ⚠️ PARTIALLY IMPLEMENTED

**1. Reviews (Backend ready, Frontend incomplete)**

| Endpoint | Frontend Status | Issue |
|----------|-----------------|-------|
| GET /products/{id}/reviews | ❌ Missing | No review list on ProductDetailsPage |
| GET /products/{id}/reviews/stats | ❌ Missing | No rating display |
| POST /products/{id}/reviews | ❌ Missing | No review submission form |
| PUT /products/{id}/reviews/{id} | ❌ Missing | Cannot edit review |
| DELETE /products/{id}/reviews/{id} | ❌ Missing | Cannot delete review |
| PATCH /products/{id}/reviews/{id}/helpful | ❌ Missing | No helpful/unhelpful votes |

**Action Required:** Add review section to ProductDetailsPage with:
- Average rating stars
- Review list with pagination
- Filter by rating (5⭐, 4⭐, etc.)
- Sort by (newest, helpful, rating)
- Review submission form (authenticated)
- Edit/delete own reviews

---

**2. Wishlist (Partially integrated)**

| Endpoint | Frontend Status | Issue |
|----------|-----------------|-------|
| GET /wishlist | ✅ | Works but only in admin |
| GET /wishlist/{id} | ⚠️ | Not used |
| POST /wishlist | ✅ | Add to wishlist works |
| DELETE /wishlist/{id} | ✅ | Remove from wishlist works |
| GET /wishlist/count | ✅ | Badge count works |

**Action Required:** 
- Add dedicated Wishlist page in web-storefront
- Show wishlist items with "Move to Cart" action
- Share wishlist feature (public link)
- Email wishlist reminder

---

**3. Customer Account (Skeleton only)**

| Endpoint | Frontend Status | Issue |
|----------|-----------------|-------|
| GET /customers/{id} | ❌ Missing | No customer detail page in storefront |
| GET /customers/{id}/orders | ⚠️ Partial | Order history not shown |
| GET /customers/{id}/addresses | ❌ Missing | Address book missing |
| POST /customers/{id}/addresses | ❌ Missing | Cannot add address |
| PUT /customers/{id}/addresses/{id} | ❌ Missing | Cannot edit address |
| DELETE /customers/{id}/addresses/{id} | ❌ Missing | Cannot delete address |

**Action Required:** Build customer dashboard with:
- Profile/account settings
- Address book (add, edit, delete, set default)
- Order history with tracking
- Wishlist management
- Notification preferences
- Password change

---

### ❌ NOT IMPLEMENTED

**1. Coupons/Discounts (In cart but not full flow)**

| Endpoint | Frontend Status | Issue |
|----------|-----------------|-------|
| PUT /cart/apply-coupon | ❌ Missing | No coupon input field |
| PUT /cart/remove-coupon | ❌ Missing | No remove button |
| GET /promotions | ❌ Missing | No promotions page |
| POST /checkout/sessions/{id}/apply-coupon | ❌ Missing | Not in checkout |

**Action Required:** Add coupon/discount flow to checkout

---

**2. Order Tracking**

| Endpoint | Frontend Status | Issue |
|----------|-----------------|-------|
| GET /orders/{id}/tracking | ❌ Missing | No tracking page |
| POST /orders/{id}/cancel | ❌ Missing | Cannot cancel order |

**Action Required:** Add order tracking/cancellation to order details page

---

**3. Returns (Not exposed yet)**

| Endpoint | Frontend Status | Issue |
|----------|-----------------|-------|
| POST /orders/{id}/return | ❌ Missing | No return UI |
| GET /returns | ❌ Missing | No return list |

**Note:** Returns endpoint not yet in controllers. Awaiting backend implementation.

---

## Storefront Screen Audit

### Screen 1: HomePage ✅

**Current State:** Working  
**Endpoints Used:**
- GET /storefront/info
- GET /storefront/featured-products

**What's Missing:**
- Reviews from featured products ❌
- "New Arrivals" section (has endpoint, not used)
- Featured collections
- Customer testimonials/reviews feed
- Trust badges/reviews count

**Recommended Additions:**
```typescript
// Add these sections to HomePage
- Latest reviews from all products (last 10)
- Customer ratings distribution (e.g., "4.8 ⭐ from 1,200+ reviews")
- Trending products (most reviewed)
- Featured collections
```

---

### Screen 2: Product Listing (Browse/Search) ✅

**Current State:** Working  
**Endpoints Used:**
- GET /storefront/products (with categoryId filter)
- GET /storefront/search

**What's Missing:**
- Price filter bounds ⚠️ Should query min/max from products
- Availability filter ❌
- Rating filter ❌
- Review count display ❌
- Sort by rating ❌
- Sort by newest reviews ❌

**Recommended Additions:**
```typescript
// Add to filters sidebar
const Filters = {
  categories: [], // ✅ Already there
  priceRange: {
    min: 0,
    max: 0 // ❌ Query from backend
  },
  ratings: [5, 4, 3, 2, 1], // ❌ Not in UI
  availability: ['In Stock', 'Pre-order'], // ❌ Not in UI
  sortBy: [
    'Newest',
    'Price: Low to High',
    'Price: High to Low',
    'Best Reviews', // ❌ Not available
    'Most Reviewed' // ❌ Not available
  ]
}
```

---

### Screen 3: Product Details ⚠️

**Current State:** Partial  
**Endpoints Used:**
- GET /storefront/products/{id}

**What's Missing:**
- ❌ Reviews section (6 endpoints not wired)
- ❌ Wishlist integration (icon/button missing?)
- ❌ Related products
- ❌ Recently viewed
- ❌ Customer Q&A
- ⚠️ Gallery: Need to verify zooming works

**Critical Gap - REVIEWS:**

Expected UI:
```
ProductDetailsPage
├── Hero Section (Image gallery)
├── Pricing & Variants
├── Add to Cart / Wishlist
├── **REVIEWS SECTION** ❌
│   ├── Average Rating (4.8 ⭐)
│   ├── Review Distribution (5⭐ 60%, 4⭐ 25%, etc.)
│   ├── Review Filters (by rating, helpful, newest)
│   ├── Review List
│   │   ├── Reviewer name, verified purchase badge
│   │   ├── Rating, title, body
│   │   ├── Helpful count / Mark as helpful
│   │   └── Report/delete if own review
│   └── "Write a Review" button (authenticated)
├── Recently Viewed
├── Related Products
└── Shipping & Returns Info
```

**Action Required:** Implement reviews section using:
- GET /products/{id}/reviews/stats (show distribution)
- GET /products/{id}/reviews (list reviews)
- POST /products/{id}/reviews (create review, logged in)
- PATCH /products/{id}/reviews/{id}/helpful (vote)

---

### Screen 4: Checkout ✅

**Current State:** Functional  
**Endpoints Used:**
- POST /checkout/sessions
- GET /checkout/sessions/{id}
- PUT /checkout/sessions/{id}/shipping-address
- PUT /checkout/sessions/{id}/billing-address
- POST /checkout/sessions/{id}/place-order

**What's Missing:**
- ❌ Apply coupon code (has endpoint)
- ⚠️ Shipping method selection (assumed in backend)
- ⚠️ Tax display/calculation (assumed in backend)
- ⚠️ Payment method selection (assumed in backend)
- ⚠️ Order summary with line items

**Recommended Additions:**
```typescript
// Checkout flow should show:
1. Cart Review (list all items)
2. Shipping Address (new/saved)
3. Shipping Method Selection (once address set)
4. Billing Address (copy shipping or new)
5. **Coupon Code Entry** ❌
6. Order Summary with:
   - Subtotal
   - Shipping cost (calculated)
   - Tax (calculated)
   - Discount (if coupon applied)
   - Total
7. Payment Method
8. Place Order
```

---

## Missing Screens

### ❌ Screen 5: Customer Account/Dashboard

**Why Missing?** No dedicated UI built yet  
**Backend Support:** 7+ endpoints ready

```typescript
/account or /my-account should have:

1. Profile Tab
   - Avatar
   - First/Last Name
   - Email
   - Phone
   - Preferences (newsletter, notifications)

2. Addresses Tab
   - List all addresses
   - Add new address (+ button)
   - Edit/Delete (actions per item)
   - Set as default shipping/billing

3. Orders Tab
   - Order history with pagination
   - Order status badges
   - Quick links (view, cancel, return, track)
   - Filter by status

4. Wishlist Tab
   - Show all wishlist items
   - Move to cart
   - Share wishlist (copy link)

5. Settings Tab
   - Change password
   - Email preferences
   - Two-factor authentication
   - Notification settings
   - Account deletion
```

**Action Required:** Build complete customer account dashboard

---

### ❌ Screen 6: Wishlist (Dedicated Page)

**Why Missing?** Only shows in admin  
**Backend Support:** 4 endpoints ready

```typescript
/wishlist page should have:

1. Wishlist List
   - Product cards with remove button
   - Add to cart from wishlist
   - Quantity selector
   - "Buy Now" button

2. Wishlist Actions
   - Share wishlist (public link)
   - Sort (newest, oldest, price high-low)
   - Filter (in stock, price range)

3. Empty State
   - "Your wishlist is empty"
   - "Browse products" CTA
```

**Action Required:** Build Wishlist page for customers

---

### ❌ Screen 7: Order Tracking

**Why Missing?** No dedicated tracking UI  
**Backend Support:** Tracking endpoint exists

```typescript
/orders/{orderId} should have:

1. Order Header
   - Order number, date, status
   - Total amount

2. Order Timeline
   - Placed (timestamp)
   - Processing (timestamp)
   - Shipped (timestamp, tracking number link)
   - Delivered/Out for delivery (timestamp)

3. Shipment Tracking
   - Carrier (FedEx, UPS, etc.)
   - Tracking number (clickable)
   - Expected delivery date

4. Order Items
   - List all products
   - Quantity, price, subtotal

5. Shipping Address
   - Full address, contact

6. Actions
   - Print invoice
   - Request return
   - Contact seller
   - Reorder (quick add to cart)
```

**Action Required:** Build order tracking page

---

### ❌ Screen 8: Return Management

**Why Missing?** Backend not exposing endpoints yet  
**Status:** Awaiting backend implementation

---

## Storefront Data Flow Validation

### Expected Cart Flow:
```
1. GET /storefront/products ✅
2. GET /storefront/products/{id} ✅
3. POST /wishlist ✅ (optional)
4. POST /cart ✅
5. POST /cart/items ✅
6. GET /cart ✅
7. PUT /cart/apply-coupon ❌ MISSING
8. POST /checkout/sessions ✅
9. PUT /checkout/sessions/{id}/shipping-address ✅
10. PUT /checkout/sessions/{id}/billing-address ✅
11. POST /checkout/sessions/{id}/place-order ✅

Coverage: 10/11 (91%) ✅
```

### Expected Review Flow:
```
1. GET /products/{id} ✅
2. GET /products/{id}/reviews/stats ❌ MISSING
3. GET /products/{id}/reviews ❌ MISSING
4. GET /products/{id}/reviews?sort=helpful ❌ MISSING
5. POST /products/{id}/reviews ❌ MISSING (logged in)
6. PATCH /products/{id}/reviews/{id}/helpful ❌ MISSING

Coverage: 0/6 (0%) ❌
```

### Expected Wishlist Flow:
```
1. GET /wishlist ⚠️ Works but not in storefront
2. POST /wishlist ✅
3. DELETE /wishlist/{id} ✅
4. GET /wishlist/count ✅

Coverage: 4/4 (100%) but wrong UI ⚠️
```

---

## Storefront Summary

| Category | Status | Notes |
|----------|--------|-------|
| **Product Discovery** | ✅ 90% | Browse, search, filters working |
| **Shopping Cart** | ✅ 90% | Missing coupon application |
| **Checkout** | ✅ 85% | Missing coupon, tax calc may be backend |
| **Reviews** | ❌ 0% | CRITICAL - 6 endpoints not used |
| **Wishlist** | ⚠️ 50% | Works but wrong place (admin only) |
| **Customer Account** | ❌ 0% | CRITICAL - 7 endpoints unused |
| **Order Tracking** | ⚠️ 25% | List works, detail page incomplete |

**Overall Storefront Coverage:** 50%

---

## Web-Storefront Action Items

### CRITICAL (Block purchase flow if not done):
1. ❌ Implement reviews section on ProductDetailsPage
2. ❌ Add coupon code input to checkout
3. ❌ Build customer account dashboard

### HIGH (Affects user experience):
1. ⚠️ Move wishlist to customer-facing page
2. ❌ Add address book (checkout needs this)
3. ❌ Improve product detail page (related products, recently viewed)
4. ⚠️ Add order tracking details

### MEDIUM (Can wait):
1. ⚠️ Return request flow
2. ⚠️ Share wishlist feature
3. ⚠️ Customer notifications
4. ⚠️ Product Q&A

---

