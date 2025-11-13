# Color Guide - Semantic Tailwind Classes

This guide shows how to use the semantic color names defined in `tailwind.config.js` for consistent styling across the application.

**✅ These semantic names are now actively used throughout the codebase** - see header, sidebar, dashboard, and layout components for examples.

## Page Backgrounds

```html
<!-- Page wrapper -->
<div class="bg-page-bg-light dark:bg-page-bg-dark">
  ...
</div>
```

## Cards & Surfaces

```html
<!-- Card or panel -->
<div class="bg-surface-light dark:bg-surface-dark border border-border-light-alt dark:border-border-dark">
  ...
</div>

<!-- Card with hover state -->
<div class="bg-surface-light dark:bg-surface-dark hover:bg-surface-hover-light dark:hover:bg-surface-hover-dark">
  ...
</div>
```

## Sidebar

```html
<!-- Sidebar container -->
<aside class="bg-sidebar-light dark:bg-sidebar-dark border-r border-border-light-alt dark:border-border-dark">

  <!-- Main menu item -->
  <button class="text-text-sidebar-light hover:bg-white/10">
    Menu Item
  </button>

  <!-- Submenu item -->
  <a class="text-text-sidebar-submenu hover:bg-white/10">
    Submenu Item
  </a>

</aside>
```

## Header/Navbar

```html
<!-- Header -->
<header class="bg-header-light dark:bg-header-dark border-b border-border-light-alt dark:border-border-dark">
  ...
</header>
```

## Dropdowns & Menus

```html
<!-- Dropdown menu -->
<div class="bg-surface-light dark:bg-surface-dark border border-border-light-alt dark:border-border-dark">

  <!-- Menu item with hover -->
  <a class="hover:bg-surface-hover-light dark:hover:bg-surface-hover-dark">
    Menu Item
  </a>

</div>
```

## Text Colors

```html
<!-- Headings -->
<h1 class="text-text-primary dark:text-text-primary-dark">
  Heading Text
</h1>

<!-- Body text -->
<p class="text-text-secondary dark:text-text-secondary-dark">
  Body text content
</p>

<!-- Sidebar text -->
<span class="text-text-sidebar-light">Main Menu</span>
<span class="text-text-sidebar-submenu">Submenu</span>
```

## Buttons

```html
<!-- Primary button -->
<button class="bg-primary-500 hover:bg-primary-600 text-white">
  Primary Action
</button>

<!-- Secondary button -->
<button class="bg-secondary-500 hover:bg-secondary-600 text-white">
  Secondary Action
</button>

<!-- Sidebar button -->
<button class="bg-sidebar-light hover:bg-sidebar-light-hover text-white">
  Sidebar Action
</button>
```

## Borders

```html
<!-- Light border -->
<div class="border border-border-light dark:border-border-dark">
  ...
</div>

<!-- Alternative gray border (for subtle dividers) -->
<div class="border-b border-border-light-alt dark:border-border-dark">
  ...
</div>
```

## Quick Reference Table

| Element | Light Mode | Dark Mode |
|---------|-----------|-----------|
| Page Background | `bg-page-bg-light` | `bg-page-bg-dark` |
| Cards/Panels | `bg-surface-light` | `bg-surface-dark` |
| Hover State | `bg-surface-hover-light` | `bg-surface-hover-dark` |
| Sidebar | `bg-sidebar-light` | `bg-sidebar-dark` |
| Header | `bg-header-light` | `bg-header-dark` |
| Heading Text | `text-text-primary` | `text-text-primary-dark` |
| Body Text | `text-text-secondary` | `text-text-secondary-dark` |
| Border | `border-border-light-alt` | `border-border-dark` |

## Color Values Reference

### Light Mode
- Page: `#EEF1F6` (soft blue-gray)
- Surface: `#ffffff` (white)
- Sidebar: `#006666` (teal)
- Text Primary: `#051A1A` (dark teal)
- Text Secondary: `#86909C` (gray)

### Dark Mode
- Page: `#1A1C23` (very dark blue)
- Surface: `#262932` (dark gray-blue)
- Sidebar: `#22242B` (dark blue-gray)
- Header: `#22242B` (dark blue-gray)
- Text Primary: `#ffffff` (white)
- Text Secondary: `rgba(255, 255, 255, 0.7)` (light gray)
