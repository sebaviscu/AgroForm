# Glosario de Términos - AgroForm

## Términos de Negocio

### Agrícolas
- **Campaña**: Período de producción agrícola (generalmente 6-12 meses)
- **Campo**: Unidad territorial principal de una explotación agrícola
- **Lote**: Subdivisión de un campo para cultivo específico
- **Cultivo**: Tipo de planta cultivada (soja, maíz, trigo, etc.)
- **Variedad**: Específica dentro de un cultivo (ej: Soja RR, Soja IPRO)
- **Actividad**: Tarea agrícola realizada (siembra, cosecha, fertilización)
- **Superficie**: Medida de terreno en hectáreas (ha)
- **Rinde**: Producción por hectárea (ton/ha o kg/ha)
- **Estado Fenológico**: Etapa de desarrollo del cultivo

### Financieros
- **Gasto**: Costo asociado a la producción agrícola
- **Insumo**: Material utilizado en producción (semillas, fertilizantes)
- **Moneda Base**: Moneda principal para cálculos (generalmente pesos)
- **Cotización**: Tasa de cambio entre monedas
- **Costo Directo**: Gasto directamente atribuible a una actividad

### Operativos
- **Licencia**: Contrato de uso del software por empresa/cliente
- **Usuario**: Persona con acceso al sistema
- **Rol**: Nivel de permisos de un usuario
- **Tenant**: Organización/cliente en modelo multi-tenant

## Términos Técnicos

### .NET y Backend
- **.NET 9.0**: Framework principal de desarrollo
- **ASP.NET Core MVC**: Framework web para aplicaciones web
- **Entity Framework Core**: ORM para acceso a bases de datos
- **Repository Pattern**: Patrón de diseño para acceso a datos
- **Unit of Work**: Patrón para gestionar transacciones
- **Dependency Injection**: Inyección de dependencias
- **AutoMapper**: Librería para mapeo entre objetos
- **Serilog**: Framework de logging estructurado

### Base de Datos
- **SQL Server**: Motor de base de datos relacional
- **Code-First**: Enfoque de desarrollo donde el código define la BD
- **Migration**: Script de cambios en estructura de BD
- **DbContext**: Clase que representa la conexión a BD
- **Entity**: Clase que representa una tabla de BD
- **DTO**: Data Transfer Object para transferencia de datos
- **Index**: Estructura para optimizar consultas
- **Foreign Key**: Relación entre tablas

### Frontend
- **Razor Views**: Motor de plantillas de ASP.NET Core
- **Bootstrap**: Framework CSS para diseño responsivo
- **jQuery**: Librería JavaScript para manipulación DOM
- **DataTables**: Plugin jQuery para tablas interactivas
- **AJAX**: Comunicación asíncrona cliente-servidor
- **JSON**: Formato de intercambio de datos
- **CSRF**: Protección contra ataques de falsificación
- **Responsive Design**: Diseño adaptable a dispositivos

### Arquitectura
- **N-Tier**: Arquitectura en capas
- **Multi-tenant**: Arquitectura multi-cliente
- **RESTful**: Estilo arquitectónico para APIs
- **Middleware**: Software que procesa peticiones HTTP
- **Controller**: Clase que maneja peticiones HTTP
- **Service**: Clase con lógica de negocio
- **ViewModel**: Modelo para vistas MVC

## Acrónimos y Abreviaciones

### Generales
- **API**: Application Programming Interface
- **CRUD**: Create, Read, Update, Delete
- **DTO**: Data Transfer Object
- **ORM**: Object-Relational Mapping
- **DI**: Dependency Injection
- **MVC**: Model-View-Controller
- **UI**: User Interface
- **UX**: User Experience

### Específicos de AgroForm
- **AGF**: AgroForm (sistema)
- **CAM**: Campaña
- **CMP**: Campo
- **LTE**: Lote
- **ACT**: Actividad
- **GTO**: Gasto
- **LIC**: Licencia
- **USR**: Usuario
- **MON**: Moneda

### Técnicos
- **EF**: Entity Framework
- **SQL**: Structured Query Language
- **HTTP**: Hypertext Transfer Protocol
- **HTTPS**: HTTP Secure
- **JSON**: JavaScript Object Notation
- **XML**: eXtensible Markup Language
- **CSS**: Cascading Style Sheets
- **JS**: JavaScript

## Conceptos de Dominio

### Multi-tenancy
- **Tenant**: Cliente/organización independiente
- **Isolation**: Separación de datos entre tenants
- **Shared Database**: Base de datos compartida con aislamiento lógico
- **IdLicencia**: Identificador único de tenant en cada entidad

### Gestión Agrícola
- **Ciclo Productivo**: Secuencia de actividades desde siembra a cosecha
- **Planificación**: Definición de actividades futuras
- **Ejecución**: Realización de actividades planificadas
- **Seguimiento**: Monitoreo de progreso y resultados
- **Cierre**: Finalización y análisis de campaña

### Control de Costos
- **Costeo**: Asignación de costos a actividades
- **Presupuesto**: Planificación de costos esperados
- **Real**: Costos efectivamente incurridos
- **Desviación**: Diferencia entre presupuesto y real
- **Análisis**: Estudio de rentabilidad y eficiencia

## Patrones de Diseño

### Creacionales
- **Repository**: Abstracción de acceso a datos
- **Factory**: Creación de objetos complejos
- **Builder**: Construcción paso a paso de objetos
- **Singleton**: Instancia única de una clase

