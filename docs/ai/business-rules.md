# Reglas de Negocio - AgroForm

## Reglas Fundamentales del Sistema

### 1. Multi-tenancy por Licencia
- **Regla**: Todos los datos están aislados por licencia
- **Implementación**: Cada entidad tiene `IdLicencia` obligatorio
- **Validación**: Solo se puede acceder a datos de la licencia del usuario autenticado
- **Excepción**: SuperAdmin puede ver todas las licencias

### 2. Contexto de Campaña
- **Regla**: Todas las operaciones operativas deben estar asociadas a una campaña activa
- **Implementación**: Usuario tiene claim `IdCampania` en el token de autenticación
- **Validación**: Actividades, gastos y lotes requieren `IdCampania` válida
- **Flujo**: Login → Selección de campaña → Operaciones

### 3. Jerarquía Territorial
```
Licencia → Campo → Lote
```
- **Regla**: Un lote pertenece a un campo, un campo a una licencia
- **Validación**: No se puede crear un lote sin campo existente
- **Cascada**: Eliminar campo elimina todos sus lotes

## Reglas por Módulo

### Gestión de Licencias

#### Creación de Licencia
- **Validación**: Razón Social única y obligatoria
- **Tipo Licencia**: 
  - 0 = Trial (30 días)
  - 1 = Basic
  - 2 = Premium
  - 3 = Enterprise
- **Estado**: Por defecto `Activo = true`
- **Período de Prueba**: Si `EsPrueba = true`, requiere `FechaFinPrueba`

#### Pagos de Licencia
- **Tipo Pago**:
  - 0 = Mensual
  - 1 = Anual
  - 2 = Único
- **Validación**: Precio > 0 obligatorio
- **Auditoría**: Registro de usuario y fecha de creación

### Gestión de Usuarios

#### Creación de Usuario
- **Email**: Único por licencia, formato email válido
- **Password**: Mínimo 8 caracteres, requiere hash y salt
- **Rol**: 
  - 1 = Administrador
  - 2 = Operador
  - 3 = Consultor
- **Estado**: Por defecto `Activo = true`

#### Permisos por Rol
| Rol | Permisos |
|-----|----------|
| Administrador | CRUD completo, gestión de usuarios, reportes |
| Operador | CRUD de datos operativos, sin gestión de usuarios |
| Consultor | Solo lectura de todos los módulos |

#### Autenticación
- **Cookie**: Expiración 180 minutos con sliding expiration
- **Claims**: IdLicencia, IdCampania, IdUsuario, Rol, UserName
- **Seguridad**: Requiere HTTPS en producción

### Gestión de Campañas

#### Creación de Campaña
- **Nombre**: Único por licencia, obligatorio
- **Fechas**: `FechaInicio` ≤ `FechaFin` (opcional)
- **Estado**: Solo una campaña activa por licencia
- **Validación**: No se puede eliminar si tiene actividades asociadas

#### Cambio de Estado
- **Activación**: Desactiva automáticamente otras campañas
- **Cierre**: No permite crear nuevas actividades
- **Reapertura**: Solo si no está cerrada definitivamente

### Gestión de Campos

#### Creación de Campo
- **Nombre**: Único por licencia, obligatorio
- **Superficie**: > 0, formato decimal con 2 decimales
- **Ubicación**: Opcional, máximo 500 caracteres
- **Campaña**: Puede ser nulo (campo permanente)

#### Validaciones de Negocio
- **Superficie Total**: Suma de lotes ≤ superficie del campo
- **Actividades**: Campo sin lotes no puede tener actividades
- **Eliminación**: Solo si no tiene actividades o gastos asociados

### Gestión de Lotes

#### Creación de Lote
- **Campo**: Obligatorio, debe existir y estar activo
- **Campaña**: Obligatoria, debe estar activa
- **Superficie**: > 0, ≤ superficie disponible del campo
- **Nombre**: Único por campo

#### Reglas de Asignación
- **Cultivo**: Opcional, si se asigna debe existir
- **Variedad**: Requiere cultivo asignado
- **Coordenadas**: Formato GPS válido si se proporciona

