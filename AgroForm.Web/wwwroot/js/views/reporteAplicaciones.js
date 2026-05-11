// ============================================================
// Reporte de Aplicaciones Agrícolas - Módulo JavaScript
// Maneja: KPIs, Tabla, Timeline, Insumos, Gráficos, Trazabilidad
// ============================================================

/** @type {AplicacionReporteDto} */
let reporteData = null;

/** @type {Object<string, Chart|null>} */
const charts = {};

/** @type {{ col: string, dir: string }} */
let sortConfig = { col: 'Fecha', dir: 'desc' };

/** @type {number} */
let currentPage = 1;

/** @type {number} */
const pageSize = 20;

// ============================================================
// Inicialización
// ============================================================
$(document).ready(function () {
    initFiltros();
    initTabs();
});

function initTabs() {
    // Redibujar gráficos al cambiar de tab para corregir tamaño
    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        const targetId = $(e.target).attr('data-bs-target');
        if (targetId === '#panel-graficos' || targetId === '#panel-dashboard') {
            // Forzar resize de charts
            Object.values(charts).forEach(chart => {
                if (chart) chart.resize();
            });
        }
    });
}

function initFiltros() {
    const model = window.aplicacionesModel || { campanias: [], campos: [], cultivos: [], productos: [] };

    // Poblar selects desde el modelo (ya vienen con opciones del servidor)
    // El select de Lotes se carga vía AJAX cuando cambia el campo
    $('#filtroCampo').on('change', function () {
        const idCampo = $(this).val();
        const $loteSelect = $('#filtroLote');
        $loteSelect.html('<option value="">Cargando...</option>').prop('disabled', true);

        if (!idCampo) {
            $loteSelect.html('<option value="">Todos</option>').prop('disabled', false);
            return;
        }

        $.get(`/Reporte/GetLotesByCampo/${idCampo}`)
            .done(function (response) {
                $loteSelect.html('<option value="">Todos</option>');
                if (response.success !== false && response.data) {
                    response.data.forEach(function (l) {
                        $loteSelect.append(`<option value="${l.id || l.Id}">${l.nombre || l.Nombre}</option>`);
                    });
                } else if (Array.isArray(response)) {
                    response.forEach(function (l) {
                        $loteSelect.append(`<option value="${l.id || l.Id}">${l.nombre || l.Nombre}</option>`);
                    });
                }
                $loteSelect.prop('disabled', false);
            })
            .fail(function () {
                $loteSelect.html('<option value="">Todos</option>').prop('disabled', false);
            });
    });

    // Enter en inputs de fecha dispara la búsqueda
    $('#filtroFechaDesde, #filtroFechaHasta').on('keydown', function (e) {
        if (e.key === 'Enter') cargarDatos();
    });
}

// ============================================================
// Carga de datos
// ============================================================
function cargarDatos() {
    const filters = {
        IdCampania: $('#filtroCampania').val() || null,
        IdCampo: $('#filtroCampo').val() || null,
        IdLote: $('#filtroLote').val() || null,
        IdCultivo: $('#filtroCultivo').val() || null,
        IdTipoAplicacion: $('#filtroTipoAplicacion').val() || null,
        IdProducto: $('#filtroProducto').val() || null,
        FechaDesde: $('#filtroFechaDesde').val() || null,
        FechaHasta: $('#filtroFechaHasta').val() || null,
        OrdenarPor: sortConfig.col,
        OrdenDireccion: sortConfig.dir,
        Pagina: currentPage,
        TamanoPagina: pageSize
    };

    // Mostrar loading
    $('#loadingState').show();
    $('#emptyState').hide();
    $('#reportContent').hide();

    $.ajax({
        url: '/Reporte/GetAplicacionesData',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(filters),
        success: function (response) {
            $('#loadingState').hide();

            if (response && response.success && response.object) {
                reporteData = response.object;
                renderReport();
            } else {
                showEmpty('No se encontraron aplicaciones con los filtros seleccionados.');
            }
        },
        error: function () {
            $('#loadingState').hide();
            showEmpty('Error al cargar los datos. Intente nuevamente.');
        }
    });
}

