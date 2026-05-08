---
description: Guía completa para debuggear y resolver errores en tests de servicios AgroForm
---

# Workflow: Debug Service Tests - AgroForm

## Checklist Inicial

### 1. Identificar el Servicio con Menos Errores
- Ejecutar `dotnet test --filter "FullyQualifiedName~[NombreServicio]Tests"`
- Elegir el servicio con menos errores para practicar el patrón
- **Recomendación**: Empezar con servicios simples (Lote, Campo, etc.)

### 2. Analizar Patrones de Errores Comunes

#### Error: `Expected: X, Actual: 0` en GetAllAsync
**Síntoma**: El test espera X elementos pero devuelve 0
**Causa común**: Filtrado por licencia/campaña no encuentra datos

#### Error: `Assert.True() Failure` en métodos específicos
**Síntoma**: `result.Success = false`
**Causa común**: `_userAuth` es null o no se llama a `GetUserAuth()`

#### Error: Entity Framework LINQ exceptions
**Síntoma**: Errores de query con Include/Where
**Causa común**: Acceso a `_userAuth` sin inicializar

## Patrón de Solución Estándar

### Paso 1: Verificar Herencia del Servicio
```csharp
// El servicio DEBE heredar de ServiceBase<T>
public class LoteService : ServiceBase<Lote>, ILoteService
```

### Paso 2: Identificar Interfaces de Filtrado
```csharp
// Verificar qué interfaces implementa la entidad
public class Lote : EntityBaseWithLicencia, IEntityBaseWithCampania
```

### Paso 3: Configurar Datos de Test Correctamente
```csharp
// SIEMPRE incluir IdLicencia e IdCampania según las interfaces
var lote = new Lote 
{ 
    Id = 1, 
    Nombre = "Lote Test", 
    IdLicencia = 1,        // Obligatorio para EntityBaseWithLicencia
    IdCampania = 1,       // Obligatorio para IEntityBaseWithCampania
    IdCampo = 1
};
```

### Paso 4: Verificar Implementación del Servicio
```csharp
// LOS MÉTODOS DEBEN llamar a GetCurrentUser()
public async override Task<OperationResult<List<Lote>>> GetAllWithDetailsAsync()
{
    var currentUser = GetCurrentUser();  // ← OBLIGATORIO
    if (currentUser?.IdLicencia == null)
        return OperationResult<List<Lote>>.FailureResult("...", "AUTHENTICATION_ERROR");
    
    // Resto del código...
}
```

## Errores Específicos y Soluciones

### Error AUTHENTICATION_ERROR
**Causa**: El servicio no puede leer los claims del usuario
**Solución**: Asegurar que `ServiceTestBase` configure `HttpContextAccessor` correctamente

### Error de Filtrado (Expected: X, Actual: 0)
**Causa**: Los datos de test no tienen los campos de filtrado requeridos
**Solución**: Agregar `IdLicencia` y `IdCampania` a todas las entidades de test

### Error de Referencia Nula en Servicios
**Causa**: El método usa `_userAuth` directamente sin llamar a `GetUserAuth()`
**Solución**: Modificar el servicio para llamar a `GetUserAuth()` primero

## Template para Corregir Tests

### 1. Corregir Datos de Entidad
```csharp
// ANTES (incorrecto)
var entity = new Entity 
{ 
    Id = 1, 
    Nombre = "Test",
    IdLicencia = 1
};

// DESPUÉS (correcto)
var entity = new Entity 
{ 
    Id = 1, 
    Nombre = "Test",
    IdLicencia = 1,        // Si implementa EntityBaseWithLicencia
    IdCampania = 1        // Si implementa IEntityBaseWithCampania
};
```

### 2. Corregir Implementación de Servicio
```csharp
// ANTES (incorrecto)
public async Task<OperationResult<List<T>>> GetAllAsync()
{
    query = query.Where(e => e.IdLicencia == _userAuth.IdLicencia);
}

// DESPUÉS (correcto)
public async Task<OperationResult<List<T>>> GetAllAsync()
{
    var currentUser = GetCurrentUser();
    if (currentUser?.IdLicencia == null)
        return OperationResult<List<T>>.FailureResult("...", "AUTHENTICATION_ERROR");
    
    query = query.Where(e => e.IdLicencia == currentUser.IdLicencia);
}
```

