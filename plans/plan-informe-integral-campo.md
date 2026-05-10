# Plan: Informe Integral de Campo/Lote

## Objetivo
Migrar la información de los modales `Visualización del Campo` e `Historial del Campo` (en [`AgroForm.Web/Views/Campo/Index.cshtml`](../AgroForm.Web/Views/Campo/Index.cshtml)) hacia la vista [`/Reporte/Campo`](../AgroForm.Web/Views/Reporte/Campo.cshtml), transformándola en un **Informe Agronómico Integral**. Luego eliminar los modales antiguos y sus botones asociados.

---

## Fase 1: Backend - Nuevo DTO y Service para el Reporte Integral

### 1.1. Crear DTO `ReporteCampoIntegralDto` en [`AgroForm.Business/Contracts/ReportesDto.cs`](../AgroForm.Business/Contracts/ReportesDto.cs)

DTO principal que contendrá todos los sub-DTOs agrupados:

```csharp
public class ReporteCampoIntegralDto
{
    public ResumenEjecutivoDto ResumenEjecutivo { get; set; }
    public List<TimelineEventoDto> Timeline { get; set; }
    public EvolucionCultivoDto EvolucionCultivo { get; set; }
    public AnalisisClimaticoDto AnalisisClimatico { get; set; }
    public AnalisisSueloDto AnalisisSuelo { get; set; }
    public CostosRentabilidadDto CostosRentabilidad { get; set; }
    public RendimientoCosechaDto RendimientoCosecha { get; set; }
    public List<AlertaDto> Alertas { get; set; }
    public List<HistorialCampaniaDto> HistorialMultiCampania { get; set; }
}
```

**Sub-DTOs requeridos:**

| DTO | Propiedades clave |
|-----|-------------------|
| `ResumenEjecutivoDto` | CultivoActual, SuperficieHa, Campaña, DiasDesdeSiembra, UltimaLluvia, NDVIPromedio, EstadoGeneral, RiesgoActual |
| `TimelineEventoDto` | Id, Fecha, TipoActividad, Icono, Color, Descripcion, Lote, CicloCultivo |
| `EvolucionCultivoDto` | List<DatoEvolucion> con Fecha, NDVI, Humedad, Temperatura, Precipitacion. También incluir comparativa entre campañas |
| `AnalisisClimaticoDto` | LluviaAcumulada, DiasSinLluvia, TempMin, TempMax, Heladas, BalanceHidrico, EstresHidrico |
| `AnalisisSueloDto` | N, P, K, MO, pH, CE, CIC, Textura, Interpretaciones por cada parametro, Recomendaciones |
| `CostosRentabilidadDto` | CostoTotalARS, CostoTotalUSD, CostoPorHaARS, CostoPorHaUSD, costos desglosados por tipo, MargenEstimado, RentabilidadProyectada |
| `RendimientoCosechaDto` | RendimientoTonHa, ProduccionTotal, HumedadCosecha, ComparativaHistorica |
| `AlertaDto` | Tipo, Severidad, Mensaje, Fecha, Recomendacion |
| `HistorialCampaniaDto` | Campaña, Cultivo, List<LaborDTO> Labores, Rendimiento, Costos |

### 1.2. Crear/Extender `IReportService` con método `GetReporteCampoIntegralAsync`

En [`AgroForm.Business/Contracts/IReportService.cs`](../AgroForm.Business/Contracts/IReportService.cs) agregar:

```csharp
Task<OperationResult<ReporteCampoIntegralDto>> GetReporteCampoIntegralAsync(int idCampo, int? idCampania = null);
```

### 1.3. Implementar método en [`AgroForm.Business/Services/ReportService.cs`](../AgroForm.Business/Services/ReportService.cs)