function showEmpty(message) {
    $('#emptyState').show().find('p').text(message || 'No se encontraron aplicaciones.');
    $('#reportContent').hide();
}

// ============================================================
// Renderizado completo del reporte
// ============================================================
function renderReport() {
    if (!reporteData) return;

    $('#reportContent').show();
    $('#emptyState').hide();

    renderKpis();
    renderTabla();
    renderPaginacion();
    renderTimeline();
    renderInsumos();
    renderGraficosDashboard();
    renderGraficosFull();
    renderTrazabilidad();
}

// ============================================================
// KPIs
// ============================================================
function renderKpis() {
    const k = reporteData.kpis || reporteData.Kpis || {};
    const moneda = k.moneda || k.Moneda || 'ARS';

    const kpis = [
        {
            icono: 'ph-drop-half',
            color: '#0d6efd',
            bg: 'rgba(13,110,253,0.1)',
            valor: formatoNumero(k.totalAplicaciones || k.TotalAplicaciones || 0),
            etiqueta: 'Total Aplicaciones',
            detalle: `${formatoNumero(k.totalPulverizaciones || k.TotalPulverizaciones || 0)} Pulv · ${formatoNumero(k.totalFertilizaciones || k.TotalFertilizaciones || 0)} Fert`
        },
        {
            icono: 'ph-drop',
            color: '#0dcaf0',
            bg: 'rgba(13,202,240,0.1)',
            valor: formatoNumero(k.totalLitrosAplicados || k.TotalLitrosAplicados || 0),
            etiqueta: 'Litros Aplicados',
            detalle: 'Volumen total de pulverización'
        },
        {
            icono: 'ph-cube',
            color: '#198754',
            bg: 'rgba(25,135,84,0.1)',
            valor: formatoNumero(k.totalKgAplicados || k.TotalKgAplicados || 0),
            etiqueta: 'Kg Aplicados',
            detalle: 'Peso total de fertilizantes'
        },
        {
            icono: 'ph-currency-circle-dollar',
            color: '#ffc107',
            bg: 'rgba(255,193,7,0.1)',
            valor: `$ ${formatoNumero(k.costoTotalARS || k.CostoTotalARS || 0)}`,
            etiqueta: 'Costo Total ARS',
            detalle: k.costoPromedioPorHaARS || k.CostoPromedioPorHaARS
                ? `$ ${formatoNumero(k.costoPromedioPorHaARS)} / Ha`
                : ''
        },
        {
            icono: 'ph-currency-dollar',
            color: '#20c997',
            bg: 'rgba(32,201,151,0.1)',
            valor: `$ ${formatoNumero(k.costoTotalUSD || k.CostoTotalUSD || 0)}`,
            etiqueta: 'Costo Total USD',
            detalle: k.costoPromedioPorHaUSD || k.CostoPromedioPorHaUSD
                ? `$ ${formatoNumero(k.costoPromedioPorHaUSD)} / Ha`
                : ''
        },
        {
            icono: 'ph-flask',
            color: '#6f42c1',
            bg: 'rgba(111,66,193,0.1)',
            valor: k.productoMasAplicado || k.ProductoMasAplicado || 'N/A',
            etiqueta: 'Agroquímico + usado',
            detalle: k.productoMasAplicadoCantidad || k.ProductoMasAplicadoCantidad
                ? `${k.productoMasAplicadoCantidad} aplicaciones` : ''
        },
        {
            icono: 'ph-leaf',
            color: '#198754',
            bg: 'rgba(25,135,84,0.1)',
            valor: k.nutrienteMasAplicado || k.NutrienteMasAplicado || 'N/A',
            etiqueta: 'Nutriente + usado',
            detalle: k.nutrienteMasAplicadoCantidad || k.NutrienteMasAplicadoCantidad
                ? `${k.nutrienteMasAplicadoCantidad} aplicaciones` : ''
        },
        {
            icono: 'ph-map-trifold',
            color: '#fd7e14',
            bg: 'rgba(253,126,20,0.1)',
            valor: formatoNumero(k.totalLotes || k.TotalLotes || 0),
            etiqueta: 'Lotes Tratados',
            detalle: `${formatoNumero(k.superficieTotalTratadaHa || k.SuperficieTotalTratadaHa || 0)} Ha · ${k.promedioAplicacionesPorLote || k.PromedioAplicacionesPorLote || 0} apps/lote`
        }
    ];

    const $container = $('#kpisSection').empty();

    kpis.forEach(function (kpi) {
        const card = `
            <div class="col-lg-3 col-md-4 col-sm-6">
                <div class="card-kpi d-flex align-items-center gap-3">
                    <div class="kpi-icon" style="background:${kpi.bg};color:${kpi.color};">
                        <i class="${kpi.icono}"></i>
                    </div>
                    <div class="flex-grow-1 min-width-0">
                        <div class="kpi-value text-truncate">${kpi.valor}</div>
                        <div class="kpi-label">${kpi.etiqueta}</div>
                        ${kpi.detalle ? `<div class="kpi-trend text-muted">${kpi.detalle}</div>` : ''}
                    </div>
                </div>
            </div>
        `;
        $container.append(card);
    });
}

