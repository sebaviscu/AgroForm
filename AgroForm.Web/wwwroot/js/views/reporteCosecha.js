// ============================================================
// reporteCosecha.js - Reporte de Rendimiento de Cosecha
// ============================================================

let reporteData = null;
let charts = {};
let sortConfig = { col: 'RendimientoTonHa', dir: 'desc' };
let currentPage = 1;
const pageSize = 20;
let tabsInitialized = false;

// ============================================================
// Initialization
// ============================================================
$(document).ready(function () {
    initFiltros();
    initTabs();
    cargarDatos();
});

function initTabs() {
    // Ensure Bootstrap tabs work properly with dynamically loaded content
    var tabEls = document.querySelectorAll('button[data-bs-toggle="tab"]');
    tabEls.forEach(function (el) {
        el.addEventListener('shown.bs.tab', function (e) {
            // Resize charts when a tab is shown (fixes chart sizing in hidden containers)
            var targetId = e.target.getAttribute('data-bs-target');
            if (!targetId) return;
            // Resize all charts when switching to a tab that contains charts
            Object.values(charts).forEach(function (chart) {
                if (chart && chart.resize) {
                    setTimeout(function () { chart.resize(); }, 50);
                }
            });
        });
    });
    tabsInitialized = true;
}

function initFiltros() {
    // Populate filters from ViewModel data passed from server
    var model = window.rendimientoCosechaModel;
    if (!model) return;

    if (model.campanias) {
        var $sel = $('#filtroCampania');
        model.campanias.forEach(function (c) {
            $sel.append('<option value="' + c.id + '">' + c.nombre + '</option>');
        });
    }
    if (model.campos) {
        var $sel = $('#filtroCampo');
        model.campos.forEach(function (c) {
            $sel.append('<option value="' + c.id + '">' + c.nombre + '</option>');
        });
    }
    if (model.cultivos) {
        var $sel = $('#filtroCultivo');
        model.cultivos.forEach(function (c) {
            $sel.append('<option value="' + c.id + '">' + c.nombre + '</option>');
        });
    }

    // Event: Campaña changed -> trigger search
    $('#filtroCampania').on('change', function () {
        cargarDatos();
    });

    // Event: Campo changed -> load lotes via AJAX
    $('#filtroCampo').on('change', function () {
        var campoId = parseInt($(this).val());
        var $loteSel = $('#filtroLote');
        $loteSel.empty().append('<option value="">Todos</option>');
        if (campoId) {
            $.getJSON('/Reporte/GetLotesByCampo/' + campoId, function (response) {
                if (response.success && response.data) {
                    response.data.forEach(function (l) {
                        $loteSel.append('<option value="' + l.id + '">' + l.nombre + '</option>');
                    });
                }
            });
        }
        // Trigger search after field change
        cargarDatos();
    });

    // Event: Cultivo changed -> trigger search
    $('#filtroCultivo').on('change', function () {
        cargarDatos();
    });

    // Event: Lote changed -> trigger search
    $('#filtroLote').on('change', function () {
        cargarDatos();
    });

    // Enter key triggers search
    $('.form-control, .form-select').on('keypress', function (e) {
        if (e.which === 13) cargarDatos();
    });
}

function limpiarFiltros() {
    $('#filtroCampania').val('');
    $('#filtroCampo').val('');
    $('#filtroLote').val('').empty().append('<option value="">Todos</option>');
    $('#filtroCultivo').val('');
    $('#filtroFechaDesde').val('');
    $('#filtroFechaHasta').val('');
    currentPage = 1;
    cargarDatos();
}

// ============================================================
// Data Loading
// ============================================================
function cargarDatos() {
    var request = {
        idCampania: parseInt($('#filtroCampania').val()) || null,
        idCampo: parseInt($('#filtroCampo').val()) || null,
        idLote: parseInt($('#filtroLote').val()) || null,
        idCultivo: parseInt($('#filtroCultivo').val()) || null,
        fechaDesde: $('#filtroFechaDesde').val() || null,
        fechaHasta: $('#filtroFechaHasta').val() || null,
        ordenarPor: sortConfig.col,
        ordenDireccion: sortConfig.dir,
        pagina: currentPage,
        tamanoPagina: pageSize
    };

    $('#loadingState').show();
    $('#reportData').hide();
    $('#emptyState').hide();

    $.ajax({
        url: '/Reporte/GetRendimientoCosechaData',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function (response) {
            $('#loadingState').hide();
            if (response.success && response.object) {
                reporteData = response.object;
                renderReport();
            } else {
                $('#emptyState').show();
            }
        },
        error: function () {
            $('#loadingState').hide();
            $('#emptyState').show();
        }
    });
}

