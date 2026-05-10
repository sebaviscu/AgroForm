---
description: Plantilla completa para crear reportes con la arquitectura de Comparativa Campos/Lotes
---

# Workflow: Plantilla Reportes AgroForm (Basado en Comparativa Campos/Lotes)

Basado en el reporte "Comparativa Campos/Lotes", este workflow guía la creación de nuevos reportes con la misma arquitectura y patrones.

## 1. Estructura de Archivos (OBLIGATORIO)

```
AgroForm.Web/
├── Models/
│   └── Reporte{Nombre}VM.cs           # ViewModel principal
├── Controllers/
│   └── ReporteController.cs           # Controller existente (extender)
├── Views/Reporte/
│   └── {Nombre}Reporte.cshtml         # Vista Razor
└── wwwroot/js/views/
    └── reporte{nombre}.js             # JavaScript específico

AgroForm.Business/
├── Contracts/
│   ├── IReportService.cs              # Interface existente (extender)
│   └── ReportesDto.cs                 # DTOs existentes (extender)
└── Services/
    └── ReportService.cs               # Service existente (extender)
```

## 2. Backend - DTOs

### 2.1. Crear DTO en `ReportesDto.cs`
```csharp
public class Reporte{Nombre}Dto
{
    // Identificadores
    public int IdCampo { get; set; }
    public string Campo { get; set; } = string.Empty;
    public int IdLote { get; set; }
    public string Lote { get; set; } = string.Empty;
    
    // Datos principales
    public decimal? SuperficieHa { get; set; }
    public string? Cultivo { get; set; }
    public DateTime? FechaSiembra { get; set; }
    public DateTime? FechaCosecha { get; set; }
    
    // Métricas específicas del reporte
    public decimal? ValorPrincipal { get; set; }
    public decimal? ValorSecundario { get; set; }
    
    // Costos
    public decimal? CostoTotalARS { get; set; }
    public decimal? CostoPorHaARS { get; set; }
    
    // Ranking (opcional)
    public int RankingValor { get; set; }
}
```

## 3. Backend - ViewModel

### 3.1. Crear ViewModel en `Models/Reporte{Nombre}VM.cs`
```csharp
namespace AgroForm.Web.Models
{
    public class Reporte{Nombre}VM
    {
        /// <summary>
        /// Lista de datos del reporte
        /// </summary>
        public List<Reporte{Nombre}Dto> Datos { get; set; } = new();

        // Filtros aplicados
        public int? FiltroIdCampania { get; set; }
        public int? FiltroIdCampo { get; set; }
        public int? FiltroIdLote { get; set; }
        public int? FiltroIdCultivo { get; set; }

        // Datos para los filtros (selects)
        public List<FiltroItem> Campos { get; set; } = new();
        public List<FiltroItem> Lotes { get; set; } = new();
        public List<FiltroItem> Cultivos { get; set; } = new();

        // Métricas resumen
        public Resumen{Nombre} Resumen { get; set; } = new();
    }

    public class Resumen{Nombre}
    {
        public int TotalRegistros { get; set; }
        public decimal? ValorPromedio { get; set; }
        public decimal? ValorMaximo { get; set; }
        public string? RegistroMejor { get; set; }
        public decimal? ValorMinimo { get; set; }
        public string? RegistroPeor { get; set; }
        public decimal? CostoPromedioPorHa { get; set; }
        public decimal? SuperficieTotal { get; set; }
    }

    public class FiltroItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}
```

## 4. Backend - Service Layer

### 4.1. Extender `IReportService.cs`
```csharp
Task<OperationResult<List<Reporte{Nombre}Dto>>> Get{Nombre}Async(
    int? idCampania = null,
    int? idCampo = null,
    int? idLote = null,
    int? idCultivo = null);
```

