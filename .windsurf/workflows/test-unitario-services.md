---
description: Guía completa para crear tests unitarios de Services en AgroForm
---

# Workflow: Creación de Tests Unitarios para Services

## 🚀 CHECKLIST RÁPIDO - ERRORES COMUNES

### Antes de empezar, revisa estos puntos comunes:

1. **❓ Missing using statements**
   - Agregar: `using Microsoft.EntityFrameworkCore;`
   - Agregar: `using static AgroForm.Model.EnumClass;`

2. **❓ OperationResult.Success vs IsSuccess**
   - Usar: `Assert.True(result.Success);`
   - NO usar: `Assert.True(result.IsSuccess);`

3. **❓ ToListAsync() genérico**
   - Usar: `await DbContext.Set<Siembra>().ToListAsync<Siembra>();`
   - NO usar: `await DbContext.Set<Siembra>().ToListAsync();`

4. **❓ Enum incorrecto**
   - Usar: `Roles.Administrador`
   - NO usar: `Roles.Admin`

5. **❓ Propiedad nullable**
   - Usar: `= null!;`
   - NO dejar sin inicializar

6. **❓ IDbContextFactory**
   - Usar: `Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext>`

7. **❓ DbContext logging**
   - Agregar mock: `var dbContextLoggerMock = new Mock<ILogger<AppDbContext>>();`

8. **❓ LINQ con _userAuth**
   - Capturar en variables: `var idLicencia = _userAuth.IdLicencia;`

9. **❓ HttpContextAccessor Mock**
   - Configurar DESPUÉS de construir ServiceProvider
   - Usar: `HttpContextAccessorMock.Object` en constructor del test

## Pasos para Crear Tests Unitarios

### 1. Estructura del Proyecto de Tests

```bash
# El proyecto ya existe: AgroForm.Tests
# Estructura de carpetas:
AgroForm.Tests/
├── Services/
│   ├── ServiceTestBase.cs          # Clase base común
│   ├── [NombreService]Tests.cs     # Tests específicos
│   └── ...                        # Otros tests de services
```

### 2. Configuración Inicial del Test

#### 2.1. Heredar de ServiceTestBase
```csharp
public class [NombreService]Tests : ServiceTestBase
{
    private [NombreService] _service;

    public [NombreService]Tests()
    {
        var unitOfWork = GetService<IUnitOfWork>();
        var logger = GetService<ILogger<[NombreService]>>();
        var httpContextAccessor = GetService<IHttpContextAccessor>();
        
        _service = new [NombreService](
            // Parámetros del constructor del service
        );
    }
}
```

#### 2.2. Configurar Dependencies en ServiceTestBase
- DbContext en memoria
- UnitOfWork con GenericRepository
- Mocks para Logger y HttpContextAccessor
- Usuario de prueba autenticado

### 3. Patrones de Tests Comunes

#### 3.1. Test de Método que Retorna Lista
```csharp
[Fact]
public async Task Get[Entidades]Async_DebeRetornarListaVacia_CuandoNoHayDatos()
{
    // Act
    var result = await _service.Get[Entidades]Async();

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Data);
}
```

#### 3.2. Test con Datos de Prueba
```csharp
[Fact]
public async Task Get[Entidades]Async_DebeRetornarDatos_CuandoExistenEntidades()
{
    // Arrange
    var entidad = new [Entidad]
    {
        // Propiedades requeridas
        RegistrationUser = TestUserAuth.UserName,
        RegistrationDate = TimeHelper.GetArgentinaTime()
    };
    await AddTestDataAsync(entidad);

    // Act
    var result = await _service.Get[Entidades]Async();

    // Assert
    Assert.True(result.Success);
    Assert.NotNull(result.Data);
    Assert.Single(result.Data);
}
```