La implementación debe:
1. Obtener el Campo con sus Lotes y datos geográficos
2. Para cada lote, obtener CicloCultivos activos e históricos
3. Para cada ciclo, obtener todas las actividades (Siembras, Cosechas, Fertilizaciones, etc.)
4. Calcular resumen ejecutivo (días desde siembra, última lluvia, NDVI estimado)
5. Construir timeline agronómico ordenado
6. Calcular evolución del cultivo (usar datos climáticos de Open-Meteo API o datos propios)
7. Analizar clima (usar `IRegistroClimaService`)
8. Analizar suelo (usar último `AnalisisSuelo` disponible)
9. Calcular costos y rentabilidad (usar costos de actividades + `IGastoService`)
10. Obtener rendimiento (usar últimas `Cosechas`)
11. Generar alertas inteligentes basadas en reglas de negocio
12. Agrupar todo por campaña para el historial multi-campaña

### 1.4. Agregar endpoint en [`ReporteController.cs`](../AgroForm.Web/Controllers/ReporteController.cs)

```csharp
[HttpPost("[action]")]
public async Task<IActionResult> GetReporteCampoIntegral([FromBody] ReporteCampoRequest request)
{
    var result = await _reportService.GetReporteCampoIntegralAsync(request.IdCampo, request.IdCampania);
    // return Ok/Error response
}
```

Agregar también endpoint para obtener campañas disponibles para un campo:
```csharp
[HttpGet("GetCampaniasByCampo/{idCampo}")]
public async Task<IActionResult> GetCampaniasByCampo(int idCampo)
```

### 1.5. DTO Request para el endpoint

```csharp
public class ReporteCampoRequest
{
    public int IdCampo { get; set; }
    public int? IdCampania { get; set; }
}
```

---

## Fase 2: Frontend - Vista del Informe Integral

### 2.1. Reemplazar [`AgroForm.Web/Views/Reporte/Campo.cshtml`](../AgroForm.Web/Views/Reporte/Campo.cshtml)

Estructura de la nueva vista:

```
┌─────────────────────────────────────────────────────┐
│ Header: "Informe Integral del Campo" + selector     │
├─────────────────────────────────────────────────────┤
│ TABS: Resumen | Timeline | Evolución | Clima |     │
│        Suelo | Costos | Rendimiento | Alertas       │
├─────────────────────────────────────────────────────┤
│                                                      │
│ [TAB: Resumen Ejecutivo]                             │
│ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐      │
│ │Cultivo│ │Sup.  │ │Días  │ │NDVI  │ │Riesgo│      │
│ │Soja   │ │50 Ha │ │45    │ │0.72  │ │Bajo  │      │
│ └──────┘ └──────┘ └──────┘ └──────┘ └──────┘      │
│                                                      │
│ [TAB: Timeline Agronómico]                           │
│ ● Siembra ── ● Fertilización ── ● Pulverización     │
│   01/10        15/10               01/11             │
│                                                      │
│ [TAB: Evolución del Cultivo]                         │
│ ┌──────────────────────────────────────────────┐    │
│ │            Gráfico NDVI / Humedad            │    │
│ │  ████████████████████████████████████████    │    │
│ └──────────────────────────────────────────────┘    │
│                                                      │
│ [TAB: Análisis Climático]                            │
│ Lluvia acumulada: 120mm | Días sin lluvia: 5       │
│ Temp: 15°C-28°C | Balance hídrico: Normal          │
│                                                      │
│ [TAB: Análisis de Suelo]                             │
│ pH: 6.2 (Óptimo) | MO: 3.5% (Medio)                │
│ N: 25ppm (Bajo) → Recomendación: fertilizar         │
│                                                      │
│ [TAB: Costos y Rentabilidad]                         │
│ Costo total: $500.000 | Por ha: $10.000             │
│ Margen estimado: $200.000                           │
│                                                      │
│ [TAB: Rendimiento y Cosecha]                         │
│ Rendimiento: 3.5 tn/ha | Total: 175 tn              │
│ Humedad cosecha: 13.5%                              │
│                                                      │
│ [TAB: Alertas Inteligentes]                          │
│ ⚠ Falta de lluvia (Media)                          │
│ ✅ NDVI en rango normal                             │
│                                                      │
└─────────────────────────────────────────────────────┘
```

**Secciones detalladas:**

#### Tab 1: Resumen Ejecutivo
- Tarjetas KPI con: cultivo actual, superficie, campaña, días desde siembra, última lluvia, NDVI promedio, estado general del lote, riesgo actual
- Mapa satelital pequeño del campo
- Badge de estado general

