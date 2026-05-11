# UI/UX Audit Report — AgroForm Web Application

> **Analysis by:** Senior UI/UX + Frontend Architect  
> **Date:** May 2026  
> **Scope:** Full visual audit of all Razor views, CSS files, shared components, and layout

---

## Executive Summary

The AgroForm web application exhibits **significant visual fragmentation** across its pages. While built on Bootstrap 5.3+, the project has accumulated **4+ distinct card patterns, 4+ tab variants, multiple modal header styles, and widely varying border-radius values**. The report views carry **200-250+ lines of inline `<style>` each**, duplicating nearly identical KPI card and tab definitions. Version inconsistencies exist between the main layout (Bootstrap 5.3.3, DataTables 2.1.8) and the Admin page (Bootstrap 5.3.2, DataTables 1.13.6). The dark theme system is well-structured via CSS variables but **missing sidebar overrides entirely**, and the sidebar itself uses **hardcoded `#2c3e50` instead of CSS variables**.

The predominant visual style is a **green-primary agricultural Bootstrap system** featuring: a dark sidebar (`#2c3e50`), gradient green accent (`#198754` → `#20c997`), white content area, soft shadows (`0 2px 10px rgba(0,0,0,0.1)`), rounded cards (10-12px), and Phosphor Icons. This style is **broken by the Admin page** (standalone layout, Bootstrap Icons, older DataTables) and **overridden by inline styles in every report view**.

---

## 1. CSS Architecture Analysis

### 1.1 File Structure

| File | Lines | Purpose |
|------|-------|---------|
| `site.css` | 440 | Core layout, CSS variables, sidebar, navbar, `.stat-card`, `.welcome-section` |
| `dark-theme.css` | 607 | Dark mode overrides (cards, modals, tables, DataTables, Select2, Choices.js) |

**Issues:**

1. **Inline `<style>` bloat in views:**
   - [`Reporte/Campo.cshtml`](AgroForm.Web/Views/Reporte/Campo.cshtml:5) — 244 lines of inline CSS (timeline, `.kpi-card`, `.nav-tabs-custom`, score-ring, soil-bar)
   - [`Reporte/Cosecha.cshtml`](AgroForm.Web/Views/Reporte/Cosecha.cshtml:6) — 243 lines (`.kpi-card` redefined, `.nav-tabs-custom` different variant, ranking badges)
   - [`Reporte/Aplicaciones.cshtml`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml:6) — 228 lines (`.kpi-card` 3rd variant, `.nav-tabs-reporte`, `.table-reporte`)
   - [`Reporte/ComparativaCampos.cshtml`](AgroForm.Web/Views/Reporte/ComparativaCampos.cshtml:6) — 92 lines (`.summary-card`, heatmap, `.table-compact`)
   - [`Home/Index.cshtml`](AgroForm.Web/Views/Home/Index.cshtml:225) — 10 lines (`.btn-icon`)
   - [`MenuLateral/Default.cshtml`](AgroForm.Web/Views/Shared/Components/MenuLateral/Default.cshtml:1) — 25 lines (sidebar flex layout)
   - [`ActividadRapida/Default.cshtml`](AgroForm.Web/Views/Shared/Components/ActividadRapida/Default.cshtml:4) — 23 lines (Select2 overrides)
   - [`CamposActividades/Default.cshtml`](AgroForm.Web/Views/Shared/Components/CamposActividades/Default.cshtml) — inline `<style>` mentioned in analysis (hardcoded colors)
   - [`Administrador/Index.cshtml`](AgroForm.Web/Views/Administrador/Index.cshtml:22) — 72 lines (custom theme switch, cultivo-color fixes)

   **Problem:** These inline definitions cannot be reused, are not theme-aware (no `[data-theme="dark"]` variants in views), and inflate page size. Most should be consolidated into `site.css` or a dedicated `reports.css`.

2. **CSS variables are underutilized.**  
   [`site.css:9-15`](AgroForm.Web/wwwroot/css/site.css:9) defines `--app-bg`, `--surface`, `--surface-2`, `--text`, `--text-muted`, `--border` with dark overrides at [`:17-24`](AgroForm.Web/wwwroot/css/site.css:17). But:
   - Sidebar uses hardcoded `#2c3e50` ([`site.css:42`](AgroForm.Web/wwwroot/css/site.css:42)), `#b3b3b3` ([`:83`](AgroForm.Web/wwwroot/css/site.css:83)), `#1a252f` ([`:349`](AgroForm.Web/wwwroot/css/site.css:349))
   - Gradient header uses hardcoded `#198754` → `#20c997` ([`:67`](AgroForm.Web/wwwroot/css/site.css:67))
   - `.stat-number` uses hardcoded `#198754` ([`:401`](AgroForm.Web/wwwroot/css/site.css:401))
   - No sidebar dark theme override exists in `dark-theme.css`

