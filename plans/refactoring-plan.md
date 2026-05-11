# AgroForm Design System — Plan de Refactorización Visual por Etapas

## Arquitectura actual (problemas detectados)

| Problema | Severidad | Impacto |
|----------|-----------|---------|
| ~900+ líneas de `<style>` inline en vistas | 🔴 Crítico | Sin reutilización, duplicación masiva |
| 4+ variantes de `.kpi-card` en reportes | 🟠 Alto | Misma clase, distintas implementaciones |
| 4+ variantes de tabs (`.nav-tabs-custom` ×2, `.nav-tabs-reporte`, card-header-tabs) | 🟠 Alto | Inconsistencia visual entre reportes |
| Bootstrap 5.3.3 vs 5.3.2 (Admin) | 🟡 Medio | Posibles conflictos |
| DataTables 2.1.8 vs 1.13.6 (Admin) | 🟠 Alto | APIs incompatibles |
| Sidebar sin dark mode | 🔴 Crítico | Ruptura visual en dark mode |
| 2 icon systems (Phosphor + Bootstrap Icons) | 🟡 Medio | Peso extra, inconsistencia |
| 2 chart libraries (Chart.js + ApexCharts) | 🟢 Bajo | Dependencia extra innecesaria |
| Extra `</script>` en `_Layout.cshtml:342` | 🔴 Crítico | Error HTML que rompe script parsing |
| jQuery duplicado en Admin (3.7.0 + 3.7.1) | 🔴 Crítico | Conflictos, peso duplicado |
| Typo `text-succsess` en CamposActividades:8 | 🟢 Bajo | Clase CSS inválida |
| Iconos sin prefijo `ph ` en Aplicaciones.cshtml:241-244 | 🟡 Medio | Iconos no se renderizan |

---

## FASE 1 — FUNDACIÓN DEL DESIGN SYSTEM

### Objetivo

Crear la infraestructura CSS reutilizable (`components.css`, `reports.css`) y las variables CSS globales **sin modificar ninguna vista todavía**.

### Archivos a crear

| Archivo | Propósito |
|---------|-----------|
| `AgroForm.Web/wwwroot/css/components.css` | Componentes reutilizables: cards, tabs, buttons, modals, tables, badges |
| `AgroForm.Web/wwwroot/css/reports.css` | Estilos específicos de reportes (KPI cards, timeline, score-rings, ranking badges, heatmap) |

### Archivos a modificar

| Archivo | Cambio |
|---------|--------|
| `AgroForm.Web/wwwroot/css/site.css` | Agregar nuevas CSS variables (shadows, radius, sidebar, brand) + clases `.nav-tabs-app`, `.btn-app` |
| `AgroForm.Web/Views/Shared/_Layout.cshtml` | Agregar `<link>` de `components.css` y `reports.css` después de `site.css` |

### 1.1 Nuevas variables CSS en `site.css`

Agregar AL FINAL del bloque `:root` (después de línea 15) y al final del bloque `[data-theme="dark"]` (después de línea 24):

```css
/* === DESIGN SYSTEM TOKENS (add after line 15) === */
:root {
  /* Shadows */
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.08);
  --shadow-md: 0 4px 15px rgba(0,0,0,0.1);
  --shadow-lg: 0 10px 30px rgba(0,0,0,0.15);

  /* Border Radius */
  --radius-sm: 6px;
  --radius-md: 10px;
  --radius-lg: 12px;
  --radius-xl: 15px;
  --radius-full: 50%;

  /* Sidebar */
  --sidebar-bg: #2c3e50;
  --sidebar-text: #b3b3b3;
  --sidebar-text-active: #ffffff;
  --sidebar-hover-bg: rgba(255,255,255,0.1);
  --sidebar-accent: #20c997;
  --sidebar-header-gradient: linear-gradient(135deg, #198754 0%, #20c997 100%);

  /* Brand */
  --brand-primary: #198754;
  --brand-secondary: #20c997;
  --brand-gradient: linear-gradient(135deg, var(--brand-primary) 0%, var(--brand-secondary) 100%);
}

[data-theme="dark"] {
  /* Shadows - darker for dark mode */
  --shadow-sm: 0 2px 4px rgba(0,0,0,0.3);
  --shadow-md: 0 4px 15px rgba(0,0,0,0.4);
  --shadow-lg: 0 10px 30px rgba(0,0,0,0.55);

  /* Sidebar dark colors */
  --sidebar-bg: #1a1f2e;
  --sidebar-text: rgba(255,255,255,0.6);
  --sidebar-text-active: #ffffff;
  --sidebar-hover-bg: rgba(255,255,255,0.08);
}
```

### 1.2 Refactor `site.css` — Sidebar usar variables

En `site.css:42`, cambiar:
```css
background: #2c3e50 !important;
```
→
```css
background: var(--sidebar-bg) !important;
```

En `site.css:67`, cambiar:
```css
background: linear-gradient(135deg, #198754 0%, #20c997 100%) !important;
```
→
```css
background: var(--sidebar-header-gradient) !important;
```