// ============================================================
// Tabla Principal
// ============================================================
function renderTabla() {
    const datos = reporteData.datosAplicaciones || reporteData.DatosAplicaciones || [];
    const $tbody = $('#tablaBody').empty();

    if (datos.length === 0) {
        $tbody.append('<tr><td colspan="12" class="text-center text-muted py-4">No hay datos para mostrar.</td></tr>');
        return;
    }

    datos.forEach(function (a) {
        const tipoClase = a.idTipoActividad === 3 || a.IdTipoActividad === 3
            ? 'bg-primary bg-opacity-10 text-primary'
            : 'bg-success bg-opacity-10 text-success';
        const tipoIcono = a.tipoActividadIcono || a.TipoActividadIcono || 'ph-drop-half';
        const tipoNombre = a.tipoActividad || a.TipoActividad || 'N/A';
        const obs = a.observacionCortada || a.ObservacionCortada || a.observacion || a.Observacion || '-';

        const tr = `
            <tr>
                <td class="text-nowrap">${formatDate(a.fecha || a.Fecha)}</td>
                <td><span class="tipo-badge ${tipoClase}"><i class="${tipoIcono}"></i>${tipoNombre}</span></td>
                <td>${escapeHtml(a.lote || a.Lote || 'N/A')}</td>
                <td>${escapeHtml(a.campo || a.Campo || 'N/A')}</td>
                <td>${escapeHtml(a.cultivo || a.Cultivo || '-')}</td>
                <td>${escapeHtml(a.productoAplicado || a.ProductoAplicado || '-')}</td>
                <td>${a.dosis || a.Dosis ? formatoNumero(a.dosis || a.Dosis) + ' ' + (a.unidadDosis || a.UnidadDosis || '') : '-'}</td>
                <td>${a.cantidadTotal || a.CantidadTotal ? formatoNumero(a.cantidadTotal || a.CantidadTotal) + ' ' + (a.unidadCantidad || a.UnidadCantidad || '') : '-'}</td>
                <td>${a.costoARS || a.CostoARS ? '$ ' + formatoNumero(a.costoARS || a.CostoARS) : '-'}</td>
                <td>${a.costoUSD || a.CostoUSD ? '$ ' + formatoNumero(a.costoUSD || a.CostoUSD) : '-'}</td>
                <td>${escapeHtml(a.responsable || a.Responsable || '-')}</td>
                <td><span class="small text-muted" title="${escapeHtml(a.observacion || a.Observacion || '')}">${escapeHtml(obs)}</span></td>
            </tr>
        `;
        $tbody.append(tr);
    });

    // Configurar ordenamiento
    $('.table-reporte .sortable').off('click').on('click', function () {
        const col = $(this).data('col');
        if (!col) return;

        if (sortConfig.col === col) {
            sortConfig.dir = sortConfig.dir === 'asc' ? 'desc' : 'asc';
        } else {
            sortConfig.col = col;
            sortConfig.dir = 'desc';
        }

        $('.table-reporte .sortable').removeClass('active');
        $(this).addClass('active');
        $(this).find('.sort-icon').attr('class', sortConfig.dir === 'asc' ? 'ph-caret-up sort-icon' : 'ph-caret-down sort-icon');

        currentPage = 1;
        cargarDatos();
    });
}