// ============================================================
// Main Render
// ============================================================
function renderReport() {
    if (!reporteData) {
        $('#loadingState').hide();
        $('#emptyState').show();
        return;
    }

    // Hide loading and show report container first
    $('#loadingState').hide();
    $('#emptyState').hide();
    $('#reportData').show();

    // Render all components
    renderKpis(reporteData.kpis);
    renderResumen(reporteData);
    renderTabla(reporteData.datosLotes);
    renderPaginacion(reporteData.paginacion);
        renderGraficos(reporteData);
    renderEvolucion(reporteData.evolucionRendimiento);
    renderIndicadores(reporteData.indicadores);

    // Update indicator count badge
    if (reporteData.indicadores && reporteData.indicadores.length > 0) {
        $('#indicadorCount').text(reporteData.indicadores.length);
    } else {
        $('#indicadorCount').text('0');
    }

    // Resize all charts after showing the report container (fixes sizing in hidden containers)
    setTimeout(function () {
        Object.values(charts).forEach(function (chart) {
            if (chart && chart.resize) chart.resize();
        });
    }, 100);
}

// ============================================================
// KPI Cards
// ============================================================
function renderKpis(kpis) {
    if (!kpis) {
        $('#kpiContainer').html('<div class="col-12"><div class="alert alert-info">No hay datos KPI disponibles</div></div>');
        return;
    }

    var varClass = 'kpi-trend-stable';
    var varIcon = 'ph-minus';
    if (kpis.variacionVsCampaniaAnterior > 0) {
        varClass = 'kpi-trend-up';
        varIcon = 'ph-trend-up';
    } else if (kpis.variacionVsCampaniaAnterior < 0) {
        varClass = 'kpi-trend-down';
        varIcon = 'ph-trend-down';
    }

    var cards = [
        {
            icon: 'ph-chart-bar',
            color: '#4CAF50',
            label: 'Rend. Promedio',
            value: (kpis.rendimientoPromedioTonHa != null ? kpis.rendimientoPromedioTonHa.toFixed(2) : '-') + ' tn/ha',
            sub: (kpis.lotesConRendimiento || 0) + '/' + (kpis.totalLotes || 0) + ' lotes'
        },
        {
            icon: 'ph-trophy',
            color: '#FF9800',
            label: 'Mejor Lote',
            value: (kpis.rendimientoMaximoTonHa != null ? kpis.rendimientoMaximoTonHa.toFixed(2) : '-') + ' tn/ha',
            sub: kpis.loteMejorRendimiento || '-'
        },
        {
            icon: 'ph-trend-down',
            color: '#f44336',
            label: 'Peor Lote',
            value: (kpis.rendimientoMinimoTonHa != null ? kpis.rendimientoMinimoTonHa.toFixed(2) : '-') + ' tn/ha',
            sub: kpis.lotePeorRendimiento || '-'
        },
        {
            icon: 'ph-package',
            color: '#2196F3',
            label: 'Producción Total',
            value: (kpis.produccionTotalTon != null ? kpis.produccionTotalTon.toFixed(2) : '-') + ' tn',
            sub: 'Superficie: ' + (kpis.superficieTotalCosechadaHa != null ? kpis.superficieTotalCosechadaHa.toFixed(1) : '-') + ' ha'
        },
        {
            icon: 'ph-drop',
            color: '#00BCD4',
            label: 'Humedad Prom.',
            value: (kpis.humedadPromedio != null ? kpis.humedadPromedio.toFixed(1) : '-') + '%',
            sub: 'Humedad de grano'
        },
        {
            icon: 'ph-currency-circle-dollar',
            color: '#9C27B0',
            label: 'Variación vs Ant.',
            value: (kpis.variacionVsCampaniaAnterior != null ? (kpis.variacionVsCampaniaAnterior > 0 ? '+' : '') + kpis.variacionVsCampaniaAnterior.toFixed(1) : '-') + '%',
            sub: kpis.campaniaAnterior ? (kpis.campaniaActual || '') + ' vs ' + kpis.campaniaAnterior : 'Sin comparativa',
            cls: varClass
        }
    ];

    var html = '';
    cards.forEach(function (c) {
        html += '<div class="col-md-4 col-lg-2 col-sm-6">' +
            '<div class="card card-kpi border">' +
            '<div class="d-flex align-items-start gap-3">' +
            '<div class="kpi-icon" style="background:' + c.color + '22;color:' + c.color + '">' +
            '<i class="ph ' + c.icon + '"></i>' +
            '</div>' +
            '<div class="flex-grow-1 min-width-0">' +
            '<small class="text-muted d-block text-truncate">' + c.label + '</small>' +
            '<strong class="fs-5 d-block text-truncate">' + c.value + '</strong>' +
            '<small class="' + (c.cls || 'text-muted') + ' text-truncate d-block">' + c.sub + '</small>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
    });

    $('#kpiContainer').html(html);
}

