# API Endpoints - Referencia Completa

## Arquitectura de Endpoints

AgroForm utiliza una arquitectura **RESTful** con endpoints basados en convenciones MVC. Todos los controllers heredan de `BaseController<TEntity, TDto, TService>` que proporciona operaciones CRUD estándar.

### Convención de Nomenclatura
```
GET    /Controller/GetAll              - Listar todos los registros
GET    /Controller/GetAllDataTable     - Listar para DataTables
GET    /Controller/GetById/{id}         - Obtener por ID
POST   /Controller/Create              - Crear nuevo registro
PUT    /Controller/Update              - Actualizar registro
DELETE /Controller/Delete/{id}         - Eliminar registro
```

## Autenticación y Acceso

### AccessController
```csharp
// Login y Logout
GET    /Access/Login                    - Página de login
POST   /Access/Login                    - Autenticar usuario
POST   /Access/Logout                   - Cerrar sesión
GET    /Access/AccessDenied             - Acceso denegado
```

### Headers Requeridos
```http
Content-Type: application/json
X-CSRF-TOKEN: [token from form]
Cookie: AgroFormAuth=[authentication cookie]
```

## Endpoints por Módulo

### 1. Gestión de Licencias (AdministradorController)

#### Licencias
```http
GET    /Administrador/GetLicenciaById?id={id}
POST   /Administrador/Create
PUT    /Administrador/Update
DELETE /Administrador/Delete/{id}
```

#### Pagos de Licencias
```http
POST   /Administrador/CreatePagoLicencia
DELETE /Administrador/DeletePagoLicencia/{id}
```

**Request Example:**
```json
POST /Administrador/Create
{
    "razonSocial": "Empresa Agro S.A.",
    "nombreContacto": "Juan Pérez",
    "numeroContacto": "299-1234567",
    "tipoLicencia": 1,
    "esPrueba": false,
    "activo": true
}
```

### 2. Gestión de Campañas (CampaniaController)

```http
GET    /Campania/GetAllDataTable         - DataTable con detalles
GET    /Campania/GetAll                  - Lista simple
GET    /Campania/GetById/{id}            - Detalles por ID
POST   /Campania/Create                  - Nueva campaña
PUT    /Campania/Update                  - Actualizar campaña
DELETE /Campania/Delete/{id}             - Eliminar campaña
```

**Request Example:**
```json
POST /Campania/Create
{
    "idLicencia": 1,
    "nombre": "Campaña 2024/2025",
    "fechaInicio": "2024-09-01T00:00:00",
    "fechaFin": "2025-03-31T00:00:00",
    "activa": true,
    "cerrada": false
}
```

### 3. Gestión de Campos (CampoController)

```http
GET    /Campo/GetAllDataTable            - DataTable de campos
GET    /Campo/GetAll                     - Lista simple
GET    /Campo/GetById/{id}               - Detalles por ID
GET    /Campo/GetCamposConLotesYActividades - Campos con relaciones
GET    /Campo/GetByCampania              - Campos por campaña actual
POST   /Campo/Create                     - Nuevo campo
PUT    /Campo/Update                     - Actualizar campo
DELETE /Campo/Delete/{id}                - Eliminar campo
```

**Request Example:**
```json
POST /Campo/Create
{
    "idLicencia": 1,
    "idCampania": 1,
    "nombre": "Campo Norte",
    "superficie": 150.50,
    "ubicacion": "Ruta Provincial 7, km 45",
    "descripcion": "Campo principal de la explotación",
    "activo": true
}
```

### 4. Gestión de Lotes (LoteController)

```http
GET    /Lote/GetAllDataTable             - DataTable de lotes
GET    /Lote/GetAll                      - Lista simple
GET    /Lote/GetById/{id}                - Detalles por ID
GET    /Lote/GetByCampos/{idCampo}       - Lotes por campo
POST   /Lote/Create                      - Nuevo lote
PUT    /Lote/Update                      - Actualizar lote
DELETE /Lote/Delete/{id}                 - Eliminar lote
```

**Request Example:**
```json
POST /Lote/Create
{
    "idLicencia": 1,
    "idCampo": 1,
    "idCampania": 1,
    "nombre": "Lote A-1",
    "superficie": 25.30,
    "numeroLote": "A1-001",
    "coordenadas": "-38.123,-57.456",
    "activo": true
}
```

### 5. Gestión de Actividades (ActividadController)

#### Endpoints CRUD
```http
GET    /Actividad/GetAllDataTable        - DataTable de actividades
GET    /Actividad/GetAll                 - Lista simple
GET    /Actividad/GetById/{id}           - Detalles por ID
POST   /Actividad/Create                 - Nueva actividad
PUT    /Actividad/Update                 - Actualizar actividad
DELETE /Actividad/Delete/{id}            - Eliminar actividad
```