En `site.css:83`, cambiar:
```css
color: #b3b3b3;
```
→
```css
color: var(--sidebar-text);
```

En `site.css:93,99`, cambiar:
```css
border-left-color: #20c997;
```
→
```css
border-left-color: var(--sidebar-accent);
```

### 1.3 Nuevas clases `.card-app`, `.card-kpi` en `components.css`

```css
/* ==========================================================================
   components.css — Reusable UI Components
   Dependencies: site.css (CSS variables)
   Load AFTER site.css
   ========================================================================== */

/* --------------------------------------------------------------------------
   1. CARDS
   -------------------------------------------------------------------------- */
.card-app {
  border-radius: var(--radius-lg, 12px);
  border: 1px solid var(--border);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.card-app:hover {
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

.card-kpi .kpi-trend {
  font-size: 0.8rem;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

/* --------------------------------------------------------------------------
   2. TABS
   -------------------------------------------------------------------------- */
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

/* --------------------------------------------------------------------------
   3. BUTTONS
   -------------------------------------------------------------------------- */
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

/* --------------------------------------------------------------------------
   4. MODAL HEADERS
   -------------------------------------------------------------------------- */
.modal-header-app {
  /* Default - no background, standard close */
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

.modal-header-app.modal-header-info {
  background: #0dcaf0;
  color: #fff;
}

.modal-header-app.modal-header-info .btn-close {
  filter: brightness(0) invert(1);
}

/* --------------------------------------------------------------------------
   5. TABLES
   -------------------------------------------------------------------------- */
.table-app th {
  font-weight: 600;
  font-size: 0.8rem;
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
  background: var(--surface-2);
  border-bottom: 2px solid var(--border);
}

.table-app td {
  vertical-align: middle;
  font-size: 0.875rem;
}

/* --------------------------------------------------------------------------
   6. BADGES & METRICS
   -------------------------------------------------------------------------- */
.metric-value {
  font-size: 1.75rem;
  font-weight: 700;
  line-height: 1.2;
}

.metric-label {
  font-size: 0.85rem;
  color: var(--text-muted);
}

/* --------------------------------------------------------------------------
   7. UTILITY HELPERS
   -------------------------------------------------------------------------- */
.shadow-app-sm { box-shadow: var(--shadow-sm); }
.shadow-app-md { box-shadow: var(--shadow-md); }
.shadow-app-lg { box-shadow: var(--shadow-lg); }

.radius-app-sm { border-radius: var(--radius-sm); }
.radius-app-md { border-radius: var(--radius-md); }
.radius-app-lg { border-radius: var(--radius-lg); }
.radius-app-full { border-radius: var(--radius-full); }
```

### 1.4 Nuevo `reports.css`

```css
/* ==========================================================================
   reports.css — Report-specific styles (KPI cards, timeline, score rings, etc.)
   Dependencies: site.css (CSS variables), components.css (card-kpi)
   Load AFTER components.css
   ========================================================================== */

/* --------------------------------------------------------------------------
   1. KPI CARDS (unified — replaces all inline .kpi-card variants)
   -------------------------------------------------------------------------- */
/* Already in components.css as .card-kpi. This file uses that base. */

/* --------------------------------------------------------------------------
   2. TIMELINE (Reporte/Campo)
   -------------------------------------------------------------------------- */
.timeline {
  position: relative;
  padding: 20px 0;
}

.timeline::before {
  content: '';
  position: absolute;
  left: 28px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: var(--border);
}

.timeline-item {
  position: relative;
  padding-left: 70px;
  margin-bottom: 20px;
}

.timeline-icon {
  position: absolute;
  left: 14px;
  width: 30px;
  height: 30px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-size: 14px;
  z-index: 1;
}

.timeline-content {
  padding: 12px 16px;
  border-radius: var(--radius-sm, 6px);
  border: 1px solid var(--border);
  background: var(--surface);
}

/* --------------------------------------------------------------------------
   3. SCORE RINGS (Reporte/Campo)
   -------------------------------------------------------------------------- */
.score-ring {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
  font-weight: 700;
  position: relative;
}

.score-excellent { background: conic-gradient(#28a745 var(--pct), var(--border) var(--pct)); }
.score-good      { background: conic-gradient(#17a2b8 var(--pct), var(--border) var(--pct)); }
.score-regular   { background: conic-gradient(#ffc107 var(--pct), var(--border) var(--pct)); }
.score-critical  { background: conic-gradient(#dc3545 var(--pct), var(--border) var(--pct)); }

.score-ring-inner {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: var(--surface);
  display: flex;
  align-items: center;
  justify-content: center;
}

/* --------------------------------------------------------------------------
   4. SOIL BARS (Reporte/Campo)
   -------------------------------------------------------------------------- */
.soil-bar {
  height: 8px;
  border-radius: 4px;
  background: var(--border);
  overflow: hidden;
}

.soil-bar-fill {
  height: 100%;
  border-radius: 4px;
  transition: width 0.6s ease;
}

/* --------------------------------------------------------------------------
   5. RANKING BADGES (Reporte/Cosecha, Comparativa)
   -------------------------------------------------------------------------- */
.ranking-badge {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.8rem;
}

.ranking-1 {
  background: linear-gradient(135deg, #FFD700, #FFA000);
  color: #fff;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 14px;
}

.ranking-2 {
  background: linear-gradient(135deg, #C0C0C0, #9E9E9E);
  color: #fff;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 14px;
}

.ranking-3 {
  background: linear-gradient(135deg, #CD7F32, #8D6E63);
  color: #fff;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 14px;
}

/* --------------------------------------------------------------------------
   6. HEATMAP (ComparativaCampos)
   -------------------------------------------------------------------------- */
.heatmap-cell {
  padding: 0.5rem;
  text-align: center;
  border-radius: 4px;
  font-weight: 600;
  font-size: 0.85rem;
}

.heatmap-high    { background: #1a9850; color: white; }
.heatmap-mid-high{ background: #91cf60; color: #333; }
.heatmap-mid     { background: #fee08b; color: #333; }
.heatmap-mid-low { background: #fc8d59; color: #333; }
.heatmap-low     { background: #d73027; color: white; }
.heatmap-none    { background: #e9ecef; color: #999; }

/* --------------------------------------------------------------------------
   7. KPI TRENDS (Cosecha)
   -------------------------------------------------------------------------- */
.kpi-trend-up    { color: #28a745; }
.kpi-trend-down  { color: #dc3545; }
.kpi-trend-stable{ color: #ffc107; }

/* --------------------------------------------------------------------------
   8. INDICATOR CARDS (Cosecha)
   -------------------------------------------------------------------------- */
.indicator-card {
  border-left: 4px solid;
}
```

