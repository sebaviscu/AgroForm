# Workflow: Frontend Standards — AgroForm

## Workflow completo para desarrollo frontend siguiendo los estándares oficiales

### PASO 1 — Análisis y Planificación

#### 1.1. Identificar el requerimiento
- Determinar si es una vista nueva o modificación de existente
- Identificar componentes necesarios (cards, tabs, tablas, modales, etc.)
- Verificar si ya existe un patrón similar en el proyecto

#### 1.2. Consultar el Design System
- Revisar `docs/ai/frontend-standards.md` para componentes oficiales
- Verificar en `AgroForm.Web/wwwroot/css/components.css` las clases disponibles
- Buscar en vistas existentes patrones similares para reutilizar

#### 1.3. Planificar estructura
- Definir qué componentes oficiales se usarán
- Identificar si se necesitan nuevos componentes (solo si no existen)
- Planificar el layout responsive (mobile-first)

---

### PASO 2 — Creación de Archivos

#### 2.1. Crear/Modificar Vista Razor (.cshtml)

**Ubicación:** `AgroForm.Web/Views/[Controller]/[Action].cshtml`

**Estructura base obligatoria:**
```html
@{
    ViewData["Title"] = "Título de la Página";
}

<div class="container-fluid">
    <!-- Header de página -->
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="h3 mb-0">Título Principal</h1>
        <div class="d-flex gap-2">
            <button class="btn btn-primary btn-app" id="btnNew">
                <i class="ph ph-plus me-1"></i> Nuevo
            </button>
        </div>
    </div>

    <!-- Contenido principal -->
    <div class="row">
        <div class="col-12">
            <!-- Usar componentes oficiales -->
            <div class="card card-app mb-3">
                <div class="card-header">
                    <h5 class="mb-0">Título de Card</h5>
                </div>
                <div class="card-body">
                    <!-- Contenido -->
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
    <script src="~/js/views/[nombreVista].js"></script>
}
```

#### 2.2. Crear Archivo JavaScript

**Ubicación:** `AgroForm.Web/wwwroot/js/views/[nombreVista].js`

**Estructura obligatoria:**
```javascript
$(document).ready(function() {
    // Inicialización
    initTheme();
    bindEvents();
    loadInitialData();
});

function initTheme() {
    // Asegurar que el tema se aplique correctamente
    if (typeof DarkTheme !== 'undefined') {
        DarkTheme.refreshDataTables();
    }
}

function bindEvents() {
    // Event handlers usando delegación
    $(document).on('click', '#btnNew', function() {
        showModal();
    });
}

function loadInitialData() {
    // Cargar datos iniciales con fetch
    fetch('/[Controller]/[Action]')
        .then(response => response.json())
        .then(data => {
            if (data.state) {
                renderData(data.object);
            } else {
                Swal.fire('Error', data.message, 'error');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire('Error', 'Ocurrió un error inesperado', 'error');
        });
}
```

---

### PASO 3 — Aplicación de Componentes Oficiales

#### 3.1. Cards
- **Card genérica:** `.card.card-app`
- **KPI Card:** `.card.card-kpi`
- **Estructura KPI:**
```html
<div class="card card-kpi">
    <div class="d-flex align-items-center gap-3">
        <div class="kpi-icon" style="background: rgba(25, 135, 84, 0.12); color: #198754;">
            <i class="ph ph-chart-bar"></i>
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

#### 3.2. Tabs
- **Único sistema:** `.nav-tabs-app`
```html
<ul class="nav nav-tabs-app" id="myTabs" role="tablist">
    <li class="nav-item" role="presentation">
        <button class="nav-link active" data-bs-toggle="tab" data-bs-target="#tab1">
            <i class="ph ph-chart-bar me-1"></i> Resumen
        </button>
    </li>
</ul>
<div class="tab-content">
    <div class="tab-pane active show" id="tab1">...</div>
</div>
```

#### 3.3. Tablas
- **Standard:** `.table.table-app`
- **Reporte con sort:** `.table.table-reporte`
- **Compacta:** `.table.table-compact`
```html
<div class="table-responsive">
    <table class="table table-app">
        <thead>
            <tr>
                <th>Campo</th>
                <th class="text-end">Valor</th>
            </tr>
        </thead>
        <tbody>
            <!-- Datos -->
        </tbody>
    </table>
</div>
```

#### 3.4. Modales
- **Header estándar:** `.modal-header-app`
- **Variantes:** `.modal-header-success`, `.modal-header-danger`, `.modal-header-info`
```html
<div class="modal fade" id="modalForm" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header modal-header-app modal-header-success">
                <h5 class="modal-title">
                    <i class="ph ph-plus-circle me-1"></i> Nuevo Registro
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <!-- Formulario -->
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary btn-app-sm" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-success btn-app-sm" id="btnSave">
                    <i class="ph ph-check me-1"></i> Guardar
                </button>
            </div>
        </div>
    </div>
