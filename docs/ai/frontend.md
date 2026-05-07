# Frontend - Tecnologías y Arquitectura

## Stack Tecnológico Frontend

### Framework Principal
- **ASP.NET Core MVC** - Framework de servidor
- **Razor Views** - Motor de plantillas
- **Razor Pages** - Páginas dinámicas

### Librerías JavaScript
```html
<!-- jQuery -->
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

<!-- Bootstrap -->
<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>

<!-- DataTables -->
<link href="https://cdn.datatables.net/1.13.4/css/dataTables.bootstrap5.min.css" rel="stylesheet">
<script src="https://cdn.datatables.net/1.13.4/js/jquery.dataTables.min.js"></script>
<script src="https://cdn.datatables.net/1.13.4/js/dataTables.bootstrap5.min.js"></script>

<!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
```

### CSS Framework
- **Bootstrap 5.1.3** - Framework CSS principal
- **Font Awesome** - Iconos (verificar uso)
- **CSS Custom** - Estilos específicos del proyecto

## Estructura de Archivos Frontend

### wwwroot/
```
wwwroot/
├── css/
│   └── site.css              # Estilos globales
├── js/
│   ├── site.js              # Funciones globales
│   └── views/               # JavaScript por vista
│       ├── actividad.js
│       ├── actividadRapida.js
│       ├── administradorLicencias.js
│       ├── campania.js
│       ├── campo.js
│       ├── gasto.js
│       ├── registroClima.js
│       ├── reporteCampos.js
│       ├── selectorCampania.js
│       └── usuario.js
├── images/                   # Imágenes estáticas
├── favicon.ico              # Favicon
├── manifest.json            # PWA manifest
└── sw.js                    # Service Worker
```

### Views/
```
Views/
├── Shared/                  # Layouts y componentes compartidos
│   ├── _Layout.cshtml      # Layout principal
│   ├── _LoginPartial.cshtml # Partial de login
│   └── Error.cshtml        # Página de error
├── Access/                  # Vistas de autenticación
│   ├── Login.cshtml
│   └── AccessDenied.cshtml
├── Home/                    # Dashboard principal
│   └── Index.cshtml
├── Campo/                   # Gestión de campos
│   ├── Index.cshtml
│   └── CreateEdit.cshtml
├── [Other Controllers]/     # Vistas por controlador
└── _ViewStart.cshtml        # Configuración inicial de vistas
```

## Arquitectura JavaScript

### 1. Modularización por Vista
Cada vista tiene su archivo JavaScript específico en `/js/views/`:

```javascript
// js/views/campo.js
$(document).ready(function() {
    // Inicialización específica de la vista de campos
    initializeCampoDataTable();
    setupCampoFormValidation();
    bindCampoEvents();
});

function initializeCampoDataTable() {
    $('#campoTable').DataTable({
        ajax: '/Campo/GetAllDataTable',
        columns: [
            { data: 'id' },
            { data: 'nombre' },
            { data: 'superficie' },
            { data: 'acciones' }
        ],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/es.json'
        }
    });
}
```

### 2. Funciones Globales (site.js)
```javascript
// js/site.js
window.AgroForm = {
    // Utilidades AJAX
    ajax: function(options) {
        const defaults = {
            beforeSend: function(xhr) {
                xhr.setRequestHeader('X-CSRF-TOKEN', $('input[name="__RequestVerificationToken"]').val());
            },
            error: function(xhr, status, error) {
                AgroForm.showError('Error en la solicitud', error);
            }
        };
        return $.ajax($.extend(defaults, options));
    },

    // Notificaciones
    showSuccess: function(message) {
        Swal.fire({
            icon: 'success',
            title: 'Éxito',
            text: message,
            timer: 3000
        });
    },

    showError: function(message, detail = '') {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: message,
            footer: detail ? `<small>${detail}</small>` : ''
        });
    },

    // Confirmación
    confirm: function(message, callback) {
        Swal.fire({
            title: '¿Está seguro?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Sí, eliminar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                callback();
            }
        });
    }
};
```

## Componentes UI Principales

### 1. DataTables para Listados
```html
<table id="campoTable" class="table table-striped">
    <thead>
        <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Superficie</th>
            <th>Acciones</th>
        </tr>
    </thead>
</table>
```

```javascript
$('#campoTable').DataTable({
    ajax: '/Campo/GetAllDataTable',
    processing: true,
    serverSide: true,
    columns: [
        { data: 'id', visible: false },
        { data: 'nombre' },
        { data: 'superficie', render: $.fn.dataTable.render.number('.', ',', 2, ' ha') },
        {
            data: null,
            orderable: false,
            render: function(data, type, row) {
                return `
                    <button class="btn btn-sm btn-primary edit-btn" data-id="${data.id}">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-danger delete-btn" data-id="${data.id}">
                        <i class="fas fa-trash"></i>
                    </button>
                `;
            }
        }
    ]
});
```