#### 3.3. Test de Guardado
```csharp
[Fact]
public async Task Save[Entidad]Async_DebeGuardar_CuandoDatosSonValidos()
{
    // Arrange
    var entidad = new [Entidad] { /* datos */ };

    // Act
    var result = await _service.Save[Entidad]Async(entidad);

    // Assert
    Assert.True(result.Success);
    Assert.True(result.Data);
    
    // Verificar persistencia
    var guardado = await DbContext.Set<[Entidad]>().ToListAsync<[Entidad]>();
    Assert.Single(guardado);
}
```

#### 3.4. Test con Filtros
```csharp
[Fact]
public async Task Get[Entidades]Async_DebeFiltrarPor[Campo]_CuandoSeEspecifica[Campo]()
{
    // Arrange
    var entidad1 = new [Entidad] { [Campo] = valor1 };
    var entidad2 = new [Entidad] { [Campo] = valor2 };
    await AddTestDataAsync(entidad1);
    await AddTestDataAsync(entidad2);

    // Act
    var result = await _service.Get[Entidades]Async([campo]: valor1);

    // Assert
    Assert.True(result.Success);
    Assert.Single(result.Data);
    Assert.Equal(valor1, result.Data.First().[Campo]);
}
```

## Errores Comunes y Soluciones

### Error 1: `CS0246: El nombre del tipo o del espacio de nombres 'X' no se encontró`

**Causa**: Falta de directivas using

**Solución**:
```csharp
// Agregar estos using al principio del archivo de tests:
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;
```

### Error 2: `CS1061: "OperationResult<T>" no contiene una definición para "IsSuccess"`

**Causa**: La propiedad se llama `Success`, no `IsSuccess`

**Solución**:
```csharp
// Incorrecto:
Assert.True(result.IsSuccess);

// Correcto:
Assert.True(result.Success);
```

### Error 3: `CS0411: Los argumentos de tipo para el método 'ToListAsync<TSource>' no se pueden inferir`

**Causa**: Entity Framework Core necesita tipo explícito en tests

**Solución**:
```csharp
// Incorrecto:
var entidades = await DbContext.Set<Entidad>().ToListAsync();

// Correcto:
var entidades = await DbContext.Set<Entidad>().ToListAsync<Entidad>();
```

### Error 4: `CS0117: 'EnumClass.Roles' no contiene una definición para 'Admin'`

**Causa**: Nombre incorrecto del enum

**Solución**:
```csharp
// Verificar el nombre correcto del enum en EnumClass.cs
// Usar:
IdRol = Roles.Administrador  // No "Admin"
```

### Error 5: `CS8618: El elemento propiedad que no acepta valores NULL debe contener un valor distinto de NULL`

**Causa**: Propiedad nullable sin inicializar

**Solución**:
```csharp
// Agregar inicialización por defecto:
protected UserAuth TestUserAuth { get; private set; } = null!;
```

### Error 6: `IDbContextFactory<>` no se encontró

**Causa**: Namespace incorrecto

**Solución**:
```csharp
// Usar el namespace completo:
GetService<Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext>>()
```

### Error 7: DbContext no tiene logging

**Causa**: Falta de logging en DbContext

**Solución**:
```csharp
// Agregar logging mock para DbContext:
var dbContextLoggerMock = new Mock<ILogger<AppDbContext>>();
services.AddSingleton(dbContextLoggerMock.Object);
```

### Error 8: LINQ con _userAuth

**Causa**: Problema con LINQ y _userAuth

**Solución**:
```csharp
// Capturar valores en variables locales:
var idLicencia = _userAuth.IdLicencia;
var idCampania = _userAuth.IdCampaña;
  
var list = await context.Set<Siembra>()
    .Where(_ => _.IdLicencia == idLicencia && _.IdCampania == idCampania)
    .ToListAsync();
```

### Error 9: HttpContextAccessor Mock no configurado correctamente

**Causa**: El mock del HttpContextAccessor no está configurado cuando se crea el servicio