</div>
```

#### 3.5. Botones
- **Standard:** `.btn.btn-app`
- **Pequeño:** `.btn.btn-app-sm`
- **Icono:** `.btn.btn-app-icon`
- **Circular:** `.btn.btn-app-circle`

---

### PASO 4 — Aplicación de CSS Variables y Tema

#### 4.1. Usar siempre CSS variables
**✅ CORRECTO:**
```html
<div class="card" style="background: var(--surface); color: var(--text);">
<div class="card card-app"> <!-- Mejor aún -->
```

**❌ INCORRECTO:**
```html
<div class="card" style="background: #ffffff; color: #212529;">
```

#### 4.2. Variables principales
- `--app-bg`: Fondo general
- `--surface`: Superficie de cards
- `--surface-2`: Superficie secundaria
- `--text`: Texto principal
- `--text-muted`: Texto secundario
- `--border`: Bordes
- `--shadow-sm/md/lg`: Sombras
- `--radius-sm/md/lg/xl`: Border radius

#### 4.3. Utility helpers
```html
<div class="shadow-app-md radius-app-lg">
<!-- En lugar de -->
<div style="box-shadow: var(--shadow-md); border-radius: var(--radius-lg);">
```

---

### PASO 5 — Iconos Phosphor

#### 5.1. Reglas estrictas
- **SIEMPRE** usar Phosphor Icons: `<i class="ph ph-nombre"></i>`
- **NUNCA** usar FontAwesome (`fa fa-*`) o Bootstrap Icons (`bi bi-*`)
- **SIEMPRE** incluir el prefijo `ph `

#### 5.2. Iconos comunes
```html
<!-- Usuario -->
<i class="ph ph-user"></i>

<!-- Campo/Lote -->
<i class="ph ph-map-trifold"></i>

<!-- Tractor/Actividad -->
<i class="ph ph-tractor"></i>

<!-- Gráfico -->
<i class="ph ph-chart-bar"></i>

<!-- Tabla -->
<i class="ph ph-table"></i>

<!-- Editar -->
<i class="ph ph-pencil"></i>

<!-- Eliminar -->
<i class="ph ph-trash"></i>

<!-- Guardar -->
<i class="ph ph-check"></i>

<!-- Buscar -->
<i class="ph ph-magnifying-glass"></i>
```

---

### PASO 6 — Dark Mode

#### 6.1. Verificación obligatoria
1. **Activar dark mode** en la aplicación
2. **Verificar cada componente** en modo oscuro
3. **Si algo falla**, agregar override en `dark-theme.css`

#### 6.2. Agregar overrides en dark-theme.css
```css
[data-theme="dark"] .mi-componente {
    background: var(--surface) !important;
    color: var(--text) !important;
    border-color: var(--border) !important;
}
```

#### 6.3. JavaScript para tema
```javascript
// Verificar tema actual
if (typeof DarkTheme !== 'undefined') {
    const currentTheme = DarkTheme.getTheme();
    console.log('Tema actual:', currentTheme);
    
    // Escuchar cambios
    document.addEventListener('themeChanged', function(e) {
        console.log('Tema cambiado a:', e.detail.theme);
        // Refrescar componentes si es necesario
    });
}
```

---

### PASO 7 — Responsive

#### 7.1. Mobile-first (360px mínimo)
- **Sidebar:** Debe colapsar automáticamente
- **Tablas:** Usar `.table-responsive` para scroll horizontal
- **Cards:** Deben apilarse verticalmente
- **Tabs:** Deben ser scrollables si no caben

#### 7.2. Grid Bootstrap
```html
<div class="row g-2">
    <div class="col-md-4 col-lg-3">
        <!-- Columna -->
    </div>
    <div class="col-md-8 col-lg-9">
        <!-- Columna -->
    </div>
</div>
```

#### 7.3. Espaciado responsive
- `gap-2` (0.5rem) para elementos en fila
- `mb-3` (1rem) entre secciones
- `g-2` para grid rows

---

### PASO 8 — Librerías y Versiones

#### 8.1. Versiones oficiales (verificar en _Layout.cshtml)
- **Bootstrap:** 5.3.3
- **jQuery:** 3.7.1
- **DataTables:** 2.1.8
- **SweetAlert2:** 11
- **Phosphor Icons:** 2.1.1
- **Chart.js:** 4.4.0

#### 8.2. DataTables configuration
```javascript
$('#myTable').DataTable({
    responsive: true,
    language: {
        url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es.json'
    },
    dom: 'Bfrtip',
    buttons: [
        {
            extend: 'excel',
            text: '<i class="ph ph-download-simple me-1"></i> Excel'
        }
    ]
});
```

---

### PASO 9 — Formularios

#### 9.1. Estructura estándar
```html
<div class="mb-3">
    <label for="campo" class="form-label fw-semibold small">Nombre del Campo</label>
    <input type="text" class="form-control form-control-sm" id="campo" name="campo" required>
</div>