### 4.2. Implementar en `ReportService.cs`
```csharp
public async Task<OperationResult<List<Reporte{Nombre}Dto>>> Get{Nombre}Async(
    int? idCampania = null,
    int? idCampo = null,
    int? idLote = null,
    int? idCultivo = null)
{
    try
    {
        // 1. Obtener lotes con filtros y includes necesarios
        var lotesQuery = _unitOfWork.Repository<Lote>().Query()
            .Include(l => l.Campo)
            .Include(l => l.Campania)
            .Include(l => l.Siembras).ThenInclude(s => s.Cultivo)
            // Agregar otros includes según necesites
            .Where(l => l.IdLicencia == _userContext.IdLicencia)
            .AsQueryable();

        // Aplicar filtros
        if (idCampania.HasValue)
            lotesQuery = lotesQuery.Where(l => l.IdCampania == idCampania.Value);
        else if (_userContext.IdCampaña.HasValue)
            lotesQuery = lotesQuery.Where(l => l.IdCampania == _userContext.IdCampaña.Value);

        if (idCampo.HasValue)
            lotesQuery = lotesQuery.Where(l => l.IdCampo == idCampo.Value);

        if (idLote.HasValue)
            lotesQuery = lotesQuery.Where(l => l.Id == idLote.Value);

        var lotes = await lotesQuery.ToListAsync();

        // 2. Construir DTOs
        var result = new List<Reporte{Nombre}Dto>();

        foreach (var lote in lotes)
        {
            // Lógica específica del reporte
            var ultimaSiembra = lote.Siembras
                .OrderByDescending(s => s.Fecha)
                .FirstOrDefault();

            // Filtrar por cultivo si se especificó
            if (idCultivo.HasValue && ultimaSiembra != null && ultimaSiembra.IdCultivo != idCultivo.Value)
                continue;

            // Calcular valores específicos
            var valorPrincipal = CalcularValorPrincipal(lote);
            var valorSecundario = CalcularValorSecundario(lote);
            
            // Calcular costos
            var costosARS = ObtenerCostosARS(lote);
            var superficieHa = lote.SuperficieHectareas ?? 0;
            var costoPorHaARS = superficieHa > 0 ? costosARS / superficieHa : costosARS;

            result.Add(new Reporte{Nombre}Dto
            {
                IdCampo = lote.Campo?.Id ?? 0,
                Campo = lote.Campo?.Nombre ?? "Sin campo",
                IdLote = lote.Id,
                Lote = lote.Nombre,
                SuperficieHa = lote.SuperficieHectareas,
                Cultivo = ultimaSiembra?.Cultivo?.Nombre,
                FechaSiembra = ultimaSiembra?.Fecha,
                
                // Valores calculados
                ValorPrincipal = valorPrincipal,
                ValorSecundario = valorSecundario,
                
                // Costos
                CostoTotalARS = costosARS > 0 ? costosARS : null,
                CostoPorHaARS = costoPorHaARS > 0 ? Math.Round(costoPorHaARS, 2) : null
            });
        }

        // 3. Calcular ranking (si aplica)
        var conValor = result
            .Where(r => r.ValorPrincipal.HasValue)
            .OrderByDescending(r => r.ValorPrincipal)
            .ToList();

        for (int i = 0; i < conValor.Count; i++)
        {
            var item = result.First(r => r.IdLote == conValor[i].IdLote);
            item.RankingValor = i + 1;
        }

        return OperationResult<List<Reporte{Nombre}Dto>>.SuccessResult(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al generar reporte {nombre}");
        return OperationResult<List<Reporte{Nombre}Dto>>.Failure(
            $"Error al generar reporte: {ex.Message}", "DATABASE_ERROR");
    }
}

private decimal CalcularValorPrincipal(Lote lote)
{
    // Implementar lógica específica
    // Ejemplo: rendimiento, eficiencia, etc.
    return 0;
}

private decimal CalcularValorSecundario(Lote lote)
{
    // Implementar lógica secundaria
    return 0;
}
```

## 5. Backend - Controller