3. **No `reports.css` or `components.css`** — All report-specific KPI card, timeline, tab, and table styles are scattered across views.

---

## 2. Card Pattern Inconsistencies

There are **4+ distinct card naming patterns** across the codebase, each with different dimensions, shadows, and border-radius:

### 2.1 `.stat-card` (site.css)
- **File:** [`site.css:385-396`](AgroForm.Web/wwwroot/css/site.css:385)
- **border-radius:** 10px
- **padding:** 20px
- **box-shadow:** `0 2px 10px rgba(0,0,0,0.1)`
- **border-left:** 4px solid `#198754`
- **hover:** translateY(-5px)
- **Usage:** Home Dashboard stats (dólar, hectáreas, gastos, acopio) — but these actually use `card card-hover border-0 shadow-sm` in `Home/Index.cshtml`, **not** `.stat-card`

### 2.2 `.kpi-card` (Reporte/Campo)
- **File:** [`Reporte/Campo.cshtml:45-53`](AgroForm.Web/Views/Reporte/Campo.cshtml:45)
- **border-radius:** 12px
- **padding:** 20px
- **height:** 100%
- **hover:** translateY(-3px)
- **No box-shadow or border defined** (inherits from card context)

### 2.3 `.kpi-card` (Reporte/Cosecha)
- **File:** [`Reporte/Cosecha.cshtml:24-32`](AgroForm.Web/Views/Reporte/Cosecha.cshtml:24)
- **border-radius:** 12px
- **padding:** 20px
- **height:** 100%
- **hover:** translateY(-3px)
- **Definition is IDENTICAL to Campo.cshtml's** — still duplicated.

### 2.4 `.kpi-card` (Reporte/Aplicaciones)
- **File:** [`Reporte/Aplicaciones.cshtml:8-18`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml:8)
- **border-radius:** 12px
- **border:** 1px solid `var(--bs-border-color)`
- **background:** `var(--bs-card-bg)`
- **padding:** 1.25rem
- **hover:** translateY(-2px) + box-shadow `0 4px 12px rgba(0,0,0,0.08)`
- **This is the THIRD variant of `.kpi-card`**, adding border/background explicitly.

### 2.5 `.summary-card` (Reporte/Comparativa)
- **File:** [`Reporte/ComparativaCampos.cshtml:7-16`](AgroForm.Web/Views/Reporte/ComparativaCampos.cshtml:7)
- **border-radius:** 12px
- **hover:** translateY(-2px)
- **card-body padding:** 1rem 1.25rem
- **Completely different name** from kpi-card or stat-card

### 2.6 `.indicator-card` (Reporte/Cosecha)
- **File:** [`Reporte/Cosecha.cshtml`](AgroForm.Web/Views/Reporte/Cosecha.cshtml) — defined inline, used for yield indicators
- **border-left:** 4px solid (colored left border, like stat-card but different class name)

### 2.7 Dashboard cards (Home/Index)
- **File:** [`Home/Index.cshtml:34`](AgroForm.Web/Views/Home/Index.cshtml:34)
- Uses **Bootstrap utilities**: `card card-hover border-0 shadow-sm`
- Icon in colored circle: `bg-success bg-opacity-10 rounded-circle p-3`
- **Does NOT use `.stat-card` class**, despite it being defined in site.css

---

## 3. Tab Component Variants

There are **4+ tab styling patterns**:

### 3.1 Default Bootstrap tabs (Administrador)
- **File:** [`Administrador/Index.cshtml:128`](AgroForm.Web/Views/Administrador/Index.cshtml:128)
- Uses `card-header-tabs` with Bootstrap defaults

### 3.2 `.nav-tabs-custom` (Reporte/Campo)
- **File:** [`Reporte/Campo.cshtml`](AgroForm.Web/Views/Reporte/Campo.cshtml) — defined inline
- Custom border-bottom active indicator

### 3.3 `.nav-tabs-custom` (Reporte/Cosecha)
- **File:** [`Reporte/Cosecha.cshtml:47-71`](AgroForm.Web/Views/Reporte/Cosecha.cshtml:47)
- Different from Campo's version: has `gap: 4px`, `border-radius: 8px 8px 0 0`, `padding: 10px 18px`, active state with `border-bottom: 2px solid var(--bs-primary)`
- **Same class name, different implementation**

### 3.4 `.nav-tabs-reporte` (Reporte/Aplicaciones)
- **File:** [`Reporte/Aplicaciones.cshtml`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml) — defined inline
- **Fourth naming variant** — should be unified with the above

---

## 4. Button Inconsistencies

