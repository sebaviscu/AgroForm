# Backend - Tecnologías y Patrones

## Stack Tecnológico

### Framework Principal
- **.NET 9.0** (net9.0) - Última versión LTS
- **ASP.NET Core MVC** - Framework web
- **C# 12** - Lenguaje principal

### Base de Datos y ORM
- **Entity Framework Core 9.0.10** - ORM principal
- **SQL Server** - Motor de base de datos
- **Code-First Approach** - Migraciones automáticas

### Librerías Principales
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.10" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.10" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="Serilog" Version="4.3.0" />
<PackageReference Include="Serilog.AspNetCore" Version="9.0.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="9.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
```

## Patrones de Diseño Backend

### 1. Repository Pattern
```csharp
// Interfaz genérica
public interface IGenericRepository<T> where T : EntityBase
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}

// Implementación
public class GenericRepository<T> : IGenericRepository<T> where T : EntityBase
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
}
```

### 2. Unit of Work Pattern
```csharp
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : EntityBase;
    Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private readonly Dictionary<Type, object> _repositories;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        _repositories = new Dictionary<Type, object>();
    }
}
```

### 3. Service Layer Pattern
```csharp
// Interfaz base
public interface IServiceBase<T> where T : EntityBase
{
    Task<OperationResult<T>> GetByIdAsync(int id);
    Task<OperationResult<IEnumerable<T>>> GetAllAsync();
    Task<OperationResult<T>> CreateAsync(T entity);
    Task<OperationResult<T>> UpdateAsync(T entity);
    Task<OperationResult<bool>> DeleteAsync(int id);
}

// Implementación base
public abstract class ServiceBase<T> : IServiceBase<T> where T : EntityBase
{
    protected readonly IGenericRepository<T> _repository;
    protected readonly ILogger _logger;

    public ServiceBase(IGenericRepository<T> repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

### 4. Generic Controller Pattern
```csharp
public abstract class BaseController<TEntity, TDto, TService> : Controller
    where TEntity : EntityBase
    where TService : IServiceBase<TEntity>
{
    protected readonly ILogger _logger;
    protected readonly IMapper _mapper;
    protected readonly TService _service;

    // CRUD operations genéricas
    [HttpGet]
    public virtual async Task<IActionResult> GetAll()
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(int id)
    [HttpPost]
    public virtual async Task<IActionResult> Create([FromBody] TDto dto)
    [HttpPut]
    public virtual async Task<IActionResult> Update([FromBody] TDto dto)
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(int id)
}
```

## Arquitectura de Proyectos Backend

### AgroForm.Business (Lógica de Negocio)
```
AgroForm.Business/
├── Contracts/                    # Interfaces
│   ├── IServiceBase.cs          # Interfaz genérica
│   ├── ICampoService.cs         # Interfaces específicas
│   ├── IActividadService.cs
│   └── IExternos/               # Servicios externos
├── Services/                    # Implementaciones
│   ├── ServiceBase.cs           # Clase base genérica
│   ├── CampoService.cs          # Servicios específicos
│   ├── ActividadService.cs
│   └── Externos/                # Integraciones externas
└── StartupHelper.cs             # Configuración DI
```

### AgroForm.Data (Acceso a Datos)
```
AgroForm.Data/
├── DBContext/
│   └── AppDbContext.cs          # Contexto EF Core
├── Repository/
│   ├── GenericRepository.cs     # Repositorio genérico
│   └── UnitOfWork.cs            # Unit of Work
└── AgroForm.Data.csproj
```

### AgroForm.Model (Dominio)
```
AgroForm.Model/
├── EntityBase.cs                # Entidad base con auditoría
├── [Entities]/                 # Entidades de negocio
│   ├── Campo.cs
│   ├── Lote.cs
│   ├── Actividad.cs
│   └── Campania.cs
├── Configuraciones/             # Config del sistema
├── EnumClass.cs                # Enums
└── DTOs/                       # Data Transfer Objects
```

## Configuración de Servicios (DI Container)

### Registro en Program.cs
```csharp
// Entity Framework
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"));
});

// Repository y Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Servicios de aplicación
builder.Services.AddApplicationServices(); // Extension method en StartupHelper
```

### StartupHelper.cs
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Registro de todos los servicios
        services.AddScoped<ICampoService, CampoService>();
        services.AddScoped<IActividadService, ActividadService>();
        services.AddScoped<ICampaniaService, CampaniaService>();
        // ... más servicios
        
        return services;
    }
}
```

## Entity Framework Core Configuration

### AppDbContext
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets para cada entidad
    public DbSet<Campo> Campos { get; set; }
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<Actividad> Actividades { get; set; }
    public DbSet<Campania> Campanias { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de entidades
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        // Configuración de auditoría
        ConfigureAuditableEntities(modelBuilder);
    }

    private void ConfigureAuditableEntities(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).Property(nameof(EntityBase.RegistrationDate))
                    .ValueGeneratedOnAdd();
            }
        }
    }
}
```

