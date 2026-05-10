# REGLA GLOBAL — AgroForm Engineering Playbook

---

## REGLAS DE DECISIÓN AI (OBLIGATORIO)

Estas reglas controlan CÓMO debe comportarse la AI. Son de cumplimiento obligatorio.

- Si existen múltiples implementaciones → elegir la MÁS USADA en el codebase.
- Si hay duda sobre qué patrón usar → DETENERSE y preguntar al usuario.
- NUNCA inventar patrones nuevos. Copiar de código existente.
- Preferir CONSISTENCIA sobre optimización.
- Preferir CÓDIGO EXISTENTE sobre abstracciones nuevas.
- Si un método ya existe en el proyecto → reutilizarlo. No crear uno nuevo.
- Si una convención existe → seguirla exactamente. No "mejorarla".
- Ante ambigüedad → preguntar. No asumir.

---

## FORMATO DE RESPUESTA (OBLIGATORIO)

Cuando se genere código, seguir este formato:

1. Explicar brevemente en español qué se va a hacer.
2. Mostrar el código.
3. Verificar que el código compila lógicamente.
4. Confirmar que sigue los patrones existentes del proyecto.

Reglas adicionales:
- NO generar código que no se haya pedido.
- NO generar alternativas salvo que se pidan.
- NO explicar conceptos básicos de C# o JS.
- Ir directo al punto.

---

## REGLAS DE SEGURIDAD PARA MODELOS PEQUEÑOS (OBLIGATORIO)

- Evitar abstracciones complejas.
- Evitar sobre-ingeniería.
- Preferir soluciones simples y directas.
- NO generar código no utilizado.
- NO asumir contexto que no se haya proporcionado.
- NO generar imports no necesarios.
- NO agregar métodos "por si acaso".
- Cada línea de código debe tener un propósito claro.
- Si la tarea es simple → la solución debe ser simple.

---

## IDIOMA (OBLIGATORIO)

- Explicaciones, razonamientos, comentarios al usuario: **español**.
- Código (variables, métodos, clases, interfaces, namespaces): **inglés OBLIGATORIO**.
- Nombres de entidades de negocio: pueden estar en **español SOLO si ya existen así** en el codebase.
- Mensajes de error/UX en la aplicación: **español** (para el usuario final).
- Commits y PRs: **español**.

---

## ARQUITECTURA DEL PROYECTO (AgroForm)

### Capas (NO modificar, NO agregar capas)

```
AgroForm.Web/              → Web (MVC Controllers, Razor Views, ViewModels, wwwroot/js)
AgroForm.Business/         → Lógica de negocio (Services, Contracts/Interfaces)
AgroForm.Model/            → Entidades, DTOs, Enums, EntityBase
AgroForm.Data/             → Acceso a datos (GenericRepository, UnitOfWork, DBContext)
AgroForm.Tests/            → Tests unitarios (xUnit)
```

### Clases base obligatorias

- **Controllers** → SIEMPRE heredan de `BaseController<TEntity, TDto, TService>`.
  - Provee: `_logger`, `_service`, `gResponse`, `CurrentUser`, `Map()`, `ValidarAutorizacion()`, `HandleException()`, `UpdateClaimAsync()`.
  - Constructor: `base(logger, service)`.
  - Usa **Mapster** (`source.Adapt<TDest>()`) para mapeo, NO AutoMapper.
  - Usa `GenericResponse<T>` para TODAS las respuestas JSON.
  - Atributo `[Authorize(AuthenticationSchemes = "AgroFormAuth")]` en cada controller.
  - Ruteo: `[Route("[controller]/[action]")]` a nivel de clase.
- **Services** → SIEMPRE heredan de `ServiceBase<T>`.
  - Constructor: `base(unitOfWork, logger, userContext)`.
  - Inyecta: `IUnitOfWork`, `ILogger<ServiceBase<T>>`, `IUserContext`.
  - Usa `_repository` (obtenido de `_unitOfWork.Repository<T>()`).
  - Usa `_userContext` para obtener datos del usuario autenticado.
  - Usa `GetQuery()` para consultas personalizadas con `Include`.
  - Usa `OperationResult<T>` / `OperationResult` para retornos.