| Location | Class | Border-Radius | Size | Style |
|----------|-------|--------------|------|-------|
| [`_Layout.cshtml:75`](AgroForm.Web/Views/Shared/_Layout.cshtml:75) | `btn-success btn-sm rounded-pill` | 50% (pill) | height:42px, padding:0 18px | Green, add action |
| [`site.css:196`](AgroForm.Web/wwwroot/css/site.css:196) | `.btn-circle` | 50% | flex centered | Generic circular button |
| [`Home/Index.cshtml:226`](AgroForm.Web/Views/Home/Index.cshtml:226) | `.btn-icon` | 6px | 32x32px | Square icon button |
| [`Administrador/Index.cshtml:173`](AgroForm.Web/Views/Administrador/Index.cshtml:173) | `btn-primary` | default Bootstrap | default | Nueva Licencia |
| [`Administrador/Index.cshtml:246`](AgroForm.Web/Views/Administrador/Index.cshtml:246) | `btn-sm btn-primary` | default Bootstrap | small | Nuevo Cultivo |
| [`MenuLateral/Default.cshtml:55`](AgroForm.Web/Views/Shared/Components/MenuLateral/Default.cshtml:55) | `btn btn-sm btn-outline-light` | default | small | Simulation exit |
| [`ActividadRapida/Default.cshtml`](AgroForm.Web/Views/Shared/Components/ActividadRapida/Default.cshtml) | Multiple `btn-*` classes | mixed | mixed | Various modal actions |
| [`Reporte/Campo.cshtml`](AgroForm.Web/Views/Reporte/Campo.cshtml) | Inline styled buttons | mixed | mixed | Report action buttons |

**Key Issue:** No standard button size/border-radius exists. The project uses `rounded-pill` (Layout add button), `6px` (Home icon buttons), `8px` (pagination), `12px` (KPI cards), `50%` (circles), and Bootstrap defaults simultaneously.

### 4.1 Action button (Layout) vs Admin
The "+" add button in the layout navbar ([`_Layout.cshtml:75`](AgroForm.Web/Views/Shared/_Layout.cshtml:75)) is a `btn-success btn-sm rounded-pill` with explicit `height: 42px; padding: 0 18px`. This should be standardized as a reusable component.

---

## 5. Modal Header Variants

The project uses **8+ different modal header color schemes**:

| Location | Header Style | Has `btn-close-white`? |
|----------|-------------|----------------------|
| [`_Layout.cshtml:261`](AgroForm.Web/Views/Shared/_Layout.cshtml:261) — Perfil Modal | Default (white) | No (default) |
| [`_Layout.cshtml:298`](AgroForm.Web/Views/Shared/_Layout.cshtml:298) — Config Modal | Default (white) | No (default) |
| [`Administrador/Index.cshtml:569`](AgroForm.Web/Views/Administrador/Index.cshtml:569) — Catalogo | `bg-primary text-white` | Yes |
| [`Administrador/Index.cshtml:650`](AgroForm.Web/Views/Administrador/Index.cshtml:650) — Cultivo | `bg-success text-white` | Yes |
| [`Administrador/Index.cshtml:438`](AgroForm.Web/Views/Administrador/Index.cshtml:438) — Pagos | `bg-info text-white` | Yes |
| [`ActividadRapida/Default.cshtml:31`](AgroForm.Web/Views/Shared/Components/ActividadRapida/Default.cshtml:31) — Labor | `bg-success text-white` | Yes |
| ActividadRapida templates: Siembra | `bg-success text-white` | Yes |
| ActividadRapida templates: Riego | `bg-info text-white` | Yes |
| ActividadRapida templates: Fertilización | `bg-primary text-white` | Yes |
| ActividadRapida templates: Pulverización | `bg-warning text-dark` | Yes |
| ActividadRapida templates: Monitoreo | `bg-danger text-white` | Yes |
| ActividadRapida templates: OtraLabor | `bg-secondary text-white` | Yes |
| ActividadRapida templates: AnálisisSuelo | `bg-dark text-white` | Yes |
| ActividadRapida templates: SiloBolsa | Custom `#8B5E3C` (brown) | N/A |

**Issue:** The rainbow of modal header colors provides visual distinction but creates inconsistency. Standardizing on **2-3 header types** (info/default, warning/danger, success) would be more cohesive.

---

## 6. Border-Radius Fragmentation

| Value | Where Used |
|-------|-----------|
| **4px** | Heatmap cells (`ComparativaCampos.cshtml:52`) |
| **6px** | `.btn-icon` (`Home/Index.cshtml:233`), dropdown items (`site.css:222`), DataTables pagination (`dark-theme.css:197`) |
| **8px** | `.pagination-custom .btn-page` (Cosecha), timeline-content (`Campo.cshtml:40`), nav-tabs-custom (`Cosecha.cshtml:53`) |
| **10px** | `.stat-card` (`site.css:387`), `.dropdown-menu` (`site.css:217`), sidebar scrollbar thumb (`site.css:353`) |
| **12px** | `.kpi-card` (all report views), `.kpi-icon` (48x48), `.summary-card` (`ComparativaCampos.cshtml:8`), `.filter-section` (`ComparativaCampos.cshtml:47`), modals (`.modal-content`) |
| **15px** | `.welcome-section` (`site.css:380`) |
| **20px** | Ranking badges (Cosecha) |
| **50%** | `.btn-circle` (`site.css:197`), `.rounded-circle` (various), `.score-ring` (`Campo.cshtml:67`) |