**Solución**:
  1. Configurar el mock DESPUÉS de construir el ServiceProvider
  2. Usar el mock configurado en el constructor del test
  3. Asegurar que el HttpContext tenga User con Claims válidos
  
  ```csharp
  // En ServiceTestBase:
  protected ServiceTestBase()
  {
      // ... configuración de servicios ...
      ServiceProvider = services.BuildServiceProvider();
      DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
      DbContext.Database.EnsureCreated();
      
      // Configurar el mock DESPUÉS de construir el ServiceProvider
      SetupTestUser();
  }

  // En el test:
  public ActividadServiceTests()
  {
      var unitOfWork = GetService<IUnitOfWork>();
      var logger = GetService<ILogger<ActividadService>>();
      var httpContextAccessor = HttpContextAccessorMock.Object; // Usar el mock configurado
      
      _actividadService = new ActividadService(
          GetService<IDbContextFactory<AppDbContext>>(),
          logger,
          httpContextAccessor,
          unitOfWork);
  }
  ```

## Convenciones de Nomenclatura

### Tests
- **Nombre del método**: `[MetodoTesteado]_[Condicion]_[ResultadoEsperado]`
- **Ejemplo**: `GetLaboresByAsync_DebeRetornarListaVacia_CuandoNoHayActividades()`
- **Idioma**: Español para descripciones, Inglés para código

### Archivos
- **Nombre**: `[NombreService]Tests.cs`
- **Ubicación**: `AgroForm.Tests/Services/`

## Configuración de Datos de Prueba

### Entidades Relacionadas
Siempre crear las entidades dependientes primero:
```csharp
// Orden correcto:
var campania = new Campania { Id = 1, Nombre = "Campaña 2024", IdLicencia = 1 };
var campo = new Campo { Id = 1, Nombre = "Campo Test", IdLicencia = 1 };
var lote = new Lote { Id = 1, Nombre = "Lote Test", IdCampo = 1, Campo = campo };

await AddTestDataAsync(campania);
await AddTestDataAsync(campo);
await AddTestDataAsync(lote);

// Luego la entidad principal:
var siembra = new Siembra { IdCampania = 1, IdLote = 1, /* ... */ };
await AddTestDataAsync(siembra);
```

### Propiedades Obligatorias
Siempre incluir:
```csharp
var entidad = new Entidad
{
    // Propiedades específicas
    RegistrationUser = TestUserAuth.UserName,
    RegistrationDate = TimeHelper.GetArgentinaTime()
};
```

## Ejecución de Tests

### Compilar y Ejecutar
```bash
# Compilar proyecto de tests
dotnet build AgroForm.Tests/AgroForm.Tests.csproj

# Ejecutar todos los tests
dotnet test AgroForm.Tests/

# Ejecutar tests específicos
dotnet test AgroForm.Tests/ --filter "FullyQualifiedName~ActividadServiceTests"
```

### Debug de Tests
- Usar breakpoints en el método de test
- Inspeccionar variables en Arrange/Act/Assert
- Verificar estado de DbContext con `DbContext.ChangeTracker.DebugView`

## Mejores Prácticas

### 1. Un Test por Escenario
- Un assert principal por test
- Tests cortos y enfocados
- Nombres descriptivos

### 2. Arrange-Act-Assert
```csharp
[Fact]
public void Metodo_DebeHacerAlgo_CuandoCondicion()
{
    // Arrange: Configurar datos y mocks
    // Act: Ejecutar método bajo prueba
    // Assert: Verificar resultados
}
```

### 3. Datos de Prueba Consistentes
- Usar `TestUserAuth` para datos de usuario
- IDs secuenciales y predecibles
- Fechas con `TimeHelper.GetArgentinaTime()`

### 4. Limpieza Automática
- `ServiceTestBase` maneja limpieza automática
- Cada test tiene base de datos aislada
- No compartir estado entre tests

## Checklist Antes de Commitear

- [ ] Todos los tests compilan sin errores
- [ ] Tests pasan exitosamente
- [ ] Nomenclatura sigue convenciones
- [ ] Datos de prueba son realistas
- [ ] Tests cubren casos principales y edge cases
- [ ] No hay código duplicado innecesario
- [ ] Mocks están configurados correctamente