- **Entities** → SIEMPRE heredan de `EntityBase`.
  - Provee: `Id`, `RegistrationDate`, `RegistrationUser`, `ModificationDate`, `ModificationUser`.
  - Atributo `[NonUpdatable]` para campos inmutables.
  - `EntityBaseWithLicencia` para entidades multi-tenant por licencia.
  - `IEntityBaseWithCampania` para entidades que requieren campaña.
  - `IEntityBaseWithMoneda` para entidades que requieren moneda.
- **Repository** → usar `IGenericRepository<T>` / `GenericRepository<T>`.
  - NO crear repositorios específicos. El genérico es suficiente.
- **UnitOfWork** → `IUnitOfWork` con `Repository<T>()` y `SaveAsync()`.

### Convenciones de naming (OBLIGATORIO)

| Tipo | Naming | Ubicación | Ejemplo |
|------|--------|-----------|---------|
| ViewModel | Sufijo `VM` | `AgroForm.Web/Models/` | `CampoVM` |
| Entity | Sin prefijo | `AgroForm.Model/` | `Campo` |
| DTO | Sufijo `Dto` | `AgroForm.Model/` | `LaborDTO` |
| Interfaz Service | Prefijo `I` | `Business/Contracts/` | `ICampoService` |
| Service | Sufijo `Service` | `Business/Services/` | `CampoService` |
| JS por vista | 1 archivo por vista | `wwwroot/js/views/` | `campo.js` |
| C# clases/métodos | PascalCase | — | `GetAllAsync()` |
| C# variables/params | camelCase | — | `campoId`, `userAuth` |
| C# campos privados | camelCase con prefijo `_` | — | `_logger`, `_service` |

---

## PIPELINE DE DESARROLLO

Cuando se cree, modifique o refactorice código, seguir este orden estricto:

1. **Analizar** → Leer código existente. Buscar patrones similares. Entender contexto completo.
2. **Planificar** → Presentar plan al usuario ANTES de implementar. Esperar confirmación.
3. **Implementar** → Escribir código copiando patrones existentes del proyecto.
4. **Compilar** → Ejecutar `dotnet build` para verificar que compila.
5. **Tests** → Ejecutar `dotnet test` para verificar que tests pasan.
6. **Entregar** → SOLO cuando compile y tests pasen.

Regla: NO saltar pasos.

---

## REGLAS BACKEND (C#)

### DO (Hacer siempre)

- Usar `async/await` para TODA operación de E/S.
- Usar inyección de dependencias. Registrar en `StartupHelper.ConfigurarServicios()`.
- Usar **Mapster** (`source.Adapt<TDest>()`) para mapear Entity ↔ ViewModel. Nunca mapear manualmente.
- Usar `GenericResponse<T>` para TODAS las respuestas JSON.
- Usar `TimeHelper.GetArgentinaTime()` para timestamps. Nunca `DateTime.Now`.
- Usar `ValidarAutorizacion()` al inicio de CADA acción de controller que requiera roles.
- Usar `HandleException()` en CADA bloque catch de controller.
- Controllers delgados: SOLO orquestan. Delegan a Services.
- Métodos pequeños. Responsabilidad única.
- Validar inputs al inicio del método.
- LINQ cuando mejore legibilidad. No abusar.
- Usar `OperationResult<T>.SuccessResult(data)` y `OperationResult<T>.Failure(msg, code)`.
- Usar `OperationResult.SuccessResult()` y `OperationResult.Failure(msg, code)` para métodos sin retorno de datos.
- Usar `_repository.GetAsync(filtro)` para obtener una entidad.
- Usar `_repository.GetAllAsync(filtro)` para listar entidades.
- Usar `_repository.Query()` para consultas personalizadas con `Include`, `Where`, etc.
- Usar `_unitOfWork.SaveAsync()` después de operaciones de escritura.
- Usar `.AsNoTracking()` para consultas de solo lectura.
- Usar `GetQuery()` del ServiceBase para acceder al IQueryable.
- Implementar `IEntityBaseWithCampania` si la entidad necesita filtrado por campaña.
- Implementar `IEntityBaseWithMoneda` si la entidad necesita moneda.
- Usar `_userContext.IdLicencia`, `_userContext.IdCampaña` para multi-tenancy.
- Usar `_userContext.User.Moneda` para obtener la moneda del usuario.
- Capturar excepciones en Services y retornar `OperationResult<T>.Failure()`.
- Hacer override de `ValidateAsync()` para validaciones personalizadas.

