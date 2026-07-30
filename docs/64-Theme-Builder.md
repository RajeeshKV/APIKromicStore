# Kromic Store Frontend Documentation

# Phase 04 -- 64 Theme Builder

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

The Theme Builder is the flagship capability of Kromic Store. It enables
tenants to visually design and customize their storefront without
writing code through a modern drag-and-drop experience with live
preview.

------------------------------------------------------------------------

# Goals

-   No-code page building
-   Live visual editing
-   Reusable components
-   Responsive design
-   Safe publishing
-   Extensible architecture

------------------------------------------------------------------------

# Workspace Layout

``` text
Toolbar
↓
---------------------------------------------------------
| Component Library | Canvas | Properties Inspector |
---------------------------------------------------------
| Layer Panel | Live Preview | History |
---------------------------------------------------------
```

Resizable panels should preserve their size between sessions.

------------------------------------------------------------------------

# Core Areas

## Toolbar

-   Save
-   Publish
-   Preview
-   Undo
-   Redo
-   Device Switcher
-   Zoom
-   Theme Settings

------------------------------------------------------------------------

## Component Library

Organize components into:

-   Layout
-   Navigation
-   Hero
-   Banner
-   Products
-   Collections
-   Media
-   Forms
-   Testimonials
-   FAQ
-   Footer
-   Custom Components (future)

Support search, categories, and favorites.

------------------------------------------------------------------------

## Canvas

The visual editing surface.

Capabilities:

-   Drag & Drop
-   Resize
-   Reorder
-   Duplicate
-   Delete
-   Multi-select
-   Snap guides

------------------------------------------------------------------------

## Layer Panel

Display page hierarchy.

Support:

-   Collapse/Expand
-   Drag reorder
-   Rename
-   Lock
-   Hide
-   Duplicate

------------------------------------------------------------------------

## Properties Inspector

Edit selected component properties.

Examples:

-   Content
-   Typography
-   Colors
-   Spacing
-   Border
-   Shadow
-   Animation
-   Visibility
-   Responsive overrides

------------------------------------------------------------------------

# Responsive Editing

Support editing for:

-   Desktop
-   Tablet
-   Mobile

Allow device-specific overrides while inheriting common styles by
default.

------------------------------------------------------------------------

# Live Preview

Preview updates instantly.

Modes:

-   Design
-   Preview
-   Fullscreen Preview

Future:

-   Shareable preview links

------------------------------------------------------------------------

# Section Library

Reusable sections:

-   Hero
-   Featured Products
-   Categories
-   Collections
-   Promotional Banner
-   Newsletter
-   Testimonials
-   FAQ
-   Contact
-   Footer

Allow saving custom sections.

------------------------------------------------------------------------

# Theme Assets

Manage:

-   Images
-   Videos
-   Icons
-   Fonts
-   Documents

Support folders, search, and drag-and-drop upload.

------------------------------------------------------------------------

# Theme Settings

Configure:

-   Colors
-   Typography
-   Buttons
-   Inputs
-   Cards
-   Borders
-   Shadows
-   Global spacing
-   Breakpoints

Changes should apply consistently across supported components.

------------------------------------------------------------------------

# Draft & Publish

Workflow:

1.  Auto-save draft
2.  Manual save
3.  Preview
4.  Validate
5.  Publish
6.  Rollback if required

------------------------------------------------------------------------

# Version History

Maintain:

-   Drafts
-   Published versions
-   Restore points
-   Change summaries

Allow restoring previous versions.

------------------------------------------------------------------------

# Import & Export

Support:

-   Theme export
-   Theme import
-   Template cloning

Validate compatibility before importing.

------------------------------------------------------------------------

# Productivity Features

-   Undo / Redo
-   Autosave
-   Keyboard shortcuts
-   Copy / Paste
-   Duplicate
-   Multi-select

Future:

-   Real-time collaboration
-   Comments
-   Presence indicators

------------------------------------------------------------------------

# Validation

Before publishing:

-   Missing images
-   Invalid links
-   Empty required sections
-   SEO warnings
-   Accessibility checks

------------------------------------------------------------------------

# Performance

-   Virtualize large canvases
-   Lazy-load heavy assets
-   Incremental rendering
-   Debounced autosave

------------------------------------------------------------------------

# Accessibility

Ensure:

-   Keyboard navigation
-   Accessible drag-and-drop
-   Focus management
-   Screen-reader labels

------------------------------------------------------------------------

# Best Practices

-   Preserve user work automatically.
-   Make destructive actions reversible.
-   Keep editing responsive.
-   Prefer reusable sections over duplication.
-   Validate before publishing.

------------------------------------------------------------------------

# Next Document

**65 -- Product Management**

Topics:

-   Product listing
-   Product editor
-   Variants
-   Inventory
-   Images
-   SEO
-   Bulk operations
-   Publishing workflow
