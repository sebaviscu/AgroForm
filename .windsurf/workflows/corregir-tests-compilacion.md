---
description: Corrección sistemática de errores de compilación en tests de servicios AgroForm
---

# Workflow: Corrección de Errores de Compilación en Tests de Servicios

## Objetivo
Corregir errores de compilación en tests de servicios causados por propiedades inexistentes en las entidades del modelo.

## Pasos Systemáticos

### 1. Análisis Inicial
- Ejecutar `dotnet test` para identificar errores de compilación
- Revisar mensajes de error para identificar propiedades inexistentes
- Leer archivos de modelo para verificar estructura real de entidades

### 2. Correcciones por Entidad

#### GastoServiceTests.cs
**Problemas comunes:**
- Propiedad `Nombre` no existe → usar `TipoGasto`
- Propiedad `Descripcion` no existe → usar `Observacion`
- Propiedad `Monto` no existe → usar `Costo`
- Propiedad `Activo` no existe → eliminar
- Enum `TipoGastoEnum` no reconocido → usar `EnumClass.TipoGastoEnum`

**Correcciones:**
```csharp
// ANTES (incorrecto)
var gasto = new Gasto 
{ 
    Nombre = "Combustible",
    Descripcion = "Gasto de combustible",
    Monto = 1000,
    Activo = true
};

// DESPUÉS (correcto)
var gasto = new Gasto 
{ 
    TipoGasto = EnumClass.TipoGastoEnum.Combustible,
    Observacion = "Gasto de combustible",
    Costo = 1000
};
```

#### LicenciaServiceTests.cs
**Problemas comunes:**
- Propiedad `Nombre` no existe → usar `RazonSocial`
- Propiedad `Descripcion` no existe → usar `NombreContacto`

**Correcciones:**
```csharp
// ANTES (incorrecto)
var licencia = new Licencia 
{ 
    Nombre = "Empresa Test",
    Descripcion = "Contacto Test",
    Activo = true
};

// DESPUÉS (correcto)
var licencia = new Licencia 
{ 
    RazonSocial = "Empresa Test",
    NombreContacto = "Contacto Test",
    Activo = true
};
```

#### LoteServiceTests.cs
**Problemas comunes:**
- Propiedad `Superficie` no existe → usar `SuperficieHectareas`
- Propiedad `Descripcion` no existe → eliminar
- Propiedad `Activo` no existe → eliminar

**Correcciones:**
```csharp
// ANTES (incorrecto)
var lote = new Lote 
{ 
    Nombre = "Lote Test",
    Descripcion = "Descripción test",
    Superficie = 100,
    Activo = true
};

// DESPUÉS (correcto)
var lote = new Lote 
{ 
    Nombre = "Lote Test",
    SuperficieHectareas = 100
};
```

#### CampoServiceTests.cs
**Problemas comunes:**
- Propiedad `Descripcion` no existe → usar `Ubicacion`
- Propiedad `Activo` no existe → eliminar
- Propiedad `IdCampania` no existe → eliminar

**Correcciones:**
```csharp
// ANTES (incorrecto)
var campo = new Campo 
{ 
    Nombre = "Campo Test",
    Descripcion = "Descripción campo",
    Activo = true,
    IdCampania = 1
};

// DESPUÉS (correcto)
var campo = new Campo 
{ 
    Nombre = "Campo Test",
    Ubicacion = "Ubicación campo"
};
```

#### CampaniaServiceTests.cs
**Problemas comunes:**
- Propiedad `Descripcion` no existe → usar `EstadosCampania` y `FechaInicio`
- Propiedad `Activo` no existe → eliminar
- Propiedad `IdCampania` no existe → eliminar

**Correcciones:**
```csharp
// ANTES (incorrecto)
var campania = new Campania 
{ 
    Nombre = "Campaña Test",
    Descripcion = "Descripción campaña",
    Activo = true,
    IdCampania = 1
};

// DESPUÉS (correcto)
var campania = new Campania 
{ 
    Nombre = "Campaña Test",
    EstadosCampania = EnumClass.EstadosCamapaña.Iniciada,
    FechaInicio = DateTime.Now
};
```

#### UsuarioServiceTests.cs
**Problemas comunes:**
- Propiedad `Usuario` no existe → eliminar
- Propiedad `IdCampania` no existe → eliminar

**Correcciones:**
```csharp
// ANTES (incorrecto)
var usuario = new Usuario 
{ 
    Nombre = "Juan Pérez",
    Email = "juan@ejemplo.com",
    Usuario = "juanp",
    IdCampania = 1,
    Activo = true
};

// DESPUÉS (correcto)
var usuario = new Usuario 
{ 
    Nombre = "Juan Pérez",
    Email = "juan@ejemplo.com",
    Activo = true
};
```

#### RegistroClimaServiceTests.cs
**Problemas comunes:**
- Propiedades `Temperatura`, `Humedad`, `Lluvia` no existen → usar `Milimetros`
- Propiedad `Activo` no existe → eliminar
- Enum `TipoClima` no reconocido → usar `EnumClass.TipoClima`