### DO NOT (No hacer nunca)

- NO usar `.Result` ni `.Wait()`. SIEMPRE `await`.
- NO poner lógica de negocio en controllers. NUNCA.
- NO mezclar entidades con ViewModels. NUNCA.
- NO hacer `catch` vacío. NO ocultar excepciones.
- NO introducir patrones nuevos de arquitectura.
- NO crear repositorios específicos sin justificación real.
- NO usar `FromSqlRaw` salvo que ya exista ese patrón.
- NO duplicar lógica. Buscar si ya existe ANTES de crear.
- NO hacer breaking changes sin pedido explícito del usuario.
- NO hardcodear strings. Usar constantes o enums existentes.
- NO crear clases/métodos "por si acaso". Solo lo necesario.
- NO usar AutoMapper. Usar Mapster (`Adapt()`).
- NO usar `Ok()` en controllers. Usar `StatusCode()` o `return Ok(gResponse)`.
- NO usar `_context` directamente desde controllers o services. Usar el Repository.

### Entity Framework Core

- Consultas async SIEMPRE: `FirstOrDefaultAsync`, `ToListAsync`.
- `.AsNoTracking()` para consultas de solo lectura.
- Evitar N+1: usar `.Include()` cuando sea necesario.
- Filtrar en la query, NO en memoria. `.Where()` antes de `.ToList()`.
- Minimizar roundtrips: agrupar queries cuando sea posible.
- Evitar múltiples enumeraciones de `IQueryable`. Materializar una sola vez.
- NO cargar navegaciones innecesarias. Solo las que se usan.
- NO usar `_context` directamente desde controllers o services. Usar el Repository.
- El filtro global de `IdLicencia` ya está configurado en `AppDbContext`.

### Manejo de errores (patrón existente)

- **Controller**: `HandleException(ex, "mensaje descriptivo", "EndpointName")` en catch.
- **Controller**: `HandleException(ex, "mensaje", "EndpointName", model)` en POST (loguea el model).
- **Service**: Retornar `OperationResult<T>.Failure("mensaje", "ERROR_CODE")` para errores de negocio.
- **Service**: Retornar `OperationResult.Failure("mensaje", "ERROR_CODE")` para métodos sin datos.
- **Auth**: `ValidarAutorizacion()` lanza `UnauthorizedAccessException` o `AccessViolationException`.
- **Logging**: `_logger.LogError()` para errores inesperados.
- Mensajes de error: claros, descriptivos, en español (para el usuario final).
- Códigos de error comunes: `"NOT_FOUND"`, `"DATABASE_ERROR"`, `"SAVE_FAILED"`, `"VALIDATION_ERROR"`, `"AUTHENTICATION_ERROR"`.

### Patrón Service CRUD (copiar exactamente)

```csharp
public class EntidadService : ServiceBase<Entidad>, IEntidadService
{
    public EntidadService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Entidad>> logger, IUserContext userContext)
        : base(unitOfWork, logger, userContext)
    {
    }

    public override async Task<OperationResult<List<Entidad>>> GetAllWithDetailsAsync()
    {
        try
        {
            var list = await GetQuery()
                .Include(e => e.Relacion)
                .AsNoTracking()
                .ToListAsync();

            return OperationResult<List<Entidad>>.SuccessResult(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer todos los registros con detalles de Entidad");
            return OperationResult<List<Entidad>>.Failure($"Ocurrió un problema al leer los registros: {ex.Message}", "DATABASE_ERROR");
        }
    }

    public override async Task<OperationResult<Entidad>> GetByIdWithDetailsAsync(int id)
    {
        try
        {
            var entity = await GetQuery()
                .Include(e => e.Relacion)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                return OperationResult<Entidad>.Failure("No se encontró el registro", "NOT_FOUND");

            return OperationResult<Entidad>.SuccessResult(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer el registro con detalles con ID {Id}", id);
            return OperationResult<Entidad>.Failure($"Ocurrió un problema al leer el registro: {ex.Message}", "DATABASE_ERROR");
        }
    }
}
```

### Patrón Controller CRUD (copiar exactamente)