### 1.5 Agregar `<link>` en `_Layout.cshtml`

Después de la línea existente de `dark-theme.css` (~line 14), agregar:

```html
<link rel="stylesheet" href="~/css/components.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/reports.css" asp-append-version="true" />
```

### 1.6 Naming conventions definidas

| Prefix | Tipo | Ejemplo |
|--------|------|---------|
| `.card-*` | Cards | `.card-app`, `.card-kpi`, `.card-hover` |
| `.nav-tabs-app` | Tabs | `.nav-tabs-app` (único tab system) |
| `.btn-app*` | Buttons | `.btn-app`, `.btn-app-sm`, `.btn-app-icon`, `.btn-app-circle` |
| `.table-app` | Tables | `.table-app` |
| `.modal-header-app` | Modals | `.modal-header-app.modal-header-success` |
| `.metric-*` | Metrics | `.metric-value`, `.metric-label` |
| `.shadow-app-*` | Shadows | `.shadow-app-sm`, `.shadow-app-md`, `.shadow-app-lg` |
| `.radius-app-*` | Radius | `.radius-app-sm`, `.radius-app-md`, `.radius-app-lg` |

**Reglas:**
- Preferir Bootstrap utilities cuando sea posible (`shadow-sm`, `rounded-*`, `bg-*`, `text-*`, `d-flex`, `gap-*`)
- Usar clases custom solo cuando Bootstrap no tenga equivalente
- Nunca mezclar `.nav-tabs-custom`, `.nav-tabs-reporte` — usar siempre `.nav-tabs-app`
- Nunca crear nuevas variantes de KPI card — usar siempre `.card-kpi`
- Una clase por propósito, sin duplicación

### Riesgos Fase 1

- ⚠️ Las nuevas clases no rompen nada porque solo se **agregan**, no se reemplazan aún
- ⚠️ Verificar que `components.css` y `reports.css` se carguen en el orden correcto: `site.css` → `dark-theme.css` → `components.css` → `reports.css`
- ⚠️ Las variables `--sidebar-bg` ya existen en `site.css:42` — al reemplazar `background: #2c3e50` por `var(--sidebar-bg)`, el valor visual NO cambia (sigue siendo `#2c3e50`), solo se desacopla para permitir dark mode después

---

## FASE 2 — CONSOLIDACIÓN DE CSS INLINE

### Objetivo

Eliminar los ~900+ lines de `<style>` inline de las vistas, migrando todo a `components.css` y `reports.css`.

### Archivos a modificar (eliminar inline CSS)

| Vista | Líneas inline | Clases a migrar | Destino |
|-------|---------------|-----------------|---------|
| `Reporte/Campo.cshtml` | 244 (líneas 5-248) | `.kpi-card`, `.kpi-icon`, `.timeline-*`, `.score-*`, `.soil-bar`, `.nav-tabs-custom` | `reports.css` |
| `Reporte/Cosecha.cshtml` | 243 (líneas 6-248) | `.kpi-card`, `.kpi-icon`, `.nav-tabs-custom`, `.ranking-*`, `.kpi-trend-*`, `.indicator-card` | `reports.css` |
| `Reporte/Aplicaciones.cshtml` | 228 (líneas 6-233) | `.kpi-card`, `.kpi-icon`, `.kpi-value`, `.kpi-label`, `.kpi-trend`, `.nav-tabs-reporte`, `.table-reporte` | `reports.css` |
| `Reporte/ComparativaCampos.cshtml` | 92 (líneas 6-92) | `.summary-card`, `.icon-circle`, `.ranking-badge`, `.ranking-*`, `.heatmap-*`, `.table-compact`, `.filter-section` | `reports.css` |
| `Administrador/Index.cshtml` | 72 (líneas 22-94) | `.theme-switch`, `.theme-switch-*`, `.cultivo-color-*` | `components.css` |
| `Home/Index.cshtml` | 10 (líneas 225-235) | `.btn-icon` | `components.css` (ya existe `.btn-app-icon`) |
| `MenuLateral/Default.cshtml` | 25 | sidebar flex | Revisar si aún necesario |