**Correcciones:**
```csharp
// ANTES (incorrecto)
var registro = new RegistroClima 
{ 
    Fecha = DateTime.Now,
    Temperatura = 25,
    Humedad = 60,
    Lluvia = 10,
    Activo = true
};

// DESPUÉS (correcto)
var registro = new RegistroClima 
{ 
    Fecha = DateTime.Now,
    Milimetros = 10,
    TipoClima = EnumClass.TipoClima.Lluvia,
    Observaciones = "Registro de lluvia",
    IdCampo = 1
};
```

#### CierreCampaniaServiceTests.cs
**Problemas comunes:**
- Propiedades `NombreLicencia`, `Periodo` no existen → usar `NombreCampania`, `FechaCreacion`
- Propiedad `Activo` no existe → eliminar

**Correcciones:**
```csharp
// ANTES (incorrecto)
var reporte = new ReporteCierreCampania 
{ 
    NombreLicencia = "Licencia Test",
    Periodo = "2024",
    Activo = true
};

// DESPUÉS (correcto)
var reporte = new ReporteCierreCampania 
{ 
    NombreCampania = "Campaña 2024",
    FechaCreacion = DateTime.Now,
    FechaInicio = DateTime.Now,
    FechaFin = DateTime.Now.AddMonths(6)
};
```

#### MonedaServiceTests.cs
**Problemas comunes:**
- Propiedad `Activo` no existe → eliminar

**Correcciones:**
```csharp
// ANTES (incorrecto)
var moneda = new Moneda 
{ 
    Codigo = "USD",
    Nombre = "Dólar",
    Simbolo = "$",
    Activo = true
};

// DESPUÉS (correcto)
var moneda = new Moneda 
{ 
    Codigo = "USD",
    Nombre = "Dólar",
    Simbolo = "$"
};
```

#### AjusteServiceTests.cs
**Problemas comunes:**
- Propiedades `Nombre`, `Descripcion`, `Activo` no existen → eliminar (solo hereda de EntityBaseWithLicencia)

**Correcciones:**
```csharp
// ANTES (incorrecto)
var ajuste = new Ajuste 
{ 
    Nombre = "Ajuste Test",
    Descripcion = "Descripción test",
    Activo = true
};

// DESPUÉS (correcto)
var ajuste = new Ajuste 
{ 
    // Solo propiedades de EntityBaseWithLicencia
};
```

### 3. Correcciones de Enums

#### TipoGastoEnum
```csharp
// ANTES (incorrecto)
TipoGasto = TipoGastoEnum.Combustible

// DESPUÉS (correcto)
TipoGasto = EnumClass.TipoGastoEnum.Combustible
```

**Valores válidos:** `Sueldo`, `Combustible`, `Mantenimiento`, `Impuestos`, `Otros`

#### TipoClima
```csharp
// ANTES (incorrecto)
TipoClima = TipoClima.Lluvia

// DESPUÉS (correcto)
TipoClima = EnumClass.TipoClima.Lluvia
```

**Valores válidos:** `Lluvia`, `Granizo`

### 4. Correcciones de Accesibilidad

#### Servicios con visibilidad incorrecta
```csharp
// ANTES (incorrecto)
internal class CampaniaService : ServiceBase<Campania>, ICampaniaService
internal class LicenciaService : ServiceBase<Licencia>, ILicenciaService

// DESPUÉS (correcto)
public class CampaniaService : ServiceBase<Campania>, ICampaniaService
public class LicenciaService : ServiceBase<Licencia>, ILicenciaService
```

### 5. Correcciones de Assertions

#### Eliminar assertions de propiedades inexistentes
```csharp
// ANTES (incorrecto)
Assert.Equal("Nombre Test", result.Data.Nombre);
Assert.Equal("Descripción", result.Data.Descripcion);
Assert.True(result.Data.Activo);
Assert.Equal(1, result.Data.IdCampania);

// DESPUÉS (correcto)
// Eliminar o reemplazar con propiedades existentes
```

### 6. Verificación Final

1. **Compilación:**
   ```bash
   dotnet test AgroForm.Tests/AgroForm.Tests.csproj
   ```

2. **Cobertura:**
   ```bash
   dotnet test AgroForm.Tests/AgroForm.Tests.csproj --collect:"XPlat Code Coverage"
   ```

## Reglas Generales

### Para Entidades que Heredan de EntityBaseWithLicencia
- Siempre incluir `IdLicencia` en datos de prueba
- No incluir propiedades que no existen en la entidad
- Usar `TestUserAuth.UserName` para `RegistrationUser`

### Para Enums
- Siempre usar el namespace completo: `EnumClass.TipoEnum`
- Verificar valores válidos en `EnumClass.cs`

### Para Assertions
- Solo verificar propiedades que existen en la entidad
- Eliminar assertions de propiedades eliminadas

### Para Sintaxis
- Agregar comas donde falten después de correcciones
- Verificar que no queden comas extrañas

## Errores Comunes a Evitar

1. **No asumir propiedades:** Siempre verificar la estructura real en los archivos del modelo
2. **No usar enums sin namespace:** Siempre usar `EnumClass.TipoEnum`
3. **No olvidar comas:** Después de eliminar propiedades, verificar la sintaxis
4. **No dejar assertions huérfanas:** Eliminar assertions de propiedades eliminadas

## Herramientas Útiles

- `read_file` para verificar estructura de entidades
- `grep_search` para buscar patrones en múltiples archivos
- `replace_all` para correcciones masivas
- `dotnet test` para verificación constante