// ============================================================
// Paginación
// ============================================================
function renderPaginacion() {
    const pag = reporteData.paginacion || reporteData.Paginacion || {};
    const totalReg = pag.totalRegistros || pag.TotalRegistros || 0;
    const totalPag = pag.totalPaginas || pag.TotalPaginas || 1;
    const pagActual = pag.paginaActual || pag.PaginaActual || 1;

    const $container = $('#paginacionContainer').empty();

    if (totalReg === 0) {
        $container.html('<span class="text-muted small">Sin registros</span>');
        return;
    }

    // Info de registros
    const desde = (pagActual - 1) * pageSize + 1;
    const hasta = Math.min(pagActual * pageSize, totalReg);
    const info = `<span class="text-muted small">Mostrando ${desde}-${hasta} de ${totalReg} registros</span>`;

    // Botones de paginación
    let pagHtml = '<ul class="pagination pagination-sm pagination-reporte mb-0">';

    // Anterior
    pagHtml += `<li class="page-item ${pagActual <= 1 ? 'disabled' : ''}">
        <a class="page-link" href="#" onclick="${pagActual > 1 ? `irPagina(${pagActual - 1})` : 'return false'}"><i class="ph-caret-left"></i></a>
    </li>`;

    // Páginas
    const inicio = Math.max(1, pagActual - 2);
    const fin = Math.min(totalPag, pagActual + 2);

    if (inicio > 1) {
        pagHtml += `<li class="page-item"><a class="page-link" href="#" onclick="irPagina(1)">1</a></li>`;
        if (inicio > 2) pagHtml += `<li class="page-item disabled"><a class="page-link">...</a></li>`;
    }

    for (let i = inicio; i <= fin; i++) {
        pagHtml += `<li class="page-item ${i === pagActual ? 'active' : ''}">
            <a class="page-link" href="#" onclick="irPagina(${i})">${i}</a>
        </li>`;
    }

    if (fin < totalPag) {
        if (fin < totalPag - 1) pagHtml += `<li class="page-item disabled"><a class="page-link">...</a></li>`;
        pagHtml += `<li class="page-item"><a class="page-link" href="#" onclick="irPagina(${totalPag})">${totalPag}</a></li>`;
    }

    // Siguiente
    pagHtml += `<li class="page-item ${pagActual >= totalPag ? 'disabled' : ''}">
        <a class="page-link" href="#" onclick="${pagActual < totalPag ? `irPagina(${pagActual + 1})` : 'return false'}"><i class="ph-caret-right"></i></a>
    </li>`;

    pagHtml += '</ul>';

    $container.append(info);
    $container.append(pagHtml);
}

function irPagina(pagina) {
    currentPage = pagina;
    cargarDatos();
}