```csharp
[Authorize(AuthenticationSchemes = "AgroFormAuth")]
public class EntidadController : BaseController<Entidad, EntidadVM, IEntidadService>
{
    public EntidadController(ILogger<EntidadController> logger, IEntidadService service)
        : base(logger, service)
    {
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public virtual async Task<IActionResult> MetodoPersonalizado()
    {
        try
        {
            var user = ValidarAutorizacion(new[] { Roles.Administrador });

            var result = await _service.MetodoAsync();
            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.ListObject = Map<List<Entidad>, List<EntidadVM>>(result.Data);
            gResponse.Message = "Datos obtenidos correctamente";
            return Ok(gResponse);
        }
        catch (Exception ex)
        {
            return HandleException(ex, "Error al ejecutar método", "MetodoPersonalizado");
        }
    }
}
```

### Patrón GenericResponse (copiar exactamente)

```csharp
var gResponse = new GenericResponse<VMEntidad>();
try
{
    var user = ValidarAutorizacion(new[] { Roles.Administrador });
    // ... lógica delegada al service ...
    gResponse.Success = true;
    gResponse.Object = Map<Entidad, VMEntidad>(result.Data);
    return StatusCode(StatusCodes.Status200OK, gResponse);
}
catch (Exception ex)
{
    return HandleException(ex, "Error al [descripción]", "EndpointName");
}
```

---

## REGLAS FRONTEND (Razor + jQuery + Bootstrap)

### DO (Hacer siempre)

- `$.ajax()` para llamadas AJAX (es el patrón existente en AgroForm, NO fetch).
- `DataTables` para tablas con datos dinámicos.
- `swal()` (SweetAlert) para alertas y confirmaciones.
- Verificar `response.success` para respuestas DataTable.
- Verificar `response.Success` para respuestas `GenericResponse`.
- Un archivo JS por vista en `wwwroot/js/views/`.
- Clases Bootstrap existentes.
- `$(document).ready()` como punto de entrada.
- Event delegation para botones dinámicos dentro de tablas.
- DataTable con `language: { url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json' }` para español.
- DataTable con `dataSrc: function(response) { return response.success ? response.data : []; }`.
- Usar iconos Phosphor (`ph ph-*`) en lugar de FontAwesome o MDI.
- Usar Bootstrap 5 modales.

### DO NOT (No hacer nunca)

- NO usar `fetch()` si el proyecto usa `$.ajax()`. Seguir el patrón existente.
- NO crear JS fuera de `wwwroot/js/views/` para lógica de vista.
- NO mezclar lógica de múltiples vistas en un solo JS.
- NO usar `alert()` nativo. SIEMPRE `swal()`.
- NO usar `var`. Usar `let` o `const`.
- NO agregar librerías JS nuevas sin justificación.

### Patrón DataTable (copiar exactamente)

```javascript
table = $('#tblEntidad').DataTable({
    language: {
        url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
    },
    dom: '<"row"<"col-md-6"B><"col-md-6"f>>rt<"row"<"col-md-6"l><"col-md-6"p>>',
    buttons: {
        dom: { button: { className: 'btn' } },
        buttons: [
            {
                extend: 'excel',
                text: '<i class="ph ph-file-xls me-1"></i>Excel',
                className: 'btn btn-outline-success btn-sm'
            }
        ]
    },
    ajax: {
        url: '/Entidad/GetAllDataTable',
        type: 'GET',
        dataType: 'json',
        dataSrc: function (response) {
            return response.success ? response.data : [];
        }
    },
    columns: [
        { data: 'propiedad1' },
        { data: 'propiedad2' },
        {
            data: 'id',
            className: 'text-center',
            orderable: false,
            render: function (data, type, row) {
                return `
                    <div class="btn-group btn-group-sm">
                        <button type="button" class="btn btn-outline-primary btn-edit"
                                title="Editar" data-id="${data}">
                            <i class="ph ph-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger btn-delete"
                                title="Eliminar" data-id="${data}">
                            <i class="ph ph-trash"></i>
                        </button>
                    </div>
                `;
            }
        }
    ],
    order: [[0, 'asc']],
    pageLength: 25,
    responsive: true
});
```

### Patrón AJAX con $.ajax (copiar exactamente)

