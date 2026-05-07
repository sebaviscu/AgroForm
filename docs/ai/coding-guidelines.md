# Guías de Codificación - AgroForm

## Estándares y Convenciones

### 1. Nomenclatura

#### C# - Backend
| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Clases | PascalCase | `CampoService`, `BaseController` |
| Interfaces | PascalCase con prefijo "I" | `ICampoService`, `IGenericRepository` |
| Métodos | PascalCase | `GetAllAsync()`, `CreateAsync()` |
| Propiedades | PascalCase | `Nombre`, `FechaInicio` |
| Campos privados | camelCase con prefijo "_" | `_logger`, `_service` |
| Variables locales | camelCase | `campoId`, `userAuth` |
| Constantes | PascalCase | `MaxRetryAttempts` |
| Enums | PascalCase | `Roles`, `TipoActividad` |

#### JavaScript - Frontend
| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Funciones | camelCase | `initializeDataTable()`, `validateForm()` |
| Variables | camelCase | `campoData`, `userSettings` |
| Constantes | UPPER_SNAKE_CASE | `API_BASE_URL`, `MAX_RETRIES` |
| Clases CSS | kebab-case | `campo-form`, `btn-primary` |
| IDs HTML | camelCase | `campoForm`, `saveButton` |

#### Base de Datos
| Tipo | Formato | Ejemplo |
|------|---------|---------|
| Tablas | PascalCase plural | `Campos`, `Actividades`, `Usuarios` |
| Columnas | PascalCase | `Nombre`, `FechaCreacion`, `IdLicencia` |
| FK Constraints | FK_TablaOrigen_TablaDestino | `FK_Campos_Licencias` |
| Índices | IX_Tabla_Columnas | `IX_Usuarios_IdLicencia_Email` |

### 2. Estructura de Archivos

#### Controllers
```csharp
namespace AgroForm.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class CampoController : BaseController<Campo, CampoDto, ICampoService>
    {
        private readonly ILogger<CampoController> _logger;
        private readonly IMapper _mapper;
        private readonly ICampoService _service;

        public CampoController(ILogger<CampoController> logger, 
                              IMapper mapper, 
                              ICampoService service) 
            : base(logger, mapper, service)
        {
            _logger = logger;
            _mapper = mapper;
            _service = service;
        }

        // Constructor
        // Actions públicas
        // Actions privadas
        // Helper methods
    }
}
```

#### Services
```csharp
namespace AgroForm.Business.Services
{
    public class CampoService : ServiceBase<Campo>, ICampoService
    {
        private readonly IGenericRepository<Campo> _repository;
        private readonly ILogger<CampoService> _logger;
        private readonly IUserContextService _userContext;

        public CampoService(IGenericRepository<Campo> repository,
                           ILogger<CampoService> logger,
                           IUserContextService userContext)
            : base(repository, logger)
        {
            _repository = repository;
            _logger = logger;
            _userContext = userContext;
        }

        // Constructor
        // Public methods (interface implementation)
        // Private helper methods
        // Validation methods
    }
}
```

#### JavaScript por Vista
```javascript
// js/views/campo.js
$(document).ready(function() {
    // Inicialización
    initializeComponents();
    bindEvents();
    setupValidation();
});

// Inicialización de componentes
function initializeComponents() {
    initializeDataTable();
    initializeModals();
    initializeSelectors();
}

// Event handlers
function bindEvents() {
    $('#saveButton').click(saveCampo);
    $('.edit-btn').click(editCampo);
    $('.delete-btn').click(deleteCampo);
}

// CRUD operations
function saveCampo() { /* ... */ }
function editCampo() { /* ... */ }
function deleteCampo() { /* ... */ }

// Helper functions
function showSuccessMessage(message) { /* ... */ }
function showErrorMessage(message) { /* ... */ }
```

### 3. Patrones de Código

#### Async/Await
```csharp
// ✅ Correcto
public async Task<IActionResult> GetAll()
{
    try
    {
        var result = await _service.GetAllAsync();
        if (!result.Success)
        {
            return BadRequest(result.ErrorMessage);
        }
        
        var dtoList = _mapper.Map<List<Campo>, List<CampoDto>>(result.Data);
        return Ok(new GenericResponse<List<CampoDto>>
        {
            Success = true,
            ListObject = dtoList,
            Message = "Datos obtenidos correctamente"
        });
    }
    catch (Exception ex)
    {
        return HandleException(ex, "Error al obtener campos", "GetAll");
    }
}

// ❌ Incorrecto (sin await, sin manejo de errores)
public IActionResult GetAll()
{
    var result = _service.GetAllAsync().Result;
    return Ok(result);
}
```