#### Tab 2: Timeline Agronómico
- Línea de tiempo visual horizontal/vertical
- Cada actividad con: ícono distinto, color según tipo, fecha, descripción
- Tipos: Siembra (🌱), Fertilización (🧪), Pulverización (✈️), Análisis (🔬), Lluvias (🌧️), Cosecha (🌾)
- Mostrar tambien los demas labores que faltan
- Agrupado por ciclo de cultivo

#### Tab 3: Evolución del Cultivo
- Gráficos Chart.js:
  - NDVI histórico (línea)
  - Humedad del suelo (línea)
  - Temperatura (área)
  - Precipitaciones (barras)
- Comparador: hoy vs hace 30 días, campaña actual vs anterior

#### Tab 4: Análisis Climático
- Lluvia acumulada en período
- Días sin lluvia
- Temperaturas mín/max
- Alertas de heladas
- Balance hídrico calculado
- Estrés hídrico estimado

#### Tab 5: Análisis de Suelo
- Tabla de parámetros: N, P, K, MO, pH, CE, CIC (compara labores de Análisis, Fertilización y demas que tengan datos como estos)
- Barra de nivel con código de colores (Bajo/Medio/Alto)
- Interpretación automática agronómica
- Recomendaciones por parámetro

#### Tab 6: Costos y Rentabilidad
- Desglose por tipo: semillas, fertilizantes, fitosanitarios, maquinaria, mano de obra
- Gráfico de torta de distribución de costos
- Costo por hectárea
- Margen estimado
- Rentabilidad proyectada

#### Tab 7: Rendimiento y Cosecha
- Rendimiento por ha
- Producción total
- Humedad de cosecha
- Gráfico comparativo histórico

#### Tab 8: Alertas Inteligentes
- Alertas automáticas: falta de lluvia, exceso hídrico, caída de NDVI, riesgo de helada, estrés del cultivo
- Cada alerta con: nivel de severidad, mensaje, recomendación

#### Historial Multi-Campaña (sub-sección en el tab de Historial o tab separado)
- Acordeón por campaña
- Cada campaña muestra: cultivo, labores, rendimiento, costos

### 2.2. Reescribir [`AgroForm.Web/wwwroot/js/views/reporteCampos.js`](../AgroForm.Web/wwwroot/js/views/reporteCampos.js)

JavaScript necesario:
- Cargar selector de campo
- Cargar datos del reporte vía AJAX
- Renderizar cada sección/tab
- Inicializar Chart.js para gráficos
- Inicializar Leaflet para el mapa del resumen
- Funcionalidad de exportación a PDF
- Filtro por campaña
- Comparador temporal

### 2.3. Librerías adicionales necesarias

Ya están en el layout:
- ✅ Chart.js (en ComparativaCampos)
- ✅ Leaflet (en Campo)
- ✅ Bootstrap 5

Necesario agregar en el view:
- Chart.js (si no está global)
- html2canvas (para exportar PDF)
- Leaflet CSS + JS (para mapa pequeño en resumen)

---

## Fase 3: Eliminar Modales Antiguos y sus Botones

### 3.1. Eliminar modales de [`AgroForm.Web/Views/Campo/Index.cshtml`](../AgroForm.Web/Views/Campo/Index.cshtml)

Eliminar:
- `modalVisualizacion` (Visualización del Campo) - líneas 309-505
- `modalHistorial` (Historial del Campo) - líneas 252-306
- CSS relacionado con estos modales

### 3.2. Eliminar botones de la DataTable en campo.js

En [`AgroForm.Web/wwwroot/js/views/campo.js`](../AgroForm.Web/wwwroot/js/views/campo.js):

En la columna de acciones (líneas 95-115), reemplazar:
```javascript
// ANTES - 4 botones:
<button class="btn btn-outline-info btn-view"...>  // ← ELIMINAR
<button class="btn btn-outline-secondary btn-history"...>  // ← ELIMINAR
<button class="btn btn-outline-primary btn-edit"...>  // ← CONSERVAR
<button class="btn btn-outline-danger btn-delete"...>  // ← CONSERVAR

// DESPUÉS - 2 botones:
<button class="btn btn-outline-primary btn-edit"...>  // Editar
<button class="btn btn-outline-danger btn-delete"...>  // Eliminar
```