```javascript
function guardarEntidad() {
    var formData = {
        Id: $('#hdnId').val(),
        Nombre: $('#txtNombre').val()
    };

    $.ajax({
        url: '/Entidad/Create',
        type: 'POST',
        data: JSON.stringify(formData),
        contentType: 'application/json',
        success: function (response) {
            if (response.success) {
                table.ajax.reload();
                $('#modalEntidad').modal('hide');
                swal('Listo', response.message || 'Operación exitosa', 'success');
            } else {
                swal('Error', response.message, 'error');
            }
        },
        error: function (xhr) {
            var response = xhr.responseJSON;
            swal('Error', response && response.message ? response.message : 'Ocurrió un error inesperado', 'error');
        }
    });
}
```

### Patrón confirmación SweetAlert (copiar exactamente)

```javascript
swal({
    title: '¿Está seguro?',
    text: 'Esta acción no se puede deshacer',
    icon: 'warning',
    buttons: ['Cancelar', 'Sí, eliminar'],
    dangerMode: true
}).then(function (confirmed) {
    if (confirmed) {
        // ejecutar acción
    }
});
```

---

## REGLAS DE TESTS UNITARIOS (xUnit)

### Estructura

- Heredar de `ServiceTestBase`.
- Usar `AddTestDataAsync()` para agregar datos de prueba.
- Usar `DbContext.Set<T>().ToListAsync<T>()` con tipo explícito.
- Usar `Assert.True(result.Success)` (NO `result.IsSuccess`).
- Usar `OperationResult<T>.Success` (NO `IsSuccess`).
- Usar `static AgroForm.Model.EnumClass` para enums.

### Patrón de Test (copiar exactamente)

```csharp
[Fact]
public async Task GetAllAsync_DebeRetornarListaVacia_CuandoNoHayDatos()
{
    // Act
    var result = await _service.GetAllAsync();

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Data);
}

[Fact]
public async Task CreateAsync_DebeGuardar_CuandoDatosSonValidos()
{
    // Arrange
    var entity = new Entidad
    {
        Nombre = "Test",
        RegistrationUser = TestUserAuth.UserName,
        RegistrationDate = TimeHelper.GetArgentinaTime()
    };

    // Act
    var result = await _service.CreateAsync(entity);

    // Assert
    Assert.True(result.Success);
    Assert.True(result.Data.Id > 0);
}
```

---

## REGLAS DE PERFORMANCE

- Evitar loops innecesarios. Preferir LINQ/batch.
- Evitar múltiples enumeraciones de colecciones.
- Paginar resultados grandes en DataTables (server-side si >1000 registros).
- NO cargar `byte[]` (fotos) en listados. Solo en detalle individual.
- Cachear lookups que no cambian frecuentemente.
- Minimizar roundtrips a BD: agrupar queries.
- `.AsNoTracking()` en queries de solo lectura.
- NO cargar navegaciones (`.Include()`) que no se usan.
- Preferir `.Select()` con proyección cuando solo se necesitan algunos campos.

---

## REGLAS DE CONSISTENCIA (MÁXIMA PRIORIDAD)

- NUNCA introducir nueva arquitectura. NUNCA.
- SIEMPRE reusar patrones existentes. SIEMPRE.
- Controllers DEBEN ser delgados. Solo orquestan.
- Lógica de negocio SOLO en Services.
- Respetar separación Entity / ViewModel.
- Respetar convenciones de naming EXACTAMENTE como están.
- Preferir CONSISTENCIA sobre "mejoras".
- ANTES de crear algo nuevo → buscar si ya existe.
- Si un patrón se usa 10 veces en el proyecto → usarlo así. No "mejorarlo".

---

## QUALITY GATES

Antes de entregar CUALQUIER cambio, verificar TODOS estos puntos:

1. ¿Compila? → `dotnet build`
2. ¿Tests pasan? → `dotnet test`
3. ¿Sigue los patrones existentes? → Comparar con código similar del proyecto.
4. ¿Cambios son mínimos y enfocados? → Verificar diff.
5. ¿No se introdujeron patrones nuevos? → Revisar imports y clases nuevas.
6. ¿Código en inglés, mensajes UX en español? → Verificar.

Si ALGUNO falla → corregir ANTES de entregar.
