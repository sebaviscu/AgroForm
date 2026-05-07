# Documentación AI - AgroForm

Esta carpeta contiene la documentación completa del sistema AgroForm, generada automáticamente para facilitar el trabajo eficiente en futuras sesiones de desarrollo.

## 📁 Estructura de Documentación

### 🎯 Documentación Principal
- **[overview.md](./overview.md)** - Visión general del sistema y propósito
- **[architecture.md](./architecture.md)** - Arquitectura técnica detallada
- **[getting-started-for-ai.md](./getting-started-for-ai.md)** - **GUÍA ESENCIAL** para futuras sesiones de IA

### 🔧 Documentación Técnica
- **[backend.md](./backend.md)** - Tecnologías y patrones backend
- **[frontend.md](./frontend.md)** - Tecnologías y arquitectura frontend
- **[database.md](./database.md)** - Esquema de base de datos y configuración
- **[api-endpoints.md](./api-endpoints.md)** - Referencia completa de endpoints

### 📋 Documentación Operativa
- **[business-rules.md](./business-rules.md)** - Reglas de negocio y validaciones
- **[coding-guidelines.md](./coding-guidelines.md)** - Guías de codificación y estándares
- **[workflows.md](./workflows.md)** - Flujos de trabajo operativos
- **[pending-questions.md](./pending-questions.md)** - Decisiones pendientes y riesgos

### 📚 Referencia
- **[glossary.md](./glossary.md)** - Glosario de términos técnicos y de negocio

## 🚀 Quick Start para IA

### 1. Leer Primero
```
getting-started-for-ai.md → Esencial para entender el contexto
```

### 2. Para Desarrollo Rápido
```
overview.md → Entender el sistema
architecture.md → Entender la estructura
coding-guidelines.md → Seguir convenciones
```

### 3. Para Debugging
```
workflows.md → Flujos de troubleshooting
api-endpoints.md → Referencia de endpoints
business-rules.md → Validaciones y reglas
```

## ⚠️ Reglas Críticas

### 🔴 Zonas de Alto Riesgo (NO TOCAR SIN VALIDACIÓN)
- **Multi-tenant**: Nunca remover filtros por `IdLicencia`
- **Authentication**: No modificar `ValidarAutorizacion()` sin tests
- **Database**: No cambiar tablas sin migration
- **BaseController**: Cambios afectan a TODOS los controllers

### ✅ Prácticas Seguras
- Seguir patrones existentes (copiar de código similar)
- Validar impacto antes de modificar
- Agregar logging para debugging
- Escribir tests para cambios críticos

## 🏗️ Arquitectura Clave

### Stack Principal
- **.NET 9.0** + **ASP.NET Core MVC** + **Entity Framework Core 9.0**
- **SQL Server** + **Multi-tenant por licencia**
- **Bootstrap + jQuery + DataTables** (Frontend)

### Patrones Fundamentales
- **Repository Pattern** → `IGenericRepository<T>`
- **Unit of Work** → `IUnitOfWork`
- **Service Layer** → `IServiceBase<T>`
- **Generic Controller** → `BaseController<TEntity, TDto, TService>`

### Flujo de Datos
```
Browser → Controller → Service → Repository → EF Core → SQL Server
```

## 🔄 Actualización de Documentación

### Cuándo Actualizar
- Nueva funcionalidad implementada
- Cambios en arquitectura
- Nuevas reglas de negocio
- Decisiones importantes tomadas

### Cómo Actualizar
1. Identificar archivo(s) afectados
2. Actualizar contenido específico
3. Mantener formato y estructura
4. Agregar fecha de actualización si corresponde

## 📊 Métricas del Proyecto

### Complejidad
- **4 Proyectos** (Web, Business, Data, Model)
- **13 Controllers** principales
- **Entidades principales**: Campo, Lote, Actividad, Campania, Gasto
- **Multi-tenant**: Aislamiento completo por licencia

### Tecnologías
- **Backend**: .NET 9.0, EF Core 9.0, AutoMapper, Serilog
- **Frontend**: Bootstrap 5, jQuery 3.6, DataTables 1.13
- **Database**: SQL Server con Code-First migrations

## 🎯 Objetivos de la Documentación

1. **Eficiencia**: Reducir tiempo de onboarding para futuras sesiones
2. **Consistencia**: Mantener patrones y convenciones
3. **Seguridad**: Evitar errores críticos en multi-tenant
4. **Mantenimiento**: Facilitar debugging y extensiones
5. **Conocimiento**: Preservar contexto de decisiones

## 📞 Soporte y Contexto

### Para Futuras Sesiones IA
- Empezar siempre por `getting-started-for-ai.md`
- Consultar `workflows.md` para troubleshooting
- Usar `pending-questions.md` para decisiones pendientes

### Para Desarrollo Humano
- `coding-guidelines.md` para estándares
- `api-endpoints.md` para referencia de API
- `business-rules.md` para lógica de negocio

---

**Última Actualización**: 7 de Mayo de 2026  
**Generado Por**: Cascade AI Assistant  
**Versión**: 1.0 - Documentación Inicial Completa
