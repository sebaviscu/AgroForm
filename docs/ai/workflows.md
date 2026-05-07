# Workflows Operativos - Guía para IA

## Flujo de Trabajo para Desarrollo

### 1. Análisis Inicial de Requerimientos

#### Pasos a Seguir
1. **Leer el requerimiento cuidadosamente**
2. **Identificar el módulo afectado** (Campo, Lote, Actividad, etc.)
3. **Determinar el tipo de cambio**:
   - Nueva funcionalidad
   - Bug fix
   - Refactorización
   - Mejora de performance

#### Preguntas Clave
- ¿Qué entidades de dominio están involucradas?
- ¿Qué reglas de negocio aplican?
- ¿Qué permisos de usuario son necesarios?
- ¿Impacta en la base de datos?

### 2. Navegación del Proyecto

#### Estructura de Archivos por Módulo
```
Módulo Ejemplo: Campo
├── AgroForm.Web/
│   ├── Controllers/CampoController.cs
│   ├── Views/Campo/
│   │   ├── Index.cshtml
│   │   └── CreateEdit.cshtml
│   └── wwwroot/js/views/campo.js
├── AgroForm.Business/
│   ├── Contracts/ICampoService.cs
│   └── Services/CampoService.cs
├── AgroForm.Model/
│   ├── Campo.cs
│   └── DTOs/CampoDto.cs
└── AgroForm.Data/
    └── (Repository genérico)
```

#### Archivos a Revisar Primero
1. **Entity** (`AgroForm.Model/Campo.cs`)
2. **DTO** (`AgroForm.Model/DTOs/CampoDto.cs`)
3. **Service Interface** (`AgroForm.Business/Contracts/ICampoService.cs`)
4. **Service Implementation** (`AgroForm.Business/Services/CampoService.cs`)
5. **Controller** (`AgroForm.Web/Controllers/CampoController.cs`)
6. **View** (`AgroForm.Web/Views/Campo/Index.cshtml`)
7. **JavaScript** (`AgroForm.Web/wwwroot/js/views/campo.js`)

### 3. Entendimiento de Features Existentes

#### Método de Análisis
1. **Leer el Controller** para entender endpoints disponibles
2. **Revisar el Service** para ver lógica de negocio
3. **Examinar la Entity** para entender estructura de datos
4. **Analizar la View** para entender UI/UX
5. **Estudiar el JavaScript** para ver interacciones cliente

#### Ejemplo: Entender Gestión de Campos
```csharp
// 1. Controller - Endpoints disponibles
GET /Campo/GetAllDataTable
GET /Campo/GetAll
GET /Campo/GetById/{id}
POST /Campo/Create
PUT /Campo/Update
DELETE /Campo/Delete/{id}

// 2. Service - Lógica de negocio
ValidarNombreUnico()
ValidarSuperficiePositiva()
AsignarAuditoría()

// 3. Entity - Estructura
Id, IdLicencia, IdCampania, Nombre, Superficie, Ubicación

// 4. View - UI
DataTable con acciones CRUD
Modal para crear/editar
Form validation

// 5. JavaScript - Interacciones
DataTable initialization
AJAX calls
Form validation
Modal management
```

### 4. Debugging de Problemas

#### Flujo de Debugging
1. **Reproducir el error**
2. **Identificar el layer afectado**:
   - Frontend (JavaScript/View)
   - Controller
   - Service
   - Repository/Database
3. **Analizar logs** (`logs/logfile.txt`)
4. **Revisar validaciones**
5. **Verificar permisos**
6. **Checkear datos**

#### Herramientas de Debugging
```csharp
// Logging en Service
_logger.LogInformation("Procesando campo: {CampoId} - {CampoNombre}", campo.Id, campo.Nombre);
_logger.LogError("Error al validar campo: {Error}", validationResult.ErrorMessage);

// Try-catch en Controller
try
{
    var result = await _service.CreateAsync(entity);
    return Ok(result);
}
catch (Exception ex)
{
    return HandleException(ex, "Error al crear campo", "Create");
}

// Console logging en JavaScript
console.log('Campo data:', campoData);
console.error('AJAX error:', xhr.responseText);
```

### 5. Rastreo de Flujo de Datos End-to-End

#### Ejemplo: Creación de Campo
```
1. Frontend (JavaScript)
   └── saveCampo() → AJAX POST /Campo/Create

2. ASP.NET Core Middleware
   └── Authentication → Authorization → Routing

3. Controller (CampoController.Create)
   └── ModelState validation
   └── AutoMapper: CampoDto → Campo
   └── _service.CreateAsync(entity)

4. Service (CampoService.CreateAsync)
   └── ValidateCampo(entity)
   └── AssignAuditFields(entity)
   └── _repository.AddAsync(entity)
   └── _unitOfWork.SaveChangesAsync()

5. Repository (GenericRepository.AddAsync)
   └── _context.Set<Campo>().AddAsync(entity)

6. Entity Framework Core
   └── Change tracking
   └── SQL generation

7. SQL Server
   └── INSERT INTO Campos...

8. Response Flow
   └── Database → Repository → Service → Controller → JSON Response → Frontend
```