**Recommendation:** Standardize to 3 values: `6px` (small elements/buttons), `10px` (cards/sections), `12px` (modals/KPIs), `50%` (circles/avatars).

---

## 7. Shadow/Depth Inconsistencies

| Pattern | Box Shadow | Where |
|---------|-----------|-------|
| Navbar | `0 2px 4px rgba(0,0,0,0.1)` | [`site.css:168`](AgroForm.Web/wwwroot/css/site.css:168) |
| Stat card | `0 2px 10px rgba(0,0,0,0.1)` | [`site.css:389`](AgroForm.Web/wwwroot/css/site.css:389) |
| Dropdown menu | `0 4px 15px rgba(0,0,0,0.1)` | [`site.css:216`](AgroForm.Web/wwwroot/css/site.css:216) |
| Dashboard cards | `shadow-sm` (Bootstrap) | [`Home/Index.cshtml:34`](AgroForm.Web/Views/Home/Index.cshtml:34) |
| KPI card hover (Aplicaciones) | `0 4px 12px rgba(0,0,0,0.08)` | [`Reporte/Aplicaciones.cshtml:18`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml:18) |
| Dark dropdown | `0 10px 30px rgba(0,0,0,0.55)` | [`dark-theme.css:57`](AgroForm.Web/wwwroot/css/dark-theme.css:57) |
| Cards in dark mode | `0 2px 14px rgba(0,0,0,0.35)` | [`dark-theme.css:257`](AgroForm.Web/wwwroot/css/dark-theme.css:257) |

**Recommendation:** Define `--shadow-sm`, `--shadow-md`, `--shadow-lg` CSS variables with dark-theme overrides.

---

## 8. Page-Specific Issues

### 8.1 Layout (`_Layout.cshtml`)
- **Line 342:** [`</script>`](AgroForm.Web/Views/Shared/_Layout.cshtml:342) — Extra closing tag after `<script src="~/js/layout.js"></script>` (line 341). This is a **breaking HTML error** that may cause script loading issues.
- The `ActividadRapida` component is invoked at line 255, which means the modal HTML is rendered at the bottom of every page, even when never opened. Consider lazy-loading.

### 8.2 Admin Page (`Administrador/Index.cshtml`)
- **`Layout = null`** — Standalone page bypassing main layout entirely. Uses different Bootstrap version (`5.3.2` vs `5.3.3` in layout).
- **Uses Bootstrap Icons** (`bi bi-*`) for the theme switch icons but Phosphor Icons (`ph ph-*`) everywhere else. Icon inconsistency.
- **jQuery loaded twice** — lines 741 and 744: `<script src="https://code.jquery.com/jquery-3.7.0.min.js"></script>` and `<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>`. Two different versions of jQuery loaded — this causes conflicts and memory waste.
- **DataTables 1.13.6** (older) vs main layout's **DataTables 2.1.8** (newer). Incompatible APIs between versions.
- **Custom theme switch** (CSS-only toggle at lines 27-81) vs main layout's **Bootstrap form-switch**. Two completely different theme switch implementations.

### 8.3 Report Views (`Reporte/Campo.cshtml`, `Cosecha.cshtml`, `Aplicaciones.cshtml`)
- **Massive inline `<style>` blocks** (200-250+ lines each) with nearly identical `.kpi-card`, `.kpi-icon` definitions.
- **Each redefines the same class names** (`kpi-card`, `kpi-icon`) with slight variations — this is the #1 refactoring priority.
- **Cosecha.cshtml** has a [Bootstrap tab-pane stacking fix](AgroForm.Web/Views/Reporte/Cosecha.cshtml:6-21) using `!important` — suggesting a Bootstrap/z-index issue that should be fixed in the layout, not per-page.
- **Aplicaciones.cshtml** has duplicate chart canvases (e.g., `chartCostosProducto` + `chartCostosProductoFull`) for different tab visibility — wasteful.

### 8.4 Home Dashboard (`Home/Index.cshtml`)
- **Welcome section is commented out** ([lines 10-28](AgroForm.Web/Views/Home/Index.cshtml:10)) — dead code.
- **Uses Bootstrap `card card-hover border-0 shadow-sm`** instead of the defined `.stat-card` class from site.css. This means `.stat-card` CSS is **completely unused**.
- **`.btn-icon` defined inline** (line 225) instead of being in site.css.

### 8.5 ActividadRapida Component
- **934 lines** — single massive modal with 8+ activity type templates, plus Clima and Gasto sub-modals.
- **Repetitive cost input pattern** (input-group with `$` prefix + ARS/USD form-switch) appears **9+ times** — should be extracted to a partial view.
- **Multiple modal headers** in the same component use different colors (`bg-success`, `bg-info`, `bg-primary`, `bg-warning`, `bg-danger`, `bg-secondary`, `bg-dark`, custom `#8B5E3C`).