<div class="mb-3">
    <label class="form-label fw-semibold small">Seleccionar</label>
    <select class="form-select form-select-sm" id="select">
        <option value="">Seleccione...</option>
    </select>
</div>
```

#### 9.2. Validación
```javascript
function validateForm() {
    const campo = $('#campo').val().trim();
    if (!campo) {
        Swal.fire('Atención', 'Complete el campo obligatorio', 'warning');
        return false;
    }
    return true;
}
```

---

### PASO 10 — Checklist Final de Verificación

#### 10.1. Estructura y Archivos
- [ ] La vista usa el `_Layout.cshtml` compartido
- [ ] El JS está en `wwwroot/js/views/[nombre].js`
- [ ] No hay `<style>` tags en la vista
- [ ] Se carga el JS con `@section Scripts`

#### 10.2. CSS y Design System
- [ ] Todos los colores usan CSS variables
- [ ] No hay valores hardcodeados
- [ ] Se usan componentes oficiales (`.card-app`, `.nav-tabs-app`, etc.)
- [ ] Las sombras usan `var(--shadow-*)` o `.shadow-app-*`
- [ ] Los bordes usan `var(--radius-*)` o `.radius-app-*`

#### 10.3. Iconos
- [ ] Todos los iconos son Phosphor (`ph ph-*`)
- [ ] No hay FontAwesome o Bootstrap Icons
- [ ] El prefijo `ph ` está presente

#### 10.4. Dark Mode
- [ ] La vista funciona correctamente en dark mode
- [ ] No hay `[data-theme="dark"]` en la vista
- [ ] Los overrides específicos están en `dark-theme.css`

#### 10.5. Responsive
- [ ] Funciona en 360px de ancho
- [ ] Sidebar colapsa en móvil
- [ ] Tablas tienen scroll horizontal
- [ ] Cards se apilan verticalmente

#### 10.6. Librerías
- [ ] Versiones correctas de Bootstrap, jQuery, DataTables
- [ ] Phosphor Icons (no otros sets)
- [ ] SweetAlert2 para alertas

#### 10.7. JavaScript
- [ ] Se usa `fetch()` para AJAX
- [ ] Se usa `Swal.fire()` para alertas
- [ ] Event delegation para elementos dinámicos
- [ ] Manejo de errores con `.catch()`

---

### PASO 11 — Testing y Validación

#### 11.1. Pruebas manuales
1. **Light mode:** Verificar toda la interfaz
2. **Dark mode:** Activar y verificar cada componente
3. **Responsive:** Probar en 360px, 768px, 1024px, 1440px
4. **Interacciones:** Probar todos los botones, forms, tabs
5. **DataTables:** Verificar paginación, sort, exportación

#### 11.2. Validación de consola
- No errores JavaScript
- No warnings de CSS
- Todos los fetch completan correctamente

---

### PASO 12 — Comunes Errores a Evitar

#### 12.1. ❌ Errores comunes
- `<style>` inline en vistas
- Colores hardcodeados (`#ffffff`, `#212529`)
- Iconos incorrectos (`fa fa-*`, `bi bi-*`)
- `[data-theme="dark"]` en vistas
- Versiones mixtas de librerías
- `$.ajax()` en lugar de `fetch()`
- `alert()` en lugar de `Swal.fire()`

#### 12.2. ✅ Prácticas correctas
- Usar siempre CSS variables
- Componentes oficiales del Design System
- Phosphor Icons exclusivamente
- Dark mode en `dark-theme.css`
- `fetch()` + `Swal.fire()` para AJAX
- Event delegation para elementos dinámicos

---

### PASO 13 — Referencias Rápidas

#### 13.1. Archivos clave
- `site.css` - Variables CSS, layout, sidebar
- `components.css` - Componentes reutilizables
- `reports.css` - Estilos específicos de reportes
- `dark-theme.css` - Overrides dark mode
- `dark-theme.js` - API DarkTheme

#### 13.2. Comandos útiles
```bash
# Buscar iconos no-Phosphor
findstr /s /n "bi bi-" *.cshtml
findstr /s /n "fa fa-" *.cshtml

# Buscar colores hardcodeados
findstr /s /n "style=.*#[0-9a-fA-F]" *.cshtml

# Buscar inline styles
findstr /s /n "<style>" *.cshtml
```

---

## RESUMEN DEL WORKFLOW

1. **Analizar** requerimiento y buscar patrones existentes
2. **Planificar** componentes oficiales a usar
3. **Crear** vista Razor con estructura estándar
4. **Crear** JavaScript con fetch + Swal
5. **Aplicar** componentes oficiales (cards, tabs, tables)
6. **Usar** CSS variables para todo
7. **Verificar** dark mode functionality
8. **Testear** responsive design
9. **Validar** checklist completo
10. **Entregar** con pruebas pasadas

> **REGLA DE ORO:** Si un patrón visual aparece más de una vez, debe ser una clase reusable en `components.css` o `reports.css`.