### 2. Formularios Modal
```html
<div class="modal fade" id="campoModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Crear/Editar Campo</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="campoForm">
                    @Html.AntiForgeryToken()
                    <div class="mb-3">
                        <label for="nombre" class="form-label">Nombre</label>
                        <input type="text" class="form-control" id="nombre" name="nombre" required>
                    </div>
                    <div class="mb-3">
                        <label for="superficie" class="form-label">Superficie (ha)</label>
                        <input type="number" class="form-control" id="superficie" name="superficie" step="0.01" required>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>
                <button type="button" class="btn btn-primary" id="saveCampo">Guardar</button>
            </div>
        </div>
    </div>
</div>
```

### 3. Selectores de Campaña
```javascript
// js/views/selectorCampania.js
function initializeCampaniaSelector() {
    $('#campaniaSelector').select2({
        placeholder: 'Seleccione una campaña',
        ajax: {
            url: '/Campania/GetAll',
            dataType: 'json',
            processResults: function(data) {
                return {
                    results: data.listObject.map(item => ({
                        id: item.id,
                        text: item.nombre
                    }))
                };
            }
        }
    });
}
```

## Patrones de Interacción

### 1. CRUD Operations Pattern
```javascript
// Create
$('#saveCampo').click(function() {
    const data = $('#campoForm').serialize();
    
    AgroForm.ajax({
        url: '/Campo/Create',
        method: 'POST',
        data: data,
        success: function(response) {
            if (response.success) {
                $('#campoModal').modal('hide');
                $('#campoTable').DataTable().ajax.reload();
                AgroForm.showSuccess('Campo creado correctamente');
            } else {
                AgroForm.showError(response.message);
            }
        }
    });
});

// Read (DataTable)
$('#campoTable').DataTable({
    ajax: '/Campo/GetAllDataTable'
});

// Update
$('.edit-btn').click(function() {
    const id = $(this).data('id');
    
    AgroForm.ajax({
        url: `/Campo/GetById/${id}`,
        success: function(response) {
            if (response.success) {
                $('#nombre').val(response.object.nombre);
                $('#superficie').val(response.object.superficie);
                $('#campoModal').modal('show');
            }
        }
    });
});

// Delete
$('.delete-btn').click(function() {
    const id = $(this).data('id');
    
    AgroForm.confirm('¿Está seguro de eliminar este campo?', function() {
        AgroForm.ajax({
            url: `/Campo/Delete/${id}`,
            method: 'DELETE',
            success: function(response) {
                if (response.success) {
                    $('#campoTable').DataTable().ajax.reload();
                    AgroForm.showSuccess('Campo eliminado correctamente');
                }
            }
        });
    });
});
```

### 2. Form Validation Pattern
```javascript
function setupCampoFormValidation() {
    $('#campoForm').validate({
        rules: {
            nombre: {
                required: true,
                maxlength: 200
            },
            superficie: {
                required: true,
                number: true,
                min: 0.01
            }
        },
        messages: {
            nombre: {
                required: 'El nombre del campo es requerido',
                maxlength: 'El nombre no puede exceder 200 caracteres'
            },
            superficie: {
                required: 'La superficie es requerida',
                number: 'Debe ser un número válido',
                min: 'La superficie debe ser mayor a cero'
            }
        },
        submitHandler: function(form) {
            saveCampo();
        }
    });
}
```

## Layout y Navegación

### _Layout.cshtml
```html
<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - AgroForm</title>
    
    <!-- CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet">
    <link href="https://cdn.datatables.net/1.13.4/css/dataTables.bootstrap5.min.css" rel="stylesheet">
    <link href="~/css/site.css" rel="stylesheet" />
</head>
<body>
    <!-- Navigation -->
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
        <div class="container">
            <a class="navbar-brand" href="/">AgroForm</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse">
                <ul class="navbar-nav me-auto">
                    <li class="nav-item">
                        <a class="nav-link" href="/Home/Index">Dashboard</a>
                    </li>
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" data-bs-toggle="dropdown">Gestión</a>
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" href="/Campo/Index">Campos</a></li>
                            <li><a class="dropdown-item" href="/Lote/Index">Lotes</a></li>
                            <li><a class="dropdown-item" href="/Actividad/Index">Actividades</a></li>
                        </ul>
                    </li>
                </ul>
                <partial name="_LoginPartial" />
            </div>
        </div>
    </nav>

    <!-- Main Content -->
    <main role="main" class="pb-3">
        <div class="container">
            @RenderBody()
        </div>
    </main>

    <!-- Scripts -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.4/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.4/js/dataTables.bootstrap5.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <script src="~/js/site.js"></script>
    @RenderSection("Scripts", required: false)
</body>
</html>
```

### _LoginPartial.cshtml
```html
@using Microsoft.AspNetCore.Identity
@inject SignInManager<IdentityUser> SignInManager
@inject UserManager<IdentityUser> UserManager

@if (SignInManager.IsSignedIn(User))
{
    <ul class="navbar-nav">
        <li class="nav-item dropdown">
            <a class="nav-link dropdown-toggle" href="#" id="userDropdown" role="button" data-bs-toggle="dropdown">
                <i class="fas fa-user"></i> @User.Identity?.Name
            </a>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="/Access/Profile">Mi Perfil</a></li>
                <li><hr class="dropdown-divider"></li>
                <li><a class="dropdown-item" href="/Access/Logout">Cerrar Sesión</a></li>
            </ul>
        </li>
    </ul>
}
else
{
    <ul class="navbar-nav">
        <li class="nav-item">
            <a class="nav-link" href="/Access/Login">Iniciar Sesión</a>
        </li>
    </ul>
}
```