### 8.6 CamposActividades Component
- **Typo at line 8:** [`text-succsess`](AgroForm.Web/Views/Shared/Components/CamposActividades/Default.cshtml:8) should be `text-success`.
- **Inline JavaScript** rendering dynamic HTML — mixing strings with Razor syntax, hard to maintain.

### 8.7 Lluvia Component
- **Uses ApexCharts** (different charting library from Chart.js used in reports). This adds an extra dependency (ApexCharts CSS + JS) for a single chart. Consider unifying on Chart.js.

---

## 9. Typography Analysis

### 9.1 Font Family
- Bootstrap 5.3 default system font stack (no custom font loaded beyond Bootstrap).
- No `@import` or custom `font-family` defined in any CSS file.

### 9.2 Font Weight Usage
| Weight | Where |
|--------|-------|
| `fw-bold` | Stat numbers (`stat-number`), card titles, KPI values |
| `fw-semibold` | Labels (e.g., "COTIZACIÓN"), some headings |
| `fw-600` | Navbar brand (`site.css:234`) |
| Default (400) | Body text, table content |

### 9.3 Font Size Usage
| Size | Where |
|------|-------|
| `2rem` | `.stat-number` (`site.css:399`) |
| `1.75rem` | `.kpi-value` (Aplicaciones) |
| `1.5rem` | `.kpi-icon` font-size (Aplicaciones) |
| `0.9rem` | `.stat-label` (`site.css:406`), sidebar username |
| `0.85rem` | `.kpi-label` (Aplicaciones), `.table-reporte td` |
| `0.8rem` | `.table-reporte th`, `.kpi-trend` |

**Issue:** No typographic scale is defined. Sizes are fragmented and not based on a modular scale.

---

## 10. Iconography

### 10.1 Icon Systems Used
1. **Phosphor Icons** (`ph ph-*`) — Primary system, loaded via CDN in layout
2. **Bootstrap Icons** (`bi bi-*`) — Used only in Admin page theme switch

### 10.2 Icon Class Issues
- [`Reporte/Aplicaciones.cshtml:241-244`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml:241): `ph-file-xls` and `ph-file-pdf` used **without the `ph ` prefix** — should be `ph ph-file-xls`.
- Some icons use inline `style="font-size: 20px;"` instead of Bootstrap's `fs-*` classes or CSS variables.

### 10.3 Icon Sizing
| Size | Usage |
|------|-------|
| `24px` | `.kpi-icon` icons, actividad-icono |
| `20px` | Layout add button icon |
| `1.5rem (24px)` | Summary card icons (Comparativa) |
| `fs-4` | Dashboard stat icons |
| `display-1` | Welcome section (commented out) |

---

## 11. Form Control Inconsistencies

### 11.1 Select/Moneda Pattern
The "moneda selector" pattern appears in **at least 3 places** with different implementations:
1. [`TablaGastos/Default.cshtml:14`](AgroForm.Web/Views/Shared/Components/TablaGastos/Default.cshtml:14) — `<select>` with inline `onchange="cambiarMoneda()"`
2. [`Actividad/Index.cshtml`](AgroForm.Web/Views/Actividad/Index.cshtml) — Has its own moneda selector
3. [`Reporte/ComparativaCampos.cshtml`](AgroForm.Web/Views/Reporte/ComparativaCampos.cshtml) — Has its own moneda selector

Each has its own `cambiarMoneda()` function. This should be a single shared component/partial.

### 11.2 Form Checkbox Styles
- Some use `form-check form-switch` (Bootstrap toggles)
- Admin page uses a completely custom CSS toggle (`.theme-switch-slider`)
- Some switches have `form-check-label`, some don't

---

## 12. Dark Theme Issues

### 12.1 Missing Sidebar Overrides
The sidebar ([`site.css:39-51`](AgroForm.Web/wwwroot/css/site.css:39)) uses:
```css
background: #2c3e50 !important;
color: #fff;
```
There is **NO `[data-theme="dark"] .sidebar`** rule in `dark-theme.css`. When dark mode is active, the sidebar remains a light-medium blue (`#2c3e50`), while the rest of the UI goes dark. This is a **major visual break**.

### 12.2 Inline Styles Not Theme-Aware
All inline `<style>` blocks in report views define colors without `[data-theme="dark"]` variants. For example:
- [`ComparativaCampos.cshtml:56-60`](AgroForm.Web/Views/Reporte/ComparativaCampos.cshtml:56) — heatmap colors (`#1a9850`, `#91cf60`, etc.) don't adapt to dark mode
- [`Campo.cshtml:76-77`](AgroForm.Web/Views/Reporte/Campo.cshtml:76) — score-ring colors (`#28a745`, `#17a2b8`) don't adapt
- [`Cosecha.cshtml:42-44`](AgroForm.Web/Views/Reporte/Cosecha.cshtml:42) — trend colors hardcoded