// ============================================================
// Timeline
// ============================================================
function renderTimeline() {
    const items = reporteData.timeline || reporteData.Timeline || [];
    const $container = $('#timelineContainer').empty();

    if (items.length === 0) {
        $container.html('<p class="text-muted text-center py-4">No hay eventos de timeline para mostrar.</p>');
        return;
    }

    items.forEach(function (item) {
        const color = item.color || item.Color || '#6c757d';
        const icono = item.icono || item.Icono || 'ph-drop-half';
        const tipo = item.tipoActividad || item.TipoActividad || 'Aplicación';
        const desc = item.descripcion || item.Descripcion || '';
        const resp = item.responsable || item.Responsable || '';
        const costo = item.costoARS || item.CostoARS;

        const html = `
            <div class="timeline-item">
                <div class="timeline-icon" style="background:${color};">
                    <i class="${icono}"></i>
                </div>
                <div class="timeline-content">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <strong class="small" style="color:${color};">${tipo}</strong>
                            <span class="text-muted small ms-2">${formatDate(item.fecha || item.Fecha)}</span>
                            ${item.cultivo || item.Cultivo ? `<span class="badge bg-light text-dark ms-2">${escapeHtml(item.cultivo || item.Cultivo)}</span>` : ''}
                        </div>
                        ${costo ? `<small class="text-muted">$ ${formatoNumero(costo)}</small>` : ''}
                    </div>
                    <div class="mt-1">
                        <strong>${escapeHtml(item.lote || item.Lote || 'N/A')}</strong>
                        ${item.campania || item.Campania ? `<span class="text-muted small">· ${escapeHtml(item.campania || item.Campania)}</span>` : ''}
                    </div>
                    ${desc ? `<p class="mb-0 mt-1 small text-muted">${escapeHtml(desc)}</p>` : ''}
                    ${resp ? `<small class="text-muted"><i class="ph-user me-1"></i>${escapeHtml(resp)}</small>` : ''}
                </div>
            </div>
        `;
        $container.append(html);
    });
}

// ============================================================
// Insumos
// ============================================================
function renderInsumos() {
    const items = reporteData.analisisInsumos || reporteData.AnalisisInsumos || [];
    const $tbody = $('#insumosBody').empty();

    if (items.length === 0) {
        $tbody.append('<tr><td colspan="10" class="text-center text-muted py-4">No hay datos de insumos.</td></tr>');
        return;
    }

    const maxCantidad = Math.max(...items.map(i => i.cantidadTotal || i.CantidadTotal || 0), 1);

    items.forEach(function (item) {
        const cant = item.cantidadTotal || item.CantidadTotal || 0;
        const pct = (cant / maxCantidad) * 100;
        const barColor = (item.tipoProducto || item.TipoProducto || '').toLowerCase().includes('fertilizante')
            ? '#198754' : '#0d6efd';

        const tr = `
            <tr>
                <td>
                    <div class="d-flex align-items-center gap-2">
                        <div class="insumo-bar" style="width:${Math.max(pct, 4)}%;background:${barColor};"></div>
                        <span class="small">${escapeHtml(item.producto || item.Producto || 'N/A')}</span>
                    </div>
                </td>
                <td>${escapeHtml(item.tipoProducto || item.TipoProducto || '-')}</td>
                <td class="fw-semibold">${formatoNumero(cant)}</td>
                <td>${item.unidad || item.Unidad || '-'}</td>
                <td>${item.costoTotalARS || item.CostoTotalARS ? '$ ' + formatoNumero(item.costoTotalARS || item.CostoTotalARS) : '-'}</td>
                <td>${item.costoTotalUSD || item.CostoTotalUSD ? '$ ' + formatoNumero(item.costoTotalUSD || item.CostoTotalUSD) : '-'}</td>
                <td>${item.cantidadAplicaciones || item.CantidadAplicaciones || 0}</td>
                <td>${item.cantidadLotes || item.CantidadLotes || 0}</td>
                <td>${escapeHtml(item.cultivoPrincipal || item.CultivoPrincipal || '-')}</td>
                <td>${escapeHtml(item.campaniaPrincipal || item.CampaniaPrincipal || '-')}</td>
            </tr>
        `;
        $tbody.append(tr);
    });
}

// ============================================================
// Gráficos - Dashboard
// ============================================================
function renderGraficosDashboard() {
    renderCostosProducto('chartCostosProducto');
    renderDistribucionTipo('chartDistribucionTipo');
    renderTimelineApps('chartTimelineApps');
    renderComparativaCampania('chartComparativaCampania');
    renderComparativaCampo('chartComparativaCampo');
}

function renderGraficosFull() {
    renderCostosProducto('chartCostosProductoFull');
    renderDistribucionTipo('chartDistribucionTipoFull');
    renderTimelineApps('chartTimelineAppsFull');
    renderComparativaCampania('chartComparativaCampaniaFull');
    renderComparativaCampo('chartComparativaCampoFull');
}