// ============================================================
// Resumen (tab) - Charts Summary
// ============================================================
function renderResumen(data) {
    if (!data) {
        $('#resumenContainer').html('<div class="text-center py-4"><p class="text-muted">No hay datos para el resumen</p></div>');
        return;
    }

    var html = '<div class="row g-4">' +
        // Chart: Rendimiento por Cultivo
        '<div class="col-md-6">' +
        '<div class="card border h-100">' +
        '<div class="card-header bg-transparent py-2"><strong>Rendimiento por Cultivo</strong></div>' +
        '<div class="card-body"><div class="chart-container"><canvas id="chartResumenCultivo"></canvas></div></div>' +
        '</div>' +
        '</div>' +
        // Chart: Rendimiento por Campaña
        '<div class="col-md-6">' +
        '<div class="card border h-100">' +
        '<div class="card-header bg-transparent py-2"><strong>Rendimiento por Campaña</strong></div>' +
        '<div class="card-body"><div class="chart-container"><canvas id="chartResumenCampania"></canvas></div></div>' +
        '</div>' +
        '</div>' +
        '</div>';

    $('#resumenContainer').html(html);

    // Destroy existing charts
    if (charts.resumenCultivo) { charts.resumenCultivo.destroy(); charts.resumenCultivo = null; }
    if (charts.resumenCampania) { charts.resumenCampania.destroy(); charts.resumenCampania = null; }

    // Small delay to ensure DOM is ready
    setTimeout(function() {
        // Chart: Rendimiento por Cultivo (Bar)
        if (data.rendimientoPorCultivo && data.rendimientoPorCultivo.length > 0) {
            var canvas = document.getElementById('chartResumenCultivo');
            if (canvas) {
                var ctx = canvas.getContext('2d');
                var labels = data.rendimientoPorCultivo.map(function (d) { return d.cultivo; });
                var values = data.rendimientoPorCultivo.map(function (d) { return d.rendimientoPromedioTonHa; });
                var colors = data.rendimientoPorCultivo.map(function (d) { return d.color || '#4CAF50'; });

                charts.resumenCultivo = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: labels,
                        datasets: [{
                            label: 'tn/ha',
                            data: values,
                            backgroundColor: colors,
                            borderRadius: 6
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (ctx) {
                                        var item = data.rendimientoPorCultivo[ctx.dataIndex];
                                        return ctx.parsed.y.toFixed(2) + ' tn/ha | Sup: ' + (item.superficieTotalHa || 0).toFixed(1) + ' ha';
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: { display: true, text: 'tn/ha' }
                            }
                        }
                    }
                });
            }
        }

        // Chart: Rendimiento por Campaña (Line)
        if (data.rendimientoPorCampania && data.rendimientoPorCampania.length > 0) {
            var canvas2 = document.getElementById('chartResumenCampania');
            if (canvas2) {
                var ctx2 = canvas2.getContext('2d');
                charts.resumenCampania = new Chart(ctx2, {
                    type: 'line',
                    data: {
                        labels: data.rendimientoPorCampania.map(function (d) { return d.campania; }),
                        datasets: [{
                            label: 'Rendimiento (tn/ha)',
                            data: data.rendimientoPorCampania.map(function (d) { return d.rendimientoPromedioTonHa; }),
                            borderColor: '#4CAF50',
                            backgroundColor: 'rgba(76, 175, 80, 0.1)',
                            fill: true,
                            tension: 0.4,
                            pointRadius: 5,
                            pointBackgroundColor: '#4CAF50'
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (ctx) {
                                        var item = data.rendimientoPorCampania[ctx.dataIndex];
                                        return ctx.parsed.y.toFixed(2) + ' tn/ha | ' + (item.cantidadCosechas || 0) + ' cosechas';
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: { display: true, text: 'tn/ha' }
                            }
                        }
                    }
                });
            }
        }
    }, 100);
}