### 2.1 Plan de migración por vista (orden seguro)

#### Reporte/Campo.cshtml
**Acción:** Eliminar líneas 5-248 completas (todo el `<style>` block).
**Riesgo:** Ninguno si `reports.css` tiene todas las clases. Verificar que las clases usadas en HTML sean exactamente las mismas.
**HTML usa:** `kpi-card`, `kpi-icon`, `timeline`, `timeline-item`, `timeline-icon`, `timeline-content`, `score-ring`, `score-excellent/good/regular/critical`, `score-ring-inner`, `soil-bar`, `nav-tabs-custom`.
**Observación:** `nav-tabs-custom` debe reemplazarse por `nav-tabs-app` en el HTML (Fase 3). Por ahora mantener clase original pero definida en `reports.css`.

#### Reporte/Cosecha.cshtml
**Acción:** Eliminar líneas 6-248 completas.
**Riesgo:** El fix de tab-pane stacking (líneas 6-21) usa `!important` — debe preservarse pero moverse a `reports.css`.
**HTML usa:** `kpi-card`, `kpi-icon`, `nav-tabs-custom`, `ranking-1/2/3`, `kpi-trend-up/down/stable`, `indicator-card`.
**Observación:** `.indicator-card` actualmente no tiene estilo propio además de `border-left: 4px solid` — se define en `reports.css`.

#### Reporte/Aplicaciones.cshtml
**Acción:** Eliminar líneas 6-233 completas.
**Riesgo:** `.nav-tabs-reporte` es diferente de `.nav-tabs-custom` — unificar en `.nav-tabs-app` en Fase 3. Por ahora mantener clases originales en `reports.css`.
**HTML usa:** `kpi-card`, `kpi-icon`, `kpi-value`, `kpi-label`, `kpi-trend`, `table-reporte`, `nav-tabs-reporte`, `tipo-badge`.

#### Reporte/ComparativaCampos.cshtml
**Acción:** Eliminar líneas 6-92 completas.
**Riesgo:** `.summary-card` tiene padding específico (`.card-body { padding: 1rem 1.25rem }`) — preservar en reports.css.
**HTML usa:** `summary-card`, `icon-circle`, `ranking-badge`, `ranking-1/2/3`, `heatmap-*`, `table-compact`, `filter-section`, `chart-container`.

#### Administrador/Index.cshtml
**Acción:** Eliminar líneas 22-94 completas.
**Riesgo:** El `.theme-switch` es un controlador de tema completo — al migrar a Bootstrap `form-switch` (Fase 3) se eliminará. Por ahora mover a `components.css`.
**HTML usa:** `theme-switch`, `theme-switch-label`, `theme-switch-input`, `theme-switch-slider`, `cultivo-color-box`, `cultivo-color-display`.

#### Home/Index.cshtml
**Acción:** Eliminar líneas 225-235 completas.
**Riesgo:** Mínimo — `.btn-icon` se reemplaza por `.btn-app-icon` de `components.css`.
**HTML usa:** `btn-icon` → cambiar a `btn-app-icon`.

### 2.2 Orden de migración recomendado

1. **Home/Index.cshtml** (más simple, 10 líneas, bajo riesgo) ✅
2. **ComparativaCampos.cshtml** (92 líneas, clases únicas, riesgo medio) 
3. **Campo.cshtml** (244 líneas, pero clases ya estandarizadas en reports.css)
4. **Cosecha.cshtml** (243 líneas, similar a Campo)
5. **Aplicaciones.cshtml** (228 líneas, tiene `.table-reporte` único)
6. **Administrador/Index.cshtml** (72 líneas, tiene el theme switch custom)
7. **MenuLateral/Default.cshtml** (25 líneas) y otros componentes menores

### Riesgos Fase 2

- ⚠️ **CRÍTICO:** Cada vista debe verificarse individualmente después de eliminar su `<style>` inline. NO eliminar todos de una vez.
- ⚠️ Algunas vistas pueden tener estilos inline que **no están en el `<style>`** sino en atributos `style=""` — esos deben migrarse a clases.
- ⚠️ `!important` en Cosecha.cshtml (tab-pane fix) sugiere un problema de z-index que debería resolverse en el layout, no en reports.css.

---

## FASE 3 — ESTANDARIZACIÓN VISUAL

### Objetivo

Hacer que todo el sistema visual sea coherente: unificar variantes de componentes, eliminar clases duplicadas, estandarizar iconografía y espaciado.