#### Endpoints Específicos
```http
GET    /Actividad/GetRecent              - Actividades recientes
GET    /Actividad/GetBy/{id}/{idTipoActividad} - Actividad por tipo
POST   /Actividad/CrearLabor              - Crear labor rápida
POST   /Actividad/EditarLabor             - Editar labor rápida
DELETE /Actividad/Delete/{id}/{idTipoActividad} - Eliminar por tipo
```

**Request Example - Crear Labor:**
```json
POST /Actividad/CrearLabor
{
    "idCampania": 1,
    "idCampo": 1,
    "idLote": 1,
    "idTipoActividad": 1,
    "idCultivo": 1,
    "idVariedad": 1,
    "fechaActividad": "2024-10-15T08:00:00",
    "descripcion": "Siembra de soja",
    "superficieRealizada": 25.30,
    "costoDirecto": 15000.00,
    "observaciones": "Condiciones climáticas favorables",
    "estado": 1
}
```

### 6. Gestión de Cultivos (CultivoController)

```http
GET    /Cultivo/GetAllDataTable           - DataTable de cultivos
GET    /Cultivo/GetAll                    - Lista simple
GET    /Cultivo/GetById/{id}              - Detalles por ID
POST   /Cultivo/Create                    - Nuevo cultivo
PUT    /Cultivo/Update                    - Actualizar cultivo
DELETE /Cultivo/Delete/{id}               - Eliminar cultivo
```

### 7. Gestión de Variedades (VariedadController)

```http
GET    /Variedad/GetAllDataTable          - DataTable de variedades
GET    /Variedad/GetAll                   - Lista simple
GET    /Variedad/GetById/{id}             - Detalles por ID
GET    /Variedad/GetByCultivo/{idCultivo} - Variedades por cultivo
POST   /Variedad/Create                   - Nueva variedad
PUT    /Variedad/Update                   - Actualizar variedad
DELETE /Variedad/Delete/{id}              - Eliminar variedad
```

### 8. Gestión de Gastos (GastoController)

```http
GET    /Gasto/GetAllDataTable             - DataTable de gastos
GET    /Gasto/GetAll                      - Lista simple
GET    /Gasto/GetById/{id}                - Detalles por ID
GET    /Gasto/GetGatosIndex               - Gastos para dashboard
POST   /Gasto/Create                      - Nuevo gasto
PUT    /Gasto/Update                      - Actualizar gasto
DELETE /Gasto/Delete/{id}                 - Eliminar gasto
```

**Request Example:**
```json
POST /Gasto/Create
{
    "idLicencia": 1,
    "idCampania": 1,
    "idCampo": 1,
    "idLote": 1,
    "idActividad": 1,
    "idTipoGasto": 1,
    "idMoneda": 1,
    "fechaGasto": "2024-10-15T00:00:00",
    "descripcion": "Compra de semillas",
    "importe": 25000.00,
    "proveedor": "Semillera S.A.",
    "comprobante": "FAC-00123",
    "observaciones": "Semilla certificada"
}
```

### 9. Gestión de Monedas (MonedaController)

```http
GET    /Moneda/GetAllDataTable            - DataTable de monedas
GET    /Moneda/GetAll                     - Lista simple
GET    /Moneda/GetById/{id}               - Detalles por ID
GET    /Moneda/ActualizarCotizacion       - Actualizar cotizaciones
POST   /Moneda/Create                     - Nueva moneda
PUT    /Moneda/Update                     - Actualizar moneda
DELETE /Moneda/Delete/{id}                - Eliminar moneda
```

### 10. Registro Clima (RegistroClimaController)

```http
GET    /RegistroClima/GetAllDataTable      - DataTable de registros climáticos
GET    /RegistroClima/GetAll               - Lista simple
GET    /RegistroClima/GetById/{id}         - Detalles por ID
GET    /RegistroClima/GetDatosLluvia?meses=6&campoId=0 - Datos de lluvia
POST   /RegistroClima/Create              - Nuevo registro climático
PUT    /RegistroClima/Update              - Actualizar registro climático
DELETE /RegistroClima/Delete/{id}         - Eliminar registro climático
```

**Request Example:**
```json
POST /RegistroClima/Create
{
    "idLicencia": 1,
    "fecha": "2024-10-15T00:00:00",
    "temperaturaMaxima": 28.5,
    "temperaturaMinima": 12.3,
    "precipitacion": 15.0,
    "humedadRelativa": 65,
    "velocidadViento": 12.5,
    "observaciones": "Día despejado con viento moderado"
}
```

### 11. Catálogos (CatalogoController)

```http
GET    /Catalogo/GetByTipo?tipo={TipoCatalogoEnum} - Items por tipo
GET    /Catalogo/GetAllActive             - Todos los activos
```

**TipoCatalogoEnum:**
```csharp
public enum TipoCatalogoEnum
{
    TipoActividad = 1,
    TipoGasto = 2,
    EstadoFenologico = 3,
    // ... otros tipos
}
```

### 12. Usuarios (UsuarioController)

