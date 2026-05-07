# Arquitectura Técnica de AgroForm

## Arquitectura General

AgroForm sigue una arquitectura **N-Tier** con separación clara de responsabilidades:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                      │
│                AgroForm.Web (MVC)                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐    │
│  │ Controllers │  │ Views       │  │ Models/VMs      │    │
│  └─────────────┘  └─────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Business Layer                           │
│                AgroForm.Business                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐    │
│  │ Services    │  │ Contracts   │  │ Validation      │    │
│  └─────────────┘  └─────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                     Data Layer                              │
│                  AgroForm.Data                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐    │
│  │ Repository  │  │ UnitOfWork  │  │ DBContext       │    │
│  └─────────────┘  └─────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Model Layer                              │
│                  AgroForm.Model                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐    │
│  │ Entities    │  │ DTOs        │  │ Enums           │    │
│  └─────────────┘  └─────────────┘  └─────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## Patrones de Diseño Implementados

### 1. Repository Pattern
```csharp
public interface IGenericRepository<T> where T : EntityBase
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

### 2. Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : EntityBase;
    Task<int> SaveChangesAsync();
}
```

### 3. Service Layer Pattern
```csharp
public interface IServiceBase<T> where T : EntityBase
{
    Task<OperationResult<T>> GetByIdAsync(int id);
    Task<OperationResult<IEnumerable<T>>> GetAllAsync();
    Task<OperationResult<T>> CreateAsync(T entity);
    Task<OperationResult<T>> UpdateAsync(T entity);
    Task<OperationResult<bool>> DeleteAsync(int id);
}
```

### 4. Generic Controller Pattern
```csharp
public abstract class BaseController<TEntity, TDto, TService> : Controller
    where TEntity : EntityBase
    where TService : IServiceBase<TEntity>
```

## Flujo de Datos End-to-End

### 1. Flujo de Lectura (GET)
```
Browser Request
    ↓
ASP.NET Core Middleware Pipeline
    ↓
Controller Action (BaseController.GetAll)
    ↓
Service Layer (Service.GetAllAsync)
    ↓
Repository (GenericRepository.GetAllAsync)
    ↓
Entity Framework Core
    ↓
SQL Server Database
    ↓
Response (JSON/DataTable)
```

### 2. Flujo de Escritura (POST/PUT)
```
Browser Request (FormData/JSON)
    ↓
Model Binding Validation
    ↓
Controller Action (BaseController.Create/Update)
    ↓
AutoMapper (DTO → Entity)
    ↓
Service Layer (Business Rules)
    ↓
Repository (GenericRepository.AddAsync/UpdateAsync)
    ↓
Entity Framework Core (Change Tracking)
    ↓
SQL Server Database
    ↓
Response (Success/Error)
```

## Componentes Arquitectónicos

### 1. Middleware Pipeline
```csharp
// Orden de ejecución en Program.cs
1. Exception Handler Middleware
2. HTTPS Redirection Middleware
3. Static Files Middleware
4. Routing Middleware
5. Authentication Middleware
6. Authorization Middleware
```

### 2. Dependency Injection Container
```csharp
// Servicios registrados en Program.cs
- DbContextFactory
- GenericRepository
- UnitOfWork
- Application Services
- AutoMapper
- HttpContextAccessor
```

### 3. Authentication & Authorization
```csharp
// Cookie-based Authentication
services.AddAuthentication("AgroFormAuth")
    .AddCookie("AgroFormAuth", options => {
        options.LoginPath = "/Access/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(180);
    });

// Role-based Authorization
[Authorize(Roles = "Administrador")]
public class AdminController : BaseController
```

## Estructura de Proyectos

### AgroForm.Web (Presentation Layer)
```
AgroForm.Web/
├── Controllers/           # Controladores MVC
│   ├── BaseController.cs  # Controlador genérico base
│   ├── AccessController.cs # Autenticación
│   └── [Entity]Controller.cs # CRUD por entidad
├── Views/                 # Razor Views
│   ├── Shared/           # Layouts y partials
│   └── [Controller]/     # Vistas por controlador
├── Models/               # ViewModels
├── Utilities/            # Helper classes
├── Components/           # View Components
└── wwwroot/             # Static assets
    ├── css/
    ├── js/
    └── images/
```