### 3.3. Eliminar funciones JS asociadas en [`AgroForm.Web/wwwroot/js/views/campo.js`](../AgroForm.Web/wwwroot/js/views/campo.js)

Eliminar todas las funciones relacionadas con:
- `abrirModalVisualizacion` (línea 643)
- `cargarDatosParaVisualizacion` (línea 653)
- `inicializarMapaVisualizacion` (línea 768)
- `configurarCapasOverlay` (línea 832)
- `cargarPoligonoVisualizacion` (línea 864)
- `cambiarVisualizacion` (línea 907)
- `aplicarVisualizacionNormal` / NDVI / Humedad / Sentinel (líneas 972-1101)
- `actualizarAnalisisCompleto` (línea 1105)
- `cargarDatosYAnalisis` (línea 723)
- `cargarDatosMeteorologicos` (línea 737)
- `inicializarFecha` (línea 717)
- `cargarHistorico` (línea 1436)
- `mostrarHistorial` (línea 1463)
- `agruparPorAnio` / `llenarFiltroAnios` / `mostrarHistorialSimple` / `aplicarFiltro` (líneas 1499-1620)
- Eventos de `btn-viz`, `layer-date`, `modalVisualizacion`, `modalHistorial`
- Variables globales asociadas: `viewMap`, `currentViz`, `campoPolygon`, `baseLayers`, `overlayLayers`, `weatherData`, `currentFieldData`, `historialData`

### 3.4. Actualizar eventos en campo.js

Eliminar event listeners que ya no se necesitan:
```javascript
// ELIMINAR:
$('#tblCampos tbody').on('click', '.btn-view', function () { ... });
$('#tblCampos tbody').on('click', '.btn-history', function () { ... });
```

### 3.5. Agregar botón "Ver Informe" que redirija a Reporte/Campo

En lugar de los botones eliminados, agregar un botón que redirija a la página de reporte con el ID del campo:
```javascript
// NUEVO - en las acciones de la tabla:
<button type="button" class="btn btn-outline-success btn-report"
        title="Ver informe completo" data-id="${data}">
    <i class="ph ph-file-text"></i>
</button>

// NUEVO - event listener:
$('#tblCampos tbody').on('click', '.btn-report', function () {
    var id = $(this).data('id');
    window.location.href = '/Reporte/Campo?idCampo=' + id;
});
```

---

## Fase 4: Navegación y Mejoras Adicionales

### 4.1. Actualizar menú lateral

En [`MenuLateral/Default.cshtml`](../AgroForm.Web/Views/Shared/Components/MenuLateral/Default.cshtml):
- Renombrar "Reporte Campos" a "Informe de Campos"
- No es necesario agregar nuevos items si la navegación se maneja por selector

### 4.2. Score del Lote (indicador visual)

En el Resumen Ejecutivo, agregar:
- Score de productividad (0-100)
- Score de salud del cultivo (0-100)
- Score de humedad (0-100)
- Score de riesgo (Bajo/Medio/Alto) con color

### 4.3. Exportación

Agregar botones de exportación:
- Exportar PDF profesional (usar html2canvas + jsPDF)
- Impresión (print-friendly CSS)
- Compartir reporte (copiar URL)

---

## Orden de Implementación

| # | Tarea | Archivos | Depende |
|---|-------|----------|---------|
| 1 | Crear DTOs de reporte | `ReportesDto.cs` | - |
| 2 | Extender `IReportService` | `IReportService.cs` | #1 |
| 3 | Implementar `GetReporteCampoIntegralAsync` | `ReportService.cs` | #2 |
| 4 | Agregar endpoints en `ReporteController` | `ReporteController.cs` | #3 |
| 5 | Re-escribir vista `Reporte/Campo.cshtml` | `Campo.cshtml` | #4 |
| 6 | Re-escribir `reporteCampos.js` | `reporteCampos.js` | #5 |
| 7 | Eliminar modales y botones antiguos | `Campo/Index.cshtml`, `campo.js` | #5 |
| 8 | Agregar botón "Ver Informe" en tabla de campos | `campo.js`, `Campo/Index.cshtml` | #7 |
| 9 | Exportación PDF/Impresión | `reporteCampos.js`, `Campo.cshtml` | #6 |
| 10 | Pruebas y refinamiento | varios | #1-9 |