### 12.3 Select2 Overrides in ActividadRapida
[`ActividadRapida/Default.cshtml:9-16`](AgroForm.Web/Views/Shared/Components/ActividadRapida/Default.cshtml:9) defines Select2 styles that **conflict** with the centralized Select2 dark mode rules in `dark-theme.css`. The inline styles use hardcoded `#e9ecef`, `#0d6efd` — not theme-aware.

---

## 13. Code Quality Issues

### 13.1 HTML/JS Errors
| Issue | Location | Severity |
|-------|----------|----------|
| Extra `</script>` closing tag | [`_Layout.cshtml:342`](AgroForm.Web/Views/Shared/_Layout.cshtml:342) | **HIGH** — breaks script parsing |
| jQuery loaded twice (versions 3.7.0 and 3.7.1) | [`Administrador/Index.cshtml:741,744`](AgroForm.Web/Views/Administrador/Index.cshtml:741) | **HIGH** — conflicts, wasted bandwidth |
| Typo: `text-succsess` → `text-success` | [`CamposActividades/Default.cshtml:8`](AgroForm.Web/Views/Shared/Components/CamposActividades/Default.cshtml:8) | LOW — CSS class typo, no effect |
| Missing `ph ` prefix on icons | [`Reporte/Aplicaciones.cshtml:241-244`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml:241) | MEDIUM — icons won't render |

### 13.2 Version Fragmentation
| Library | Main Layout | Admin Page |
|---------|-------------|------------|
| Bootstrap | 5.3.3 | 5.3.2 |
| DataTables | 2.1.8 | 1.13.6 |
| DataTables Buttons | 3.1.2 | 2.4.1 |
| jQuery | 3.7.1 | 3.7.0 + 3.7.1 (duplicate) |

---

## 14. Unified Visual Guide (Proposed Standards)

> These standards should be applied consistently across all pages and components.

### 14.1 CSS Variable Additions

```css
/* Add to site.css :root */
:root {
  /* Existing */
  --sidebar-width: 215px;
  --sidebar-width-collapsed: 80px;
  --top-navbar-height: 70px;
  --transition-speed: 0.3s;
  --app-bg: #f8f9fa;
  --surface: #ffffff;
  --surface-2: #f1f3f5;
  --text: #212529;
  --text-muted: #6c757d;
  --border: rgba(0,0,0,0.12);

  /* NEW - Shadows */
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.08);
  --shadow-md: 0 4px 15px rgba(0,0,0,0.1);
  --shadow-lg: 0 10px 30px rgba(0,0,0,0.15);

  /* NEW - Border Radius */
  --radius-sm: 6px;
  --radius-md: 10px;
  --radius-lg: 12px;
  --radius-xl: 15px;
  --radius-full: 50%;

  /* NEW - Sidebar Colors */
  --sidebar-bg: #2c3e50;
  --sidebar-text: #b3b3b3;
  --sidebar-text-active: #ffffff;
  --sidebar-hover-bg: rgba(255,255,255,0.1);
  --sidebar-accent: #20c997;
  --sidebar-header-gradient: linear-gradient(135deg, #198754 0%, #20c997 100%);

  /* NEW - Brand Colors */
  --brand-primary: #198754;
  --brand-secondary: #20c997;
  --brand-gradient: linear-gradient(135deg, var(--brand-primary) 0%, var(--brand-secondary) 100%);
}

[data-theme="dark"] {
  /* Existing */
  --app-bg: #0f1115;
  --surface: #161a22;
  --surface-2: #1d2230;
  --text: #e9edf3;
  --text-muted: rgba(233, 237, 243, 0.7);
  --border: rgba(255,255,255,0.14);

  /* NEW - Shadows in dark */
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.3);
  --shadow-md: 0 4px 15px rgba(0,0,0,0.4);
  --shadow-lg: 0 10px 30px rgba(0,0,0,0.55);

  /* NEW - Sidebar Dark */
  --sidebar-bg: #1a1f2e;
  --sidebar-text: rgba(255,255,255,0.6);
  --sidebar-text-active: #ffffff;
  --sidebar-hover-bg: rgba(255,255,255,0.08);
}
```

### 14.2 Unified Card Component

```css
/* Unified card classes in site.css */
.card-standard {
  border-radius: var(--radius-lg, 12px);
  border: 1px solid var(--border);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.card-standard:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-md);
}

.card-kpi {
  border-radius: var(--radius-lg, 12px);
  padding: 1.25rem;
  height: 100%;
  border: 1px solid var(--border);
  background: var(--surface);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.card-kpi:hover {
  transform: translateY(-3px);
  box-shadow: var(--shadow-md);
}

.card-kpi .kpi-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg, 12px);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
  flex-shrink: 0;
}

.card-kpi .kpi-value {
  font-size: 1.75rem;
  font-weight: 700;
  line-height: 1.2;
}

.card-kpi .kpi-label {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin-top: 2px;
}
```