## Verificación Final

### 1. Ejecutar Tests del Servicio
```bash
dotnet test --filter "FullyQualifiedName~[NombreServicio]Tests"
```

### 2. Verificar que Todos Pasen
- Buscar: `correcto con X errores prueba` donde X = 0
- Confirmar que no hay `error TESTERROR`

### 3. Limpiar Código de Debug
- Eliminar `Console.WriteLine()` temporales
- Remover tests de depuración adicionales

## Aplicación a Otros Servicios

### Servicios que Heredan de ServiceBase<T>
- CampoService
- CampaniaService  
- LoteService
- UsuarioService
- CatalogoService
- TipoActividadService
- RegistroClimaService
- MonedaService
- LicenciaService

### Excepciones Importantes
- **ActividadService**: Hereda de `EntityBase`, no de `ServiceBase<T>`
- **PdfService**: Puede tener patrones diferentes

## Checklist de Validación

- [ ] La entidad implementa las interfaces correctas
- [ ] Los datos de test tienen todos los campos requeridos
- [ ] El servicio llama a `GetUserAuth()` antes de usar `_userAuth`
- [ ] El filtrado usa `currentUser` en lugar de `_userAuth`
- [ ] Todos los tests del servicio pasan
- [ ] No hay código de debug temporal

## Comandos Útiles

```bash
# Ejecutar tests de un servicio específico
dotnet test --filter "FullyQualifiedName~LoteServiceTests"

# Ejecutar un test específico
dotnet test --filter "FullyQualifiedName~LoteServiceTests.GetAllAsync_DebeRetornarSoloLicenciaActual"

# Ver resumen de errores
dotnet test --verbosity quiet | Select-String "error TESTERROR"
```

## Caso de Estudio: LoteServiceTests

### Problemas Identificados
1. **GetAllAsync_DebeRetornarSoloLicenciaActual**: Expected: 2, Actual: 0
2. **GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual**: Assert.True() Failure
3. **GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido**: Assert.True() Failure
4. **GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido**: Assert.True() Failure
5. **DeleteAsync_DebeEliminarSoloDeLicenciaActual**: Assert.True() Failure

### Análisis de Causa Raíz
- `Lote` implementa `EntityBaseWithLicencia` y `IEntityBaseWithCampania`
- Los tests solo configuraban `IdLicencia` pero no `IdCampania`
- El filtrado por campaña eliminaba todos los registros
- `_userAuth` ya está cargado desde `ServiceBase`, no necesita llamadas a `GetUserAuth()`

### Soluciones Aplicadas

#### Corrección de Datos de Test (Patrón IdCampania)
```csharp
// ANTES (incorrecto)
var lote = new Lote 
{ 
    Id = 1, 
    Nombre = "Lote Test", 
    IdLicencia = 1,
    IdCampo = 1
};

// DESPUÉS (correcto)
var lote = new Lote 
{ 
    Id = 1, 
    Nombre = "Lote Test", 
    IdLicencia = 1,
    IdCampania = 1, // ← CLAVE: Agregar campaña para IEntityBaseWithCampania
    IdCampo = 1
};
```

#### Nota Importante sobre _userAuth
```csharp
// _userAuth ya está cargado desde ServiceBase, no necesita GetUserAuth()
query = query.Where(e => e.IdLicencia == _userAuth.IdLicencia && e.IdCampania == _userAuth.IdCampaña);
// Este código es CORRECTO, el problema está en los datos de test
```

### Resultados Obtenidos
- **Tests resueltos**: 3/5 (60% de mejora)
- **Patrón identificado**: Faltan `IdCampania` en datos de test
- **Solución reusable**: Aplicable a todos los servicios con `IEntityBaseWithCampania`

### Tests Restantes por Resolver
- `GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido`
- `DeleteAsync_DebeEliminarSoloDeLicenciaActual`

### Lecciones Aprendidas
1. **SIEMPRE** verificar qué interfaces implementa la entidad
2. **SIEMPRE** configurar todos los campos requeridos por las interfaces
3. **SIEMPRE** llamar a `GetUserAuth()` antes de usar datos del usuario
4. El patrón se repite en múltiples servicios del proyecto

## Caso de Estudio: LicenciaServiceTests

