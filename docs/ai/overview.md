# AgroForm - Visión General del Sistema

## Descripción del Sistema

AgroForm es una aplicación web de gestión agrícola diseñada para administrar campañas, campos, lotes, actividades y gastos en el sector agropecuario. El sistema permite a los productores agropecuarios llevar un control detallado de sus operaciones agrícolas.

## Propósito Principal

- **Gestión de Campañas Agrícolas**: Administrar ciclos de producción por campaña
- **Control de Campos y Lotes**: Organizar la división territorial de las explotaciones
- **Seguimiento de Actividades**: Registrar tareas agrícolas (siembra, cosecha, fertilización, etc.)
- **Control de Gastos**: Administrar costos asociados a las operaciones
- **Gestión de Recursos**: Control de cultivos, variedades y estados fenológicos
- **Reportes y Análisis**: Generación de informes de cierre de campaña

## Arquitectura General

```
AgroForm/
├── AgroForm.Web/          # Presentación Web (MVC + Razor Views)
├── AgroForm.Business/     # Lógica de Negocio (Services + Contracts)
├── AgroForm.Data/         # Acceso a Datos (EF Core + Repository)
└── AgroForm.Model/        # Modelos y Entidades
```

## Tecnologías Principales

### Backend
- **.NET 9.0** - Framework principal
- **ASP.NET Core MVC** - Framework web
- **Entity Framework Core 9.0** - ORM
- **SQL Server** - Base de datos
- **AutoMapper** - Mapeo de objetos
- **Serilog** - Logging estructurado

### Frontend
- **Razor Views** - Motor de vistas
- **JavaScript/jQuery** - Interactividad cliente
- **Bootstrap** - Framework CSS
- **DataTables** - Componentes de tabla

### Infraestructura
- **Autenticación por Cookies** - Sistema de login
- **Inyección de Dependencias** - DI Container
- **Repository Pattern** - Acceso a datos
- **Unit of Work** - Gestión de transacciones

## Módulos Principales

### 1. Gestión de Usuarios y Licencias
- Sistema multi-tenant por licencia
- Roles de usuario (Administrador, Operador, etc.)
- Control de acceso basado en roles

### 2. Gestión de Campañas
- Configuración de períodos de campaña
- Asignación de usuarios a campañas
- Control de estados de campaña

### 3. Gestión de Campos y Lotes
- División territorial de explotaciones
- Asignación de cultivos por lote
- Control de superficies

### 4. Registro de Actividades
- Tipos de actividades predefinidas
- Asignación de recursos (insumos, maquinaria)
- Seguimiento de estados fenológicos

### 5. Control Financiero
- Registro de gastos por campaña/campo/lote
- Categorización de costos
- Control de monedas

### 6. Sistema de Reportes
- Cierre de campaña
- Análisis de costos
- Reportes de productividad

## Características Clave

### Seguridad
- Autenticación basada en cookies
- Sistema de roles y permisos
- Protección CSRF
- Validación de acceso por licencia

### Multi-tenancy
- Aislamiento de datos por licencia
- Configuración independiente por tenant
- Control de usuarios por licencia

### Internacionalización
- Configuración regional español (es-ES)
- Formatos de fecha y moneda localizados

### Logging y Auditoría
- Registro de operaciones críticas
- Auditoría de cambios (usuario y fecha)
- Logs estructurados con Serilog

## Flujo de Trabajo Típico

1. **Login del usuario** → Validación de credenciales y asignación de claims
2. **Selección de campaña** → Configuración del contexto de trabajo
3. **Operaciones CRUD** → Gestión de entidades según permisos
4. **Reportes** → Generación de informes de gestión
5. **Logout** → Cierre de sesión y limpieza

## Puntos de Entrada Principales

- **URL Base**: `/Access/Login` - Página de login
- **Dashboard**: `/Home/Index` - Panel principal
- **API Endpoints**: `/[Controller]/[Action]` - Endpoints AJAX
- **Health Check**: `/health` - Verificación de estado

## Contexto de Negocio

El sistema está diseñado para productores agropecuarios que necesitan:
- Control preciso de costos por campaña
- Trazabilidad de operaciones agrícolas
- Análisis de rentabilidad
- Cumplimiento de normativas del sector
- Gestión multi-campo y multi-campaña
