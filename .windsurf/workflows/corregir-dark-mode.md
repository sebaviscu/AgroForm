# Workflow: Corregir Problemas de Modo Oscuro

## Pasos para corregir problemas específicos del modo oscuro

### 1. Identificar el problema
- Activar modo oscuro en la aplicación
- Navegar a la vista/problemática específica
- Identificar elementos que se ven incorrectos (texto blanco, fondos incorrectos, etc.)

### 2. Analizar el problema
- Inspeccionar el elemento problemático con herramientas de desarrollador
- Verificar si el elemento tiene reglas CSS específicas para `[data-theme="dark"]`
- Identificar si está usando CSS custom properties correctamente

### 3. Aplicar corrección en `dark-theme.css`

#### 3.1. Para headers de campo (como `.campo-header`)
```css
[data-theme="dark"] .campo-header {
    color: var(--text) !important;
}
[data-theme="dark"] .campo-header h5 {
    color: var(--text) !important;
}
[data-theme="dark"] .campo-header .text-muted {
    color: var(--text-muted) !important;
}
```

#### 3.2. Para tablas con `table-light`
```css
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
```

#### 3.3. Para paginación de DataTables
```css
[data-theme="dark"] .dataTables_wrapper .dataTables_paginate .paginate_button {
    background-color: var(--surface-2) !important;
    border-color: var(--border) !important;
    color: var(--text) !important;
}
[data-theme="dark"] .dataTables_wrapper .dataTables_paginate .paginate_button:hover {
    background-color: var(--surface) !important;
    color: var(--text) !important;
}
```

#### 3.4. Para Choice.js
```css
[data-theme="dark"] .choices__inner {
    background-color: var(--surface-2) !important;
    border-color: var(--border) !important;
    color: var(--text) !important;
}
[data-theme="dark"] .choices__list--dropdown {
    background-color: var(--surface) !important;
    border-color: var(--border) !important;
}
[data-theme="dark"] .choices__list--dropdown .choices__item--selectable {
    color: var(--text) !important;
}
```

### 4. Verificar la corrección
- Recargar la página con modo oscuro activado
- Verificar que los elementos se muestren correctamente
- Probar cambiar entre modo claro y oscuro
- Verificar que no se rompa el modo claro

### 5. Testing adicional
- Probar en diferentes navegadores
- Verificar responsive design
- Probar con diferentes tamaños de pantalla
- Verificar que los elementos interactivos funcionen correctamente

## Problemas comunes y soluciones rápidas

### Texto blanco en headers
```css
[data-theme="dark"] .elemento-problematico {
    color: var(--text) !important;
}
[data-theme="dark"] .elemento-problematico h1,
[data-theme="dark"] .elemento-problematico h2,
[data-theme="dark"] .elemento-problematico h3,
[data-theme="dark"] .elemento-problematico h4,
[data-theme="dark"] .elemento-problematico h5,
[data-theme="dark"] .elemento-problematico h6 {
    color: var(--text) !important;
}
```

### Fondos incorrectos
```css
[data-theme="dark"] .elemento-con-fondo {
    background-color: var(--surface) !important;
}
```

### Bordes incorrectos
```css
[data-theme="dark"] .elemento-con-borde {
    border-color: var(--border) !important;
}
```

## Checklist final
- [ ] El elemento se ve correctamente en modo oscuro
- [ ] El elemento se ve correctamente en modo claro
- [ ] Los colores usan CSS custom properties
- [ ] No hay `!important` innecesarios
- [ ] La solución es consistente con el resto del tema
- [ ] Se probó en diferentes navegadores
- [ ] Se probó en responsive

## Archivos a modificar
- `AgroForm.Web/wwwroot/css/dark-theme.css` - Archivo principal para correcciones
- `AgroForm.Web/wwwroot/css/site.css` - Solo si se necesitan nuevas variables CSS
