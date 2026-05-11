# Dark Mode Implementation Workflow for New Views

## Overview

The project uses a centralized dark mode system. All `[data-theme="dark"]` CSS rules live in [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css), and all theme toggle logic lives in [`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js). **Never** write inline `[data-theme="dark"]` rules in a view.

## How It Works

### CSS Variables (defined in [`site.css`](AgroForm.Web/wwwroot/css/site.css:1))

| Variable | Light Value | Dark Value |
|----------|-----------|-----------|
| `--app-bg` | `#f8f9fa` | `#0f1115` |
| `--surface` | `#ffffff` | `#161a22` |
| `--surface-2` | `#f1f3f5` | `#1d2230` |
| `--text` | `#212529` | `#e9edf3` |
| `--text-muted` | `#6c757d` | `rgba(233,237,243,0.7)` |
| `--border` | `rgba(0,0,0,0.12)` | `rgba(255,255,255,0.14)` |

### JavaScript API ([`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js))

The `DarkTheme` global object provides:

| Method | Description |
|--------|-------------|
| `DarkTheme.applyTheme(theme)` | Apply `'dark'` or `'light'` |
| `DarkTheme.getTheme()` | Returns current theme |
| `DarkTheme.toggleTheme()` | Toggles between dark/light |
| `DarkTheme.refreshDataTables()` | Refreshes all DataTable instances |

An event `themeChanged` is dispatched on `document` whenever the theme changes:
```js
document.addEventListener('themeChanged', function(e) {
    console.log('Theme changed to:', e.detail.theme);
});
```

## Creating a New View — Step by Step

### 1. Write only light-mode CSS in your view's `<style>` block

```html
<style>
    .my-custom-card {
        background: white;
        border: 1px solid #dee2e6;
        color: #212529;
    }
    .my-custom-card .title {
        color: #6c757d;
    }
</style>
```

### 2. Do NOT add `[data-theme="dark"]` rules inline

❌ **Wrong:**
```css
/* Don't do this */
[data-theme="dark"] .my-custom-card {
    background: var(--surface);
}
```

✅ **Right approach — Use CSS custom properties instead:**

```css
.my-custom-card {
    background: var(--surface);
    border: 1px solid var(--border);
    color: var(--text);
}
.my-custom-card .title {
    color: var(--text-muted);
}
```

> **Why:** The CSS variables automatically resolve to the correct light/dark value depending on the current `data-theme` attribute. No `[data-theme="dark"]` selector needed!

### 3. If your component truly needs unique dark mode styling

Add the rule to [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css) instead of the view:

```css
/* In dark-theme.css only, not in the view */
[data-theme="dark"] .my-unique-component {
    /* dark overrides using --surface, --text, etc. */
}
```

### 4. If your view uses Chart.js

No action needed — [`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js) automatically applies Chart.js dark defaults when the theme is dark (sets `Chart.defaults.color` and `Chart.defaults.borderColor`).

### 5. If your view uses DataTables and needs refresh on theme change

The centralized [`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js) automatically calls `DarkTheme.refreshDataTables()` when the theme changes. No additional code needed.

### 6. If your view has a theme toggle switch

Use `id="themeSwitchUser"` for the user layout switch, or `id="themeSwitch"` for admin panel. The [`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js) auto-wires these elements.

```html
<label>
    <input type="checkbox" id="themeSwitchUser">
    Dark Mode
</label>
```

## Common Dark Mode Issues and Solutions

### Headers and Titles (like `.campo-header`)
If you have headers that appear white in dark mode:

```css
/* Add to dark-theme.css */
[data-theme="dark"] .my-header {
    color: var(--text) !important;
}

[data-theme="dark"] .my-header h5,
[data-theme="dark"] .my-header h1,
[data-theme="dark"] .my-header h2,
[data-theme="dark"] .my-header h3,
[data-theme="dark"] .my-header h4,
[data-theme="dark"] .my-header h6 {
    color: var(--text) !important;
}

[data-theme="dark"] .my-header .text-muted {
    color: var(--text-muted) !important;
}
```

### Tables with Bootstrap Classes
If you're using `.table-light` or similar Bootstrap table classes:

```css
/* Add to dark-theme.css */
[data-theme="dark"] .table-light {
    --bs-table-bg: var(--surface-2) !important;
    --bs-table-color: var(--text) !important;
    --bs-table-border-color: var(--border) !important;
}

[data-theme="dark"] .table-light th,
[data-theme="dark"] .table-light td {
    background-color: var(--surface-2) !important;
    color: var(--text) !important;
    border-color: var(--border) !important;
}

[data-theme="dark"] .table-light thead th {
    background-color: var(--surface-2) !important;
    color: var(--text) !important;
    border-color: var(--border) !important;
}
```

### DataTables Pagination
If pagination buttons are not visible:

```css
/* Add to dark-theme.css */
[data-theme="dark"] .dataTables_wrapper .dataTables_paginate .paginate_button {
    background-color: var(--surface-2) !important;
    border-color: var(--border) !important;
    color: var(--text) !important;
    border-radius: 6px;
    margin: 0 2px;
}

[data-theme="dark"] .dataTables_wrapper .dataTables_paginate .paginate_button:hover {
    background-color: var(--surface) !important;
    color: var(--text) !important;
    border-color: var(--border) !important;
}

[data-theme="dark"] .dataTables_wrapper .dataTables_paginate .ellipsis {
    color: var(--text-muted) !important;
}
```

### Select2 Dropdowns
If Select2 components are not styled correctly:

```css
/* Add to dark-theme.css */
[data-theme="dark"] .select2-container--default .select2-selection--single,
[data-theme="dark"] .select2-container--default .select2-selection--multiple {
    background-color: var(--surface-2) !important;
    border-color: var(--border) !important;
    color: var(--text) !important;
}

[data-theme="dark"] .select2-dropdown {
    background-color: var(--surface) !important;
    border-color: var(--border) !important;
    box-shadow: 0 10px 30px rgba(0,0,0,0.55);
}

[data-theme="dark"] .select2-results__option {
    background-color: var(--surface) !important;
    color: var(--text) !important;
}

[data-theme="dark"] .select2-results__option--highlighted {
    background-color: #0d6efd !important;
    color: #ffffff !important;
}
```

### Choices.js Dropdowns
If Choices.js components are not styled correctly:

```css
/* Add to dark-theme.css */
[data-theme="dark"] .choices__inner {
    background-color: var(--surface-2) !important;
    border-color: var(--border) !important;
    color: var(--text) !important;
}

[data-theme="dark"] .choices__list--dropdown {
    background-color: var(--surface) !important;
    border-color: var(--border) !important;
    box-shadow: 0 10px 30px rgba(0,0,0,0.55);
}

[data-theme="dark"] .choices__list--dropdown .choices__item--selectable {
    color: var(--text) !important;
}

[data-theme="dark"] .choices__list--dropdown .choices__item--selectable.is-highlighted {
    background-color: rgba(13, 110, 253, 0.15) !important;
    color: var(--text) !important;
}

[data-theme="dark"] .choices__placeholder {
    color: var(--text-muted) !important;
    opacity: 1;
}
```

## Checklist for New Views

- [ ] All colors use CSS custom properties (`--surface`, `--text`, `--border`, etc.)
- [ ] No inline `[data-theme="dark"]` or `body[data-theme="dark"]` selectors
- [ ] No inline `applyTheme()` or theme toggle logic
- [ ] If using Chart.js: rely on central dark config in `dark-theme.js`
- [ ] If you must add a unique dark override: put it in `dark-theme.css`, not in the view

## Architecture Diagram

```
site.css                         dark-theme.css                    dark-theme.js
┌─────────────────┐             ┌──────────────────────┐         ┌──────────────────────┐
│ :root           │             │ [data-theme="dark"]  │         │ DarkTheme.applyTheme │
│   --surface     │──────┐     │   .card { ... }      │         │ DarkTheme.getTheme   │
│   --text        │      │     │   .table { ... }     │         │ themeChanged event   │
│   --border      │      │     │   .kpi-card { ... }  │         │ Chart.js dark config │
│ [data-theme="dark"] │   │     │   .stat-card { ... } │         │ DataTables refresh   │
│   { vars }      │      │     │   .dropdown { ... }  │         └──────────────────────┘
└─────────────────┘      │     │   ...all other rules │
                         │     └──────────────────────┘
                         │              ↑
                         │     Loaded AFTER site.css
                         │
                    ┌────┘
                    ▼
         ┌──────────────────────┐
         │    View's <style>    │
         │  Uses var(--surface) │
         │  Uses var(--text)    │
         │  No [data-theme]     │
         └──────────────────────┘
```

## File Locations

| File | Purpose | When to modify |
|------|---------|---------------|
| [`wwwroot/css/site.css`](AgroForm.Web/wwwroot/css/site.css) | CSS variable definitions (`:root` + `[data-theme="dark"]`) | Only when adding new design tokens |
| [`wwwroot/css/dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css) | All `[data-theme="dark"]` component rules | When adding a component that needs unique dark styling |
| [`wwwroot/js/dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js) | Theme toggle logic, Chart.js config, DataTables refresh | When modifying theme behavior |
| Your view's `<style>` block | Light-mode specific styles using CSS vars | For new view development |