### 5.1. Extender `ReporteController.cs`
```csharp
/// <summary>
/// Vista principal del reporte {Nombre}
/// </summary>
[HttpGet("{Nombre}Reporte")]
public async Task<IActionResult> {Nombre}Reporte()
{
    var vm = new Reporte{Nombre}VM();

    try
    {
        // Cargar datos para filtros
        var camposResult = await _campoService.GetAllAsync();
        if (camposResult.Success)
        {
            vm.Campos = camposResult.Data.Select(c => new FiltroItem
            {
                Id = c.Id,
                Nombre = c.Nombre
            }).ToList();
        }

        var cultivosResult = await _cultivoService.GetAllAsync();
        if (cultivosResult.Success)
        {
            vm.Cultivos = cultivosResult.Data
                .Where(c => c.Activo)
                .Select(c => new FiltroItem
                {
                    Id = c.Id,
                    Nombre = c.Nombre
                }).ToList();
        }

        // Cargar datos iniciales
        var datosResult = await _reportService.Get{Nombre}Async();
        if (datosResult.Success)
        {
            vm.Datos = datosResult.Data;
            CalcularResumen(vm);
        }
    }
    catch (Exception ex)
    {
        // Log error
    }

    return View(vm);
}

/// <summary>
/// Endpoint AJAX para obtener datos del reporte con filtros
/// </summary>
[HttpPost("Get{Nombre}Data")]
public async Task<IActionResult> Get{Nombre}Data(
    int? idCampania,
    int? idCampo,
    int? idLote,
    int? idCultivo)
{
    var gResponse = new GenericResponse<Reporte{Nombre}Dto>();

    try
    {
        var result = await _reportService.Get{Nombre}Async(
            idCampania: idCampania,
            idCampo: idCampo,
            idLote: idLote,
            idCultivo: idCultivo);

        if (!result.Success)
        {
            gResponse.Success = false;
            gResponse.Message = result.ErrorMessage;
            return BadRequest(gResponse);
        }

        gResponse.Success = true;
        gResponse.ListObject = result.Data;
        gResponse.Message = "Datos obtenidos correctamente";
        return Ok(gResponse);
    }
    catch (Exception ex)
    {
        gResponse.Success = false;
        gResponse.Message = $"Error al obtener datos: {ex.Message}";
        return BadRequest(gResponse);
    }
}

private void CalcularResumen(Reporte{Nombre}VM vm)
{
    if (vm.Datos == null || vm.Datos.Count == 0)
        return;

    var resumen = new Resumen{Nombre}
    {
        TotalRegistros = vm.Datos.Count,
        SuperficieTotal = vm.Datos.Sum(d => d.SuperficieHa.GetValueOrDefault())
    };

    var conValor = vm.Datos.Where(d => d.ValorPrincipal.HasValue).ToList();
    if (conValor.Any())
    {
        resumen.ValorPromedio = Math.Round(conValor.Average(d => d.ValorPrincipal!.Value), 2);
        resumen.ValorMaximo = conValor.Max(d => d.ValorPrincipal!.Value);
        resumen.ValorMinimo = conValor.Min(d => d.ValorPrincipal!.Value);

        var mejor = conValor.OrderByDescending(d => d.ValorPrincipal).First();
        resumen.RegistroMejor = $"{mejor.Lote} ({mejor.Campo})";

        var peor = conValor.OrderBy(d => d.ValorPrincipal).First();
        resumen.RegistroPeor = $"{peor.Lote} ({peor.Campo})";
    }

    var conCosto = vm.Datos.Where(d => d.CostoPorHaARS.HasValue).ToList();
    if (conCosto.Any())
    {
        resumen.CostoPromedioPorHa = Math.Round(conCosto.Average(d => d.CostoPorHaARS!.Value), 2);
    }

    vm.Resumen = resumen;
}
```

## 6. Frontend - Vista Razor