## Responsive Design

### Bootstrap Grid System
```html
<div class="container">
    <div class="row">
        <div class="col-md-8">
            <!-- Contenido principal -->
        </div>
        <div class="col-md-4">
            <!-- Sidebar -->
        </div>
    </div>
</div>
```

### Mobile-First Approach
```css
/* css/site.css */
.card {
    margin-bottom: 1rem;
}

@media (max-width: 768px) {
    .table-responsive {
        font-size: 0.875rem;
    }
    
    .btn-group-vertical .btn {
        margin-bottom: 0.25rem;
    }
}
```

## Performance Optimizations

### 1. Asset Optimization
```html
<!-- Bundle CSS -->
<environment include="Development">
    <link href="~/css/site.css" rel="stylesheet" />
</environment>
<environment exclude="Development">
    <link href="~/css/site.min.css" rel="stylesheet" asp-append-version="true" />
</environment>
```

### 2. JavaScript Lazy Loading
```javascript
// Cargar scripts específicos de vista
document.addEventListener('DOMContentLoaded', function() {
    const viewName = $('body').data('view');
    if (viewName) {
        loadViewScript(`/js/views/${viewName}.js`);
    }
});

function loadViewScript(src) {
    const script = document.createElement('script');
    script.src = src;
    document.head.appendChild(script);
}
```

### 3. DataTables Server-Side Processing
```javascript
$('#dataTable').DataTable({
    processing: true,
    serverSide: true,
    ajax: {
        url: '/Controller/GetDataTable',
        type: 'POST'
    },
    columns: [
        { data: 'id' },
        { data: 'name' }
    ]
});
```

## Accessibility (WCAG)

### Semantic HTML
```html
<main role="main" aria-label="Contenido principal">
    <section aria-labelledby="campos-heading">
        <h2 id="campos-heading">Gestión de Campos</h2>
        <!-- Contenido -->
    </section>
</main>
```

### ARIA Labels
```html
<button type="button" class="btn btn-primary" 
        aria-label="Editar campo" 
        title="Editar campo">
    <i class="fas fa-edit" aria-hidden="true"></i>
</button>
```

### Keyboard Navigation
```javascript
// Manejo de eventos de teclado
$(document).on('keydown', function(e) {
    if (e.key === 'Escape') {
        $('.modal').modal('hide');
    }
});

$('.modal').on('shown.bs.modal', function() {
    $(this).find('input:visible:first').focus();
});
```

## Error Handling Frontend

### Global Error Handler
```javascript
$(document).ajaxError(function(event, xhr, settings, error) {
    console.error('AJAX Error:', {
        url: settings.url,
        status: xhr.status,
        response: xhr.responseText
    });

    if (xhr.status === 401) {
        window.location.href = '/Access/Login';
    } else if (xhr.status === 403) {
        AgroForm.showError('Acceso denegado', 'No tiene permisos para realizar esta acción');
    } else {
        AgroForm.showError('Error de comunicación', 'Intente nuevamente más tarde');
    }
});
```

### Form Validation Feedback
```javascript
function showValidationErrors(errors) {
    $('.form-control').removeClass('is-invalid');
    $('.invalid-feedback').remove();

    Object.keys(errors).forEach(key => {
        const input = $(`#${key}`);
        input.addClass('is-invalid');
        input.after(`<div class="invalid-feedback">${errors[key]}</div>`);
    });
}
```

## Testing Frontend

### JavaScript Unit Tests (Jest)
```javascript
// tests/campo.test.js
describe('Campo Management', () => {
    test('should validate campo form', () => {
        // Mock DOM
        document.body.innerHTML = `
            <form id="campoForm">
                <input id="nombre" type="text" required>
                <input id="superficie" type="number" required>
            </form>
        `;

        // Test validation
        setupCampoFormValidation();
        expect($('#campoForm').valid()).toBe(false);
    });
});
```

### E2E Tests (Playwright)
```javascript
// tests/e2e/campo.spec.js
test('should create new campo', async ({ page }) => {
    await page.goto('/Campo/Index');
    await page.click('[data-testid="new-campo-btn"]');
    await page.fill('#nombre', 'Campo Test');
    await page.fill('#superficie', '100');
    await page.click('#saveCampo');
    
    await expect(page.locator('.swal2-success')).toBeVisible();
});
```

## PWA Features

### Service Worker (sw.js)
```javascript
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open('agroform-v1').then(cache => {
            return cache.addAll([
                '/',
                '/css/site.css',
                '/js/site.js',
                '/manifest.json'
            ]);
        })
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});
```

### Manifest.json
```json
{
  "name": "AgroForm",
  "short_name": "AgroForm",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#ffffff",
  "theme_color": "#000000",
  "icons": [
    {
      "src": "/icon-192.png",
      "sizes": "192x192",
      "type": "image/png"
    }
  ]
}
```