// ============================================================
// Data Table
// ============================================================
function renderTabla(datos) {
    if (!datos || datos.length === 0) {
        $('#tablaContainer').html('<div class="text-center py-4"><i class="ph ph-database display-6 text-muted"></i><p class="text-muted mt-2">No hay datos disponibles</p></div>');
        return;
    }

    var columns = [
        { key: 'rankingRendimiento', label: '#', width: '50px' },
        { key: 'lote', label: 'Lote', width: '' },
        { key: 'campo', label: 'Campo', width: '' },
        { key: 'cultivo', label: 'Cultivo', width: '' },
        { key: 'campania', label: 'Campaña', width: '' },
        { key: 'rendimientoTonHa', label: 'Rend. (tn/ha)', width: '120px' },
        { key: 'produccionTotalTon', label: 'Prod. (tn)', width: '110px' },
        { key: 'superficieCosechadaHa', label: 'Sup. (ha)', width: '90px' },
        { key: 'humedadGrano', label: 'Humedad %', width: '100px' },
        { key: 'fechaCosecha', label: 'Fecha', width: '110px' }
    ];

    var html = '<div class="table-responsive"><table class="table table-hover table-cosecha mb-0">';
    html += '<thead><tr>';
    columns.forEach(function (col) {
        var active = sortConfig.col === col.key ? 'active' : '';
        var arrow = '';
        if (sortConfig.col === col.key) {
            arrow = sortConfig.dir === 'asc' ? '&#9650;' : '&#9660;';
        }
        html += '<th style="width:' + col.width + '" class="' + active + '" onclick="ordenarPor(\'' + col.key + '\')">' +
            col.label + ' <span class="sort-icon">' + arrow + '</span></th>';
    });
    html += '</tr></thead><tbody>';

    datos.forEach(function (d) {
        var trendIcon = '';
        if (d.tendencia === 'subiendo') trendIcon = '<i class="ph ph-trend-up trend-up ms-1"></i>';
        else if (d.tendencia === 'bajando') trendIcon = '<i class="ph ph-trend-down trend-down ms-1"></i>';
        else trendIcon = '<i class="ph ph-minus trend-stable ms-1"></i>';

        var rankingClass = 'ranking-default';
        if (d.rankingRendimiento === 1) rankingClass = 'ranking-1';
        else if (d.rankingRendimiento === 2) rankingClass = 'ranking-2';
        else if (d.rankingRendimiento === 3) rankingClass = 'ranking-3';

        html += '<tr>' +
            '<td><span class="' + rankingClass + '">' + d.rankingRendimiento + '</span></td>' +
            '<td><strong>' + escapeHtml(d.lote) + '</strong></td>' +
            '<td>' + escapeHtml(d.campo) + '</td>' +
            '<td>' + escapeHtml(d.cultivo || '-') + '</td>' +
            '<td>' + escapeHtml(d.campania || '-') + '</td>' +
            '<td><strong>' + (d.rendimientoTonHa != null ? d.rendimientoTonHa.toFixed(2) : '-') + '</strong> ' + trendIcon + '</td>' +
            '<td>' + (d.produccionTotalTon != null ? d.produccionTotalTon.toFixed(2) : '-') + '</td>' +
            '<td>' + (d.superficieCosechadaHa != null ? d.superficieCosechadaHa.toFixed(1) : '-') + '</td>' +
            '<td>' + (d.humedadGrano != null ? d.humedadGrano.toFixed(1) + '%' : '-') + '</td>' +
            '<td>' + (d.fechaCosecha ? formatDate(d.fechaCosecha) : '-') + '</td>' +
            '</tr>';
    });

    html += '</tbody></table></div>';
    $('#tablaContainer').html(html);
}

function renderPaginacion(pag) {
    if (!pag || pag.totalPaginas <= 1) {
        $('#paginacionContainer').empty();
        return;
    }

    var html = '<div class="pagination-custom">' +
        '<button class="btn-page" onclick="irPagina(1)" ' + (pag.tieneAnterior ? '' : 'disabled') + '><i class="ph ph-caret-double-left"></i></button>' +
        '<button class="btn-page" onclick="irPagina(' + (pag.paginaActual - 1) + ')" ' + (pag.tieneAnterior ? '' : 'disabled') + '><i class="ph ph-caret-left"></i></button>';

    var start = Math.max(1, pag.paginaActual - 2);
    var end = Math.min(pag.totalPaginas, pag.paginaActual + 2);
    for (var i = start; i <= end; i++) {
        html += '<button class="btn-page ' + (i === pag.paginaActual ? 'active' : '') + '" onclick="irPagina(' + i + ')">' + i + '</button>';
    }

    html += '<button class="btn-page" onclick="irPagina(' + (pag.paginaActual + 1) + ')" ' + (pag.tieneSiguiente ? '' : 'disabled') + '><i class="ph ph-caret-right"></i></button>' +
        '<button class="btn-page" onclick="irPagina(' + pag.totalPaginas + ')" ' + (pag.tieneSiguiente ? '' : 'disabled') + '><i class="ph ph-caret-double-right"></i></button>' +
        '</div>' +
        '<div class="text-center pagination-info mt-2">Mostrando ' +
        ((pag.paginaActual - 1) * pag.tamanoPagina + 1) + '-' +
        Math.min(pag.paginaActual * pag.tamanoPagina, pag.totalRegistros) + ' de ' + pag.totalRegistros + ' registros</div>';

    $('#paginacionContainer').html(html);
}

function irPagina(pag) {
    currentPage = pag;
    cargarDatos();
}