### 6. Identificación de Dependencias

#### Tipos de Dependencias
1. **Directas**: Referencias explícitas en código
2. **Indirectas**: A través de interfaces o eventos
3. **Transaccionales**: Base de datos y Unit of Work
4. **Configuración**: App settings y DI container

#### Mapeo de Dependencias
```csharp
// CampoController depende de:
- ICampoService (Business logic)
- IMapper (DTO mapping)
- ILogger (Logging)

// CampoService depende de:
- IGenericRepository<Campo> (Data access)
- ILogger<CampoService> (Logging)
- IUserContextService (Current user context)

// Campo Entity depende de:
- EntityBase (Audit fields)
- Licencia (Multi-tenancy)
- Campania (Campaign context)
```

### 7. Validación de Cambios Antes de Implementar

#### Checklist Pre-Implementación
- [ ] **Impact Analysis**: ¿Qué otras partes del sistema afecta?
- [ ] **Backward Compatibility**: ¿Rompe algo existente?
- [ ] **Database Changes**: ¿Requiere migration?
- [ ] **UI Changes**: ¿Afecta vistas existentes?
- [ ] **API Changes**: ¿Cambia endpoints?
- [ ] **Testing**: ¿Qué tests necesitan actualización?

#### Ejemplo: Agregar campo "UbicaciónGPS" a Campo
```csharp
// Impact Analysis:
1. Model/Campo.cs - Agregar propiedad
2. DTOs/CampoDto.cs - Agregar propiedad
3. Database - Nueva columna + migration
4. Views/Campo/CreateEdit.cshtml - Agregar campo al formulario
5. js/views/campo.js - Validación y manejo
6. CampoService.cs - Validación de formato GPS
7. Tests - Actualizar unit tests

// Backward Compatibility:
- Columna nullable para datos existentes
- Default value null
- No breaking changes en API
```

### 8. Implementación de Nuevas Funcionalidades

#### Pasos Estandarizados
1. **Model Changes**
   ```csharp
   // 1. Entity
   public class Campo : EntityBase
   {
       // ... propiedades existentes
       public string? UbicacionGPS { get; set; }
   }

   // 2. DTO
   public class CampoDto
   {
       // ... propiedades existentes
       public string? UbicacionGPS { get; set; }
   }
   ```

2. **Database Migration**
   ```bash
   dotnet ef migrations add AddUbicacionGPSToCampo
   dotnet ef database update
   ```

3. **Service Layer**
   ```csharp
   // Validation
   private ValidationResult ValidateUbicacionGPS(string? gps)
   {
       if (!string.IsNullOrEmpty(gps))
       {
           // Validar formato GPS: -xx.xxxxx,xx.xxxxx
           var regex = new Regex(@"^-?\d{1,3}\.\d{1,6},-?\d{1,3}\.\d{1,6}$");
           if (!regex.IsMatch(gps))
               return ValidationResult.Failure("Formato GPS inválido");
       }
       return ValidationResult.Success();
   }
   ```

4. **Controller Layer**
   ```csharp
   // No changes needed si hereda de BaseController
   // Solo si hay validación específica
   ```

5. **View Layer**
   ```html
   <div class="mb-3">
       <label for="ubicacionGps" class="form-label">Ubicación GPS</label>
       <input type="text" class="form-control" id="ubicacionGps" 
              name="ubicacionGps" placeholder="-38.123456,-57.654321">
   </div>
   ```

6. **JavaScript Layer**
   ```javascript
   function validateGPS(gps) {
       const regex = /^-?\d{1,3}\.\d{1,6},-?\d{1,3}\.\d{1,6}$/;
       return regex.test(gps);
   }
   ```

7. **Testing**
   ```csharp
   [Test]
   public void CreateCampo_WithValidGPS_ReturnsSuccess()
   {
       var campo = new Campo { UbicacionGPS = "-38.123456,-57.654321" };
       var result = _service.CreateAsync(campo);
       Assert.IsTrue(result.Success);
   }
   ```

### 9. Manejo de Bugs Comunes

#### Bug Type 1: Validation Errors
```csharp
// Síntoma: ModelState always invalid
// Causa: Missing [FromBody] or incorrect naming
// Solución:
[HttpPost]
public async Task<IActionResult> Create([FromBody] CampoDto dto) // Add [FromBody]
```