// 1. Gráfico de Barras - Costos por Producto
function renderCostosProducto(canvasId) {
    destroyChart(canvasId);

    const data = reporteData.costosPorProducto || reporteData.CostosPorProducto || [];
    if (data.length === 0) return;

    const labels = data.map(d => d.producto || d.Producto || 'N/A');
    const valores = data.map(d => d.costoARS || d.CostoARS || 0);
    const colores = data.map(d => d.color || d.Color || '#0d6efd');

    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Costo ARS',
                data: valores,
                backgroundColor: colores,
                borderColor: colores.map(c => c),
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                datalabels: {
                    anchor: 'end',
                    align: 'end',
                    color: '#666',
                    font: { size: 10 },
                    formatter: function (value) { return value ? '$' + formatoNumero(value) : ''; }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (value) { return '$' + formatoNumero(value); }
                    }
                },
                x: {
                    ticks: {
                        maxRotation: 45,
                        font: { size: 10 }
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// 2. Gráfico de Torta/Donut - Distribución por Tipo
function renderDistribucionTipo(canvasId) {
    destroyChart(canvasId);

    const data = reporteData.distribucionPorTipo || reporteData.DistribucionPorTipo || [];
    if (data.length === 0) return;

    const labels = data.map(d => d.tipo || d.Tipo || 'N/A');
    const valores = data.map(d => d.cantidad || d.Cantidad || 0);
    const colores = data.map(d => d.color || d.Color || '#6c757d');
    const porcentajes = data.map(d => d.porcentaje || d.Porcentaje || 0);

    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    charts[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: valores,
                backgroundColor: colores,
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '55%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { padding: 16, usePointStyle: true, font: { size: 11 } }
                },
                datalabels: {
                    color: '#fff',
                    font: { weight: 'bold', size: 13 },
                    formatter: function (value, ctx) {
                        const pct = porcentajes[ctx.dataIndex] || 0;
                        return pct > 0 ? pct + '%' : '';
                    }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// 3. Gráfico de Líneas/Área - Timeline de Aplicaciones
function renderTimelineApps(canvasId) {
    destroyChart(canvasId);

    const data = reporteData.aplicacionesTimeline || reporteData.AplicacionesTimeline || [];
    if (data.length === 0) return;

    const labels = data.map(d => d.periodo || d.Periodo || '');
    const pulv = data.map(d => d.cantidadPulverizaciones || d.CantidadPulverizaciones || 0);
    const fert = data.map(d => d.cantidadFertilizaciones || d.CantidadFertilizaciones || 0);
    const total = data.map(d => d.totalAplicaciones || d.TotalAplicaciones || 0);

    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Pulverizaciones',
                    data: pulv,
                    backgroundColor: 'rgba(13,110,253,0.7)',
                    borderRadius: 2,
                    barPercentage: 0.4
                },
                {
                    label: 'Fertilizaciones',
                    data: fert,
                    backgroundColor: 'rgba(25,135,84,0.7)',
                    borderRadius: 2,
                    barPercentage: 0.4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: { usePointStyle: true, font: { size: 11 } }
                },
                datalabels: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1 }
                },
                x: {
                    ticks: { maxRotation: 45, font: { size: 10 } }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// 4. Gráfico de Barras Agrupadas - Comparativa por Campaña
function renderComparativaCampania(canvasId) {
    destroyChart(canvasId);

    const data = reporteData.comparativaPorCampania || reporteData.ComparativaPorCampania || [];
    if (data.length === 0) return;

    const labels = data.map(d => d.campania || d.Campania || '');
    const pulv = data.map(d => d.pulverizaciones || d.Pulverizaciones || 0);
    const fert = data.map(d => d.fertilizaciones || d.Fertilizaciones || 0);

    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Pulverizaciones',
                    data: pulv,
                    backgroundColor: 'rgba(13,110,253,0.7)',
                    borderRadius: 3,
                    barPercentage: 0.3
                },
                {
                    label: 'Fertilizaciones',
                    data: fert,
                    backgroundColor: 'rgba(25,135,84,0.7)',
                    borderRadius: 3,
                    barPercentage: 0.3
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    labels: { usePointStyle: true, font: { size: 11 } }
                },
                datalabels: { display: false }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { stepSize: 1 }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// 5. Gráfico de Barras Horizontal - Comparativa por Campo
function renderComparativaCampo(canvasId) {
    destroyChart(canvasId);

    const data = reporteData.comparativaPorCampo || reporteData.ComparativaPorCampo || [];
    if (data.length === 0) return;

    const labels = data.map(d => d.campo || d.Campo || '');
    const total = data.map(d => d.totalAplicaciones || d.TotalAplicaciones || 0);

    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    charts[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Total Aplicaciones',
                data: total,
                backgroundColor: 'rgba(13,110,253,0.6)',
                borderColor: 'rgba(13,110,253,0.8)',
                borderWidth: 1,
                borderRadius: 3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            plugins: {
                legend: { display: false },
                datalabels: {
                    anchor: 'end',
                    align: 'end',
                    color: '#666',
                    font: { size: 10 },
                    formatter: function (value) { return value || ''; }
                }
            },
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: { stepSize: 1 }
                }
            }
        },
        plugins: [ChartDataLabels]
    });
}