### 3.1 Unificar tabs

| Clase actual | Vistas donde aparece | Nueva clase |
|-------------|---------------------|-------------|
| `.nav-tabs-custom` (Campo) | `Campo.cshtml` | `.nav-tabs-app` |
| `.nav-tabs-custom` (Cosecha, diferente) | `Cosecha.cshtml` | `.nav-tabs-app` |
| `.nav-tabs-reporte` | `Aplicaciones.cshtml` | `.nav-tabs-app` |
| `card-header-tabs` | `Administrador/Index.cshtml` | `.nav-tabs-app` |
| Bootstrap default tabs | Layout modals | `.nav-tabs-app` (opcional) |

**Acción:** Reemplazar en HTML todas las ocurrencias de `nav-tabs-custom`, `nav-tabs-reporte` por `nav-tabs-app`. Mantener la estructura Bootstrap (`nav nav-tabs`).

### 3.2 Unificar KPI cards

| Clase actual | Vistas | Nueva clase |
|-------------|--------|-------------|
| `.kpi-card` (3 variantes inline) | Campo, Cosecha, Aplicaciones | `.card-kpi` |
| `.summary-card` | ComparativaCampos | `.card-kpi` |
| `.stat-card` | site.css (no usado en HTML) | Eliminar o convertir a `.card-kpi` |

**Acción:** Migrar HTML a `.card-kpi`. Ajustar si es necesario algún padding específico (`.summary-card` usa `1rem 1.25rem` en card-body).

### 3.3 Unificar modal headers

| Color actual | Nueva clase |
|-------------|-------------|
| `bg-success text-white` | `modal-header-app modal-header-success` |
| `bg-primary text-white` → info/danger/success? | Decidir: info → `modal-header-info`, danger → `modal-header-danger` |
| `bg-info text-white` | `modal-header-info` |
| `bg-warning text-dark` | Mantener como excepción o usar `modal-header-warning` |
| `bg-danger text-white` | `modal-header-danger` |
| `bg-secondary text-white` | Default (sin clase, solo `modal-header-app`) |
| `bg-dark text-white` | Default |
| Custom `#8B5E3C` (SiloBolsa) | Evaluar si merece color propio |

**Propuesta:** Reducir a 3 tipos: `success` (acciones positivas), `info` (información), `danger` (eliminación/advertencia). Los demás usar el header default (sin color).

### 3.4 Unificar iconografía

**Acción:** Reemplazar Bootstrap Icons (`bi bi-*`) por Phosphor Icons (`ph ph-*`) en `Administrador/Index.cshtml`.

**Archivo:** `Administrador/Index.cshtml`
- Buscar: `bi bi-sun-fill` → `ph ph-sun`
- Buscar: `bi bi-moon-stars-fill` → `ph ph-moon`
- Eliminar CDN de Bootstrap Icons: `<link rel="stylesheet" href="...bootstrap-icons...">` (línea 14)

### 3.5 Estandarizar border-radius

Aplicar las variables CSS en lugar de valores hardcodeados en todo el CSS:

| Valor actual | Variable | Dónde se aplica |
|-------------|----------|-----------------|
| `4px` | `--radius-sm` (6px) | Heatmap cells — subir a 6px |
| `6px` | `--radius-sm` | ✅ Ya consistente |
| `8px` | `--radius-sm` (6px) | Timeline-content, pagination — cambiar a 6px |
| `10px` | `--radius-md` | ✅ Ya consistente |
| `12px` | `--radius-lg` | ✅ Ya consistente |
| `15px` | `--radius-xl` | Welcome section |
| `50%` | `--radius-full` | ✅ Ya consistente |

### 3.6 Unificar moneda selector

**Crear:** `AgroForm.Web/Views/Shared/_MonedaSelector.cshtml`

Contenido del partial:
```html
@* Partial view: _MonedaSelector.cshtml *@
@* Usage: <partial name="_MonedaSelector" /> *@
<div class="d-flex align-items-center gap-2">
  <label class="form-label mb-0 small fw-semibold">Moneda:</label>
  <select class="form-select form-select-sm moneda-selector" style="width: auto;" onchange="cambiarMoneda(this.value)">
    <option value="ARS" selected>🇦🇷 ARS ($)</option>
    <option value="USD">🇺🇸 USD (U$S)</option>
  </select>
</div>
```

**Archivo JS compartido:** Agregar en `site.js` (o `layout.js`):
```javascript
// Shared moneda selector function
function cambiarMoneda(moneda) {
  // Dispatch custom event for tables/listeners
  document.dispatchEvent(new CustomEvent('monedaChanged', { detail: { moneda } }));
}
```

**Reemplazar** en:
- `TablaGastos/Default.cshtml` (línea 14) — `<partial name="_MonedaSelector" />`
- `Reporte/ComparativaCampos.cshtml` — `<partial name="_MonedaSelector" />`
- `Actividad/Index.cshtml` — `<partial name="_MonedaSelector" />`

### Riesgos Fase 3

