# Kromic Store Backend Implementation Guide

# Phase 02 -- 18 Outbox and Notifications

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the event-driven notification architecture used throughout Kromic
Store.

Goals:

-   Reliable message delivery
-   No lost emails
-   Retry support
-   Provider independence
-   Event-driven architecture

------------------------------------------------------------------------

# Architecture

``` text
Business Transaction
        │
Persist Outbox Event
        │
Commit Database Transaction
        │
Outbox Worker
        │
Publish Event
        │
Email / SMS / Webhook
```

------------------------------------------------------------------------

# Entity Overview

``` text
OutboxEvent
 ├── NotificationTemplate
 ├── NotificationPreference
 ├── NotificationLog
 └── WebhookSubscription
```

------------------------------------------------------------------------

# OutboxEvent

Stores events waiting to be published.

Columns:

-   Id
-   TenantId
-   EventType
-   AggregateId
-   Payload
-   Status
-   RetryCount
-   CreatedOnUtc
-   ProcessedOnUtc
-   LastError

Statuses:

-   Pending
-   Processing
-   Completed
-   Failed

------------------------------------------------------------------------

# NotificationTemplate

Reusable templates.

Examples:

-   OrderPlaced
-   OrderConfirmed
-   ShipmentDispatched
-   PasswordReset
-   EmailVerification
-   Welcome

Columns:

-   Id
-   Name
-   Subject
-   HtmlContent
-   IsSystemTemplate

------------------------------------------------------------------------

# NotificationPreference

Stores user preferences.

Columns:

-   UserId
-   EmailEnabled
-   SmsEnabled
-   WhatsAppEnabled
-   PushEnabled

------------------------------------------------------------------------

# NotificationLog

Delivery history.

Columns:

-   Id
-   Recipient
-   Channel
-   Template
-   Provider
-   Status
-   SentOnUtc
-   ErrorMessage

------------------------------------------------------------------------

# WebhookSubscription

Future integration support.

Columns:

-   TenantId
-   Url
-   Secret
-   Enabled
-   EventTypes

------------------------------------------------------------------------

# Retry Strategy

Maximum retries:

-   5

Suggested delays:

1.  Immediate
2.  30 seconds
3.  2 minutes
4.  10 minutes
5.  30 minutes

After maximum retries, mark as Failed for manual review.

------------------------------------------------------------------------

# Background Workers

Workers:

-   Outbox Publisher
-   Email Dispatcher
-   Cleanup Worker

Responsibilities:

-   Publish pending events
-   Update delivery status
-   Retry failures
-   Archive completed events

------------------------------------------------------------------------

# Business Rules

-   Business transactions never call email providers directly.
-   Events are written in the same transaction as business data.
-   Notifications are asynchronous.
-   Duplicate event processing must be idempotent.

------------------------------------------------------------------------

# Supported Channels

Current:

-   Email (Brevo)

Future:

-   SMS
-   WhatsApp
-   Push Notifications
-   In-App Notifications
-   Webhooks

------------------------------------------------------------------------

# Recommended Indexes

-   (Status, CreatedOnUtc)
-   (TenantId, EventType)
-   (UserId)
-   (Recipient)

------------------------------------------------------------------------

# Monitoring

Track:

-   Pending events
-   Failed events
-   Retry count
-   Delivery latency
-   Provider failures

Expose metrics on the admin dashboard.

------------------------------------------------------------------------

# Testing

Verify:

-   Outbox persistence
-   Retry behavior
-   Duplicate protection
-   Email delivery logging
-   Template rendering
-   Failure recovery

------------------------------------------------------------------------

# Future Enhancements

-   Dead-letter queue
-   Scheduled notifications
-   Multi-provider failover
-   Event replay
-   Template localization

------------------------------------------------------------------------

# Next Document

**19-Indexes-And-Performance.md**

Topics:

-   Index strategy
-   Query optimization
-   Database performance
-   Partitioning
-   Caching
-   Reporting queries