#### Validación en Services
```csharp
public async Task<OperationResult<Campo>> CreateAsync(Campo entity)
{
    // Validaciones de negocio
    var validationResult = ValidateCampo(entity);
    if (!validationResult.IsValid)
    {
        return OperationResult<Campo>.Failure(validationResult.ErrorMessage);
    }

    // Asignación de auditoría
    entity.RegistrationDate = DateTime.UtcNow;
    entity.RegistrationUser = _userContext.GetCurrentUserName();
    entity.IdLicencia = _userContext.GetCurrentLicenciaId();

    try
    {
        var result = await _repository.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("Campo creado: {CampoId} - {CampoNombre}", result.Id, result.Nombre);
        return OperationResult<Campo>.SuccessResult(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al crear campo: {CampoNombre}", entity.Nombre);
        return OperationResult<Campo>.Failure("Error al crear campo");
    }
}

private ValidationResult ValidateCampo(Campo campo)
{
    if (string.IsNullOrWhiteSpace(campo.Nombre))
        return ValidationResult.Failure("El nombre del campo es requerido");

    if (campo.Superficie <= 0)
        return ValidationResult.Failure("La superficie debe ser mayor a cero");

    if (campo.Nombre.Length > 200)
        return ValidationResult.Failure("El nombre no puede exceder 200 caracteres");

    return ValidationResult.Success();
}
```

#### Error Handling en Controllers
```csharp
protected IActionResult HandleException(Exception ex, string errorMessage, string endpoint = "")
{
    if (ex is UnauthorizedAccessException)
    {
        _logger.LogWarning("Acceso no autorizado en {Endpoint}", endpoint);
        return RedirectToAction("Login", "Access");
    }

    if (ex is ArgumentException argEx)
    {
        _logger.LogWarning("Argumento inválido en {Endpoint}: {Error}", endpoint, argEx.Message);
        return BadRequest(new GenericResponse<object>
        {
            Success = false,
            Message = argEx.Message
        });
    }

    _logger.LogError(ex, "Error no controlado en {Endpoint}: {Error}", endpoint, ex.Message);
    
    return StatusCode(StatusCodes.Status500InternalServerError, new GenericResponse<object>
    {
        Success = false,
        Message = "Error interno del servidor"
    });
}
```

### 4. Convenciones de Base de Datos