### Configuración de SQL Server
```csharp
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.CommandTimeout(60);
    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
});
```

## AutoMapper Configuration

### AutoMapperProfile
```csharp
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Mapeo de Entidades → DTOs
        CreateMap<Campo, CampoDto>()
            .ForMember(dest => dest.NombreCampo, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.IdCampaniaActual, opt => opt.MapFrom(src => src.IdCampania));

        // Mapeo de DTOs → Entidades
        CreateMap<CampoDto, Campo>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreCampo))
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignorar ID en create

        // Mapeos para otras entidades...
        CreateMap<Lote, LoteDto>();
        CreateMap<Actividad, ActividadDto>();
        CreateMap<Campania, CampaniaDto>();
    }
}
```

## Logging con Serilog

### Configuración en Program.cs
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning()
    .Enrich.FromLogContext()
    .Filter.ByExcluding(logEvent => logEvent.Exception is UnauthorizedAccessException)
    .WriteTo.File(
        Path.Combine(logsDirectory, "logfile.txt"),
        rollingInterval: RollingInterval.Month,
        retainedFileCountLimit: 3,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error
    )
    .WriteTo.Console()
    .CreateLogger();
```

### Logging en Controllers
```csharp
protected IActionResult HandleException(Exception ex, string errorMessage, string endpoint = "")
{
    var logParams = new Dictionary<string, object>
    {
        { "ErrorMessage", errorMessage },
        { "ExceptionMessage", ex?.Message ?? "null" },
        { "RequestPath", HttpContext?.Request?.Path.ToString() ?? "null" }
    };

    _logger.LogError(ex, $"Error en {endpoint}: {errorMessage}", logParams);
    
    return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
}
```

## Autenticación y Autorización

### Cookie Authentication
```csharp
builder.Services.AddAuthentication("AgroFormAuth")
    .AddCookie("AgroFormAuth", options =>
    {
        options.Cookie.Name = "AgroFormAuth";
        options.LoginPath = "/Access/Login";
        options.LogoutPath = "/Access/Logout";
        options.AccessDeniedPath = "/Access/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(180);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
```

### Validación de Autorización en BaseController
```csharp
protected UserAuth ValidarAutorizacion(Roles[]? rolesPermitidos = null)
{
    if (!HttpContext.User.Identity.IsAuthenticated)
        throw new UnauthorizedAccessException("El usuario no esta autenticado");

    var userAuth = new UserAuth
    {
        UserName = UtilidadService.GetClaimValue<string>(claimUser, ClaimTypes.Name),
        IdLicencia = UtilidadService.GetClaimValue<int>(claimUser, "Licencia"),
        IdCampaña = UtilidadService.GetClaimValue<int>(claimUser, "Campania"),
        IdUsuario = UtilidadService.GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
        IdRol = UtilidadService.GetClaimValue<Roles>(claimUser, ClaimTypes.Role)
    };

    if (rolesPermitidos != null && !rolesPermitidos.Contains((Roles)userAuth.IdRol))
        throw new AccessViolationException("USUARIO CON PERMISOS INSUFICIENTES");

    return userAuth;
}
```

## Manejo de Respuestas Genéricas

### GenericResponse<T>
```csharp
public class GenericResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Object { get; set; }
    public List<T>? ListObject { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### OperationResult<T>
```csharp
public class OperationResult<T>
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static OperationResult<T> SuccessResult(T data)
    {
        return new OperationResult<T> { Success = true, Data = data };
    }

    public static OperationResult<T> Failure(string errorMessage)
    {
        return new OperationResult<T> { Success = false, ErrorMessage = errorMessage };
    }
}
```

## Validación de Entidades

### Data Annotations
```csharp
public class Campo : EntityBase
{
    [Required(ErrorMessage = "El nombre del campo es requerido")]
    [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
    public string Nombre { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "La superficie debe ser un valor positivo")]
    public decimal Superficie { get; set; }

    public int IdCampania { get; set; }
    public Campania? Campania { get; set; }
}
```

### Validación en Service Layer
```csharp
public async Task<OperationResult<Campo>> CreateAsync(Campo entity)
{
    // Validaciones de negocio
    if (string.IsNullOrWhiteSpace(entity.Nombre))
        return OperationResult<Campo>.Failure("El nombre del campo es requerido");

    if (entity.Superficie <= 0)
        return OperationResult<Campo>.Failure("La superficie debe ser mayor a cero");

    // Verificar duplicados
    var existing = await _repository.FindAsync(c => c.Nombre == entity.Nombre && c.IdLicencia == entity.IdLicencia);
    if (existing.Any())
        return OperationResult<Campo>.Failure("Ya existe un campo con ese nombre");

    // Asignar auditoría
    entity.RegistrationDate = DateTime.UtcNow;
    entity.RegistrationUser = _currentUserService.GetUserName();

    var result = await _repository.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();

    return OperationResult<Campo>.SuccessResult(result);
}
```

## Configuración de CORS y Seguridad

### CORS Policy
```csharp
builder.Services.AddCors(_ =>
{
    _.AddPolicy("NuevaPolitica", app =>
    {
        app.AllowAnyOrigin()
           .AllowAnyHeader()
           .AllowAnyMethod();
    });
});
```

### Anti-Forgery
```csharp
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
```

## Configuración de Middleware

### Pipeline en Program.cs
```csharp
// 1. Exception Handling
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. Response Compression
app.UseResponseCompression();

// 4. CORS
app.UseCors("NuevaPolitica");

// 5. Static Files
app.UseStaticFiles();

// 6. Routing
app.UseRouting();

// 7. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 8. Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");
```

## Configuración de Ambiente

### Development
```json
{
  "ConnectionStrings": {
    "SQL": "Server=localhost\\SQLEXPRESS;Database=AgroForm;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Production
```json
{
  "ConnectionStrings": {
    "SQL": "Server=tcp:production-server.database.windows.net;Initial Catalog=AgroForm;..."
  }
}
```

## Performance Optimizations

### Entity Framework
- **Query Splitting**: Para consultas complejas
- **Command Timeout**: 60 segundos
- **Retry Logic**: 5 reintentos con exponential backoff
- **Connection Pooling**: Configurado por defecto

### Application Level
- **Async/Await**: Todas las operaciones I/O
- **Response Compression**: Habilitado
- **Static File Caching**: 1 año para assets estáticos
- **DI Lifetime**: Scoped para servicios, Singleton para configuraciones

## Monitoreo y Health Checks

### Health Check Endpoint
```csharp
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow 
}));
```

### Logging Strategy
- **Error Level**: Solo errores y críticos en producción
- **Structured Logging**: Datos contextuales en cada log
- **File Rotation**: Mensual con retención de 3 archivos
- **Console Output**: Disponible en development

## Testing Strategy

### Unit Tests
```csharp
[Test]
public async Task CreateCampo_ShouldReturnSuccess_WhenValidData()
{
    // Arrange
    var campo = new Campo { Nombre = "Test Campo", Superficie = 100 };
    _repository.Setup(r => r.AddAsync(It.IsAny<Campo>())).ReturnsAsync(campo);
    
    // Act
    var result = await _service.CreateAsync(campo);
    
    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual("Test Campo", result.Data.Nombre);
}
```

### Integration Tests
```csharp
[Test]
public async Task CampoController_GetAll_ShouldReturnAllCampos()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.GetAsync("/Campo/GetAll");
    
    // Assert
    response.EnsureSuccessStatusCode();
}
```