- ⚠️ Cambiar clases en HTML requiere **prueba visual de cada vista**
- ⚠️ `modal-header-app` + clase de color requiere cambiar tanto el `<style>` como el HTML del modal-header
- ⚠️ Al eliminar Bootstrap Icons de Admin, verificar que no haya otros `bi-*` en la página
- ⚠️ El partial `_MonedaSelector` debe estar accesible desde todas las vistas (Shared)

---

## FASE 4 — DARK THEME CONSISTENTE

### Objetivo

Cubrir todos los gaps de dark mode: sidebar, reportes, componentes inline, Select2, DataTables, Choices.js.

### 4.1 Sidebar dark mode (AGREGAR en `dark-theme.css`)

```css
/* ==========================================================================
   14. SIDEBAR DARK MODE
   ========================================================================== */
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

### 4.2 Report views dark mode (AGREGAR en `dark-theme.css`)

```css
/* ==========================================================================
   15. REPORT VIEWS DARK MODE
   ========================================================================== */

/* Summary cards (Comparativa) */
[data-theme="dark"] .summary-card {
  background: var(--surface);
  border-color: var(--border);
}

/* Filter section (Comparativa) */
[data-theme="dark"] .filter-section {
  background: var(--surface-2);
}

/* Table compact (Comparativa) */
[data-theme="dark"] .table-compact th:hover {
  background-color: var(--surface-2);
}

[data-theme="dark"] .table-compact th.sort-asc,
[data-theme="dark"] .table-compact th.sort-desc {
  background-color: rgba(25, 135, 84, 0.15);
  color: #20c997;
}

/* Heatmap (Comparativa) */
[data-theme="dark"] .heatmap-none {
  background: var(--surface-2);
  color: var(--text-muted);
}

/* Nav tabs app dark mode */
[data-theme="dark"] .nav-tabs-app .nav-link:hover {
  background: var(--surface-2);
}

[data-theme="dark"] .nav-tabs-app .nav-link.active {
  background: var(--surface);
}