---

## Diagrama de Arquitectura

```mermaid
flowchart TD
    A[Campo/Index.cshtml] -->|Click btn-report| B[/Reporte/Campo]
    
    B --> C{Selector de Campo}
    C --> D[jQuery AJAX]
    D --> E[ReporteController.GetReporteCampoIntegral]
    E --> F[ReportService.GetReporteCampoIntegralAsync]
    
    F --> G[CampoService - GetByIdWithDetailsAsync]
    F --> H[ActividadService - GetLaboresByAsync]
    F --> I[CicloCultivoService - ObtenerCiclosPorLoteAsync]
    F --> J[RegistroClimaService - GetRegistroClimasAsync]
    F --> K[GastoService - GetAllAsync]
    
    G --> L[ReporteCampoIntegralDto]
    H --> L
    I --> L
    J --> L
    K --> L
    
    L --> M[Reporte/Campo.cshtml renderizado]
    
    M --> N[Resumen Ejecutivo - KPI Cards]
    M --> O[Timeline Agronómico - Visual]
    M --> P[Evolución Cultivo - Chart.js]
    M --> Q[Análisis Climático - Cards]
    M --> R[Análisis Suelo - Tabla + Interpretación]
    M --> S[Costos - Gráficos + Tablas]
    M --> T[Rendimiento - Datos + Comparativa]
    M --> U[Alertas - Tarjetas de alerta]
    M --> V[Historial Multi-Campaña - Acordeón]
    
    N --> W[html2canvas + jsPDF]
    W --> X[PDF Exportable]
```

---

## Reglas de Negocio para Alertas Inteligentes

| Alerta | Condición | Severidad |
|--------|-----------|-----------|
| Falta de lluvia | > 7 días sin precipitación > 5mm | Media |
| Exceso hídrico | Lluvia acumulada 15 días > 100mm | Alta |
| Caída de NDVI | NDVI bajó > 0.15 en 7 días | Alta |
| Riesgo de helada | Temp.min < 2°C pronosticada | Alta |
| Estrés del cultivo | Temp > 35°C + humedad < 30% | Media |
| Bajo rendimiento | Rendimiento < 70% del promedio histórico | Media |

## Interpretación de Suelo Automática

| Parámetro | Bajo | Óptimo | Alto |
|-----------|------|--------|------|
| pH | < 5.5 | 5.5 - 7.0 | > 7.0 |
| MO | < 2% | 2% - 5% | > 5% |
| N | < 15 ppm | 15 - 30 ppm | > 30 ppm |
| P | < 10 ppm | 10 - 20 ppm | > 20 ppm |
| K | < 0.3 meq/100g | 0.3 - 0.7 meq/100g | > 0.7 meq/100g |
| CE | < 0.5 dS/m | 0.5 - 1.5 dS/m | > 1.5 dS/m |
| CIC | < 10 meq/100g | 10 - 25 meq/100g | > 25 meq/100g |

---

## Notas Técnicas

1. **Open-Meteo API**: Se puede seguir usando para datos climáticos en tiempo real e históricos (gratuito, sin API key)
2. **Chart.js**: Usar version 4.x ya incluida en el proyecto
3. **Leaflet**: Ya incluido en el proyecto, reutilizar para el mapa en resumen
4. **html2canvas**: Ya incluido en Campo/Index, puede moverse al layout o al view de reporte
5. **Exportación PDF**: Usar html2canvas + jsPDF o la función window.print() con CSS @media print
6. **Mantener responsividad**: Todo el diseño debe funcionar en móvil y desktop
7. **Tema oscuro**: Respetar data-theme="dark" ya implementado