### 14.3 Unified Tab Component

```css
/* Unified tab styles in site.css */
.nav-tabs-app {
  border-bottom: 2px solid var(--border);
  gap: 4px;
}

.nav-tabs-app .nav-link {
  border: none;
  border-radius: 8px 8px 0 0;
  padding: 10px 18px;
  font-weight: 500;
  color: var(--text-muted);
  transition: all 0.2s;
}

.nav-tabs-app .nav-link:hover {
  background: var(--surface-2);
  color: var(--text);
}

.nav-tabs-app .nav-link.active {
  color: var(--text);
  background: var(--surface);
  border-bottom: 2px solid var(--bs-primary);
  margin-bottom: -2px;
}

.nav-tabs-app .nav-link i {
  margin-right: 6px;
}
```

### 14.4 Unified Button Standards

```css
/* Add to site.css */
.btn-app {
  border-radius: var(--radius-sm, 6px);
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
  transition: all 0.2s ease;
}

.btn-app-sm {
  border-radius: var(--radius-sm, 6px);
  padding: 0.25rem 0.5rem;
  font-size: 0.8rem;
}

.btn-app-icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm, 6px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}

.btn-app-circle {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-full, 50%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0;
}
```

### 14.5 Standardized Modal Headers

Limit to **3 header types**:
```css
/* In site.css */
.modal-header-app {
  /* Default - no background color, standard close button */
}
.modal-header-app.modal-header-success {
  background: var(--brand-gradient);
  color: #fff;
}
.modal-header-app.modal-header-success .btn-close {
  filter: brightness(0) invert(1);
}
.modal-header-app.modal-header-danger {
  background: #dc3545;
  color: #fff;
}
.modal-header-app.modal-header-danger .btn-close {
  filter: brightness(0) invert(1);
}
```

### 14.6 Sidebar Dark Mode Fix

```css
/* Add to dark-theme.css */
[data-theme="dark"] .sidebar {
  background: var(--sidebar-bg) !important;
}

[data-theme="dark"] .sidebar-header {
  background: linear-gradient(135deg, #0d6efd 0%, #6610f2 100%) !important;
}

[data-theme="dark"] .sidebar-link {
  color: var(--sidebar-text);
}

[data-theme="dark"] .sidebar-link:hover,
[data-theme="dark"] .sidebar-link:focus {
  color: var(--sidebar-text-active);
  background: var(--sidebar-hover-bg);
}

[data-theme="dark"] .sidebar-link.active {
  color: var(--sidebar-text-active);
  background: var(--sidebar-hover-bg);
}

[data-theme="dark"] .sidebar-subitem {
  color: var(--sidebar-text);
}

[data-theme="dark"] .sidebar-subitem:hover {
  color: var(--sidebar-text-active);
}

[data-theme="dark"] .sidebar::-webkit-scrollbar-track {
  background: var(--sidebar-bg);
}

[data-theme="dark"] .sidebar::-webkit-scrollbar-thumb {
  background: rgba(255,255,255,0.2);
}
```

---

## 15. Refactoring Roadmap

### Phase 1 — Critical Fixes (Immediate)

| Priority | Task | Location |
|----------|------|----------|
| 🔴 Critical | Fix extra `</script>` tag | [`_Layout.cshtml:342`](AgroForm.Web/Views/Shared/_Layout.cshtml:342) |
| 🔴 Critical | Remove duplicate jQuery (3.7.0) from Admin | [`Administrador/Index.cshtml:741`](AgroForm.Web/Views/Administrador/Index.cshtml:741) |
| 🔴 Critical | Add sidebar dark mode overrides | `dark-theme.css` |
| 🔴 Critical | Fix missing `ph ` prefix on icons | [`Reporte/Aplicaciones.cshtml:241-244`](AgroForm.Web/Views/Reporte/Aplicaciones.cshtml:241) |

### Phase 2 — CSS Consolidation (Short-term)

| Priority | Task |
|----------|------|
| 🟠 High | Create `reports.css` — consolidate all `.kpi-card`, `.kpi-icon`, `.nav-tabs-app`, `.table-reporte` from inline styles |
| 🟠 High | Create `components.css` — consolidate `.btn-app`, `.card-standard`, modal header styles |
| 🟠 High | Define CSS variables for shadows, border-radius, sidebar colors |
| 🟠 High | Unify Admin page: use main layout, update DataTables to v2.1.8, switch to Phosphor Icons |
| 🟠 High | Extract moneda selector as shared partial view (currently duplicated in 3+ places) |

### Phase 3 — Component Standardization (Medium-term)

