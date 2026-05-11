# Frontend Standards Guide — AgroForm

> **Versión:** 2.0  
> **Propósito:** Design System liviano, guía de frontend, estándar visual oficial, workflow de desarrollo y manual de buenas prácticas.  
> **Audiencia:** Desarrolladores humanos + asistentes IA (Claude, ChatGPT, Copilot, Cursor, etc.)  
> **Actualizado:** Mayo 2026

---

## Índice

1. [Filosofía Visual del Sistema](#1-filosofía-visual-del-sistema)
2. [Estructura Frontend Oficial](#2-estructura-frontend-oficial)
3. [Sistema de Variables CSS (Design Tokens)](#3-sistema-de-variables-css-design-tokens)
4. [Reglas para Crear Nuevas Vistas — Workflow Paso a Paso](#4-reglas-para-crear-nuevas-vistas--workflow-paso-a-paso)
5. [Componentes Oficiales](#5-componentes-oficiales)
   - 5.1 [Cards](#51-cards)
   - 5.2 [KPI Cards (indicadores)](#52-kpi-cards-indicadores)
   - 5.3 [Botones](#53-botones)
   - 5.4 [Botones Icono](#54-botones-icono)
   - 5.5 [Tabs](#55-tabs)
   - 5.6 [Tablas](#56-tablas)
   - 5.7 [Modales](#57-modales)
   - 5.8 [Badges y Etiquetas](#58-badges-y-etiquetas)
   - 5.9 [Formularios](#59-formularios)
   - 5.10 [Alertas y Estados Vacíos](#510-alertas-y-estados-vacíos)
   - 5.11 [Métricas y Trends](#511-métricas-y-trends)
   - 5.12 [Filtros](#512-filtros)
   - 5.13 [Dashboard Widgets](#513-dashboard-widgets)
6. [Sistema de Spacing](#6-sistema-de-spacing)
7. [Reglas de Tipografía](#7-reglas-de-tipografía)
8. [Reglas de Colores](#8-reglas-de-colores)
9. [Reglas de Iconografía](#9-reglas-de-iconografía)
10. [Dark Theme](#10-dark-theme)
11. [Reglas sobre CSS — Prohibido vs Recomendado](#11-reglas-sobre-css--prohibido-vs-recomendado)
12. [Workflow de Refactorización](#12-workflow-de-refactorización)
13. [Checklist Final para Nuevas Vistas](#13-checklist-final-para-nuevas-vistas)
14. [Ejemplos Reales — Correcto, Incorrecto, Before/After](#14-ejemplos-reales--correcto-incorrecto-beforeafter)
15. [Objetivo Final](#15-objetivo-final)

---

## 1. Filosofía Visual del Sistema

### Principios Rectores

| Principio | Descripción |
|-----------|-------------|
| **Consistencia ante todo** | Un mismo patrón visual en todo el sistema. Un solo tipo de tab, un solo tipo de KPI card, un solo set de sombras. |
| **Tema-aware por defecto** | Todo componente nuevo debe funcionar correctamente en light y dark mode usando CSS variables. No se aceptan colores hardcodeados. |
| **Mobile-first responsive** | Toda vista nueva debe ser funcional desde 360px de ancho. Sidebar colapsable, tabs scrollables, tablas con scroll horizontal. |
| **Rendimiento visual** | Transiciones suaves (0.2s–0.3s), sombras sutiles, hover effects que no degraden performance. |
| **Cero inline CSS en vistas** | Todo el CSS debe vivir en los 4 archivos CSS del proyecto. Nada de `<style>` en vistas (salvo casos excepcionales justificados). |
| **Phosphor Icons como único set** | Bootstrap Icons, FontAwesome, u otros sets están prohibidos. Solo Phosphor Icons (`ph ph-*`). |

### Paleta Conceptual

```
Marca:   Verde agricultura (#198754 → #20c997 gradient)
Fondo:   Gris cálido (#f8f9fa) → #0f1115 (dark)
Superficie: Blanco puro (#fff) → #161a22 (dark)
Acento:  Verde esmeralda (#20c997)
Texto:   Casi negro (#212529) → casi blanco (#e9edf3) (dark)
Bordes:  Muy sutiles (rgba(0,0,0,0.12) → rgba(255,255,255,0.14))
```

---

## 2. Estructura Frontend Oficial

### Árbol de Archivos CSS (orden de carga crítico)

```
AgroForm.Web/wwwroot/css/
├── site.css              ← 1°: Variables, layout, sidebar, navbar, helpers
├── components.css        ← 2°: Componentes reutilizables (cards, tabs, modals, etc.)
├── reports.css           ← 3°: Estilos específicos de reportes
└── dark-theme.css        ← 4°: Todos los [data-theme="dark"] overrides
```

> **⚠️ REGLA DE ORO:** El orden de carga es **obligatorio**.
> - [`site.css`](AgroForm.Web/wwwroot/css/site.css) debe cargarse ANTES que [`components.css`](AgroForm.Web/wwwroot/css/components.css) porque define las variables CSS.
> - [`reports.css`](AgroForm.Web/wwwroot/css/reports.css) depende de clases de [`components.css`](AgroForm.Web/wwwroot/css/components.css) como `.card-kpi`.
> - [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css) debe cargarse ÚLTIMO porque overridea todo.

### Orden de carga en [`_Layout.cshtml`](AgroForm.Web/Views/Shared/_Layout.cshtml)

```html
<!-- Bootstrap (CDN) -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet">

<!-- Librerías de terceros (DataTables, Select2, Choices.js, SweetAlert2) -->

<!-- Nuestros CSS en orden estricto -->
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/dark-theme.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/components.css" asp-append-version="true" />
<link rel="stylesheet" href="~/css/reports.css" asp-append-version="true" />
```

> **Nota:** [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css) se carga antes que [`components.css`](AgroForm.Web/wwwroot/css/components.css) y [`reports.css`](AgroForm.Web/wwwroot/css/reports.css) porque estos últimos usan variables CSS que ya tienen sus valores dark definidos en `:root/[data-theme="dark"]` de [`site.css`](AgroForm.Web/wwwroot/css/site.css). El `dark-theme.css` solo contiene **overrides específicos de componente** bajo `[data-theme="dark"]`, no redefiniciones de variables.

### Árbol de Archivos JavaScript

```
AgroForm.Web/wwwroot/js/
├── site.js                  ← Funciones globales (AgroForm.ajax, showSuccess, etc.)
├── dark-theme.js            ← DarkTheme API (toggle, persistencia, Chart.js config)
├── layout.js                ← Sidebar toggle, responsive behavior
└── views/
    ├── actividad.js
    ├── actividadRapida.js
    ├── administradorCategorias.js
    ├── administradorLicencias.js
    ├── campania.js
    ├── campo.js
    ├── cicloCultivo.js
    ├── gasto.js
    ├── registroClima.js
    ├── reporteAplicaciones.js
    ├── reporteCampos.js
    ├── reporteComparativa.js
    ├── reporteCosecha.js
    ├── selectorCampania.js
    └── usuario.js
```

### Librerías Externas (versiones oficiales unificadas)

| Librería | Versión | CDN |
|----------|---------|-----|
| Bootstrap CSS/JS | 5.3.3 | `cdn.jsdelivr.net/npm/bootstrap@5.3.3` |
| jQuery | 3.7.1 | `code.jquery.com/jquery-3.7.1.min.js` |
| DataTables | 2.1.8 | `cdn.datatables.net/2.1.8` |
| DataTables Buttons | 3.1.2 | `cdn.datatables.net/buttons/3.1.2` |
| SweetAlert2 | 11 | `cdn.jsdelivr.net/npm/sweetalert2@11` |
| Phosphor Icons | — | `cdn.jsdelivr.net/npm/@phosphor-icons/web@2.1.1` |
| Select2 | 4.0.13 | `cdnjs.cloudflare.com/ajax/libs/select2/4.0.13` |
| Choices.js | — | `cdn.jsdelivr.net/npm/choices.js` |
| Chart.js | 4.4.0 | `cdn.jsdelivr.net/npm/chart.js` |
| Toastr | — | `cdnjs.cloudflare.com/ajax/libs/toastr.js` |
| html2canvas | — | `cdnjs.cloudflare.com/ajax/libs/html2canvas` |
| jsPDF | — | `cdnjs.cloudflare.com/ajax/libs/jspdf` |

> **⚠️ REGLA:** No mezclar versiones. Si agregás una librería nueva, usá la misma versión en todas las vistas. Si actualizás una versión, actualizala en **todos** los archivos.

---

## 3. Sistema de Variables CSS (Design Tokens)

### 3.1 Surface Colors (tema-aware)

Definidas en [`site.css`](AgroForm.Web/wwwroot/css/site.css:8-14):

```css
:root {
    --app-bg: #f8f9fa;           /* Fondo general de la app */
    --surface: #ffffff;           /* Superficie de cards, modales, dropdowns */
    --surface-2: #f1f3f5;         /* Superficie secundaria (headers de tabla, hover) */
    --text: #212529;              /* Color de texto principal */
    --text-muted: #6c757d;        /* Color de texto secundario */
    --border: rgba(0,0,0,0.12);   /* Bordes */
}

[data-theme="dark"] {
    --app-bg: #0f1115;
    --surface: #161a22;
    --surface-2: #1d2230;
    --text: #e9edf3;
    --text-muted: rgba(233, 237, 243, 0.7);
    --border: rgba(255,255,255,0.14);
}
```

### 3.2 Design Tokens

| Token | Light Value | Dark Value | Uso |
|-------|-------------|------------|-----|
| `--shadow-sm` | `0 2px 4px rgba(0,0,0,0.08)` | `0 2px 4px rgba(0,0,0,0.3)` | Cards, navbar |
| `--shadow-md` | `0 4px 15px rgba(0,0,0,0.1)` | `0 4px 15px rgba(0,0,0,0.4)` | Dropdowns, modales |
| `--shadow-lg` | `0 10px 30px rgba(0,0,0,0.15)` | `0 10px 30px rgba(0,0,0,0.55)` | Modales grandes |
| `--radius-sm` | `6px` | `6px` | Inputs, botones pequeños |
| `--radius-md` | `10px` | `10px` | Cards, dropdowns |
| `--radius-lg` | `12px` | `12px` | Cards principales |
| `--radius-xl` | `15px` | `15px` | Secciones destacadas |
| `--radius-full` | `50%` | `50%` | Avatares, botones circulares |

### 3.3 Brand Tokens

```css
:root {
    --brand-primary: #198754;        /* Verde Bootstrap — color principal */
    --brand-secondary: #20c997;      /* Verde esmeralda — acento */
    --brand-gradient: linear-gradient(135deg, #198754 0%, #20c997 100%);
}
```

### 3.4 Sidebar Tokens

```css
:root {
    --sidebar-width: 215px;
    --sidebar-width-collapsed: 80px;
    --sidebar-bg: #2c3e50;
    --sidebar-text: #b3b3b3;
    --sidebar-text-active: #ffffff;
    --sidebar-hover-bg: rgba(255,255,255,0.1);
    --sidebar-accent: #20c997;
    --sidebar-header-gradient: linear-gradient(135deg, #198754 0%, #20c997 100%);
}

[data-theme="dark"] {
    --sidebar-bg: #1a1f2e;
    --sidebar-text: rgba(255,255,255,0.6);
    --sidebar-text-active: #ffffff;
    --sidebar-hover-bg: rgba(255,255,255,0.08);
}
```

### 3.5 Utility Helpers (para usar directamente en HTML)

```css
/* Shadows */
.shadow-app-sm { box-shadow: var(--shadow-sm); }
.shadow-app-md { box-shadow: var(--shadow-md); }
.shadow-app-lg { box-shadow: var(--shadow-lg); }

/* Border Radius */
.radius-app-sm { border-radius: var(--radius-sm); }
.radius-app-md { border-radius: var(--radius-md); }
.radius-app-lg { border-radius: var(--radius-lg); }
.radius-app-full { border-radius: var(--radius-full); }
```

> **✅ USO CORRECTO:**
> ```html
> <div class="card shadow-app-md radius-app-lg">...</div>
> ```

> **❌ USO INCORRECTO:**
> ```html
> <div class="card" style="box-shadow: 0 4px 15px rgba(0,0,0,0.1); border-radius: 12px;">...</div>
> ```

### 3.6 Reglas de Uso de Variables

1. **Siempre usar `var(--token)` en lugar de valores hardcodeados.**
2. **Proveer fallbacks:** `var(--radius-lg, 12px)` — el fallback es el valor standard por si la variable no existe.
3. **No redefinir variables en componentes.** Las variables se definen solo en `:root` y `[data-theme="dark"]` en [`site.css`](AgroForm.Web/wwwroot/css/site.css).
4. **Usar los utility helpers** (`.shadow-app-*`, `.radius-app-*`) antes que escribir la variable manualmente.

---

## 4. Reglas para Crear Nuevas Vistas — Workflow Paso a Paso

### Paso 1: Crear el archivo `.cshtml`

```
AgroForm.Web/Views/[Controller]/[Action].cshtml
```

### Paso 2: Usar solo componentes oficiales

No inventes nuevos patrones visuales. Usá exclusivamente los componentes listados en la [Sección 5](#5-componentes-oficiales).

### Paso 3: NO escribir `<style>` tags

❌ **MAL:**
```html
<style>
    .mi-clase-unica { color: red; }
</style>
```

✅ **BIEN:** Usar clases existentes o agregar CSS nuevo a los archivos correspondientes.

### Paso 4: Todas las surfaces deben usar CSS variables

✅ **BIEN:**
```html
<div class="card" style="background: var(--surface); color: var(--text);">
```

✅ **MEJOR:** Usar clases que ya aplican las variables:
```html
<div class="card card-app">
```

### Paso 5: Todos los iconos deben ser Phosphor

✅ **BIEN:** `<i class="ph ph-user"></i>`  
❌ **MAL:** `<i class="fas fa-user"></i>` o `<i class="bi bi-person"></i>`

### Paso 6: Agregar el `<script>` del view-specific JS

```html
@section Scripts {
    <script src="~/js/views/miVista.js"></script>
}
```

### Paso 7: Verificar dark mode

Probar la vista con el tema oscuro activado. Si algo se ve mal, agregar el override a [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css), **nunca** en la vista.

### Paso 8: Verificar responsive

Probar en móvil (360px). Sidebar debe colapsar, tablas tener scroll horizontal, cards apilarse verticalmente.

### Paso 9: Pasar el checklist de la [Sección 13](#13-checklist-final-para-nuevas-vistas)

---

## 5. Componentes Oficiales

### 5.1 Cards

#### `.card-app` — Card genérica con hover effect

Definida en [`components.css`](AgroForm.Web/wwwroot/css/components.css:12-23).

```html
<div class="card card-app">
    <div class="card-header">
        <h5 class="mb-0">Título</h5>
    </div>
    <div class="card-body">
        <p>Contenido</p>
    </div>
</div>
```

**CSS:**
```css
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
```

### 5.2 KPI Cards (indicadores)

#### `.card-kpi` — Tarjeta de indicador/KPI

Definida en [`components.css`](AgroForm.Web/wwwroot/css/components.css:26-68). Usada extensivamente en reportes (Cosecha, Aplicaciones, ComparativaCampos, Campo).

```html
<div class="card card-kpi">
    <div class="d-flex align-items-center gap-3">
        <div class="kpi-icon" style="background: rgba(25, 135, 84, 0.12); color: #198754;">
            <i class="ph ph-tractor"></i>
        </div>
        <div>
            <div class="kpi-value">$ 1,234,567</div>
            <div class="kpi-label">Costo Total</div>
            <div class="kpi-trend text-success">
                <i class="ph ph-trend-up"></i> +12.5%
            </div>
        </div>
    </div>
</div>
```

**Estructura interna:**
- `.kpi-icon` — Icono de 48×48 con fondo semitransparente (`background: rgba(...)`)
- `.kpi-value` — Valor numérico grande (1.75rem, bold 700)
- `.kpi-label` — Texto descriptivo (0.85rem, `--text-muted`)
- `.kpi-trend` — Indicador de tendencia (0.8rem, `text-success`, `text-danger`, `text-warning`)

> **📦 USOS CONOCIDOS:**
> - Reporte/Cosecha — 6 KPI cards (Rendimiento, Costo Total, Precio, Margen, Producción, Área)
> - Reporte/Aplicaciones — KPIs de aplicación (Costo total, costo/ha, etc.)
> - Reporte/ComparativaCampos — Resumen comparativo de campos
> - Reporte/Campo — KPIs de resumen ejecutivo

### 5.3 Botones

#### `.btn-app` — Botón standard

Definido en [`components.css`](AgroForm.Web/wwwroot/css/components.css:109-114).

```html
<button class="btn btn-primary btn-app">
    <i class="ph ph-plus me-1"></i> Crear
</button>
```

#### `.btn-app-sm` — Botón pequeño

```html
<button class="btn btn-success btn-app-sm">
    <i class="ph ph-check me-1"></i> Guardar
</button>
```

**CSS:**
```css
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
```

> **Regla:** Siempre usar clases Bootstrap (`btn btn-primary`, `btn btn-success`, etc.) combinadas con `btn-app` o `btn-app-sm`. No crear clases de botón personalizadas.

### 5.4 Botones Icono

#### `.btn-app-icon` — Botón cuadrado solo icono (32×32)

```html
<button class="btn btn-outline-secondary btn-app-icon" title="Editar">
    <i class="ph ph-pencil"></i>
</button>
```

#### `.btn-app-circle` — Botón circular solo icono (36×36)

```html
<button class="btn btn-primary btn-app-circle">
    <i class="ph ph-plus"></i>
</button>
```

#### `.btn-circle` — Botón circular grande (Bootstrap extendido)

Definido en [`site.css`](AgroForm.Web/wwwroot/css/site.css:234-240).

```css
.btn-circle {
    border-radius: 50% !important;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 0;
}
```

### 5.5 Tabs

#### `.nav-tabs-app` — Único sistema de tabs del proyecto

Definido en [`components.css`](AgroForm.Web/wwwroot/css/components.css:75-103).

```html
<ul class="nav nav-tabs-app" id="myTabs" role="tablist">
    <li class="nav-item" role="presentation">
        <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#tab1">
            <i class="ph ph-chart-bar"></i> Resumen
        </button>
    </li>
    <li class="nav-item" role="presentation">
        <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab2">
            <i class="ph ph-table"></i> Detalle
        </button>
    </li>
    <li class="nav-item" role="presentation">
        <button class="nav-link" data-bs-toggle="tab" data-bs-target="#tab3">
            <i class="ph ph-trend-up"></i> Gráficos
        </button>
    </li>
</ul>

<div class="tab-content">
    <div class="tab-pane active show" id="tab1">...</div>
    <div class="tab-pane" id="tab2">...</div>
    <div class="tab-pane" id="tab3">...</div>
</div>
```

**CSS clave:**
```css
.nav-tabs-app {
    border-bottom: 2px solid var(--border);
    gap: 4px;
}
.nav-tabs-app .nav-link.active {
    color: var(--text);
    background: var(--surface);
    border-bottom: 2px solid var(--bs-primary);
    margin-bottom: -2px;
}
```

> **⚠️ IMPORTANTE:** Se requiere el fix de stacking en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:300-316):
> ```css
> .tab-content > .tab-pane {
>     display: none !important;
>     position: relative;
>     width: 100%;
> }
> .tab-content > .tab-pane.active {
>     display: block !important;
> }
> ```

### 5.6 Tablas

#### `.table-app` — Tabla de reportes standard

Definida en [`components.css`](AgroForm.Web/wwwroot/css/components.css:196-209).

```html
<table class="table table-app">
    <thead>
        <tr>
            <th>Campo</th>
            <th>Cultivo</th>
            <th class="text-end">Rendimiento</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Lote 1</td>
            <td>Maíz</td>
            <td class="text-end">8,500 kg/ha</td>
        </tr>
    </tbody>
</table>
```

**CSS:**
```css
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
```

#### `.table-reporte` — Variante con sort (Reporte/Aplicaciones)

Definida en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:669-701).

```html
<table class="table table-reporte">
    <thead>
        <tr>
            <th class="sortable" onclick="ordenarPor(0)">
                Campo <i class="ph ph-arrows-down-up sort-icon"></i>
            </th>
        </tr>
    </thead>
</table>
```

#### `.table-compact` — Tabla ultra-compacta (ComparativaCampos)

Definida en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:259-281).

```html
<table class="table table-compact">
    <!-- celdas con padding reducido a 0.4rem 0.5rem -->
</table>
```

#### `.table-cosecha` — Tabla específica de cosecha con sort

Definida en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:537-564).

### 5.7 Modales

#### `.modal-header-app` + variantes de color

Definido en [`components.css`](AgroForm.Web/wwwroot/css/components.css:147-189).

```html
<div class="modal fade" id="crearModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header modal-header-app modal-header-success">
                <h5 class="modal-title">
                    <i class="ph ph-plus-circle me-1"></i> Nuevo Registro
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                ...
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary btn-app-sm" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-success btn-app-sm">Guardar</button>
            </div>
        </div>
    </div>
</div>
```

**Variantes disponibles:**

| Clase adicional | Color | Uso típico |
|----------------|-------|-----------|
| *(ninguna)* | Default (Bootstrap) | Modales simples |
| `modal-header-success` | Gradient verde `#198754 → #20c997` | Crear/éxito |
| `modal-header-danger` | Rojo `#dc3545` | Eliminar/peligro |
| `modal-header-info` | Cyan `#0dcaf0` | Información |
| `modal-header-primary` | Azul Bootstrap | Acciones primarias |

> **⚠️ Importante:** Siempre incluir `filter: brightness(0) invert(1);` para el `.btn-close` cuando el header tiene fondo de color. Esto ya está cubierto en [`components.css`](AgroForm.Web/wwwroot/css/components.css) para cada variante.

### 5.8 Badges y Etiquetas

#### `.tipo-badge` — Badge de tipo (Aplicaciones report)

Definido en [`components.css`](AgroForm.Web/wwwroot/css/components.css:237-245).

```html
<span class="tipo-badge" style="background: #e7f1ff; color: #0d6efd;">
    <i class="ph ph-drop"></i> Fumigación
</span>
```

#### `.ranking-badge` — Badge circular de ranking 1°, 2°, 3°

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:105-153).

```html
<span class="ranking-badge ranking-1">1</span>
<span class="ranking-badge ranking-2">2</span>
<span class="ranking-badge ranking-3">3</span>
```

**CSS:** Las posiciones 1, 2, 3 tienen gradient y tamaño ligeramente mayor (32×32 vs 28×28 base).

#### `.ranking-default` — Badge circular genérico (otros rankings)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:520-531).

```html
<span class="ranking-default">4</span>
```

### 5.9 Formularios

Usar siempre Bootstrap standard con toques del Design System.

✅ **Ejemplo correcto:**
```html
<div class="mb-3">
    <label for="nombre" class="form-label fw-semibold small">Nombre del Campo</label>
    <input type="text" class="form-control form-control-sm" id="nombre" name="nombre" required>
</div>
<div class="d-flex gap-2 justify-content-end">
    <button type="button" class="btn btn-secondary btn-app-sm" data-bs-dismiss="modal">Cancelar</button>
    <button type="submit" class="btn btn-success btn-app-sm">
        <i class="ph ph-check me-1"></i> Guardar
    </button>
</div>
```

> **Reglas de formularios:**
> - Labels: `.form-label.fw-semibold.small` (bold sutil + tamaño pequeño)
> - Inputs: `.form-control.form-control-sm` para compactos
> - Selects: `.form-select.form-select-sm`
> - Botones: Siempre `.btn-app-sm` para modales
> - Espaciado: `.mb-3` entre campos

### 5.10 Alertas y Estados Vacíos

#### Loading state

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:416-422).

```html
<div class="report-loading" id="loadingState">
    <div class="spinner-border text-primary mb-3" role="status">
        <span class="visually-hidden">Cargando...</span>
    </div>
    <p class="text-muted">Cargando datos del reporte...</p>
</div>
```

#### Empty state

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:767-770).

```html
<div id="emptyState" class="text-center py-5">
    <i class="ph ph-database" style="font-size: 3rem; color: var(--text-muted);"></i>
    <p class="text-muted mt-2">No hay datos disponibles para los filtros seleccionados.</p>
</div>
```

#### Alert cards

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:369-381).

```html
<div class="alert-card alert-severity-alta">
    <div class="d-flex align-items-center gap-2">
        <i class="ph ph-warning-circle text-danger"></i>
        <strong>Alta</strong>
    </div>
    <p class="mb-0 mt-1">Rendimiento estimado por debajo del promedio histórico.</p>
</div>
```

**Severidades:** `alert-severity-alta` (rojo), `alert-severity-media` (amarillo), `alert-severity-baja` (cyan).

### 5.11 Métricas y Trends

Definido en [`components.css`](AgroForm.Web/wwwroot/css/components.css:215-231).

```html
<div>
    <div class="metric-value">$ 1,234,567</div>
    <div class="metric-label">Costo Total</div>
    <div class="metric-trend text-success">
        <i class="ph ph-trend-up"></i> +12.5% vs campaña anterior
    </div>
</div>
```

**Trend colors (definidos en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:178-180, 570-572)):**

| Clase | Color | Significado |
|-------|-------|-------------|
| `.kpi-trend-up` / `.trend-up` | `#28a745` | Incremento positivo |
| `.kpi-trend-down` / `.trend-down` | `#dc3545` | Decremento negativo |
| `.kpi-trend-stable` / `.trend-stable` | `#ffc107` | Sin cambio significativo |

### 5.12 Filtros

#### `.filter-section` — Sección de filtros agrupados (ComparativaCampos)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:216-220).

```html
<div class="filter-section mb-3">
    <div class="row g-2 align-items-end">
        <div class="col-md-4">
            <label class="form-label small mb-1">Campaña</label>
            <select class="form-select form-select-sm" id="campaniaFilter">
                <option value="">Todas</option>
            </select>
        </div>
        <div class="col-md-3 d-flex gap-2">
            <button class="btn btn-primary btn-app-sm flex-grow-1" onclick="cargarDatos()">
                <i class="ph ph-magnifying-glass me-1"></i> Buscar
            </button>
            <button class="btn btn-outline-secondary btn-app-sm" onclick="limpiarFiltros()">
                <i class="ph ph-x"></i>
            </button>
        </div>
    </div>
</div>
```

#### Filtros compactos (Reporte/Campo)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:441-456).

```html
<div class="row g-2 filters-row">
    <div class="col-md-3">
        <label class="form-label">Campo</label>
        <select class="form-select">...</select>
    </div>
</div>
```

### 5.13 Dashboard Widgets

#### Score Ring (Reporte/Campo)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:57-82).

```html
<div class="score-ring score-excellent" style="--pct: 85%;">
    <div class="score-ring-inner">85</div>
</div>
```

**Clases de score:** `score-excellent` (verde), `score-good` (cyan), `score-regular` (amarillo), `score-critical` (rojo).

#### Timeline vertical (Reporte/Campo)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:11-51).

```html
<div class="timeline">
    <div class="timeline-item">
        <div class="timeline-icon" style="background: #28a745;">
            <i class="ph ph-seedling"></i>
        </div>
        <div class="timeline-content">
            <strong>Siembra</strong>
            <small class="text-muted d-block">15 Oct 2025</small>
        </div>
    </div>
</div>
```

#### Soil Bars (Reporte/Campo)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:88-99).

```html
<div class="soil-bar mb-1" style="position: relative;">
    <div class="soil-bar-fill soil-optimum" style="width: 75%;"></div>
</div>
```

**Clases de nivel:** `soil-low` (rojo), `soil-medium` (amarillo), `soil-optimum` (verde), `soil-high` (cyan).

#### Heatmap (ComparativaCampos)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:159-172).

```html
<div class="heatmap-cell heatmap-high">8,500</div>
```

**Clases:** `heatmap-high`, `heatmap-mid-high`, `heatmap-mid`, `heatmap-mid-low`, `heatmap-low`, `heatmap-none`.

#### Indicator Cards (Reporte/Cosecha)

Definido en [`reports.css`](AgroForm.Web/wwwroot/css/reports.css:186-210).

```html
<div class="indicator-card indicator-severity-alta">
    <div class="d-flex align-items-center gap-3">
        <div class="indicator-icon" style="background: rgba(220,53,69,0.12); color: #dc3545;">
            <i class="ph ph-warning"></i>
        </div>
        <div>
            <strong>Estrés Hídrico</strong>
            <small class="text-muted d-block">Riego recomendado</small>
        </div>
    </div>
</div>
```

---

## 6. Sistema de Spacing

### Reglas Generales

1. Usar **Bootstrap spacing utilities** (`m-*`, `p-*`, `gap-*`).
2. Espaciado base entre cards/sections: `mb-3` (1rem) o `mb-4` (1.5rem).
3. Gap entre elementos en fila: `gap-2` (0.5rem) o `gap-3` (1rem).
4. Padding interno de cards: `p-3` (1rem) o Bootstrap default.

### Tabla de Espaciados Recomendados

| Contexto | Clase | Valor | Ejemplo |
|----------|-------|-------|---------|
| Entre cards en grid | `mb-3` | 1rem | `.row > .col-md-4.mb-3` |
| Gap entre ícono y texto en KPI | `gap-3` | 1rem | `.d-flex.align-items-center.gap-3` |
| Padding de KPI card | `p-3` o `1.25rem` (nativo) | 1.25rem | Definido en `.card-kpi` |
| Entre secciones grandes | `mb-4` | 1.5rem | |
| Entre campos de formulario | `mb-3` | 1rem | |
| Gap en filter section | `g-2` | 0.5rem | `.row.g-2` |
| Padding de modal body | Bootstrap default | 1rem | |
| Gap entre botones de toolbar | `gap-2` | 0.5rem | `.d-flex.gap-2` |

### Regla de Espaciado Interno de Componentes

- **Cards KPI:** Padding ya definido en CSS (`padding: 1.25rem`) — no agregar padding inline.
- **Tablas:** Padding de celdas definido por Bootstrap + overrides en clases específicas.
- **Botones:** Padding definido en `.btn-app`/`.btn-app-sm`.
- **Tabs:** Padding de links definido en `.nav-tabs-app .nav-link`.

---

## 7. Reglas de Tipografía

### Font Family

```css
/* Bootstrap 5.3.3 default — no override */
font-family: system-ui, -apple-system, "Segoe UI", Roboto, "Helvetica Neue", "Noto Sans", "Liberation Sans", Arial, sans-serif;
```

### Jerarquía Tipográfica

| Elemento | Tamaño | Weight | Color |
|----------|--------|--------|-------|
| Page title (`h1`) | Bootstrap default (~2rem) | 600–700 | `var(--text)` |
| Section title (`h3`) | Bootstrap default (~1.5rem) | 600 | `var(--text)` |
| Card title (`h5`) | Bootstrap default (~1.25rem) | 600 | `var(--text)` |
| KPI value (`.kpi-value`) | `1.75rem` | `700` | `var(--text)` |
| KPI label (`.kpi-label`) | `0.85rem` | `400` | `var(--text-muted)` |
| Table header (`.table-app th`) | `0.8rem` | `600` | `var(--text)` |
| Table cell (`.table-app td`) | `0.875rem` | `400` | `var(--text)` |
| Form label | Bootstrap default | `600 (fw-semibold)` | `var(--text)` |
| Small / muted | Bootstrap default | `400` | `var(--text-muted)` |
| Metric value (`.metric-value`) | `1.75rem` | `700` | `var(--text)` |
| Metric label (`.metric-label`) | `0.85rem` | `400` | `var(--text-muted)` |
| KPI trend (`.kpi-trend`) | `0.8rem` | `400` | según dirección |

### Reglas

1. **No forzar font-family personalizada.** Bootstrap system-ui stack es suficiente.
2. **Text-transform uppercase** solo en headers de tabla (`.table-app th`, `.table-reporte th`).
3. **Letter-spacing** solo en headers de tabla (`0.3px–0.5px`).
4. **Line-height** de valores KPI/metric: `1.2` para compacto.
5. **Usar `fw-semibold` (600)** en lugar de `fw-bold` (700) para títulos. Reservar 700 solo para valores numéricos grandes.

---

## 8. Reglas de Colores

### Paleta Oficial

```css
/* --- Brand / Accent --- */
--brand-primary: #198754;        /* Bootstrap success - color principal */
--brand-secondary: #20c997;      /* Bootstrap teal - acento */
--brand-gradient: linear-gradient(135deg, #198754 0%, #20c997 100%);

/* --- Surface (theme-aware) --- */
--app-bg: #f8f9fa;               /* Fondo general */
--surface: #ffffff;               /* Superficie principal */
--surface-2: #f1f3f5;             /* Superficie secundaria */

/* --- Text (theme-aware) --- */
--text: #212529;                  /* Texto principal */
--text-muted: #6c757d;            /* Texto secundario */

/* --- Borders (theme-aware) --- */
--border: rgba(0,0,0,0.12);       /* Bordes generales */

/* --- Shadows (theme-aware) --- */
--shadow-sm, --shadow-md, --shadow-lg;

/* --- Sidebar --- */
--sidebar-bg: #2c3e50;
--sidebar-text: #b3b3b3;
--sidebar-text-active: #ffffff;
--sidebar-accent: #20c997;
```

### Colores Funcionales (Bootstrap — usar siempre los nativos)

| Propósito | Color Bootstrap | Variable |
|-----------|----------------|----------|
| Primary/Action | `var(--bs-primary)` / `#0d6efd` | Acción principal |
| Success | `var(--bs-success)` / `#198754` | Operaciones exitosas |
| Danger | `var(--bs-danger)` / `#dc3545` | Eliminar, errores |
| Warning | `var(--bs-warning)` / `#ffc107` | Advertencias |
| Info | `var(--bs-info)` / `#0dcaf0` | Información |
| Secondary | `var(--bs-secondary)` / `#6c757d` | Acciones secundarias |
| Light | `var(--bs-light)` / `#f8f9fa` | Fondos alternativos |

### Reglas de Color

1. **Nunca usar colores hardcodeados** en vistas. Usar variables CSS (tema-aware) o clases Bootstrap.
2. **Fondos de iconos KPI:** Usar `rgba(color, 0.12)` para el background del icono + `color` sólido para el icono mismo.
3. **Trends:** Siempre usar `text-success` (⬆), `text-danger` (⬇), `text-warning` (➡).
4. **Gradients:** Usar `var(--brand-gradient)` para headers de modal success. No recrear gradients manualmente.
5. **Sidebar:** No hardcodear colores de sidebar. Usar variables `--sidebar-*`.

### ❌ Prohibido

```css
/* MAL - hardcodeado */
background: #f8f9fa;
color: #212529;
border: 1px solid #dee2e6;
box-shadow: 0 2px 10px rgba(0,0,0,0.1);

/* MAL - no crees gradients manuales */
background: linear-gradient(135deg, #198754 0%, #20c997 100%);
```

### ✅ Permitido

```css
/* BIEN - usa variables */
background: var(--surface);
color: var(--text);
border: 1px solid var(--border);
box-shadow: var(--shadow-sm);

/* BIEN - usa la variable brand */
background: var(--brand-gradient);
```

---

## 9. Reglas de Iconografía

### Set Oficial: Phosphor Icons

**CDN:** `https://cdn.jsdelivr.net/npm/@phosphor-icons/web@2.1.1`

```html
<!-- Estructura correcta -->
<i class="ph ph-user"></i>
<i class="ph ph-tractor"></i>
<i class="ph ph-chart-bar"></i>
```

### Reglas

1. **Siempre anteponer `ph ph-`** al nombre del icono. `ph` es la familia regular, `ph-fill` para versiones rellenas, `ph-bold` para bold.
2. **NUNCA usar otros sets** (FontAwesome, Bootstrap Icons, Material Icons). Si no encontrás el icono en Phosphor, buscá en [phosphoricons.com](https://phosphoricons.com/).
3. **Tamaños:** Los iconos Phosphor heredan `font-size` del contenedor. Usar `font-size: 1.5rem` para iconos de KPI, `1.1em` para sidebar, `1rem` para botones.
4. **Espaciado:** En botones, usar `me-1` o `ms-1` para separar icono del texto.
5. **Accesibilidad:** En iconos decorativos, agregar `aria-hidden="true"`. En iconos informativos, agregar `title` o `aria-label`.
6. **Prohibido** `<span class="icon">...</span>` o `<span class="fas">...</span>`.

### Mapeo de Iconos Comunes

| Concepto | Clase Phosphor |
|----------|---------------|
| Usuario | `ph ph-user` |
| Campo/Lote | `ph ph-map-trifold` |
| Tractor/Actividad | `ph ph-tractor` |
| Cultivo/Planta | `ph ph-seedling` |
| Calendario | `ph ph-calendar` |
| Moneda/Dinero | `ph ph-currency-circle-dollar` |
| Gráfico | `ph ph-chart-bar` |
| Tabla | `ph ph-table` |
| PDF | `ph ph-file-pdf` |
| Excel | `ph ph-file-xls` |
| Exportar | `ph ph-download-simple` |
| Buscar | `ph ph-magnifying-glass` |
| Editar | `ph ph-pencil` |
| Eliminar | `ph ph-trash` |
| Agregar | `ph ph-plus` |
| Cerrar | `ph ph-x` |
| Guardar | `ph ph-check` |
| Cancelar | `ph ph-x-circle` |
| Warning | `ph ph-warning-circle` |
| Info | `ph ph-info` |
| Success | `ph ph-check-circle` |
| Error | `ph ph-x-circle` |
| Filtro | `ph ph-funnel` |
| Ordenar | `ph ph-arrows-down-up` |
| Trending Up | `ph ph-trend-up` |
| Trending Down | `ph ph-trend-down` |

---

## 10. Dark Theme

### Arquitectura

```
site.css                         dark-theme.css                    dark-theme.js
┌─────────────────┐             ┌──────────────────────┐         ┌──────────────────────┐
│ :root           │             │ [data-theme="dark"]  │         │ DarkTheme.applyTheme │
│   --surface     │──────┐     │   .card { ... }      │         │ DarkTheme.getTheme   │
│   --text        │      │     │   .table { ... }     │         │ themeChanged event   │
│   --border      │      │     │   .kpi-card { ... }  │         │ Chart.js dark config │
│ [data-theme="dark"] │   │     │   ...all other rules │         │ DataTables refresh   │
│   { vars }      │      │     └──────────────────────┘         └──────────────────────┘
└─────────────────┘      │              ↑
                         │     Loaded AFTER site.css
                         │
                    ┌────┘
                    ▼
         ┌──────────────────────┐
         │    View's HTML        │
         │  Uses var(--surface)  │
         │  Uses var(--text)     │
         │  No [data-theme]      │
         └──────────────────────┘
```

### Cómo Funciona

1. **CSS Variables** en [`site.css`](AgroForm.Web/wwwroot/css/site.css:44-62) definen valores light y dark.
2. **`dark-theme.js`** ([AgroForm.Web/wwwroot/js/dark-theme.js](AgroForm.Web/wwwroot/js/dark-theme.js)) aplica `data-theme="dark"` al `<body>` y persiste en localStorage.
3. **`dark-theme.css`** ([AgroForm.Web/wwwroot/css/dark-theme.css](AgroForm.Web/wwwroot/css/dark-theme.css)) contiene overrides específicos de componentes bajo `[data-theme="dark"]`.
4. **Flash prevention**: Un script inline en `<head>` de [`_Layout.cshtml`](AgroForm.Web/Views/Shared/_Layout.cshtml:32-39) aplica el tema antes del render.

### Reglas para Desarrolladores

1. **Usar CSS variables siempre.** NUNCA escribir `[data-theme="dark"]` en vistas.
2. **Si un componente necesita dark mode específico**, agregar el override en [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css).
3. **Usar `!important` en dark-theme.css** cuando sea necesario para overridear Bootstrap.
4. **Chart.js** se configura automáticamente desde [`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js:91-101). No necesita configuración manual.
5. **DataTables** se refrescan automáticamente via `DarkTheme.refreshDataTables()`.
6. **Select2 y Choices.js** tienen soporte completo en [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css:510-628).

### API JavaScript (DarkTheme)

```javascript
DarkTheme.applyTheme('dark');       // Aplicar tema oscuro
DarkTheme.applyTheme('light');      // Aplicar tema claro
DarkTheme.getTheme();               // → 'dark' | 'light'
DarkTheme.toggleTheme();            // Alternar tema
DarkTheme.refreshDataTables();      // Refrescar DataTables

// Escuchar cambios de tema
document.addEventListener('themeChanged', function(e) {
    console.log('Theme changed to:', e.detail.theme);
});
```

### Toggle Switch

```html
<!-- User layout -->
<input type="checkbox" id="themeSwitchUser">

<!-- Admin panel -->
<input type="checkbox" id="themeSwitch">
```

El `dark-theme.js` auto-wirea estos elementos.

---

## 11. Reglas sobre CSS — Prohibido vs Recomendado

### ❌ Prohibido

| Práctica | Por qué | Alternativa |
|----------|---------|-------------|
| `color: #212529` hardcodeado | No es dark-mode-aware | `color: var(--text)` |
| `background: white` | No es dark-mode-aware | `background: var(--surface)` |
| `border: 1px solid #dee2e6` | No es dark-mode-aware | `border: 1px solid var(--border)` |
| `<style>` en vistas | No es reusable, infla HTML | Agregar a CSS files |
| `[data-theme="dark"]` en vistas | Fragmenta dark mode | Agregar a `dark-theme.css` |
| FontAwesome (`fa fa-*`) | Set de iconos mixto | `ph ph-*` |
| Bootstrap Icons (`bi bi-*`) | Set de iconos mixto | `ph ph-*` |
| `!important` en vistas | Dificulta debug | Usar especificidad correcta |
| Valores `border-radius` hardcodeados | Inconsistencia visual | `var(--radius-*)` |
| Sombras hardcodeadas | No son tema-aware | `var(--shadow-*)` |
| Versiones duplicadas de librerías | Conflictos, errores | Unificar versión |

### ✅ Recomendado

| Práctica | Ejemplo |
|----------|---------|
| Usar CSS variables para colores/sombras/bordes | `background: var(--surface)` |
| Usar clases de componentes oficiales | `class="card card-app"` |
| Usar utility helpers | `class="shadow-app-md radius-app-lg"` |
| CSS nuevo en archivos existentes | En `site.css`, `components.css`, `reports.css`, o `dark-theme.css` |
| Phosphor Icons | `<i class="ph ph-user"></i>` |
| Bootstrap spacing utilities | `class="d-flex gap-3 mb-3"` |
| Bootstrap grid | `class="row g-2"` |
| Naming consistente | `.card-kpi`, `.nav-tabs-app`, `.modal-header-app` |
| `asp-append-version="true"` en CSS | Cache-busting automático |
| `onchange="cambiarMoneda()"` solo para partials | `_MonedaSelector.cshtml` |

### Naming Convention

```
.card-kpi                 → Componente KPI card
.nav-tabs-app             → Tabs de la aplicación
.modal-header-app         → Modal header base
.modal-header-success     → Variante de color
.btn-app / .btn-app-sm    → Botones aplicación
.table-app                → Tabla aplicación
.shadow-app-*             → Shadow utilities
.radius-app-*             → Radius utilities
```

---

## 12. Workflow de Refactorización

Este workflow se aplica cuando se necesita limpiar, consolidar o estandarizar CSS existente.

### Paso 1: Auditoría

1. Identificar patrones duplicados (ej: 4 variantes de KPI card, 3 tipos de tabs).
2. Identificar valores hardcodeados (colores, border-radius, sombras).
3. Identificar `<style>` inline en vistas.
4. Identificar versiones inconsistentes de librerías.

### Paso 2: Planificar

1. Decidir la **clase oficial** que reemplazará a las variantes (ej: `.card-kpi` reemplaza `.summary-card` y `.kpi-card`).
2. Definir la clase en el CSS file apropiado ([`components.css`](AgroForm.Web/wwwroot/css/components.css) para componentes reutilizables, [`reports.css`](AgroForm.Web/wwwroot/css/reports.css) para específicos de reportes).
3. Asegurar que la clase use **CSS variables** para colores, sombras, bordes y radius.

### Paso 3: Ejecutar (un archivo a la vez)

1. **No cambiar todo simultáneamente.** Modificar un archivo por vez.
2. Para cada archivo, hacer **cambios pequeños y atómicos**:
   - Add la clase CSS → commit/test
   - Reemplazar en View 1 → commit/test
   - Reemplazar en View 2 → commit/test
3. **Usar `apply_diff`** para cambios quirúrgicos en archivos grandes.
4. **Re-leer el archivo** después de cada cambio para confirmar que las líneas no se desplazaron.

### Paso 4: Remover código muerto

1. Después de reemplazar todos los usos, eliminar la clase/estilo antigua.
2. Buscar referencias con `search_files` para confirmar que no quedan usos.

### Paso 5: Verificar

1. Probar la vista en light mode.
2. Probar la vista en dark mode.
3. Probar responsive.

### Ejemplo Real: Unificación de Tabs

```
Fase 1: Identificar variantes
  - .nav-tabs-custom (Reporte/Campo) → 1 variante
  - .nav-tabs-custom (Reporte/Cosecha) → 2da variante (diferente padding)
  - .nav-tabs-reporte (Reporte/Aplicaciones) → 3ra variante

Fase 2: Crear clase oficial
  - .nav-tabs-app en components.css

Fase 3: Reemplazar (1 archivo por vez)
  1. Reporte/Campo: .nav-tabs-custom → .nav-tabs-app
  2. Reporte/Cosecha: .nav-tabs-custom → .nav-tabs-app
  3. Reporte/Aplicaciones: .nav-tabs-reporte → .nav-tabs-app

Fase 4: Remover CSS muerto
  - Eliminar .nav-tabs-custom de los <style> (ya no hay inline en vistas)
  - Eliminar .nav-tabs-reporte de los <style>

Fase 5: Verificar
  - Los 3 reportes funcionan con la misma clase
```

---

## 13. Checklist Final para Nuevas Vistas

### Estructura y Archivos

- [ ] La vista usa el `_Layout.cshtml` compartido (no layout propio)
- [ ] El view-specific JS está en `wwwroot/js/views/[name].js`
- [ ] No hay `<style>` tags en la vista (salvo excepción justificada)

### CSS y Design System

- [ ] Todos los colores usan variables CSS (`--surface`, `--text`, `--border`, etc.)
- [ ] No hay valores hardcodeados de color, shadow o border-radius
- [ ] Los componentes usan clases oficiales (`.card-app`, `.card-kpi`, `.nav-tabs-app`, etc.)
- [ ] Las sombras usan `var(--shadow-*)` o clases `.shadow-app-*`
- [ ] Los border-radius usan `var(--radius-*)` o clases `.radius-app-*`

### Iconos

- [ ] Todos los iconos son Phosphor (`ph ph-*`)
- [ ] No hay FontAwesome (`fa fa-*`), Bootstrap Icons (`bi bi-*`), u otros sets
- [ ] El prefijo `ph ` está presente (no `class="ph-user"` sino `class="ph ph-user"`)

### Dark Mode

- [ ] La vista se ve correctamente con tema oscuro
- [ ] No hay `[data-theme="dark"]` selectors en la vista
- [ ] Si se necesita un override específico, está en `dark-theme.css`, no en la vista

### Responsive

- [ ] La vista funciona en 360px de ancho
- [ ] Sidebar colapsa correctamente en móvil
- [ ] Tablas tienen scroll horizontal en móvil (`.table-responsive`)
- [ ] Cards se apilan verticalmente en móvil

### Librerías

- [ ] Bootstrap 5.3.3 (no otra versión)
- [ ] jQuery 3.7.1 (no otra versión)
- [ ] DataTables 2.1.8 / Buttons 3.1.2 (no otras versiones)
- [ ] Phosphor Icons (no Bootstrap Icons CDN)

### JavaScript

- [ ] El JS view-specific se carga via `@section Scripts { ... }`
- [ ] El `dark-theme.js` está presente y se inicializa
- [ ] No hay lógica de theme toggle duplicada (usar `DarkTheme` API)

### Accesibilidad

- [ ] Los botones con solo icono tienen `title` o `aria-label`
- [ ] Los iconos decorativos tienen `aria-hidden="true"`
- [ ] Los modales tienen `tabindex="-1"`

---

## 14. Ejemplos Reales — Correcto, Incorrecto, Before/After

### 14.1 KPI Card

#### ❌ Incorrecto (antes de la refactorización)

```html
<!-- Reporte/ComparativaCampos — clase antigua .summary-card -->
<div class="card summary-card border shadow-sm mb-3">
    <div class="card-body">
        <div class="d-flex justify-content-between">
            <div>
                <h6 class="text-muted mb-1">Costo Total</h6>
                <h3 class="fw-bold text-success mb-0">$ 1,234,567</h3>
            </div>
            <div class="rounded-circle bg-success bg-opacity-10 p-3">
                <i class="ph ph-currency-circle-dollar text-success" style="font-size: 1.5rem;"></i>
            </div>
        </div>
    </div>
</div>
```

**Problemas:**
- Usa clase `.summary-card` (no oficial)
- Colores hardcodeados: `text-success`, `bg-success`
- Estructura manual no reusable
- Sin hover effect

#### ✅ Correcto (después de la refactorización)

```html
<div class="card card-kpi">
    <div class="d-flex align-items-center gap-3">
        <div class="kpi-icon" style="background: rgba(25, 135, 84, 0.12); color: #198754;">
            <i class="ph ph-currency-circle-dollar"></i>
        </div>
        <div>
            <div class="kpi-value">$ 1,234,567</div>
            <div class="kpi-label">Costo Total</div>
        </div>
    </div>
</div>
```

**Ventajas:**
- Clase oficial `.card-kpi` con hover effect incluido
- Estructura estándar (`.kpi-icon`, `.kpi-value`, `.kpi-label`)
- Tema-aware (usa `var(--surface)` y `var(--text)` automáticamente)
- Reutilizable en todos los reportes

### 14.2 Tabs

#### ❌ Incorrecto

```html
<!-- Reporte/Aplicaciones — clase antigua .nav-tabs-reporte -->
<ul class="nav nav-tabs-reporte" id="appTabs" role="tablist">
    <li class="nav-item">
        <a class="nav-link active" data-bs-toggle="tab" href="#resumen">Resumen</a>
    </li>
</ul>

<!-- Reporte/Campo — clase antigua .nav-tabs-custom -->
<ul class="nav nav-tabs-custom" id="campoTabs">
    <li class="nav-item">
        <a class="nav-link active" data-bs-toggle="tab" href="#resumen">Resumen</a>
    </li>
</ul>
```

**Problemas:** 3 clases distintas para el mismo patrón visual.

#### ✅ Correcto

```html
<ul class="nav nav-tabs-app" id="appTabs" role="tablist">
    <li class="nav-item" role="presentation">
        <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#resumen">
            <i class="ph ph-chart-bar me-1"></i> Resumen
        </button>
    </li>
</ul>
```

### 14.3 Modal Header

#### ❌ Incorrecto

```html
<!-- Varios archivos — estructura manual sin estandarizar -->
<div class="modal-header" style="background: linear-gradient(135deg, #198754, #20c997); color: white;">
    <h5 class="modal-title">Nuevo</h5>
    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
</div>
```

**Problemas:**
- Gradient hardcodeado
- Sin clase reusable
- Faltaba `filter` en `.btn-close` (en dark mode el botón de cerrar era invisible)
- Sin variantes de color

#### ✅ Correcto

```html
<div class="modal-header modal-header-app modal-header-success">
    <h5 class="modal-title"><i class="ph ph-plus-circle me-1"></i> Nuevo</h5>
    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
</div>
```

### 14.4 Botones con Icono

#### ❌ Incorrecto

```html
<button class="btn btn-sm btn-outline-primary" style="border-radius: 50%; width: 32px; height: 32px; padding: 0; display: flex; align-items: center; justify-content: center;">
    <i class="ph ph-pencil"></i>
</button>
```

#### ✅ Correcto

```html
<button class="btn btn-outline-primary btn-app-icon" title="Editar">
    <i class="ph ph-pencil"></i>
</button>
```

### 14.5 Currency Selector (Before/After)

#### ❌ Antes — 4 copias inline idénticas

```html
<!-- En Actividad/Index.cshtml -->
<div class="d-flex align-items-center gap-2">
    <label class="form-label fw-semibold mb-0 small">Moneda:</label>
    <select id="selectorMoneda" class="form-select form-select-sm" style="width: auto;" onchange="cambiarMoneda()">
        <option value="ARS">Pesos (ARS)</option>
        <option value="USD">Dólares (USD)</option>
    </select>
</div>

<!-- En Gasto/Index.cshtml → exactamente el mismo código -->
<!-- En Reporte/ComparativaCampos.cshtml → exactamente el mismo código -->
<!-- En TablaGastos/Default.cshtml → exactamente el mismo código -->
```

#### ✅ Después — Partial compartido

```html
<partial name="_MonedaSelector" />
```

Definido en [`_MonedaSelector.cshtml`](AgroForm.Web/Views/Shared/_MonedaSelector.cshtml):
```html
@*
    _MonedaSelector.cshtml — Reusable currency selector partial
    Usage: <partial name="_MonedaSelector" />
    Depends on: cambiarMoneda() JS function defined in each view
*@
<div class="d-flex align-items-center gap-2">
    <label for="selectorMoneda" class="form-label fw-semibold mb-0 small">Moneda:</label>
    <select id="selectorMoneda" class="form-select form-select-sm" style="width: auto;" onchange="cambiarMoneda()">
        <option value="ARS">Pesos (ARS)</option>
        <option value="USD">Dólares (USD)</option>
    </select>
</div>
```

### 14.6 Iconos — Error Común

#### ❌ Incorrecto

```html
<!-- Faltaba el prefijo 'ph ' -->
<i class="ph-user"></i>       <!-- Phosphor no se renderiza -->
<i class="ph-tractor"></i>    <!-- Phosphor no se renderiza -->
```

#### ✅ Correcto

```html
<i class="ph ph-user"></i>      <!-- Se renderiza correctamente -->
<i class="ph ph-tractor"></i>   <!-- Se renderiza correctamente -->
```

### 14.7 CSS — Variables vs Hardcodeado

#### ❌ Incorrecto

```css
/* En site.css */
.sidebar {
    background: #2c3e50;              /* Hardcodeado */
}
.sidebar-link {
    color: #b3b3b3;                   /* Hardcodeado */
}
.sidebar::-webkit-scrollbar-track {
    background: #1a252f;              /* Hardcodeado */
}
```

#### ✅ Correcto

```css
/* En site.css */
.sidebar {
    background: var(--sidebar-bg) !important;
}
.sidebar-link {
    color: var(--sidebar-text);
}
.sidebar::-webkit-scrollbar-track {
    background: var(--sidebar-bg);
}
[data-theme="dark"] {
    --sidebar-bg: #1a1f2e;
    --sidebar-text: rgba(255,255,255,0.6);
}
```

### 14.8 Before/After Real: Reporte/Aplicaciones

**Antes (Phase 2):**
- 228 líneas de `<style>` inline
- 3 variantes de `.kpi-card` (una propia)
- `.nav-tabs-reporte` (variante única de tabs)
- Iconos Bootstrap Icons mezclados con Phosphor
- Modal header con gradient hardcodeado

**Después (Phase 3+5):**
- 0 líneas de `<style>` inline (todo en CSS files)
- `.card-kpi` oficial (reutilizable en todos los reportes)
- `.nav-tabs-app` oficial (reutilizable)
- Solo Phosphor Icons (8 errores `ph-` → `ph ph-` corregidos)
- `.modal-header-app.modal-header-success` oficial

---

## 15. Objetivo Final

### Estado Ideal del Frontend

```
┌─────────────────────────────────────────────────────────────────┐
│                    AGROFORM DESIGN SYSTEM                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  📐 1 CSS Architecture (4 files, orden estricto)                 │
│  🎨 1 Design Token System (CSS variables, tema-aware)            │
│  🧩 1 Component Library (clases oficiales)                       │
│  🌗 1 Dark Mode System (centralizado en dark-theme.css+js)       │
│  📱 1 Responsive Strategy (mobile-first con Bootstrap grid)      │
│  🔤 1 Icon Set (Phosphor Icons)                                  │
│  📦 1 Library Version (sin duplicados ni conflictos)             │
│  ✏️ 0 Inline CSS (todo en archivos CSS)                         │
│  ✏️ 0 Hardcoded Colors (todo en variables)                      │
│  ✏️ 0 Duplicated Patterns (todo unificado)                      │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### Regla de oro

> **Si un patrón visual aparece más de una vez en el código base, debe ser una clase reusable en `components.css` o `reports.css`.**

### Lo que NO se debe hacer

1. **No crear nuevas clases de componentes** sin verificar si ya existe una clase oficial que cubra el caso.
2. **No agregar CSS inline** en vistas para patrones reutilizables.
3. **No mezclar sets de iconos.**
4. **No hardcodear valores de color, shadow, radius o spacing.**
5. **No duplicar lógica de dark mode** en vistas.
6. **No usar versiones diferentes de la misma librería.**

### Lo que SI se debe hacer

1. **Siempre preguntar:** "¿Ya existe un componente oficial para esto?"
2. **Siempre usar variables CSS** para valores visuales.
3. **Siempre verificar dark mode** antes de dar por terminada una vista.
4. **Siempre usar Phosphor Icons.**
5. **Siempre mantener las versiones de librerías consistentes.**
6. **Siempre poner CSS nuevo en los archivos correspondientes** (no en la vista).

---

## Apéndice A: Referencia Rápida de Archivos

| Archivo | Path | Propósito |
|---------|------|-----------|
| [`site.css`](AgroForm.Web/wwwroot/css/site.css) | `wwwroot/css/site.css` | Variables CSS, layout, sidebar, navbar, helpers |
| [`components.css`](AgroForm.Web/wwwroot/css/components.css) | `wwwroot/css/components.css` | Componentes reutilizables (.card-app, .card-kpi, .nav-tabs-app, .btn-app, .modal-header-app, .table-app) |
| [`reports.css`](AgroForm.Web/wwwroot/css/reports.css) | `wwwroot/css/reports.css` | Estilos específicos de reportes (timeline, score rings, soil bars, ranking, heatmap, etc.) |
| [`dark-theme.css`](AgroForm.Web/wwwroot/css/dark-theme.css) | `wwwroot/css/dark-theme.css` | Todos los `[data-theme="dark"]` overrides |
| [`dark-theme.js`](AgroForm.Web/wwwroot/js/dark-theme.js) | `wwwroot/js/dark-theme.js` | API DarkTheme, Chart.js config, DataTables refresh |
| [`_Layout.cshtml`](AgroForm.Web/Views/Shared/_Layout.cshtml) | `Views/Shared/_Layout.cshtml` | Layout principal con orden de carga de CSS |
| [`_MonedaSelector.cshtml`](AgroForm.Web/Views/Shared/_MonedaSelector.cshtml) | `Views/Shared/_MonedaSelector.cshtml` | Partial reutilizable de selector de moneda |

## Apéndice B: Comandos Útiles para Desarrollo

```bash
# Buscar usos de una clase CSS en todo el proyecto
findstr /s /n "class.*card-kpi" *.cshtml

# Buscar valores hardcodeados de border-radius
findstr /s /n "border-radius" *.css

# Buscar iconos no-Phosphor (Bootstrap Icons)
findstr /s /n "bi bi-" *.cshtml

# Buscar versiones de Bootstrap
findstr /s /n "bootstrap@" *.cshtml

# Buscar inline styles con colores hardcodeados
findstr /s /n "style=.*#[0-9a-fA-F]" *.cshtml
```

---

> **Este documento es el estándar oficial de frontend para AgroForm.**  
> Cualquier desviación debe ser justificada y aprobada antes de implementarse.  
> Para preguntas o sugerencias, actualizar este documento y notificar al equipo.