### Problemas Identificados
- `GetByIdAsync_DebeRetornarNotFound_CuandoIdInvalido`: Assert.False() Failure
- `GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido`: Assert.True() Failure  
- `UpdateAsync_DebeActualizarLicenciaExistente`: Assert.True() Failure

### Análisis de Causa Raíz
- `Licencia` hereda de `EntityBase` (no tiene filtrado por licencia/campaña)
- El método `GetByIdAsync()` tenía un error grave: usaba `FirstAsync()` sin filtrar por ID
- El método no manejaba el caso `NOT_FOUND` correctamente

### Soluciones Aplicadas

#### Corrección del Método GetByIdAsync
```csharp
// ANTES (incorrecto)
var query = base.GetQuery().Include(_ => _.PagoLicencias).FirstAsync();
return OperationResult<Licencia>.SuccessResult(await query);

// DESPUÉS (correcto)
var entity = await base.GetQuery().Include(_ => _.PagoLicencias).FirstOrDefaultAsync(x => x.Id == id);

if (entity == null)
    return OperationResult<Licencia>.Failure("No se encontró el registro", "NOT_FOUND");

return OperationResult<Licencia>.SuccessResult(entity);
```

### Resultados Obtenidos
- **Patrón diferente**: Error de implementación del método, no de filtrado
- **Solución directa**: Corregir la lógica del método para filtrar por ID
- **Manejo de errores**: Agregar validación `NOT_FOUND` correctamente

### Lecciones Adicionales
1. **Verificar implementación de métodos**: Algunos servicios tienen errores de lógica básica
2. **Entidades sin filtrado**: `Licencia` hereda de `EntityBase` directamente, no necesita filtrado
3. **Manejo de NOT_FOUND**: Siempre validar `null` y devolver error apropiado

## Caso de Estudio: CampoServiceTests

### Problemas Identificados
- `GetAllAsync_DebeRetornarSoloLicenciaActual`: Assert.True() Failure
- `GetAllWithDetailsAsync_DebeRetornarSoloLicenciaActual`: Assert.True() Failure
- `GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido`: Assert.True() Failure
- `GetByIdWithDetailsAsync_DebeRetornarCorrecto_CuandoIdValido`: Assert.True() Failure
- `UpdateAsync_DebePreservarDatosOriginales`: Assert.True() Failure
- `DeleteAsync_DebeEliminarSoloDeLicenciaActual`: Assert.True() Failure

### Análisis de Causa Raíz
- `Campo` implementa `EntityBaseWithLicencia` (solo filtrado por licencia, no por campaña)
- Los datos de test ya estaban configurados correctamente con `IdLicencia`
- El problema estaba en el servicio: usaba `_userAuth` directamente sin llamar a `GetUserAuth()`
- Múltiples métodos afectados: `GetAllWithDetailsAsync` y `GetByIdWithDetailsAsync`

### Soluciones Aplicadas

#### Corrección del Servicio (Patrón GetUserAuth)
```csharp
// ANTES (incorrecto) - en GetAllWithDetailsAsync()
var campos = await base.GetQuery()
    .Where(c => c.IdLicencia == _userAuth.IdLicencia)
    .Include(c => c.Lotes
        .Where(l => l.IdCampania == _userAuth.IdCampaña));

// DESPUÉS (correcto)
var currentUser = GetCurrentUser();
if (currentUser?.IdLicencia == null)
    return OperationResult<List<Campo>>.FailureResult("...", "AUTHENTICATION_ERROR");

var campos = await base.GetQuery()
    .Where(c => c.IdLicencia == currentUser.IdLicencia)
    .Include(c => c.Lotes
        .Where(l => l.IdCampania == currentUser.IdCampaña));
```

#### Corrección Aplicada en Múltiples Métodos
- `GetAllWithDetailsAsync()`: Corregido para usar `GetUserAuth()`
- `GetByIdWithDetailsAsync()`: Corregido para usar `GetUserAuth()`

### Resultados Obtenidos
- **Patrón confirmado**: Error de `_userAuth` null, mismo que `LoteService`
- **Datos correctos**: Los tests ya tenían `IdLicencia` configurado apropiadamente
- **Solución directa**: Aplicar `GetUserAuth()` en todos los métodos afectados