### Gestión de Actividades

#### Creación de Actividad
- **Fecha**: No puede ser futura (excepto planificación)
- **Tipo Actividad**: Obligatorio, debe estar activo
- **Superficie Realizada**: ≤ superficie del lote
- **Costo Directo**: ≥ 0, formato decimal

#### Estados de Actividad
- **1 = Planificada**: Fecha futura, sin ejecutar
- **2 = En Progreso**: Fecha actual o pasada, ejecutándose
- **3 = Completada**: Finalizada correctamente
- **4 = Cancelada**: No se ejecutó

#### Validaciones Específicas
- **Siembra**: Requiere cultivo y variedad
- **Cosecha**: Requiere siembra previa del mismo cultivo
- **Fertilización**: Requiere tipo de fertilizante específico

### Gestión de Gastos

#### Creación de Gasto
- **Fecha**: Obligatoria, no puede ser futura
- **Importe**: > 0 obligatorio
- **Moneda**: Obligatoria, debe estar activa
- **Descripción**: Obligatoria, máximo 500 caracteres

#### Asignación de Gastos
- **Jerarquía**: Puede asignarse a Licencia → Campo → Lote → Actividad
- **Validación**: Si se asigna a actividad, debe existir y estar completada
- **Moneda**: Conversión automática a moneda base

### Gestión de Cultivos y Variedades

#### Creación de Cultivo
- **Nombre**: Único global, obligatorio
- **Ciclo Días**: Opcional, > 0 si se proporciona
- **Estado**: Por defecto activo

#### Creación de Variedad
- **Cultivo**: Obligatorio
- **Nombre**: Único por cultivo
- **Potencia Rinde**: > 0 si se proporciona
- **Días a Cosecha**: ≥ 0 si se proporciona

### Gestión de Monedas

#### Creación de Moneda
- **Nombre**: Único, obligatorio
- **Símbolo**: Único, obligatorio, máximo 10 caracteres
- **Cotización**: > 0 obligatorio
- **Moneda Base**: Solo una puede ser base

#### Actualización de Cotizaciones
- **Fuente**: API externa (BCRA/Dólar Blue)
- **Frecuencia**: Diaria o manual
- **Historial**: Se mantiene histórico de cambios

### Registro Climático

#### Creación de Registro
- **Fecha**: Obligatoria, una por día por licencia
- **Temperaturas**: Máxima ≥ Mínima
- **Precipitación**: ≥ 0
- **Humedad**: 0-100%
- **Viento**: ≥ 0 km/h

#### Validaciones
- **Unico por Día**: No se permite duplicar fecha
- **Rangos Válidos**: Temperatura -50°C a 60°C
- **Update**: Se puede modificar el mismo día

## Reglas Transaccionales

### Integridad Referencial
- **CASCADE DELETE**: Licencia → Usuarios, Campanias, Campos
- **SET NULL**: Campo.IdCampania (al eliminar campaña)
- **RESTRICT**: Actividades con gastos asociados

### Auditoría
- **Registro**: CreationDate, CreationUser, ModificationDate, ModificationUser
- **Actualización**: Automático en cada save
- **Usuario**: Se obtiene del contexto de autenticación

### Validaciones de Negocio Complejas

#### Cierre de Campaña
1. **Validar**: Todas las actividades completadas
2. **Calcular**: Costos totales por campo/lote
3. **Generar**: Reporte de cierre automático
4. **Bloquear**: Creación de nuevas actividades

#### Cambio de Campaña Activa
1. **Validar**: No hay actividades en progreso
2. **Actualizar**: Claims del usuario
3. **Notificar**: Otros usuarios conectados

#### Eliminación de Campo
1. **Validar**: Sin lotes asociados
2. **Validar**: Sin actividades asociadas
3. **Validar**: Sin gastos asociados
4. **Archivar**: Datos históricos si corresponde

## Reglas de Performance

