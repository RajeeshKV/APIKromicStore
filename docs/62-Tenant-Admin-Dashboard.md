# Kromic Store Frontend Documentation

# Phase 04 -- 62 Tenant Admin Dashboard

**Version:** 1.0 **Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

The Tenant Admin Dashboard is the primary workspace for store owners and
staff after signing in. It provides a real-time overview of store
performance, operational tasks, customer activity, and business insights
from a single screen.

------------------------------------------------------------------------

# Goals

-   Monitor store health
-   Surface actionable insights
-   Reduce navigation
-   Improve operational efficiency
-   Support business growth

------------------------------------------------------------------------

# Dashboard Layout

``` text
Top Navigation
↓
Welcome Header
↓
Quick Actions
↓
Business KPI Cards
↓
Sales & Revenue Charts
↓
Orders & Inventory
↓
Customers & Marketing
↓
Recent Activity
```

The dashboard should support future widget personalization.

------------------------------------------------------------------------

# Welcome Section

Display:

-   Store name
-   Greeting
-   Current subscription plan
-   Store status
-   Last synchronization
-   Important announcements

------------------------------------------------------------------------

# Quick Actions

Provide one-click access to:

-   Add Product
-   Create Category
-   Create Discount
-   View Orders
-   Open Theme Builder
-   Upload Media
-   Invite Staff
-   View Reports

Frequently used actions should appear first.

------------------------------------------------------------------------

# Business KPIs

Display:

-   Today's Revenue
-   Monthly Revenue
-   Orders Today
-   Pending Orders
-   Completed Orders
-   Average Order Value
-   Conversion Rate
-   Returning Customers

Each KPI should show:

-   Trend
-   Previous comparison
-   Percentage change

------------------------------------------------------------------------

# Sales Analytics

Charts:

-   Revenue
-   Orders
-   Sales by Category
-   Sales by Product
-   Sales by Channel

Support:

-   Daily
-   Weekly
-   Monthly
-   Custom range

------------------------------------------------------------------------

# Inventory Overview

Display:

-   Low Stock Products
-   Out of Stock Products
-   Recently Added Products
-   Inventory Value

Allow quick navigation to affected products.

------------------------------------------------------------------------

# Customer Insights

Display:

-   New Customers
-   Returning Customers
-   Top Customers
-   Customer Lifetime Value
-   Recent Reviews

------------------------------------------------------------------------

# Marketing Performance

Widgets:

-   Coupon Usage
-   Campaign Performance
-   Email Statistics
-   Traffic Sources
-   Best Performing Products

Future integrations:

-   Google Analytics
-   Meta Pixel

------------------------------------------------------------------------

# Recent Orders

Table columns:

-   Order Number
-   Customer
-   Total
-   Payment Status
-   Fulfillment Status
-   Created Time

Support quick actions:

-   View
-   Print
-   Fulfill

------------------------------------------------------------------------

# Recent Activity

Timeline examples:

-   Product created
-   Order placed
-   Inventory updated
-   Theme published
-   Staff invited

Allow filtering by activity type.

------------------------------------------------------------------------

# Tasks & Reminders

Display:

-   Pending shipments
-   Inventory warnings
-   Expiring discounts
-   Subscription reminders
-   Draft products

------------------------------------------------------------------------

# Search

Global search should locate:

-   Products
-   Customers
-   Orders
-   Categories
-   Pages

------------------------------------------------------------------------

# Dashboard Filters

Support:

-   Date range
-   Sales channel
-   Order status
-   Product category

Persist user selections during the session.

------------------------------------------------------------------------

# Responsive Design

Desktop:

-   Multi-column layout

Tablet:

-   Reduced columns
-   Scrollable charts

Mobile:

-   KPI-first design
-   Swipeable cards
-   Stacked widgets

------------------------------------------------------------------------

# Loading & Empty States

Provide:

-   Skeleton cards
-   Loading charts
-   Empty analytics
-   Retry actions

------------------------------------------------------------------------

# Accessibility

Ensure:

-   Keyboard navigation
-   High-contrast charts
-   Screen-reader summaries
-   Accessible controls

------------------------------------------------------------------------

# Best Practices

-   Prioritize business-critical information.
-   Keep important actions within one click.
-   Minimize dashboard clutter.
-   Refresh live metrics efficiently.
-   Support future widget customization.

------------------------------------------------------------------------

# Next Document

**63 -- Store Settings**

Topics:

-   General settings
-   Branding
-   Contact information
-   Business hours
-   Taxes
-   Shipping
-   Domains
-   Social links
-   SEO
-   Store preferences