// ============================================================
// Trazabilidad
// ============================================================
function renderTrazabilidad() {
    const items = reporteData.trazabilidad || reporteData.Trazabilidad || [];
    const $container = $('#trazabilidadContainer').empty();

    if (items.length === 0) {
        $container.html('<p class="text-muted text-center py-4">No hay datos de trazabilidad.</p>');
        return;
    }

    items.forEach(function (item) {
        const color = item.color || item.Color || '#6c757d';
        const icono = item.icono || item.Icono || 'ph-drop-half';
        const tipo = item.tipoActividad || item.TipoActividad || 'Aplicación';
        const regDate = item.registrationDate || item.RegistrationDate
            ? formatDateTime(item.registrationDate || item.RegistrationDate)
            : '-';
        const modDate = item.modificationDate || item.ModificationDate
            ? formatDateTime(item.modificationDate || item.ModificationDate)
            : null;

        const html = `
            <div class="trace-item" style="border-left-color:${color};">
                <div class="d-flex justify-content-between align-items-start">
                    <div>
                        <span class="badge me-1" style="background:${color};">
                            <i class="${icono} me-1"></i>${tipo}
                        </span>
                        <strong>${escapeHtml(item.lote || item.Lote || 'N/A')}</strong>
                        <span class="text-muted small">· ${escapeHtml(item.campo || item.Campo || '')}</span>
                        ${item.campania || item.Campania ? `<span class="text-muted small">· ${escapeHtml(item.campania || item.Campania)}</span>` : ''}
                    </div>
                    <small class="text-muted text-nowrap">${formatDate(item.fecha || item.Fecha)}</small>
                </div>
                <div class="mt-1 small">
                    <span><i class="ph-flask me-1"></i>${escapeHtml(item.productoAplicado || item.ProductoAplicado || '-')}</span>
                    ${item.dosis || item.Dosis ? `<span class="ms-3"><i class="ph-eyedropper me-1"></i>${formatoNumero(item.dosis || item.Dosis)}</span>` : ''}
                    ${item.cantidadTotal || item.CantidadTotal ? `<span class="ms-3"><i class="ph-scales me-1"></i>${formatoNumero(item.cantidadTotal || item.CantidadTotal)}</span>` : ''}
                    ${item.costoARS || item.CostoARS ? `<span class="ms-3"><i class="ph-currency-circle-dollar me-1"></i>$${formatoNumero(item.costoARS || item.CostoARS)}</span>` : ''}
                </div>
                <div class="mt-1 small text-muted">
                    <i class="ph-user me-1"></i>${escapeHtml(item.responsable || item.Responsable || item.registrationUser || item.RegistrationUser || 'Sistema')}
                    <span class="ms-3"><i class="ph-calendar-plus me-1"></i>Reg: ${regDate}</span>
                    ${modDate ? `<span class="ms-3"><i class="ph-pencil me-1"></i>Mod: ${modDate}</span>` : ''}
                    ${item.modificationUser || item.ModificationUser ? `<span class="ms-1">por ${escapeHtml(item.modificationUser || item.ModificationUser)}</span>` : ''}
                </div>
            </div>
        `;
        $container.append(html);
    });
}