### Límites y Cuotas
- **Registros por página**: 50 (DataTables)
- **Upload de archivos**: 10MB máximo
- **Timeout de sesión**: 180 minutos
- **Concurrent users**: 50 por licencia

### Optimizaciones
- **Lazy Loading**: Deshabilitado por defecto
- **Query Splitting**: Activado para consultas complejas
- **Caching**: No implementado (considerar para catálogos)

## Reglas de Seguridad

### Acceso por Rol
```csharp
public enum Roles
{
    SuperAdmin = 0,    // Acceso total al sistema
    Administrador = 1,  // Gestión completa de licencia
    Operador = 2,       // Operaciones del día a día
    Consultor = 3       // Solo lectura
}
```

### Validaciones de Seguridad
- **SQL Injection**: Protegido por EF Core
- **XSS**: Protegido por Razor encoding
- **CSRF**: Token obligatorio en POST/PUT/DELETE
- **Password Hash**: BCrypt o similar

## Reglas de Reportes

### Reporte de Cierre de Campaña
- **Incluir**: Todas las actividades y gastos
- **Agrupar**: Por campo → lote → cultivo
- **Calcular**: Costos por hectárea
- **Formato**: PDF y Excel

### Reportes de Productividad
- **Rendimiento**: Por cultivo y variedad
- **Costos**: Directos e indirectos
- **Comparativos**: Entre campañas
- **Tendencias**: Históricos

## Manejo de Errores

### Errores de Negocio
- **Validación**: 400 Bad Request
- **Permisos**: 403 Forbidden
- **No Encontrado**: 404 Not Found
- **Conflictos**: 409 Conflict (ej: duplicados)

### Logging de Errores
- **Nivel**: Error para excepciones
- **Contexto**: Usuario, acción, parámetros
- **Auditoría**: Intentos de acceso no autorizado

## Reglas de Integración

### API Externa (Cotizaciones)
- **Endpoint**: BCRA o similar
- **Frecuencia**: Diaria
- **Fallback**: Último valor conocido
- **Error**: Notificar administrador

### Exportación de Datos
- **Formatos**: CSV, Excel, PDF
- **Filtros**: Por fecha, campaña, campo
- **Límite**: 10,000 registros por exportación
- **Async**: Para exportaciones grandes

## Reglas Futuras (Planificadas)

### Multi-idioma
- **Soporte**: Español, Inglés, Portugués
- **Implementación**: Resource files
- **Selector**: Por usuario o licencia

### Notificaciones
- **Tipos**: Email, SMS, Push
- **Eventos**: Vencimientos, alertas climáticas
- **Frecuencia**: Inmediata o programada

### Offline Mode
- **Cache**: IndexedDB en browser
- **Sincronización**: Al reconectar
- **Conflictos**: Last write wins

## Validaciones de Datos Específicas

### Formatos de Datos
- **Email**: RFC 5322 standard
- **Teléfono**: + Country Code (10-15 dígitos)
- **Coordenadas**: Decimal degrees (6 decimales)
- **Moneda**: ISO 4217 codes

### Rangos de Valores
- **Superficie**: 0.01 - 99999.99 ha
- **Temperatura**: -50.0 - 60.0 °C
- **Precipitación**: 0.0 - 999.9 mm
- **Viento**: 0.0 - 200.0 km/h

### Expresiones Regulares
```regex
Email: ^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
Teléfono: ^\+[1-9]\d{1,14}$
Coordenadas: ^-?\d{1,3}\.\d{1,6},-?\d{1,3}\.\d{1,6}$
```

## Testing de Reglas de Negocio

### Unit Tests
- **Validaciones**: Cada regla debe tener test unitario
- **Boundary Cases**: Valores límite y casos extremos
- **Mocking**: Services y repositories

### Integration Tests
- **Workflows**: Flujos completos de usuario
- **Database**: Conexión real a datos
- **API**: End-to-end endpoints

### Escenarios Críticos
- **Multi-tenant**: Aislamiento de datos
- **Concurrencia**: Múltiples usuarios simultáneos
- **Performance**: Carga de datos masiva