#### Entity Framework Configurations
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Configuración de Campo
    modelBuilder.Entity<Campo>(entity =>
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Nombre)
              .IsRequired()
              .HasMaxLength(200);
              
        entity.Property(e => e.Superficie)
              .HasPrecision(18, 2);
              
        entity.Property(e => e.RegistrationDate)
              .ValueGeneratedOnAdd();
              
        entity.Property(e => e.ModificationDate)
              .ValueGeneratedOnUpdate();

        // Foreign Keys
        entity.HasOne(e => e.Licencia)
              .WithMany()
              .HasForeignKey(e => e.IdLicencia)
              .OnDelete(DeleteBehavior.Cascade);
              
        entity.HasOne(e => e.Campania)
              .WithMany()
              .HasForeignKey(e => e.IdCampania)
              .OnDelete(DeleteBehavior.SetNull);

        // Índices
        entity.HasIndex(e => new { e.IdLicencia, e.Nombre })
              .IsUnique()
              .HasDatabaseName("IX_Campos_IdLicencia_Nombre");
              
        entity.HasIndex(e => e.IdCampania)
              .HasDatabaseName("IX_Campos_IdCampania");
    });
}
```

### 5. Frontend Patterns

#### DataTable Configuration
```javascript
function initializeCampoDataTable() {
    $('#campoTable').DataTable({
        ajax: {
            url: '/Campo/GetAllDataTable',
            type: 'GET',
            error: function(xhr, error, thrown) {
                console.error('DataTable error:', xhr.responseText);
                AgroForm.showError('Error al cargar datos', 'Intente nuevamente');
            }
        },
        columns: [
            { 
                data: 'id',
                visible: false,
                searchable: false
            },
            { 
                data: 'nombre',
                title: 'Nombre'
            },
            { 
                data: 'superficie',
                title: 'Superficie (ha)',
                render: $.fn.dataTable.render.number('.', ',', 2, '', ' ha')
            },
            {
                data: null,
                title: 'Acciones',
                orderable: false,
                searchable: false,
                render: function(data, type, row) {
                    return `
                        <div class="btn-group" role="group">
                            <button type="button" class="btn btn-sm btn-primary edit-btn" 
                                    data-id="${data.id}" title="Editar">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-danger delete-btn" 
                                    data-id="${data.id}" title="Eliminar">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.4/i18n/es.json'
        },
        pageLength: 25,
        responsive: true,
        processing: true,
        serverSide: false
    });
}
```

#### AJAX Calls Pattern
```javascript
function saveCampo(formData) {
    return $.ajax({
        url: '/Campo/Create',
        method: 'POST',
        data: JSON.stringify(formData),
        contentType: 'application/json',
        beforeSend: function(xhr) {
            // CSRF token
            xhr.setRequestHeader('X-CSRF-TOKEN', 
                $('input[name="__RequestVerificationToken"]').val());
            
            // Loading indicator
            $('#saveButton').prop('disabled', true)
                           .html('<i class="fas fa-spinner fa-spin"></i> Guardando...');
        },
        success: function(response) {
            if (response.success) {
                AgroForm.showSuccess('Campo creado correctamente');
                $('#campoModal').modal('hide');
                $('#campoTable').DataTable().ajax.reload();
                clearForm();
            } else {
                AgroForm.showError(response.message);
            }
        },
        error: function(xhr, status, error) {
            console.error('Save error:', xhr.responseText);
            AgroForm.showError('Error al guardar', xhr.responseText || 'Intente nuevamente');
        },
        complete: function() {
            // Restore button
            $('#saveButton').prop('disabled', false)
                           .html('Guardar');
        }
    });
}
```

### 6. Testing Patterns

#### Unit Tests
```csharp
[TestFixture]
public class CampoServiceTests
{
    private Mock<IGenericRepository<Campo>> _repositoryMock;
    private Mock<ILogger<CampoService>> _loggerMock;
    private Mock<IUserContextService> _userContextMock;
    private CampoService _service;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IGenericRepository<Campo>>();
        _loggerMock = new Mock<ILogger<CampoService>>();
        _userContextMock = new Mock<IUserContextService>();
        
        _userContextMock.Setup(x => x.GetCurrentLicenciaId()).Returns(1);
        _userContextMock.Setup(x => x.GetCurrentUserName()).Returns("testuser");
        
        _service = new CampoService(_repositoryMock.Object, 
                                   _loggerMock.Object, 
                                   _userContextMock.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidCampo_ReturnsSuccess()
    {
        // Arrange
        var campo = new Campo 
        { 
            Nombre = "Campo Test", 
            Superficie = 100.50m,
            IdLicencia = 1
        };
        
        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Campo>()))
                      .ReturnsAsync(campo);

        // Act
        var result = await _service.CreateAsync(campo);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("Campo Test", result.Data.Nombre);
        Assert.IsNotNull(result.Data.RegistrationDate);
        Assert.AreEqual("testuser", result.Data.RegistrationUser);
    }

    [Test]
    public async Task CreateAsync_WithInvalidNombre_ReturnsFailure()
    {
        // Arrange
        var campo = new Campo 
        { 
            Nombre = "", 
            Superficie = 100.50m
        };

        // Act
        var result = await _service.CreateAsync(campo);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.ErrorMessage.Contains("nombre"));
    }
}
```

### 7. Configuración y Ambiente

#### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "SQL": "Server=...;Database=AgroForm;Trusted_Connection=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "AgroForm": "Information"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "AgroForm": "Information"
      }
    }
  },
  "ApplicationSettings": {
    "MaxFileUploadSize": 10485760,
    "SessionTimeoutMinutes": 180,
    "DefaultPageSize": 25
  }
}
```

### 8. Comentarios y Documentación

#### XML Documentation
```csharp
/// <summary>
/// Servicio para la gestión de campos agrícolas
/// </summary>
/// <remarks>
/// Implementa las reglas de negocio para crear, actualizar, eliminar y consultar campos.
/// Todos los datos están aislados por licencia para garantizar el multi-tenancy.
/// </remarks>
public interface ICampoService : IServiceBase<Campo>
{
    /// <summary>
    /// Obtiene todos los campos con sus lotes y actividades asociadas
    /// </summary>
    /// <param name="idLicencia">ID de la licencia del usuario actual</param>
    /// <returns>Lista de campos con detalles completos</returns>
    Task<OperationResult<IEnumerable<Campo>>> GetWithDetailsAsync(int idLicencia);
}
```

#### Code Comments
```csharp
// Validar que la campaña esté activa antes de permitir operaciones
if (!campania.Activa)
{
    return OperationResult<Actividad>.Failure("La campaña no está activa");
}

// TODO: Implementar caching para catálogos estáticos
// FIXME: Este método necesita optimización para grandes volúmenes de datos
// NOTE: Esta validación es crítica para la integridad de datos
```

### 9. Git y Version Control

#### Commits Messages
```
feat: agregar gestión de variedades de cultivos
fix: corregir validación de superficie en lotes
docs: actualizar documentación de API
refactor: optimizar consulta de actividades
test: agregar unit tests para CampoService
chore: actualizar dependencias de NuGet
```

#### Branch Naming
```
feature/gestion-variedades
bugfix/validacion-superficie
hotfix/security-patch
release/v1.2.0
```

### 10. Performance Guidelines

#### Entity Framework
```csharp
// ✅ Correcto - Async y tracking controlado
var campos = await _context.Campos
    .Include(c => c.Lotes.Take(10)) // Limitar includes
    .Where(c => c.IdLicencia == idLicencia && c.Activo)
    .AsNoTracking() // Solo lectura
    .ToListAsync();

// ❌ Incorrecto - Síncrono y sin límites
var campos = _context.Campos
    .Include(c => c.Lotes) // Todos los lotes
    .Include(c => c.Actividades) // Todas las actividades
    .Where(c => c.IdLicencia == idLicencia)
    .ToList();
```

#### Frontend Performance
```javascript
// ✅ Correcto - Debounce para búsqueda
let searchTimeout;
$('#searchInput').on('input', function() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        searchCampos($(this).val());
    }, 300);
});