### Estructurales
- **Adapter**: Adaptación entre interfaces incompatibles
- **Decorator**: Adición de funcionalidad a objetos
- **Facade**: Interfaz simplificada para sistemas complejos
- **Proxy**: Representante de otro objeto

### Comportamiento
- **Strategy**: Algoritmo intercambiable
- **Observer**: Notificación de cambios a múltiples objetos
- **Command**: Encapsulación de una petición como objeto
- **Template Method**: Esqueleto de algoritmo con pasos variables

## Tecnologías Específicas

### Backend Stack
- **C# 12**: Lenguaje de programación principal
- **ASP.NET Core**: Framework web
- **Entity Framework Core 9.0**: ORM
- **AutoMapper 12.0**: Mapeo de objetos
- **Serilog 4.3**: Logging
- **Newtonsoft.Json 13.0**: Serialización JSON

### Frontend Stack
- **Bootstrap 5.1.3**: Framework CSS
- **jQuery 3.6.0**: Librería JavaScript
- **DataTables 1.13.4**: Tablas dinámicas
- **SweetAlert2**: Notificaciones
- **Font Awesome**: Iconos

### Database Stack
- **SQL Server 2019+**: Motor de BD
- **T-SQL**: Dialecto SQL de Microsoft
- **SSMS**: SQL Server Management Studio
- **EF Core Migrations**: Gestión de cambios en BD

## Conceptos de Seguridad

### Autenticación
- **Cookie Authentication**: Autenticación mediante cookies
- **Claims**: Información sobre el usuario autenticado
- **Identity**: Sistema de gestión de usuarios
- **Authorization**: Verificación de permisos

### Protección
- **CSRF**: Cross-Site Request Forgery
- **XSS**: Cross-Site Scripting
- **SQL Injection**: Inyección de código SQL
- **HTTPS**: Comunicación segura

### Auditoría
- **Logging**: Registro de eventos del sistema
- **Audit Trail**: Rastro de auditoría
- **User Context**: Información del usuario actual
- **Change Tracking**: Seguimiento de cambios

## Métricas y KPIs

### Agronómicos
- **Rinde/ha**: Rendimiento por hectárea
- **Días a Cosecha**: Período desde siembra a cosecha
- **Eficiencia**: Relación entre insumos y producción
- **Productividad**: Producción por unidad de recurso

### Operativos
- **Response Time**: Tiempo de respuesta del sistema
- **Throughput**: Cantidad de transacciones por segundo
- **Availability**: Disponibilidad del sistema
- **Concurrent Users**: Usuarios simultáneos

### Financieros
- **Costo/ha**: Costo por hectárea
- **Rentabilidad**: Relación entre ingresos y costos
- **ROI**: Return on Investment
- **Margen**: Diferencia entre ingresos y costos

## Errores Comunes y Soluciones

### Development
- **Null Reference**: Acceder a objeto nulo
- **Async/Await**: Uso incorrecto de operaciones asíncronas
- **Memory Leak**: Fuga de memoria por objetos no liberados
- **Race Condition**: Condición de carrera en concurrencia

### Database
- **N+1 Query**: Problema de múltiples consultas innecesarias
- **Deadlock**: Bloqueo mutuo entre transacciones
- **Connection Pool**: Agotamiento de conexiones
- **Index Missing**: Falta de índices para consultas

### Frontend
- **Callback Hell**: Anidación excesiva de callbacks
- **Memory Leak**: Event listeners no removidos
- **Race Condition**: Respuestas AJAX fuera de orden
- **DOM Manipulation**: Manipulación ineficiente del DOM

## Herramientas y Utilidades

### Development
- **Visual Studio 2022**: IDE principal
- **SQL Server Management Studio**: Gestión de BD
- **Git**: Control de versiones
- **Postman**: Testing de APIs

### Testing
- **xUnit**: Framework de testing unitario
- **Moq**: Framework de mocking
- **Playwright**: Testing E2E
- **Swagger**: Documentación de API

### Deployment
- **IIS**: Internet Information Services
- **Azure Cloud**: Plataforma cloud (opcional)
- **Docker**: Contenerización (opcional)
- **CI/CD**: Integración y despliegue continuo

## Buenas Prácticas

### Código
- **SOLID**: Principios de diseño orientado a objetos
- **DRY**: Don't Repeat Yourself
- **KISS**: Keep It Simple, Stupid
- **YAGNI**: You Aren't Gonna Need It

### Base de Datos
- **Normalization**: Organización de datos para evitar redundancia
- **Indexing**: Creación de índices para optimizar consultas
- **Transactions**: Gestión de operaciones atómicas
- **Backup**: Copias de seguridad regulares

### Seguridad
- **Principle of Least Privilege**: Mínimos permisos necesarios
- **Defense in Depth**: Múltiples capas de seguridad
- **Secure by Default**: Configuración segura por defecto
- **Regular Updates**: Actualizaciones de seguridad periódicas

## Términos de Integración

### APIs Externas
- **REST**: Estilo arquitectónico para APIs web
- **JSON Web Token**: Token de autenticación estándar
- **OAuth**: Protocolo de autorización estándar
- **Webhook**: Callback para notificaciones

### Servicios Cloud
- **SaaS**: Software as a Service
- **PaaS**: Platform as a Service
- **IaaS**: Infrastructure as a Service
- **Serverless**: Computación sin servidor

### Comunicación
- **HTTP/HTTPS**: Protocolos de transferencia
- **WebSocket**: Comunicación bidireccional
- **gRPC**: Llamadas a procedimientos remotos
- **Message Queue**: Cola de mensajes asíncrona