### AgroForm.Business (Business Layer)
```
AgroForm.Business/
├── Contracts/            # Interfaces de servicios
│   ├── IServiceBase.cs
│   ├── I[Entity]Service.cs
│   └── IExternos/       # Servicios externos
├── Services/            # Implementaciones de servicios
│   ├── ServiceBase.cs
│   ├── [Entity]Service.cs
│   └── Externos/
└── StartupHelper.cs     # Configuración de DI
```

### AgroForm.Data (Data Layer)
```
AgroForm.Data/
├── DBContext/
│   └── AppDbContext.cs  # Contexto EF Core
├── Repository/
│   ├── GenericRepository.cs
│   └── UnitOfWork.cs
└── AgroForm.Data.csproj
```

### AgroForm.Model (Domain Layer)
```
AgroForm.Model/
├── EntityBase.cs         # Entidad base con auditoría
├── [Entities]/          # Entidades de dominio
├── Configuraciones/     # Configuraciones del sistema
├── EnumClass.cs         # Enums del sistema
├── TimeHelper.cs        # Utilidades de tiempo
└── DTOs/               # Data Transfer Objects
```

## Configuración de Infraestructura

### 1. Base de Datos
- **Motor**: SQL Server
- **ORM**: Entity Framework Core 9.0
- **Connection String**: Configurable por ambiente
- **Migrations**: Code-First approach

### 2. Logging
- **Framework**: Serilog
- **Sinks**: File (rolling) + Console
- **Level**: Error (production) / Information (development)
- **Format**: Structured logging

### 3. Caching
- **Response Caching**: Static assets (1 año)
- **Data Caching**: No implementado (considerar para futuro)

### 4. Security
- **Authentication**: Cookie-based
- **Authorization**: Role-based
- **CSRF Protection**: Built-in
- **HTTPS**: Enforced en producción

## Decisiones Arquitectónicas

### ¿Por qué Generic Repository?
- **Consistencia**: Mismo patrón para todas las entidades
- **Mantenibilidad**: Cambios centralizados
- **Testabilidad**: Fácil de mockear

### ¿Por qué Service Layer?
- **Separación de responsabilidades**: Business rules separadas de data access
- **Reutilización**: Lógica compartida entre controllers
- **Validación**: Centralización de reglas de negocio

### ¿Por qué BaseController?
- **DRY Principle**: Evita repetición de CRUD operations
- **Consistencia**: Mismo comportamiento en todos los controllers
- **Mantenimiento**: Cambios en un solo lugar

## Consideraciones de Escalabilidad

### 1. Base de Datos
- **Connection Pooling**: Configurado por defecto EF Core
- **Query Optimization**: Split query para consultas complejas
- **Retry Logic**: Configurado para fallos temporales

### 2. Aplicación
- **Async/Await**: Todas las operaciones I/O son asíncronas
- **DI Container**: Lifetime management adecuado
- **Memory Management**: Dispose patterns implementados

### 3. Frontend
- **Static Assets**: CDN-ready con cache headers
- **JavaScript**: Modular por vista
- **CSS**: Bootstrap para consistencia

## Patrones de Comunicación

### 1. Controller → Service
```csharp
var result = await _service.GetByIdAsync(id);
if (!result.Success) return BadRequest(result.ErrorMessage);
```

### 2. Service → Repository
```csharp
var entity = await _repository.GetByIdAsync(id);
if (entity == null) return OperationResult<T>.Failure("Not found");
```

### 3. Frontend → Backend (AJAX)
```javascript
fetch('/Campo/GetAll', {
    method: 'GET',
    headers: { 'X-CSRF-TOKEN': token }
})
.then(response => response.json())
```

## Manejo de Errores

### 1. Global Exception Handler
```csharp
app.UseExceptionHandler("/Home/Error");
```

### 2. Controller Level
```csharp
protected IActionResult HandleException(Exception ex, string errorMessage)
```

### 3. Service Layer
```csharp
return OperationResult<T>.Failure(errorMessage);
```

## Testing Strategy

### 1. Unit Tests
- **Services**: Mock repositories
- **Controllers**: Mock services
- **Business Logic**: Isolated testing

### 2. Integration Tests
- **Repository**: In-memory database
- **API Endpoints**: Test server

### 3. E2E Tests
- **User Workflows**: Playwright/Selenium
- **Database Integration**: Real database

## Deployment Architecture

### 1. Development
- **Local IIS Express**: HTTPS en puerto 5001
- **Local SQL Server**: Development database

### 2. Production
- **IIS**: HTTPS con certificado SSL
- **SQL Server**: Production database
- **Logging**: File-based con rotation