### 6.1. Crear `Views/Reporte/{Nombre}Reporte.cshtml`
```cshtml
@model Reporte{Nombre}VM
@{
    ViewData["Title"] = "Reporte {Nombre}";
}

<style>
    /* Estilos específicos del reporte */
    .summary-card {
        border-radius: 12px;
        transition: transform 0.2s;
    }
    .summary-card:hover {
        transform: translateY(-2px);
    }
    .chart-container {
        position: relative;
        height: 300px;
        width: 100%;
    }
    .filter-section {
        background: #f8f9fa;
        border-radius: 12px;
        padding: 1rem;
    }
</style>

<div class="container-fluid">
    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h1 class="h3 fw-bold mb-1">
                <i class="ph ph-chart-bar text-success me-2"></i>Reporte {Nombre}
            </h1>
            <p class="text-muted mb-0 small">Descripción del reporte</p>
        </div>
        <div>
            <button id="btnExportarExcel" class="btn btn-outline-success btn-sm me-1">
                <i class="ph ph-file-xls me-1"></i> Exportar Excel
            </button>
            <button id="btnRefrescar" class="btn btn-primary btn-sm">
                <i class="ph ph-arrows-clockwise me-1"></i> Actualizar
            </button>
        </div>
    </div>

    <!-- Filtros -->
    <div class="filter-section mb-3">
        <div class="row g-2 align-items-end">
            <div class="col-md-3">
                <label for="filtroCampo" class="form-label small fw-semibold">Campo</label>
                <select id="filtroCampo" class="form-select form-select-sm">
                    <option value="">Todos los campos</option>
                    @foreach (var c in Model.Campos)
                    {
                        <option value="@c.Id">@c.Nombre</option>
                    }
                </select>
            </div>
            <div class="col-md-3">
                <label for="filtroLote" class="form-label small fw-semibold">Lote</label>
                <select id="filtroLote" class="form-select form-select-sm">
                    <option value="">Todos los lotes</option>
                </select>
            </div>
            <div class="col-md-3">
                <label for="filtroCultivo" class="form-label small fw-semibold">Cultivo</label>
                <select id="filtroCultivo" class="form-select form-select-sm">
                    <option value="">Todos los cultivos</option>
                    @foreach (var c in Model.Cultivos)
                    {
                        <option value="@c.Id">@c.Nombre</option>
                    }
                </select>
            </div>
            <div class="col-md-3 d-flex gap-1">
                <button id="btnFiltrar" class="btn btn-primary btn-sm flex-fill">
                    <i class="ph ph-funnel me-1"></i> Filtrar
                </button>
                <button id="btnLimpiarFiltros" class="btn btn-outline-secondary btn-sm">
                    <i class="ph ph-x"></i>
                </button>
            </div>
        </div>
    </div>

    <!-- Tarjetas de Resumen -->
    <div class="row g-2 mb-3" id="summaryCards">
        <div class="col-md-3 col-6">
            <div class="card summary-card border-0 shadow-sm h-100">
                <div class="card-body">
                    <div class="d-flex align-items-center gap-3">
                        <div class="icon-circle bg-primary bg-opacity-10 text-primary">
                            <i class="ph ph-map-trifold"></i>
                        </div>
                        <div>
                            <div class="text-muted small">Total Registros</div>
                            <div class="fw-bold fs-5" id="resumen-totalRegistros">@Model.Resumen.TotalRegistros</div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- Más tarjetas de resumen según necesites -->
    </div>

    <!-- Gráficos -->
    <div class="row g-2 mb-3">
        <div class="col-md-8">
            <div class="card shadow-sm">
                <div class="card-header bg-white py-2">
                    <h6 class="mb-0 fw-semibold">
                        <i class="ph ph-chart-bar me-1"></i>Gráfico Principal
                    </h6>
                </div>
                <div class="card-body">
                    <div class="chart-container">
                        <canvas id="chartPrincipal"></canvas>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card shadow-sm">
                <div class="card-header bg-white py-2">
                    <h6 class="mb-0 fw-semibold">
                        <i class="ph ph-chart-pie me-1"></i>Gráfico Secundario
                    </h6>
                </div>
                <div class="card-body">
                    <div class="chart-container">
                        <canvas id="chartSecundario"></canvas>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Tabla de datos -->
    <div class="card shadow-sm">
        <div class="card-header bg-white py-2">
            <h6 class="mb-0 fw-semibold">
                <i class="ph ph-table me-1"></i>Datos del Reporte
            </h6>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table id="tblReporte" class="table table-striped table-hover table-compact mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>#</th>
                            <th>Campo</th>
                            <th>Lote</th>
                            <th>Superficie (Ha)</th>
                            <th>Cultivo</th>
                            <!-- Agregar columnas específicas del reporte -->
                            <th>Valor Principal</th>
                            <th>Costo Total ($)</th>
                            <th>Costo/Ha ($)</th>
                        </tr>
                    </thead>
                    <tbody id="tablaBody">
                        <!-- Se llena dinámicamente -->
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

<!-- Chart.js -->
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/xlsx@0.18.5/dist/xlsx.full.min.js"></script>

@section Scripts {
    <script src="~/js/views/reporte{nombre}.js" asp-append-version="true"></script>
}
```