```http
GET    /Usuario/GetAllDataTable            - DataTable de usuarios
GET    /Usuario/GetAll                     - Lista simple
GET    /Usuario/GetById/{id}               - Detalles por ID
POST   /Usuario/Create                     - Nuevo usuario
PUT    /Usuario/Update                     - Actualizar usuario
DELETE /Usuario/Delete/{id}                - Eliminar usuario
```

## Respuestas Estándar

### GenericResponse<T>
```json
{
    "success": true,
    "message": "Operación completada exitosamente",
    "object": { ... },           // Para respuestas individuales
    "listObject": [ ... ],        // Para listados
    "errorMessage": "Error message" // Solo en caso de error
}
```

### DataTable Response
```json
{
    "success": true,
    "data": [
        {
            "id": 1,
            "nombre": "Campo Norte",
            "superficie": 150.50,
            "acciones": "<button>...</button>"
        }
    ]
}
```

## Códigos de Estado HTTP

| Código | Descripción | Uso en AgroForm |
|--------|-------------|-----------------|
| 200 | OK | Operaciones exitosas |
| 201 | Created | Recurso creado (no usado, se usa 200) |
| 400 | Bad Request | Error de validación |
| 401 | Unauthorized | No autenticado |
| 403 | Forbidden | Sin permisos |
| 404 | Not Found | Recurso no encontrado |
| 500 | Internal Server Error | Error del servidor |

## Validación y Errores

### Errores de Validación (400)
```json
{
    "success": false,
    "message": "Error de validación",
    "errorMessage": "El nombre del campo es requerido"
}
```

### Errores de Autenticación (401/403)
```json
{
    "success": false,
    "message": "Acceso denegado",
    "errorMessage": "El usuario no está autenticado"
}
```

### Errores del Servidor (500)
```json
{
    "success": false,
    "message": "Error interno del servidor",
    "errorMessage": "Database connection failed"
}
```

## Seguridad

### CSRF Protection
Todos los endpoints POST/PUT/DELETE requieren token CSRF:
```html
<input name="__RequestVerificationToken" type="hidden" value="[token]" />
```

### Headers de Seguridad
```http
X-CSRF-TOKEN: [token]
Cookie: .AspNetCore.Cookies=[auth cookie]
```

### Rate Limiting
No implementado actualmente (considerar para futuro).

## Ejemplos de Consumo

### JavaScript/jQuery
```javascript
// Obtener todos los campos
$.ajax({
    url: '/Campo/GetAll',
    method: 'GET',
    success: function(response) {
        if (response.success) {
            console.log(response.listObject);
        }
    }
});

// Crear nuevo campo
$.ajax({
    url: '/Campo/Create',
    method: 'POST',
    data: JSON.stringify(campoData),
    contentType: 'application/json',
    headers: {
        'X-CSRF-TOKEN': $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        if (response.success) {
            Swal.fire('Éxito', 'Campo creado', 'success');
        }
    }
});
```

### cURL
```bash
# Login
curl -X POST "http://localhost:5000/Access/Login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password","rememberMe":false}'

# Obtener campos
curl -X GET "http://localhost:5000/Campo/GetAll" \
  -H "Cookie: AgroFormAuth=[auth_cookie]"

# Crear campo
curl -X POST "http://localhost:5000/Campo/Create" \
  -H "Content-Type: application/json" \
  -H "X-CSRF-TOKEN: [csrf_token]" \
  -d '{"nombre":"Campo Test","superficie":100.0}'
```

### C# HttpClient
```csharp
// Configurar cliente con cookies y CSRF
var client = new HttpClient();
client.BaseAddress = new Uri("http://localhost:5000");

// Login
var loginData = new { email = "user@example.com", password = "password" };
var loginResponse = await client.PostAsJsonAsync("/Access/Login", loginData);

// Obtener campos
var camposResponse = await client.GetAsync("/Campo/GetAll");
var campos = await camposResponse.Content.ReadFromJsonAsync<GenericResponse<CampoDto[]>>();
```

## Versionamiento de API

Actualmente no hay versionamiento implementado. Para futuro:
```
/api/v1/Campo/GetAll
/api/v2/Campo/GetAll
```

## Documentación Swagger

No implementado actualmente. Considerar agregar:
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

## Testing de Endpoints

### Unit Tests
```csharp
[Test]
public async Task Campo_GetAll_ReturnsSuccess()
{
    // Arrange
    var service = new Mock<ICampoService>();
    service.Setup(s => s.GetAllAsync()).ReturnsAsync(OperationResult<Campo>.SuccessResult(new List<Campo>()));
    
    var controller = new CampoController(logger, mapper, service.Object);
    
    // Act
    var result = await controller.GetAll();
    
    // Assert
    var okResult = result as OkObjectResult;
    Assert.IsNotNull(okResult);
    Assert.AreEqual(200, okResult.StatusCode);
}
```

### Integration Tests
```csharp
[Test]
public async Task Campo_Create_ReturnsSuccess()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    // Act
    var response = await client.PostAsJsonAsync("/Campo/Create", new CampoDto { Nombre = "Test" });
    
    // Assert
    response.EnsureSuccessStatusCode();
}
```