| Priority | Task |
|----------|------|
| 🟡 Medium | Extract repetitive cost input pattern in `ActividadRapida` to partial view |
| 🟡 Medium | Create reusable modal header partial (with configurable color type) |
| 🟡 Medium | Standardize all border-radius values to the 3-tier system |
| 🟡 Medium | Unify all tab variants into single `.nav-tabs-app` component |
| 🟡 Medium | Replace ApexCharts with Chart.js in Lluvia component for library unification |

### Phase 4 — Theme & Responsive (Long-term)

| Priority | Task |
|----------|------|
| 🟢 Low | Add `[data-theme="dark"]` overrides for all inline-styled components in report views |
| 🟢 Low | Remove dead code (commented-out welcome section, unused `.stat-card` class) |
| 🟢 Low | Implement typographic scale using CSS variables |
| 🟢 Low | Audit all pages at 768px, 992px breakpoints for responsive issues |

---

## 16. Specific Code Improvement Examples

### Example 1: Current inline KPI card (3 copies across reports)
```html
<!-- Current: Reporte/Campo.cshtml inline style -->
<style>
    .kpi-card {
        border-radius: 12px;
        padding: 20px;
        height: 100%;
        transition: transform 0.2s;
    }
    .kpi-card:hover { transform: translateY(-3px); }
</style>
```

**Should be** (in `site.css` or `reports.css`):
```css
.kpi-card {
    border-radius: var(--radius-lg);
    padding: 1.25rem;
    height: 100%;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
    border: 1px solid var(--border);
    background: var(--surface);
}
.kpi-card:hover {
    transform: translateY(-3px);
    box-shadow: var(--shadow-md);
}
```

### Example 2: Current scattered moneda selector pattern
Each instance has its own `cambiarMoneda()` JavaScript function. **Should be** a single partial view `_MonedaSelector.cshtml` with a shared JS function in `site.js`.

### Example 3: Current Admin theme switch (custom CSS toggle)
Replace with Bootstrap `form-switch` consistent with the main layout's approach, and use Phosphor Icons instead of Bootstrap Icons.

---

## 17. Responsive Design Observations

- **Sidebar**: Collapses properly at `991.98px` with overlay (`site.css:266-334`). Good implementation.
- **Navbar**: Reduces height to 60px at `768px` (`site.css:244-258`). Good.
- **Dashboard cards**: Use `col-xl-3 col-md-6` — 4 cards on desktop, 2 on tablet, 1 on mobile. Correct.
- **Admin page tabs**: Responsive via Bootstrap's card-header-tabs. May need horizontal scroll on very small screens.
- **Report views**: Most use `container-fluid` with responsive grid. Tab panes with charts may overflow on small screens (no specific responsive styles detected for chart containers).
- **ActividadRapida modal**: Uses `modal-xl` with 4-column row. On mobile, columns stack correctly via Bootstrap grid, but the form becomes very long (934 lines of content).

**Recommendation:** Add responsive-specific styles for chart containers in report views (`height: 300px` on desktop may need `height: 200px` on mobile).

---

## 18. Summary of Key Findings

| Category | Count | Severity |
|----------|-------|----------|
| Card patterns | 4+ distinct (`.stat-card`, `.kpi-card` × 3 variants, `.summary-card`, `.indicator-card`) | 🟠 High |
| Tab variants | 4+ (default, `.nav-tabs-custom` × 2, `.nav-tabs-reporte`, card-header-tabs) | 🟠 High |
| Modal header colors | 8+ different schemes | 🟡 Medium |
| Border-radius values | 10+ distinct values | 🟡 Medium |
| Shadow variations | 6+ distinct values | 🟡 Medium |
| Inline `<style>` total | ~900+ lines across all views | 🔴 Critical |
| HTML/JS errors | 4 found (extra `</script>`, duplicate jQuery, icon prefix missing, typo) | 🔴 Critical |
| Charting libraries | 2 (Chart.js + ApexCharts) | 🟢 Low |
| Icon systems | 2 (Phosphor + Bootstrap Icons) | 🟡 Medium |
| DataTables versions | 2 (2.1.8 vs 1.13.6) | 🟠 High |
| Bootstrap versions | 2 (5.3.3 vs 5.3.2) | 🟡 Medium |
| Missing dark sidebar | 1 major omission | 🔴 Critical |

---

## Conclusion

The AgroForm project has a **solid foundation** with Bootstrap 5.3+, CSS custom properties, and a dark theme system. However, the visual inconsistencies accumulated across views reduce the professional polish significantly. The **#1 priority** is consolidating the ~900+ lines of inline CSS into proper stylesheet files and fixing the critical HTML/JS errors. The **#2 priority** is unifying the card, tab, modal, and button components under a single Design System defined via CSS variables. The **#3 priority** is bringing the Admin page in line with the main layout (same Bootstrap version, same DataTables version, same icon system) and adding sidebar dark mode support.

Total estimated effort: **2-3 sprints** (1 sprint for critical fixes + CSS consolidation, 1 sprint for component standardization, 1 sprint for theme/responsive polish).