## 7. Frontend - JavaScript

### 7.1. Crear `wwwroot/js/views/reporte{nombre}.js`
```javascript
let chartPrincipal = null;
let chartSecundario = null;

$(document).ready(function () {
    // Inicializar
    cargarDatos();

    // Eventos
    $('#btnFiltrar').on('click', function () {
        cargarDatos();
    });

    $('#btnLimpiarFiltros').on('click', function () {
        $('#filtroCampo').val('');
        $('#filtroLote').val('');
        $('#filtroCultivo').val('');
        cargarDatos();
    });

    $('#btnRefrescar').on('click', function () {
        cargarDatos();
    });

    // Cuando cambia el campo, cargar lotes
    $('#filtroCampo').on('change', function () {
        var idCampo = $(this).val();
        if (idCampo) {
            cargarLotes(idCampo);
        } else {
            $('#filtroLote').empty().append('<option value="">Todos los lotes</option>');
        }
    });

    $('#btnExportarExcel').on('click', function () {
        exportarExcel();
    });
});

function cargarDatos() {
    var filtros = {
        idCampania: null,
        idCampo: $('#filtroCampo').val() || null,
        idLote: $('#filtroLote').val() || null,
        idCultivo: $('#filtroCultivo').val() || null
    };

    mostrarLoading(true);

    $.ajax({
        url: '/Reporte/Get{Nombre}Data',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(filtros),
        success: function (response) {
            if (response.success) {
                renderizarDatos(response.listObject);
            } else {
                toastr.error(response.message || 'Error al cargar datos');
            }
        },
        error: function (xhr) {
            var msg = xhr.responseJSON?.message || 'Error al conectar con el servidor';
            toastr.error(msg);
        },
        complete: function () {
            mostrarLoading(false);
        }
    });
}

function cargarLotes(idCampo) {
    $.ajax({
        url: '/Reporte/GetLotesByCampo/' + idCampo,
        type: 'GET',
        success: function (response) {
            var select = $('#filtroLote');
            select.empty().append('<option value="">Todos los lotes</option>');
            if (response.success && response.listObject) {
                $.each(response.listObject, function (i, item) {
                    select.append($('<option>', {
                        value: item.id,
                        text: item.nombre
                    }));
                });
            }
        },
        error: function () {
            toastr.error('Error al cargar lotes');
        }
    });
}

function renderizarDatos(datos) {
    if (!datos || datos.length === 0) {
        $('#tablaBody').html('<tr><td colspan="8" class="text-center text-muted py-4">No se encontraron datos con los filtros seleccionados</td></tr>');
        limpiarResumen();
        limpiarGraficos();
        return;
    }

    // Renderizar tabla
    renderizarTabla(datos);

    // Calcular y mostrar resumen
    calcularResumen(datos);

    // Renderizar gráficos
    renderizarGraficos(datos);
}

function renderizarTabla(datos) {
    var tbody = $('#tablaBody');
    tbody.empty();

    $.each(datos, function (i, item) {
        var rankingClass = '';
        if (item.rankingValor === 1) rankingClass = 'ranking-1';
        else if (item.rankingValor === 2) rankingClass = 'ranking-2';
        else if (item.rankingValor === 3) rankingClass = 'ranking-3';

        var rankingHtml = item.rankingValor > 0
            ? '<span class="ranking-badge ' + rankingClass + '">' + item.rankingValor + '</span>'
            : '<span class="text-muted">-</span>';

        var row = '<tr>' +
            '<td class="text-center">' + rankingHtml + '</td>' +
            '<td><span class="campo-name">' + escapeHtml(item.campo) + '</span></td>' +
            '<td><span class="lote-name">' + escapeHtml(item.lote) + '</span></td>' +
            '<td class="text-end">' + formatNum(item.superficieHa, 2) + '</td>' +
            '<td>' + escapeHtml(item.cultivo || '-') + '</td>' +
            '<td class="text-end fw-semibold">' + formatNum(item.valorPrincipal, 2) + '</td>' +
            '<td class="text-end">' + formatMoney(item.costoTotalARS) + '</td>' +
            '<td class="text-end">' + formatMoney(item.costoPorHaARS) + '</td>' +
            '</tr>';

        tbody.append(row);
    });
}

function calcularResumen(datos) {
    var totalRegistros = datos.length;
    var conValor = datos.filter(function (d) { return d.valorPrincipal != null; });
    var conCosto = datos.filter(function (d) { return d.costoPorHaARS != null; });

    // Valor promedio
    var valorProm = null;
    var valorMax = null;
    var valorMin = null;
    var mejorRegistro = null;
    var peorRegistro = null;

    if (conValor.length > 0) {
        var sum = conValor.reduce(function (a, b) { return a + b.valorPrincipal; }, 0);
        valorProm = sum / conValor.length;
        valorMax = Math.max.apply(null, conValor.map(function (d) { return d.valorPrincipal; }));
        valorMin = Math.min.apply(null, conValor.map(function (d) { return d.valorPrincipal; }));

        var mejor = conValor.reduce(function (a, b) { return a.valorPrincipal > b.valorPrincipal ? a : b; });
        mejorRegistro = mejor.lote + ' (' + mejor.campo + ')';

        var peor = conValor.reduce(function (a, b) { return a.valorPrincipal < b.valorPrincipal ? a : b; });
        peorRegistro = peor.lote + ' (' + peor.campo + ')';
    }

    // Costo promedio
    var costoProm = null;
    if (conCosto.length > 0) {
        var sumCosto = conCosto.reduce(function (a, b) { return a + b.costoPorHaARS; }, 0);
        costoProm = sumCosto / conCosto.length;
    }

    // Actualizar UI
    $('#resumen-totalRegistros').text(totalRegistros);
    $('#resumen-valorPromedio').text(valorProm != null ? valorProm.toFixed(2) : '-');
    $('#resumen-mejorRegistro').text(mejorRegistro || '-').attr('title', mejorRegistro || '');
    $('#resumen-mejorValor').text(valorMax != null ? valorMax.toFixed(2) : '');
    $('#resumen-costoPromedio').text(costoProm != null ? '$' + costoProm.toFixed(2) : '-');
}

function renderizarGraficos(datos) {
    var conValor = datos.filter(function (d) { return d.valorPrincipal != null; });
    var labels = conValor.map(function (d) { return d.lote; });
    var valores = conValor.map(function (d) { return d.valorPrincipal; });
    var colores = conValor.map(function (d) {
        if (d.rankingValor === 1) return '#ffd700';
        if (d.rankingValor === 2) return '#c0c0c0';
        if (d.rankingValor === 3) return '#cd7f32';
        return '#4CAF50';
    });

    // Gráfico principal (barras)
    var ctx1 = document.getElementById('chartPrincipal').getContext('2d');
    if (chartPrincipal) chartPrincipal.destroy();

    chartPrincipal = new Chart(ctx1, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Valor Principal',
                data: valores,
                backgroundColor: colores,
                borderColor: colores.map(function () { return 'rgba(0,0,0,0.1)'; }),
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Unidad'
                    }
                },
                x: {
                    ticks: {
                        maxRotation: 45,
                        font: { size: 10 }
                    }
                }
            }
        }
    });

    // Gráfico secundario (torta/donut)
    var conCostos = datos.filter(function (d) { return d.costoTotalARS != null && d.costoTotalARS > 0; });
    var labelsCostos = conCostos.map(function (d) { return d.lote; });
    var costos = conCostos.map(function (d) { return d.costoTotalARS; });
    var coloresCostos = [
        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF',
        '#FF9F40', '#C9CBCF', '#7BC8A4', '#E7E9ED', '#F7464A'
    ];

    var ctx2 = document.getElementById('chartSecundario').getContext('2d');
    if (chartSecundario) chartSecundario.destroy();

    chartSecundario = new Chart(ctx2, {
        type: 'doughnut',
        data: {
            labels: labelsCostos,
            datasets: [{
                data: costos,
                backgroundColor: coloresCostos.slice(0, costos.length),
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: { size: 10 },
                        boxWidth: 12,
                        padding: 8
                    }
                }
            }
        }
    });
}

function limpiarResumen() {
    $('#resumen-totalRegistros').text('0');
    $('#resumen-valorPromedio').text('-');
    $('#resumen-mejorRegistro').text('-');
    $('#resumen-mejorValor').text('');
    $('#resumen-costoPromedio').text('-');
}

function limpiarGraficos() {
    if (chartPrincipal) { chartPrincipal.destroy(); chartPrincipal = null; }
    if (chartSecundario) { chartSecundario.destroy(); chartSecundario = null; }
}

function exportarExcel() {
    var table = document.getElementById('tblReporte');
    if (!table) return;

    // Clonar la tabla para no modificar la original
    var clone = table.cloneNode(true);
    // Quitar la columna de ranking (primera columna)
    $(clone).find('tr').each(function () {
        $(this).find('th:first, td:first').remove();
    });

    var wb = XLSX.utils.table_to_book(clone, { sheet: 'Reporte' });
    XLSX.writeFile(wb, 'Reporte_{Nombre}.xlsx');
}

// Utilidades
function formatNum(val, decimals) {
    if (val == null) return '-';
    return Number(val).toLocaleString('es-AR', {
        minimumFractionDigits: decimals || 0,
        maximumFractionDigits: decimals || 0
    });
}

function formatMoney(val) {
    if (val == null) return '-';
    return '$ ' + Number(val).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    var d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function escapeHtml(str) {
    if (!str) return '';
    return $('<span>').text(str).html();
}

function mostrarLoading(show) {
    // Implementar spinner si es necesario
}
```