#### Bug Type 2: Null Reference Exceptions
```csharp
// Síntoma: NullReferenceException in Service
// Causa: Missing null checks
// Solución:
if (entity == null) return OperationResult<Campo>.Failure("Entity not found");
```

#### Bug Type 3: Database Connection Issues
```csharp
// Síntoma: Timeout errors
// Causa: Connection string or database down
// Solución:
// Check appsettings.json
// Verify database connectivity
// Add retry logic in EF Core configuration
```

#### Bug Type 4: AJAX Issues
```javascript
// Síntoma: 400 Bad Request
// Causa: Missing CSRF token or incorrect content type
// Solución:
headers: {
    'Content-Type': 'application/json',
    'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
}
```

### 10. Performance Optimization

#### Database Optimization
```csharp
// Bad: N+1 query problem
var campos = await _context.Campos.ToListAsync();
foreach (var campo in campos)
{
    campo.Lotes = await _context.Lotes.Where(l => l.IdCampo == campo.Id).ToListAsync();
}

// Good: Eager loading with limits
var campos = await _context.Campos
    .Include(c => c.Lotes.Take(10))
    .AsNoTracking()
    .ToListAsync();
```

#### Frontend Optimization
```javascript
// Bad: Search on every keystroke
$('#searchInput').on('input', function() {
    search($(this).val());
});

// Good: Debounced search
let searchTimeout;
$('#searchInput').on('input', function() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => search($(this).val()), 300);
});
```

### 11. Testing Workflow

#### Unit Testing
```csharp
// 1. Arrange
var mockRepo = new Mock<IGenericRepository<Campo>>();
var service = new CampoService(mockRepo.Object, loggerMock.Object, userContextMock.Object);

// 2. Act
var result = await service.CreateAsync(validCampo);

// 3. Assert
Assert.IsTrue(result.Success);
mockRepo.Verify(x => x.AddAsync(It.IsAny<Campo>()), Times.Once);
```

#### Integration Testing
```csharp
// Test full workflow
[Test]
public async Task CampoController_Create_ReturnsSuccess()
{
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var response = await client.PostAsJsonAsync("/Campo/Create", new CampoDto { 
        Nombre = "Test Campo", 
        Superficie = 100 
    });
    
    response.EnsureSuccessStatusCode();
}
```

### 12. Deployment Workflow

#### Pre-Deployment Checklist
- [ ] **Database Migrations**: Applied and tested
- [ ] **Configuration**: Environment-specific settings
- [ ] **Dependencies**: All NuGet packages updated
- [ ] **Tests**: All tests passing
- [ ] **Performance**: Load testing if needed
- [ ] **Security**: Security scan completed
- [ ] **Backup**: Current system backed up

#### Deployment Steps
1. **Staging Deployment**
2. **Smoke Testing**
3. **Production Deployment**
4. **Health Checks**
5. **Monitoring Setup**

### 13. Troubleshooting Guide

#### Common Issues and Solutions

##### Issue: "User not authenticated"
```
Check:
1. Cookie present in browser
2. Cookie not expired
3. Authentication middleware configured
4. HTTPS in production
```

##### Issue: "Database connection failed"
```
Check:
1. Connection string correct
2. Database server accessible
3. Credentials valid
4. Firewall rules
5. SQL Server running
```

##### Issue: "CSRF token missing"
```
Check:
1. @Html.AntiForgeryToken() in form
2. X-CSRF-TOKEN header in AJAX
3. Anti-forgery configured in Startup
```

##### Issue: "DataTable not loading"
```
Check:
1. AJAX URL correct
2. Server returning valid JSON
3. JavaScript errors in console
4. Network tab for failed requests
```

### 14. Code Review Process

#### Review Checklist
- [ ] **Functionality**: Works as expected
- [ ] **Performance**: No obvious performance issues
- [ ] **Security**: No security vulnerabilities
- [ ] **Code Quality**: Follows coding standards
- [ ] **Tests**: Adequate test coverage
- [ ] **Documentation**: Code documented where needed
- [ ] **Error Handling**: Proper error handling implemented

#### Review Comments Format
```
## Issues
- [Critical] SQL injection vulnerability in line 45
- [Major] Missing null check in service method
- [Minor] Inconsistent naming convention

## Suggestions
- Consider using async/await for database operations
- Add input validation for user inputs
- Implement caching for frequently accessed data

## Positive Notes
+ Good separation of concerns
+ Proper error handling
+ Comprehensive unit tests
```

### 15. Continuous Improvement

#### Metrics to Track
- **Code Coverage**: Target >80%
- **Performance**: Response time <200ms for APIs
- **Bug Rate**: <5 bugs per sprint
- **Technical Debt**: Maintain <2 days

#### Regular Tasks
- **Weekly**: Code reviews, dependency updates
- **Monthly**: Security scans, performance testing
- **Quarterly**: Architecture review, tech debt assessment
