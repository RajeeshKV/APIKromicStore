# Kromic Store Backend Implementation Guide

# Phase 03 -- 33 File Upload and Cloudinary APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the APIs and standards for uploading, storing, optimizing, and
serving media assets throughout Kromic Store.

------------------------------------------------------------------------

# Authorization

  Role           Access
  -------------- -------------------------------
  SuperUser      Full
  TenantAdmin    Full
  StoreManager   Upload/Edit (configurable)
  Customer       Profile uploads only (future)

------------------------------------------------------------------------

# Supported Media

-   Images
-   Videos
-   Documents (PDF invoices)
-   Icons
-   Theme assets

------------------------------------------------------------------------

# Endpoint Catalog

  Method   Endpoint                        Description
  -------- ------------------------------- -----------------------
  POST     /api/v1/files/upload            Upload file
  POST     /api/v1/files/upload-multiple   Upload multiple files
  DELETE   /api/v1/files/{id}              Delete asset
  GET      /api/v1/files/{id}              Asset details
  POST     /api/v1/files/{id}/replace      Replace asset

------------------------------------------------------------------------

# Cloudinary Folder Structure

``` text
/{tenantId}/
    logos/
    banners/
    products/
    categories/
    themes/
    pages/
    avatars/
    documents/
```

System assets:

``` text
/system/
    themes/
    email/
    defaults/
```

------------------------------------------------------------------------

# Upload Rules

Supported image formats:

-   JPG
-   JPEG
-   PNG
-   WEBP
-   SVG (restricted)

Supported video formats:

-   MP4
-   MOV
-   WEBM

Maximum sizes should be configurable by media type.

------------------------------------------------------------------------

# Image Processing

Automatically:

-   Optimize quality
-   Strip metadata
-   Generate thumbnails
-   Preserve aspect ratio
-   Deliver responsive sizes

------------------------------------------------------------------------

# Asset Metadata

Store:

-   PublicId
-   Url
-   SecureUrl
-   Width
-   Height
-   Size
-   MimeType
-   UploadedOnUtc
-   UploadedBy

------------------------------------------------------------------------

# Validation

Validate:

-   File extension
-   MIME type
-   File size
-   Image dimensions
-   Tenant ownership

Reject executable content.

------------------------------------------------------------------------

# Security

-   Signed uploads from backend
-   Virus scanning hook (future)
-   Authorization before delete
-   Prevent path traversal
-   Rate limit uploads

------------------------------------------------------------------------

# CDN Strategy

-   Cloudinary CDN
-   Long cache headers
-   Versioned URLs
-   Automatic cache busting on replacement

------------------------------------------------------------------------

# Business Rules

-   Soft delete metadata first
-   Remove orphaned assets via background job
-   Prevent deleting assets referenced by active entities

------------------------------------------------------------------------

# Testing

Verify:

-   Single upload
-   Multi-upload
-   Replacement
-   Delete
-   Thumbnail generation
-   Validation failures
-   Authorization
-   Tenant isolation

------------------------------------------------------------------------

# Future Enhancements

-   Drag-and-drop uploads
-   Chunked uploads
-   AI image tagging
-   Background image optimization
-   Duplicate detection

------------------------------------------------------------------------

# Next Document

**34-Webhooks-and-Integrations.md**

Topics:

-   Razorpay webhooks
-   Cloudinary callbacks
-   Brevo events
-   Third-party integrations
-   Signature verification
-   Retry handling