## 8. Checklist de Validación

### 8.1. Backend ✅
- [ ] DTO sigue convención de naming
- [ ] ViewModel con prefijo VM
- [ ] Service con async/await
- [ ] Controller con GenericResponse
- [ ] Manejo de errores con try-catch
- [ ] Logging implementado
- [ ] Autorización configurada
- [ ] Inyección de dependencias correcta

### 8.2. Frontend ✅
- [ ] jQuery + AJAX
- [ ] Bootstrap para estilos
- [ ] Chart.js para gráficos
- [ ] Toastr para notificaciones
- [ ] Estructura de carpetas correcta
- [ ] Formato localizado (es-AR)
- [ ] Exportación a Excel
- [ ] Filtros dinámicos

### 8.3. Patrones AgroForm ✅
- [ ] Sin abstracciones innecesarias
- [ ] Código simple y directo
- [ ] Consistencia sobre optimización
- [ ] Reutilización de código existente
- [ ] Nombres en inglés (código)
- [ ] Explicaciones en español (comentarios)

## 9. Pruebas Recomendadas

1. **Unit Tests** para el Service layer
2. **Integration Tests** para endpoints AJAX
3. **E2E Tests** para flujo completo de filtros
4. **Tests de carga** para reportes con muchos datos

---

Este workflow asegura que cualquier nuevo reporte mantenga la calidad y consistencia del reporte "Comparativa Campos/Lotes".