function ordenarPor(col) {
    if (sortConfig.col === col) {
        sortConfig.dir = sortConfig.dir === 'asc' ? 'desc' : 'asc';
    } else {
        sortConfig.col = col;
        sortConfig.dir = 'desc';
    }
    currentPage = 1;
    cargarDatos();
}

// ============================================================
// Rankings
// ============================================================
function renderRankings(rankings) {
    if (!rankings || rankings.length === 0) {
        $('#rankingContainer').html('<div class="text-center py-4"><p class="text-muted">Sin datos de ranking</p></div>');
        return;
    }

    var html = '<div class="row g-3">';
    rankings.forEach(function (r) {
        var badgeClass = 'ranking-default';
        if (r.posicion === 1) badgeClass = 'ranking-1';
        else if (r.posicion === 2) badgeClass = 'ranking-2';
        else if (r.posicion === 3) badgeClass = 'ranking-3';

        html += '<div class="col-md-6">' +
            '<div class="card border">' +
            '<div class="card-body d-flex align-items-center gap-3 py-3">' +
            '<span class="' + badgeClass + '">' + r.posicion + '</span>' +
            '<div class="flex-grow-1">' +
            '<strong>' + escapeHtml(r.lote) + '</strong>' +
            '<small class="text-muted d-block">' + escapeHtml(r.campo) + ' | ' + escapeHtml(r.cultivo || '-') + '</small>' +
            '</div>' +
            '<div class="text-end">' +
            '<strong class="fs-5">' + r.rendimientoTonHa.toFixed(2) + '</strong>' +
            '<small class="text-muted d-block">tn/ha</small>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '</div>';
    });
    html += '</div>';
    $('#rankingContainer').html(html);
}

// ============================================================
// Charts (Gráficos tab)
// ============================================================
function renderGraficos(data) {
    if (!data) {
        $('#graficosContainer').html('<div class="text-center py-4"><p class="text-muted">No hay datos para gráficos</p></div>');
        return;
    }

    var hasCultivos = data.rendimientoPorCultivo && data.rendimientoPorCultivo.length > 0;
    var hasCampanias = data.rendimientoPorCampania && data.rendimientoPorCampania.length > 0;
    var hasCampos = data.rendimientoPorCampo && data.rendimientoPorCampo.length > 0;

    if (!hasCultivos && !hasCampanias && !hasCampos) {
        $('#graficosContainer').html('<div class="text-center py-4"><p class="text-muted">No hay suficientes datos para generar gráficos</p></div>');
        return;
    }

    var html = '<div class="row g-4">';
    if (hasCultivos) {
        html += '<div class="col-md-6">' +
            '<div class="card border h-100">' +
            '<div class="card-header bg-transparent py-2"><strong><i class="ph ph-chart-bar me-1"></i>Rendimiento por Cultivo</strong></div>' +
            '<div class="card-body"><div class="chart-container"><canvas id="chartPieCultivo"></canvas></div></div>' +
            '</div>' +
            '</div>';
    }
    if (hasCampanias) {
        html += '<div class="col-md-6">' +
            '<div class="card border h-100">' +
            '<div class="card-header bg-transparent py-2"><strong><i class="ph ph-chart-bar me-1"></i>Rendimiento por Campaña</strong></div>' +
            '<div class="card-body"><div class="chart-container"><canvas id="chartBarCampania"></canvas></div></div>' +
            '</div>' +
            '</div>';
    }
    if (hasCampos) {
        html += '<div class="col-md-6">' +
            '<div class="card border h-100">' +
            '<div class="card-header bg-transparent py-2"><strong><i class="ph ph-buildings me-1"></i>Rendimiento por Campo</strong></div>' +
            '<div class="card-body"><div class="chart-container"><canvas id="chartBarCampo"></canvas></div></div>' +
            '</div>' +
            '</div>';
    }
    html += '</div>';

    $('#graficosContainer').html(html);

    // Destroy existing charts
    if (charts.pieCultivo) { charts.pieCultivo.destroy(); charts.pieCultivo = null; }
    if (charts.barCampania) { charts.barCampania.destroy(); charts.barCampania = null; }
    if (charts.barCampo) { charts.barCampo.destroy(); charts.barCampo = null; }

    // Small delay to ensure DOM is ready
    setTimeout(function() {
        // Pie: Rendimiento por Cultivo
        if (hasCultivos) {
            var canvas = document.getElementById('chartPieCultivo');
            if (canvas) {
                var ctx = canvas.getContext('2d');
                var colors = data.rendimientoPorCultivo.map(function (d) { return d.color || '#4CAF50'; });
                charts.pieCultivo = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: data.rendimientoPorCultivo.map(function (d) { return d.cultivo; }),
                        datasets: [{
                            label: 'tn/ha',
                            data: data.rendimientoPorCultivo.map(function (d) { return d.rendimientoPromedioTonHa; }),
                            backgroundColor: colors,
                            borderColor: colors,
                            borderWidth: 1,
                            borderRadius: 6
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (ctx) {
                                        var item = data.rendimientoPorCultivo[ctx.dataIndex];
                                        return ctx.parsed.y.toFixed(2) + ' tn/ha (' + (item.cantidadLotes || 0) + ' lotes)';
                                    }
                                }
                            }
                        },
                        scales: {
                            y: {
                                beginAtZero: true,
                                title: { display: true, text: 'tn/ha' }
                            }
                        }
                    }
                });
            }
        }

        // Bar: Rendimiento por Campaña
        if (hasCampanias) {
            var canvas2 = document.getElementById('chartBarCampania');
            if (canvas2) {
                var ctx2 = canvas2.getContext('2d');
                charts.barCampania = new Chart(ctx2, {
                    type: 'bar',
                    data: {
                        labels: data.rendimientoPorCampania.map(function (d) { return d.campania; }),
                        datasets: [{
                            label: 'tn/ha',
                            data: data.rendimientoPorCampania.map(function (d) { return d.rendimientoPromedioTonHa; }),
                            backgroundColor: 'rgba(33, 150, 243, 0.7)',
                            borderColor: '#2196F3',
                            borderWidth: 1,
                            borderRadius: 4
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (ctx) {
                                        var item = data.rendimientoPorCampania[ctx.dataIndex];
                                        return ctx.parsed.y.toFixed(2) + ' tn/ha (' + item.cantidadCosechas + ' cosechas)';
                                    }
                                }
                            }
                        },
                        scales: {
                            y: { beginAtZero: true, title: { display: true, text: 'tn/ha' } }
                        }
                    }
                });
            }
        }

        // Bar: Rendimiento por Campo
        if (hasCampos) {
            var canvas3 = document.getElementById('chartBarCampo');
            if (canvas3) {
                var ctx3 = canvas3.getContext('2d');
                charts.barCampo = new Chart(ctx3, {
                    type: 'bar',
                    data: {
                        labels: data.rendimientoPorCampo.map(function (d) { return d.campo; }),
                        datasets: [{
                            label: 'tn/ha',
                            data: data.rendimientoPorCampo.map(function (d) { return d.rendimientoPromedioTonHa; }),
                            backgroundColor: 'rgba(76, 175, 80, 0.7)',
                            borderColor: '#4CAF50',
                            borderWidth: 1,
                            borderRadius: 4
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,
                        indexAxis: 'y',
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (ctx) {
                                        var item = data.rendimientoPorCampo[ctx.dataIndex];
                                        return ctx.parsed.x.toFixed(2) + ' tn/ha (' + item.cantidadLotes + ' lotes)';
                                    }
                                }
                            }
                        },
                        scales: {
                            x: { beginAtZero: true, title: { display: true, text: 'tn/ha' } }
                        }
                    }
                });
            }
        }
    }, 100);
}

