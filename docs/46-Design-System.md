# Kromic Store Frontend Documentation

# Phase 04 -- 46 Design System

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the visual language and reusable design tokens that ensure a
consistent user experience across the Super Admin Portal, Tenant Admin
Portal, Theme Builder, and Storefront.

------------------------------------------------------------------------

# Design Principles

-   Modern and premium
-   Clean, spacious layouts
-   Accessibility first
-   Consistent spacing
-   Minimal visual noise
-   Mobile-first responsive design

------------------------------------------------------------------------

# Brand Personality

Kromic Store should feel:

-   Premium
-   Trustworthy
-   Fast
-   Modern
-   Professional
-   Configurable

------------------------------------------------------------------------

# Color System

## Primary

-   Primary
-   Primary Hover
-   Primary Active

## Neutral

-   Background
-   Surface
-   Border
-   Divider

## Semantic

-   Success
-   Warning
-   Error
-   Information

Support both Light and Dark themes through design tokens.

------------------------------------------------------------------------

# Typography

Recommended font stack:

-   Inter
-   System fallback

Scale:

-   Display
-   H1
-   H2
-   H3
-   H4
-   H5
-   Body Large
-   Body
-   Small
-   Caption

Use consistent line heights and font weights.

------------------------------------------------------------------------

# Spacing System

Use an 8-point spacing scale.

Examples:

-   4
-   8
-   12
-   16
-   24
-   32
-   48
-   64

Avoid arbitrary spacing values.

------------------------------------------------------------------------

# Grid System

Desktop:

-   12-column grid

Tablet:

-   8-column grid

Mobile:

-   4-column grid

Use consistent gutters and margins.

------------------------------------------------------------------------

# Border Radius

Recommended scale:

-   Small
-   Medium
-   Large
-   Extra Large
-   Pill

Apply consistently across buttons, cards, dialogs, and inputs.

------------------------------------------------------------------------

# Elevation & Shadows

Levels:

-   None
-   Low
-   Medium
-   High

Prefer subtle shadows over heavy elevation.

------------------------------------------------------------------------

# Iconography

Recommended:

-   Material Symbols
-   Lucide Icons

Guidelines:

-   Consistent sizing
-   Meaningful usage
-   Decorative icons should not replace labels

------------------------------------------------------------------------

# Motion

Use motion sparingly.

Examples:

-   Fade
-   Slide
-   Scale
-   Skeleton loading
-   Progress indicators

Animations should remain under 300ms where practical.

------------------------------------------------------------------------

# Accessibility

Target WCAG 2.2 AA.

Ensure:

-   Keyboard navigation
-   Focus indicators
-   Sufficient color contrast
-   Screen reader support
-   Accessible form labels

------------------------------------------------------------------------

# Design Tokens

Centralize tokens for:

-   Colors
-   Typography
-   Spacing
-   Radius
-   Shadows
-   Z-index
-   Breakpoints

Tokens should power both MUI theme customization and Tailwind
configuration.

------------------------------------------------------------------------

# Responsive Breakpoints

-   Mobile
-   Tablet
-   Laptop
-   Desktop
-   Wide

Use fluid layouts whenever possible.

------------------------------------------------------------------------

# Dark Mode

Support:

-   Light
-   Dark
-   System preference

Switch themes without page reload.

------------------------------------------------------------------------

# Best Practices

-   Never hardcode colors.
-   Use design tokens everywhere.
-   Prefer reusable components.
-   Maintain consistent spacing.
-   Avoid visual clutter.

------------------------------------------------------------------------

# Next Document

**47-Component-Library.md**

Topics:

-   Buttons
-   Forms
-   Inputs
-   Cards
-   Tables
-   Dialogs
-   Drawers
-   Navigation
-   Loaders
-   Empty states
-   Skeletons