// ============================================================
// Exportación
// ============================================================
function exportarExcel() {
    const datos = reporteData && (reporteData.datosAplicaciones || reporteData.DatosAplicaciones);
    if (!datos || datos.length === 0) {
        alert('No hay datos para exportar. Genere el reporte primero.');
        return;
    }

    const wsData = datos.map(function (a) {
        return {
            'Fecha': formatDate(a.fecha || a.Fecha),
            'Tipo': a.tipoActividad || a.TipoActividad || '',
            'Lote': a.lote || a.Lote || '',
            'Campo': a.campo || a.Campo || '',
            'Cultivo': a.cultivo || a.Cultivo || '',
            'Producto': a.productoAplicado || a.ProductoAplicado || '',
            'Dosis': a.dosis || a.Dosis || '',
            'Cantidad': a.cantidadTotal || a.CantidadTotal || '',
            'Costo ARS': a.costoARS || a.CostoARS || '',
            'Costo USD': a.costoUSD || a.CostoUSD || '',
            'Costo/Ha': a.costoPorHa || a.CostoPorHa || '',
            'Responsable': a.responsable || a.Responsable || '',
            'Observación': a.observacion || a.Observacion || ''
        };
    });

    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.json_to_sheet(wsData);

    // Ancho de columnas
    ws['!cols'] = [
        { wch: 12 }, { wch: 15 }, { wch: 12 }, { wch: 12 },
        { wch: 12 }, { wch: 18 }, { wch: 10 }, { wch: 12 },
        { wch: 14 }, { wch: 14 }, { wch: 12 }, { wch: 15 }, { wch: 30 }
    ];

    XLSX.utils.book_append_sheet(wb, ws, 'Aplicaciones');
    XLSX.writeFile(wb, `Reporte_Aplicaciones_${new Date().toISOString().slice(0, 10)}.xlsx`);
}

function exportarPDF() {
    const $content = $('#reportContent');
    if ($content.is(':hidden') || !reporteData) {
        alert('No hay datos para exportar. Genere el reporte primero.');
        return;
    }

    const { jsPDF } = window.jspdf;
    const doc = new jsPDF('landscape', 'mm', 'a4');

    html2canvas($content[0], {
        scale: 2,
        useCORS: true,
        logging: false,
        backgroundColor: '#ffffff'
    }).then(function (canvas) {
        const imgData = canvas.toDataURL('image/png');
        const imgWidth = 280;
        const imgHeight = (canvas.height * imgWidth) / canvas.width;

        doc.addImage(imgData, 'PNG', 5, 10, imgWidth, imgHeight);
        doc.save(`Reporte_Aplicaciones_${new Date().toISOString().slice(0, 10)}.pdf`);
    });
}

// ============================================================
// Helpers
// ============================================================
function formatoNumero(valor) {
    if (valor === null || valor === undefined || isNaN(valor)) return '0';
    return Number(valor).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function formatDate(fecha) {
    if (!fecha) return '-';
    try {
        const d = new Date(fecha);
        if (isNaN(d.getTime())) return fecha;
        return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
    } catch (e) {
        return fecha;
    }
}

function formatDateTime(fecha) {
    if (!fecha) return '-';
    try {
        const d = new Date(fecha);
        if (isNaN(d.getTime())) return fecha;
        return d.toLocaleDateString('es-AR', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    } catch (e) {
        return fecha;
    }
}

function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function destroyChart(canvasId) {
    if (charts[canvasId]) {
        charts[canvasId].destroy();
        charts[canvasId] = null;
    }
}