// ============================================================
// Evolution Chart
// ============================================================
function renderEvolucion(evolucion) {
    if (!evolucion || evolucion.length === 0) {
        $('#evolucionContainer').html('<div class="text-center py-4"><p class="text-muted">No hay datos históricos suficientes para mostrar evolución</p></div>');
        return;
    }

    var html = '<div class="card border">' +
        '<div class="card-header bg-transparent py-2"><strong><i class="ph ph-chart-line me-1"></i>Evolución Histórica del Rendimiento</strong></div>' +
        '<div class="card-body">' +
        '<div class="chart-container" style="height:350px;"><canvas id="chartEvolucion"></canvas></div>' +
        '<div class="table-responsive mt-3">' +
        '<table class="table table-sm table-borderless mb-0">' +
        '<thead><tr><th>Período</th><th>Campaña</th><th>Cultivo</th><th class="text-end">Rend. (tn/ha)</th><th class="text-end">Humedad</th><th class="text-end">Superficie</th></tr></thead><tbody>';

    evolucion.forEach(function (e) {
        html += '<tr>' +
            '<td>' + escapeHtml(e.periodo) + '</td>' +
            '<td>' + escapeHtml(e.campania || '-') + '</td>' +
            '<td>' + escapeHtml(e.cultivo || '-') + '</td>' +
            '<td class="text-end fw-bold">' + e.rendimientoTonHa.toFixed(2) + '</td>' +
            '<td class="text-end">' + (e.humedadPromedio != null ? e.humedadPromedio.toFixed(1) + '%' : '-') + '</td>' +
            '<td class="text-end">' + (e.superficieHa != null ? e.superficieHa.toFixed(1) + ' ha' : '-') + '</td>' +
            '</tr>';
    });

    html += '</tbody></table></div></div></div>';
    $('#evolucionContainer').html(html);

    if (charts.evolucion) { charts.evolucion.destroy(); }

    var ctx = document.getElementById('chartEvolucion').getContext('2d');
    charts.evolucion = new Chart(ctx, {
        type: 'line',
        data: {
            labels: evolucion.map(function (e) { return e.periodo; }),
            datasets: [
                {
                    label: 'Rendimiento (tn/ha)',
                    data: evolucion.map(function (e) { return e.rendimientoTonHa; }),
                    borderColor: '#4CAF50',
                    backgroundColor: 'rgba(76, 175, 80, 0.1)',
                    fill: true,
                    tension: 0.4,
                    pointRadius: 6,
                    pointBackgroundColor: '#4CAF50',
                    yAxisID: 'y'
                },
                {
                    label: 'Humedad (%)',
                    data: evolucion.map(function (e) { return e.humedadPromedio; }),
                    borderColor: '#00BCD4',
                    backgroundColor: 'rgba(0, 188, 212, 0.1)',
                    fill: true,
                    tension: 0.4,
                    pointRadius: 6,
                    pointBackgroundColor: '#00BCD4',
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { position: 'top' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            return ctx.dataset.label + ': ' + (ctx.parsed.y != null ? ctx.parsed.y.toFixed(2) : '-');
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    position: 'left',
                    title: { display: true, text: 'tn/ha' }
                },
                y1: {
                    beginAtZero: true,
                    position: 'right',
                    title: { display: true, text: '%' },
                    grid: { drawOnChartArea: false }
                }
            }
        }
    });
}

// ============================================================
// Smart Indicators
// ============================================================
function renderIndicadores(indicadores) {
    if (!indicadores || indicadores.length === 0) {
        $('#indicadoresContainer').html(
            '<div class="text-center py-4">' +
            '<i class="ph ph-check-circle display-4 text-success mb-3"></i>' +
            '<p class="text-muted">No se detectaron alertas. Todos los indicadores están en rangos normales.</p>' +
            '</div>');
        $('#indicadorCount').text('0');
        return;
    }

    $('#indicadorCount').text(indicadores.length);

    var html = '<div class="row g-3">';
    indicadores.forEach(function (ind) {
        var severityClass = 'indicator-severity-' + (ind.severidad || 'baja').toLowerCase();

        html += '<div class="col-md-6">' +
            '<div class="indicator-card ' + severityClass + '">' +
            '<div class="d-flex gap-3">' +
            '<div class="indicator-icon" style="background:' + (ind.color || '#6c757d') + '22;color:' + (ind.color || '#6c757d') + '">' +
            '<i class="ph ' + (ind.icono || 'ph-info') + '"></i>' +
            '</div>' +
            '<div class="flex-grow-1">' +
            '<div class="d-flex justify-content-between align-items-start">' +
            '<strong>' + (ind.titulo || ind.tipo) + '</strong>' +
            '<span class="badge bg-' + (ind.severidad === 'Alta' ? 'danger' : ind.severidad === 'Media' ? 'warning' : 'info') + ' ms-2">' + ind.severidad + '</span>' +
            '</div>' +
            '<p class="mb-1 small mt-1">' + escapeHtml(ind.mensaje) + '</p>';

        if (ind.recomendacion) {
            html += '<div class="mt-2 p-2 rounded" style="background:var(--bs-tertiary-bg)">' +
                '<small><strong><i class="ph ph-lightbulb me-1"></i>Recomendación:</strong> ' + escapeHtml(ind.recomendacion) + '</small>' +
                '</div>';
        }

        html += '</div></div></div></div>';
    });
    html += '</div>';
    $('#indicadoresContainer').html(html);
}

// ============================================================
// Export Functions
// ============================================================
function exportarExcel() {
    if (!reporteData || !reporteData.datosLotes || reporteData.datosLotes.length === 0) {
        alert('No hay datos para exportar');
        return;
    }

    var data = reporteData.datosLotes.map(function (d) {
        return {
            '#': d.rankingRendimiento,
            'Lote': d.lote,
            'Campo': d.campo,
            'Cultivo': d.cultivo || '',
            'Campaña': d.campania || '',
            'Rendimiento (tn/ha)': d.rendimientoTonHa != null ? d.rendimientoTonHa : '',
            'Producción (tn)': d.produccionTotalTon != null ? d.produccionTotalTon : '',
            'Superficie (ha)': d.superficieCosechadaHa != null ? d.superficieCosechadaHa : '',
            'Humedad (%)': d.humedadGrano != null ? d.humedadGrano : '',
            'Fecha Cosecha': d.fechaCosecha ? formatDate(d.fechaCosecha) : ''
        };
    });

    var ws = XLSX.utils.json_to_sheet(data);
    var wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Rendimiento');

    // Add KPIs sheet
    var kpis = reporteData.kpis;
    var kpiData = [
        { 'Indicador': 'Rendimiento Promedio', 'Valor': kpis.rendimientoPromedioTonHa != null ? kpis.rendimientoPromedioTonHa + ' tn/ha' : '-' },
        { 'Indicador': 'Mejor Rendimiento', 'Valor': kpis.rendimientoMaximoTonHa != null ? kpis.rendimientoMaximoTonHa + ' tn/ha (' + (kpis.loteMejorRendimiento || '') + ')' : '-' },
        { 'Indicador': 'Peor Rendimiento', 'Valor': kpis.rendimientoMinimoTonHa != null ? kpis.rendimientoMinimoTonHa + ' tn/ha (' + (kpis.lotePeorRendimiento || '') + ')' : '-' },
        { 'Indicador': 'Producción Total', 'Valor': kpis.produccionTotalTon != null ? kpis.produccionTotalTon + ' tn' : '-' },
        { 'Indicador': 'Superficie Cosechada', 'Valor': kpis.superficieTotalCosechadaHa != null ? kpis.superficieTotalCosechadaHa + ' ha' : '-' },
        { 'Indicador': 'Humedad Promedio', 'Valor': kpis.humedadPromedio != null ? kpis.humedadPromedio + '%' : '-' },
        { 'Indicador': 'Variación vs Anterior', 'Valor': kpis.variacionVsCampaniaAnterior != null ? kpis.variacionVsCampaniaAnterior + '%' : '-' }
    ];
    var wsKpi = XLSX.utils.json_to_sheet(kpiData);
    XLSX.utils.book_append_sheet(wb, wsKpi, 'KPIs');

    // Add rankings sheet
    if (reporteData.rankingLotes && reporteData.rankingLotes.length > 0) {
        var rankData = reporteData.rankingLotes.map(function (r) {
            return { 'Posición': r.posicion, 'Lote': r.lote, 'Campo': r.campo, 'Cultivo': r.cultivo || '', 'Rendimiento (tn/ha)': r.rendimientoTonHa };
        });
        var wsRank = XLSX.utils.json_to_sheet(rankData);
        XLSX.utils.book_append_sheet(wb, wsRank, 'Rankings');
    }

    XLSX.writeFile(wb, 'Rendimiento_Cosecha_' + new Date().toISOString().slice(0, 10) + '.xlsx');
}

function exportarPDF() {
    if (!reporteData) {
        alert('No hay datos para exportar');
        return;
    }

    var element = document.getElementById('reportData');
    if (!element) return;

    // Show loading feedback
    var btn = event && event.target ? $(event.target).closest('button') : null;
    if (btn) { btn.prop('disabled', true).html('<i class="ph ph-spinner ph-spin me-1"></i>Generando...'); }

    html2canvas(element, {
        scale: 2,
        useCORS: true,
        backgroundColor: '#ffffff',
        logging: false
    }).then(function (canvas) {
        var imgData = canvas.toDataURL('image/png');
        var { jsPDF } = window.jspdf;
        var pdf = new jsPDF('l', 'mm', 'a4');
        var pageWidth = pdf.internal.pageSize.getWidth();
        var pageHeight = pdf.internal.pageSize.getHeight();
        var imgWidth = pageWidth - 20;
        var imgHeight = (canvas.height * imgWidth) / canvas.width;

        var heightLeft = imgHeight;
        var position = 10;

        pdf.addImage(imgData, 'PNG', 10, position, imgWidth, imgHeight);
        heightLeft -= pageHeight - 20;

        while (heightLeft > 0) {
            position = heightLeft - imgHeight + 10;
            pdf.addPage();
            pdf.addImage(imgData, 'PNG', 10, position, imgWidth, imgHeight);
            heightLeft -= pageHeight - 20;
        }

        pdf.save('Rendimiento_Cosecha_' + new Date().toISOString().slice(0, 10) + '.pdf');

        if (btn) { btn.prop('disabled', false).html('<i class="ph ph-file-pdf me-1"></i>PDF'); }
    }).catch(function () {
        if (btn) { btn.prop('disabled', false).html('<i class="ph ph-file-pdf me-1"></i>PDF'); }
        alert('Error al generar PDF');
    });
}

// ============================================================
// Utility Functions
// ============================================================
function formatDate(dateStr) {
    if (!dateStr) return '-';
    var d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    return d.toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function escapeHtml(str) {
    if (!str) return '';
    // Solo escapar caracteres realmente peligrosos: <, >, &, "
    return str.replace(/&/g, '&amp;')
              .replace(/</g, '&lt;')
              .replace(/>/g, '&gt;')
              .replace(/"/g, '&quot;');
}

