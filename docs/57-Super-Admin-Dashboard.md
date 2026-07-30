# Kromic Store Frontend Documentation

# Phase 04 -- 57 Super Admin Dashboard

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the landing experience for platform administrators. The Super
Admin Dashboard provides a real-time overview of platform health, tenant
activity, subscriptions, revenue, and operational alerts from a single
workspace.

------------------------------------------------------------------------

# Goals

-   Platform-wide visibility
-   Actionable insights
-   Quick navigation
-   Operational monitoring
-   Fast decision making

------------------------------------------------------------------------

# Dashboard Layout

``` text
Top App Bar
↓
Page Header
↓
KPI Cards
↓
Analytics Widgets
↓
Tenant Overview
↓
System Health
↓
Activity Feed
```

Widgets should support drag-and-drop customization in a future release.

------------------------------------------------------------------------

# KPI Cards

Display high-level metrics such as:

-   Total Tenants
-   Active Tenants
-   Trial Tenants
-   Paid Subscriptions
-   Monthly Recurring Revenue
-   Active Users
-   Orders Today
-   API Requests

KPIs should support trend indicators and comparison with previous
periods.

------------------------------------------------------------------------

# Revenue & Subscription Analytics

Visualize:

-   Revenue trends
-   Subscription plans
-   Renewals
-   Churn
-   Trial conversions
-   New subscriptions

Support configurable date ranges.

------------------------------------------------------------------------

# Tenant Overview

Display:

-   Recently created tenants
-   Top-performing tenants
-   Suspended tenants
-   Storage usage
-   Subscription status

Quick actions:

-   View Tenant
-   Impersonate (if permitted)
-   Suspend
-   Activate

------------------------------------------------------------------------

# System Health

Monitor:

-   API status
-   Database health
-   Background jobs
-   Email delivery
-   Storage utilization
-   Queue processing

Highlight warnings and critical issues prominently.

------------------------------------------------------------------------

# Recent Activity Feed

Track events including:

-   Tenant creation
-   Subscription changes
-   Theme publication
-   User invitations
-   Failed integrations
-   Security events

Allow filtering by event type.

------------------------------------------------------------------------

# Pending Actions

Surface actionable items:

-   Tenant approvals
-   Billing failures
-   Expiring subscriptions
-   Failed background jobs
-   Unverified domains

------------------------------------------------------------------------

# Quick Actions

Provide shortcuts to:

-   Create Tenant
-   Manage Plans
-   Review Themes
-   Broadcast Announcement
-   Platform Settings

------------------------------------------------------------------------

# Search

Global search should locate:

-   Tenants
-   Users
-   Orders
-   Themes
-   Domains

------------------------------------------------------------------------

# Filters

Support filtering dashboard data by:

-   Date range
-   Subscription plan
-   Tenant status
-   Region
-   Environment

------------------------------------------------------------------------

# Responsive Design

Desktop: - Multi-column dashboard

Tablet: - Reduced columns - Stacked analytics

Mobile: - KPI-first layout - Collapsible widgets

------------------------------------------------------------------------

# Loading & Empty States

Support:

-   Skeleton loaders
-   Empty analytics state
-   Retry actions
-   Partial widget loading

------------------------------------------------------------------------

# Accessibility

Ensure:

-   Keyboard navigation
-   Screen-reader labels
-   High contrast charts
-   Focus management

------------------------------------------------------------------------

# Best Practices

-   Prioritize actionable information.
-   Keep critical alerts visible.
-   Avoid information overload.
-   Refresh live metrics efficiently.
-   Allow future dashboard personalization.

------------------------------------------------------------------------

# Next Document

**58-Tenant-Management.md**

Topics:

-   Tenant listing
-   Tenant profile
-   Tenant lifecycle
-   Suspension
-   Subscription management
-   Search & filters
-   Bulk actions