// ❌ Incorrecto - Búsqueda en cada keystroke
$('#searchInput').on('input', function() {
    searchCampos($(this).val());
});
```

### 11. Security Guidelines

#### Input Validation
```csharp
// Server-side validation
[HttpPost]
public async Task<IActionResult> Create([FromBody] CampoDto dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(new GenericResponse<object>
        {
            Success = false,
            Message = "Datos inválidos",
            Object = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
        });
    }
    
    // Sanitize input
    dto.Nombre = System.Web.HttpUtility.HtmlEncode(dto.Nombre.Trim());
    
    // Continue processing...
}
```

#### SQL Injection Prevention
```csharp
// ✅ Correcto - Parameterized queries via EF Core
var campos = await _context.Campos
    .Where(c => c.Nombre.Contains(searchTerm) && c.IdLicencia == idLicencia)
    .ToListAsync();

// ❌ Incorrecto - Raw SQL vulnerable
var sql = $"SELECT * FROM Campos WHERE Nombre LIKE '%{searchTerm}%'";
var campos = await _context.Campos.FromSqlRaw(sql).ToListAsync();
```

### 12. Code Review Checklist

#### Backend
- [ ] Métodos async/await usados correctamente
- [ ] Manejo de excepciones implementado
- [ ] Validación de inputs en controller y service
- [ ] Logging apropiado (información y errores)
- [ ] Inyección de dependencias configurada
- [ ] Tests unitarios cubren casos críticos

#### Frontend
- [ ] Validación de formularios en cliente
- [ ] Manejo de errores AJAX
- [ ] Indicadores de carga
- [ ] Responsive design
- [ ] Accesibilidad (ARIA labels)
- [ ] Performance (debounce, lazy loading)

#### Database
- [ ] Índices apropiados para queries frecuentes
- [ ] Foreign keys configuradas
- [ ] Auditoría implementada
- [ ] Migrations versionadas
- [ ] Backup strategy definida

### 13. Herramientas y Utilidades

#### Code Formatting
- **C#:** EditorConfig con .NET settings
- **JavaScript:** ESLint + Prettier
- **SQL:** SQL Formatter

#### Static Analysis
- **C#:** SonarQube, StyleCop
- **JavaScript:** JSHint, ESLint
- **Security:** OWASP ZAP

#### CI/CD Pipeline
```yaml
# azure-pipelines.yml
trigger:
- main

pool:
  vmImage: 'windows-latest'

steps:
- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    projects: '**/*.csproj'

- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: 'test'
    projects: '**/*Tests.csproj'

- task: PublishBuildArtifacts@1
  displayName: 'Publish Artifacts'
```