/* KPI trends */
[data-theme="dark"] .kpi-trend-up { color: #4caf50; }
[data-theme="dark"] .kpi-trend-down { color: #f44336; }
[data-theme="dark"] .kpi-trend-stable { color: #ffc107; }

/* Score rings */
[data-theme="dark"] .score-excellent { background: conic-gradient(#4caf50 var(--pct), var(--border) var(--pct)); }
[data-theme="dark"] .score-good { background: conic-gradient(#17a2b8 var(--pct), var(--border) var(--pct)); }
[data-theme="dark"] .score-regular { background: conic-gradient(#ffc107 var(--pct), var(--border) var(--pct)); }
[data-theme="dark"] .score-critical { background: conic-gradient(#dc3545 var(--pct), var(--border) var(--pct)); }

/* Ranking badges */
[data-theme="dark"] .ranking-1 { background: linear-gradient(135deg, #FFD700, #FFA000); }
[data-theme="dark"] .ranking-2 { background: linear-gradient(135deg, #C0C0C0, #9E9E9E); }
[data-theme="dark"] .ranking-3 { background: linear-gradient(135deg, #CD7F32, #8D6E63); }
```

### 4.3 Corregir Select2 inline en ActividadRapida

**Archivo:** `ActividadRapida/Default.cshtml` (líneas 9-16)
**Acción:** Eliminar el `<style>` inline de Select2 de este componente (usa `#e9ecef`, `#0d6efd` hardcodeados). El `dark-theme.css` ya tiene reglas para Select2 (secciones 12).

```html
<!-- ELIMINAR este bloque: -->
<style>
    .select2-container--bootstrap-5 .select2-selection {
        background-color: #e9ecef !important;
    }
    .select2-results__option--highlighted {
        background-color: #0d6efd !important;
    }
</style>
```

### Riesgos Fase 4

- ⚠️ Las `conic-gradient()` en score rings no son triviales de adaptar a dark mode — las variables `--pct` deben mantenerse
- ⚠️ Verificar que los colores de ranking badges tengan suficiente contraste en dark mode
- ⚠️ El sidebar dark mode cambia el header a azul/violeta (en lugar del verde original) — decisión de diseño intencional para marcar cambio de tema

---

## FASE 5 — LIMPIEZA ARQUITECTÓNICA

### Objetivo

Reducir deuda técnica: eliminar CSS muerto, unificar versiones de librerías, eliminar dependencias duplicadas.

### 5.1 Unificar versiones

| Librería | Versión objetivo | CDN |
|----------|-----------------|-----|
| Bootstrap | 5.3.3 (la del Layout) | `https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css` |
| DataTables | 2.1.8 (la del Layout) | `https://cdn.datatables.net/2.1.8/css/dataTables.bootstrap5.min.css` |
| DataTables Buttons | 3.1.2 (la del Layout) | `https://cdn.datatables.net/buttons/3.1.2/css/buttons.bootstrap5.min.css` |
| jQuery | 3.7.1 (una sola) | `https://code.jquery.com/jquery-3.7.1.min.js` |
| Icon system | Phosphor Icons (solo) | Eliminar Bootstrap Icons |
| Chart library | Chart.js 4.4.0 (unificar) | Reemplazar ApexCharts |

### 5.2 Corregir errores HTML/JS

| Error | Archivo | Línea | Acción |
|-------|---------|-------|--------|
| Extra `</script>` | `_Layout.cshtml` | 342 | **Eliminar línea 342** (`</script>` sobrante) |
| jQuery duplicado | `Administrador/Index.cshtml` | 741, 744 | **Eliminar línea 741** (jQuery 3.7.0), mantener 744 (3.7.1) |
| Typo `text-succsess` | `CamposActividades/Default.cshtml` | 8 | Cambiar `text-succsess` → `text-success` |
| Missing `ph ` prefix | `Aplicaciones.cshtml` | 241-244 | Cambiar `ph-file-xls` → `ph ph-file-xls`, `ph-file-pdf` → `ph ph-file-pdf` |

### 5.3 Migrar Admin a Layout principal

**Objetivo:** Que `Administrador/Index.cshtml` use el mismo layout que el resto del sistema.
**Acción:** Cambiar `Layout = null` → `Layout = "~/Views/Shared/_Layout.cshtml"` y eliminar la estructura HTML duplicada (`<!DOCTYPE>`, `<html>`, `<head>`, `<body>`).

**Riesgo:** ⚠️ ALTO — Admin tiene su propio theme switch, su propio navbar y estructura de página. Requiere refactorizar la página para que se integre con el sidebar y navbar del layout. Podría postergarse a una fase posterior si el riesgo es muy alto.

### 5.4 Reemplazar ApexCharts por Chart.js

**Archivo:** `Lluvia/Default.cshtml`
**Acción:** Convertir el gráfico de ApexCharts a Chart.js.
**Referencia:** Usar el mismo patrón que los reportes (Chart.js 4.4.0 ya cargado en el layout).
**Ventaja:** Eliminar una dependencia (ApexCharts CSS + JS), ~100KB menos.

### 5.5 Eliminar CSS muerto

| Código | Archivo | Razón |
|--------|---------|-------|
| `.stat-card` | `site.css:385-396` | No se usa en ninguna vista (Home usa `card card-hover border-0 shadow-sm`) |
| `.stat-number` | `site.css:398-402` | Solo se usa con `.stat-card` |
| `.stat-label` | `site.css:404-407` | Solo se usa con `.stat-card` |
| Welcome section comentado | `Home/Index.cshtml:10-28` | Código muerto |
| `actividad-icono` | `site.css:430-439` | Verificar si se usa |

**Acción:** Marcar para eliminación después de verificar que ninguna vista los referencia.

### 5.6 Estructura final propuesta

```
/wwwroot/css/
    site.css        ← Variables globales, layout, sidebar, navbar, helpers
    components.css  ← Cards, tabs, buttons, modals, tables, badges, utilities
    reports.css     ← Timeline, score rings, heatmap, ranking, KPI trends
    dark-theme.css  ← All [data-theme="dark"] overrides
```

---

## FASE 6 — WORKFLOW DE USO Y DOCUMENTACIÓN

### Objetivo

Documentar cómo y cuándo usar cada componente, para que el equipo nunca se salga del Design System.

### 6.1 Reglas de uso de componentes

#### Cards
```
✅ Usar Bootstrap utilities:
   <div class="card shadow-sm border-0"> → Card simple sin hover

✅ Usar .card-kpi para KPIs:
   <div class="card-kpi">...</div> → Card con hover, sin necesidad de .card

✅ Usar .card-app para cards con hover:
   <div class="card card-app">...</div> → Extiende Bootstrap .card

❌ NO crear nuevas clases para cards
❌ NO redefinir .kpi-card en <style> inline
❌ NO usar .stat-card, .summary-card — usar .card-kpi
```

#### Tabs
```
✅ Usar siempre .nav-tabs-app:
   <ul class="nav nav-tabs nav-tabs-app" role="tablist">

✅ Mantener estructura Bootstrap:
   <li class="nav-item" role="presentation">
     <button class="nav-link active" ...>

❌ NO crear nav-tabs-custom, nav-tabs-reporte
❌ NO redefinir tabs en <style> inline
```

#### Buttons
```
✅ Usar Bootstrap utilities para botones estándar:
   btn btn-primary, btn btn-success, btn btn-sm

✅ Usar .btn-app-icon para icon buttons:
   <button class="btn btn-outline-secondary btn-app-icon">

✅ Usar .btn-app-sm para botones compactos:
   <button class="btn btn-primary btn-app-sm">

❌ NO crear .btn-icon con border-radius: 6px
❌ NO crear .btn-circle con border-radius: 50% (usar .btn-app-circle)
```

#### Modals
```
✅ Usar .modal-header-app para headers estándar
✅ Agregar modal-header-success/info/danger solo si aplica
✅ Mantener .btn-close para cerrar (ya tiene dark mode support)
✅ Usar siempre btn-close-white con headers coloreados (automático)

❌ NO usar bg-* text-white en modal-header (usar modal-header-app + variante)
❌ NO crear colores nuevos de modal header
```

#### Tables
```
✅ Usar Bootstrap utilities para tablas estándar:
   table table-striped table-hover

✅ Usar .table-app para tablas de reportes:
   <table class="table table-app">

❌ NO crear .table-reporte, .table-compact
❌ NO redefinir estilos de tabla en <style> inline
```

#### Iconos
```
✅ Usar siempre Phosphor Icons con prefijo ph:
   <i class="ph ph-*"></i>

✅ Usar fs-* para tamaño (fs-4, fs-5, etc.)
✅ Usar text-* para color

❌ NO usar Bootstrap Icons
❌ NO olvidar el prefijo ph
❌ NO usar style="font-size: Npx" en icons
```

### 6.2 Cuándo usar Bootstrap vs clases custom

| Situación | Usar |
|-----------|------|
| Layout/spacing | Bootstrap grid + utilities (`d-flex`, `gap-*`, `p-*`, `m-*`) |
| Colors | Bootstrap (`text-success`, `bg-primary`, `bg-opacity-*`) |
| Buttons (standard) | Bootstrap (`btn btn-primary`) |
| Buttons (icon) | `.btn-app-icon` |
| Buttons (circle) | `.btn-app-circle` |
| Cards (simple) | Bootstrap (`card shadow-sm`) |
| Cards (KPI/metric) | `.card-kpi` |
| Tabs | `.nav-tabs-app` |
| Tables (simple) | Bootstrap (`table table-striped`) |
| Tables (report) | `.table-app` |
| Modals (header) | `.modal-header-app` |
| Shadows | CSS variables (`var(--shadow-sm)`) o `.shadow-app-*` |
| Border radius | CSS variables (`var(--radius-md)`) o `.radius-app-*` |
| Typography | Bootstrap (`fs-*`, `fw-bold`, `text-muted`) |

### 6.3 Checklist de consistencia visual (para code review)

- [ ] ¿Usa Bootstrap utilities en lugar de CSS custom cuando es posible?
- [ ] ¿Usa `.card-kpi` en lugar de clases inline de KPI?
- [ ] ¿Usa `.nav-tabs-app` en lugar de tab variants?
- [ ] ¿Usa `var(--radius-*)` en lugar de valores hardcodeados?
- [ ] ¿Usa `var(--shadow-*)` en lugar de valores hardcodeados?
- [ ] ¿Tiene dark mode support (o al menos no rompe)?
- [ ] ¿No tiene `<style>` inline nuevo?
- [ ] ¿No duplica clases existentes?
- [ ] ¿Usa Phosphor Icons con prefijo `ph`?
- [ ] ¿Usa `btn-close-white` en modal headers coloreados?
- [ ] ¿Sigue responsive (prueba en 768px y 992px)?
- [ ] ¿No agrega `!important` innecesario?

### 6.4 Proceso de contribución

```
1. Identificar qué componente se necesita
2. Revisar si existe en Bootstrap o en components.css
3. Si existe, usarlo tal cual
4. Si NO existe:
   a. Evaluar si realmente es necesario o si Bootstrap cubre el caso
   b. Si es necesario, agregar la clase en components.css o reports.css
   c. Usar CSS variables existentes (no hardcodear)
   d. Agregar dark mode override en dark-theme.css
   e. Documentar en este workflow
5. NO agregar <style> inline en la vista
6. NO duplicar clases
7. NO hardcodear valores que ya existen como variables
```

---

## RESUMEN DE ARCHIVOS A MODIFICAR POR FASE

| Fase | Archivos a crear | Archivos a modificar |
|------|-----------------|---------------------|
| **Fase 1** | `components.css`, `reports.css` | `site.css`, `_Layout.cshtml` |
| **Fase 2** | — | `Campo.cshtml`, `Cosecha.cshtml`, `Aplicaciones.cshtml`, `ComparativaCampos.cshtml`, `Administrador/Index.cshtml`, `Home/Index.cshtml` |
| **Fase 3** | `_MonedaSelector.cshtml` | Todas las vistas con tabs, cards, modals, Admin theme switch, icon classes |
| **Fase 4** | — | `dark-theme.css`, `ActividadRapida/Default.cshtml` |
| **Fase 5** | — | `_Layout.cshtml`, `Administrador/Index.cshtml`, `Lluvia/Default.cshtml`, `CamposActividades/Default.cshtml`, `Aplicaciones.cshtml`, `site.css` |
| **Fase 6** | Este documento + wiki | Ninguno |

## RIESGOS GLOBALES

| Riesgo | Probabilidad | Mitigación |
|--------|-------------|------------|
| Romper una vista al eliminar su `<style>` inline | Alta | Probar cada vista individualmente después de cada cambio |
| Clase CSS no encontrada porque `components.css` no se cargó | Media | Verificar orden de carga en _Layout.cshtml |
| Dark mode de reportes se ve mal | Media | Probar cada reporte en dark mode después de Fase 4 |
| Admin page se rompe al migrar a Layout | Alta | Postergar si es necesario, hacerlo al final |
| Alguien del equipo agrega `<style>` inline nuevo | Media | El checklist de code review debe atraparlo |