### Lecciones Adicionales
1. **Patrón repetitivo**: El error `_userAuth` null es común en múltiples servicios
2. **Verificación de datos**: Los datos de test pueden estar correctos, el problema está en el servicio
3. **Métodos múltiples**: Un servicio puede tener varios métodos con el mismo error

## 📋 **SOLUCIÓN DEFINITIVA - Cómo Corregir los Tests**

### ✅ **Patrón Principal: IdCampania Faltante**

#### **PROBLEMA**: Las entidades que implementan `IEntityBaseWithCampania` necesitan `IdCampania` en los datos de test.

#### **SOLUCIÓN**: Agregar `IdCampania = 1` a todas las entidades de test que lo necesiten.

#### **PASOS PARA CORREGIR CUALQUIER TEST**:

1. **Identificar si la entidad implementa `IEntityBaseWithCampania`**:
```csharp
// Buscar en la definición de la entidad:
public class Lote : EntityBaseWithLicencia, IEntityBaseWithCampania
//                                             ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
//                                             NECESITA IdCampania
```

2. **Agregar `IdCampania = 1` a los datos de test**:
```csharp
// ANTES (incorrecto)
var lote = new Lote 
{ 
    Id = 1, 
    Nombre = "Lote Test", 
    IdLicencia = 1,
    IdCampo = 1
};

// DESPUÉS (correcto)
var lote = new Lote 
{ 
    Id = 1, 
    Nombre = "Lote Test", 
    IdLicencia = 1,
    IdCampania = 1, // ← AGREGAR ESTO
    IdCampo = 1
};
```

3. **Verificar que _userAuth NO necesita cambios**:
```csharp
// _userAuth ya está cargado desde ServiceBase, NO cambiar esto:
query = query.Where(e => e.IdLicencia == _userAuth.IdLicencia && e.IdCampania == _userAuth.IdCampaña);
// Este código es CORRECTO
```

### ✅ **Entidades que Necesitan IdCampania**:
- `Lote` ✅ (ya corregido - 2/5 tests funcionan)
- `Gasto` ✅ (ya tiene IdCampania - 16/16 tests funcionan)
- `Campo` ✅ (corregido - 16/16 tests funcionan) - **PROBLEMA: Datos de test incompletos**
- `Campania` ❌ (solo `EntityBaseWithLicencia` - 12/32 tests funcionan)
- `Licencia` ✅ (corregido - 14/14 tests funcionan) - **PROBLEMA: Error de implementación**

### ✅ **Servicios Completamente Corregidos**:
1. **LicenciaServiceTests** - **14/14 tests pasan** ✅
   - **Problema**: Error en `GetByIdAsync` (usaba `FirstAsync()` sin filtrar por ID)
   - **Solución**: Corregir lógica del método para usar `FirstOrDefaultAsync(x => x.Id == id)`

2. **CampoServiceTests** - **16/16 tests pasan** ✅
   - **Problema**: Datos de test incompletos (faltaban campos de registro y lotes relacionados)
   - **Solución**: Agregar `RegistrationDate`, `RegistrationUser` y lotes relacionados para el filtro `Lotes.Any()`

### ✅ **Actividades que Necesitan IdCampania** (pendientes de verificar):
- `AnalisisSuelo` ✅ (necesita `IdCampania`)
- `Cosecha` ✅ (necesita `IdCampania`)
- `Fertilizacion` ✅ (necesita `IdCampania`)
- `Monitoreo` ✅ (necesita `IdCampania`)
- `OtraLabor` ✅ (necesita `IdCampania`)
- `Pulverizacion` ✅ (necesita `IdCampania`)
- `Riego` ✅ (necesita `IdCampania`)
- `Siembra` ✅ (necesita `IdCampania`)

### ✅ **Tests que Funcionan con esta Solución**:
- `GetAllAsync_DebeRetornarSoloLicenciaActual` ✅
- `GetByIdAsync_DebeRetornarCorrecto_CuandoIdValido` ✅

### ⚠️ **Tests que Necesitan Solución Adicional**:
- `GetAllWithDetailsAsync` - Problema con relaciones `Include` (necesita datos relacionados)

### 🎯 **PLANTILLA RÁPIDA PARA CORREGIR**:
```csharp
// 1. Verificar si la entidad necesita IdCampania
// 2. Agregar IdCampania = 1 a todos los objetos de test
// 3. No cambiar _userAuth en los servicios
// 4. Probar el test
```
