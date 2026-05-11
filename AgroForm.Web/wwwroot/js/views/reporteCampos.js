// ============================================================
// reporteCampos.js - Informe Integral del Campo
// ============================================================

let reporteData = null;
let charts = {};
let currentCampoId = 0;

$(document).ready(function () {
    cargarCampos();

    // Event: Select campo
    $('#selectCampos').on('change', function () {
        var id = parseInt($(this).val());
        if (id) {
            currentCampoId = id;
            cargarCampanias(id);
            $('#btnGenerarReporte').prop('disabled', false);
        } else {
            $('#btnGenerarReporte').prop('disabled', true);
        }
    });

    // Event: Generate report
    $('#btnGenerarReporte').on('click', function () {
        generarReporte();
    });
});

// Handle idCampo from URL query param (e.g., from "Ver Informe" button in Campos table)
function autoCargarCampo(idCampo) {
    currentCampoId = idCampo;
    var attempts = 0;
    var maxAttempts = 20; // 20 * 300ms = 6 seconds max wait
    
    // Wait for campos to load, then select
    var checkInterval = setInterval(function () {
        attempts++;
        var select = $('#selectCampos');
        
        if (select.find('option[value="' + idCampo + '"]').length > 0) {
            clearInterval(checkInterval);
            select.val(idCampo).trigger('change');
            setTimeout(function () {
                generarReporte();
            }, 500);
        } else if (attempts >= maxAttempts) {
            // Timeout: campo not found after max attempts
            clearInterval(checkInterval);
            // Hide spinner and show error message
            $('#reportLoading').html(`
                <i class="ph ph-warning-circle display-1 text-warning mb-3"></i>
                <h5 class="text-muted">Campo no encontrado</h5>
                <p class="text-muted">El campo con ID ${idCampo} no existe o no está disponible.</p>
                <button class="btn btn-primary" onclick="location.href='/Reporte/Campo'">
                    <i class="ph ph-house me-1"></i> Volver al Reporte
                </button>
            `);
        }
    }, 300);
}

// Check URL for idCampo query param
(function () {
    var params = new URLSearchParams(window.location.search);
    var idCampo = params.get('idCampo');
    if (idCampo) {
        // Wait for DOM ready then auto-load
        $(function () {
            setTimeout(function () {
                autoCargarCampo(parseInt(idCampo));
            }, 1000);
        });
    }
})();

// ============================================================
// DATA LOADING
// ============================================================

function cargarCampos() {
    $.ajax({
        url: '/Campo/GetAll',
        type: 'GET',
        success: function (result) {
            if (result.success) {
                var select = $('#selectCampos');
                select.empty().append('<option value="" selected disabled>Seleccione un campo...</option>');
                $.each(result.listObject, function (index, campo) {
                    select.append($('<option>', {
                        value: campo.id,
                        text: campo.nombre
                    }));
                });
            } else {
                mostrarMensaje('Error al cargar los campos', 'error');
            }
        },
        error: function () {
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}

function cargarCampanias(idCampo) {
    $.ajax({
        url: '/Reporte/GetCampaniasByCampo/' + idCampo,
        type: 'GET',
        success: function (result) {
            var select = $('#selectCampania');
            select.empty().append('<option value="">Todas las campañas</option>');
            if (result.success && result.data) {
                $.each(result.data, function (index, camp) {
                    select.append($('<option>', {
                        value: camp.id,
                        text: camp.nombre
                    }));
                });
                select.prop('disabled', false);
            }
        },
        error: function () {
            $('#selectCampania').prop('disabled', true);
        }
    });
}

function generarReporte() {
    var idCampo = parseInt($('#selectCampos').val());
    var idCampania = parseInt($('#selectCampania').val()) || null;

    if (!idCampo) {
        mostrarMensaje('Seleccione un campo', 'warning');
        return;
    }

    // Show loading
    $('#reportLoading').html(`
        <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;">
            <span class="visually-hidden">Cargando...</span>
        </div>
        <h5 class="text-muted">Generando informe integral...</h5>
        <p class="text-muted">Procesando datos agronómicos, climáticos y de costos.</p>
    `).show();
    $('#reportData').hide();

    $.ajax({
        url: '/Reporte/GetReporteCampoIntegral',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            idCampo: idCampo,
            idCampania: idCampania
        }),
        success: function (result) {
            if (result.success && result.object) {
                reporteData = result.object;
                renderizarReporte();
            } else {
                mostrarError(result.message || 'Error al generar el reporte');
            }
        },
        error: function (xhr) {
            var msg = 'Error al conectar con el servidor';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
        }
    });
}

function mostrarError(msg) {
    $('#reportLoading').html(`
        <i class="ph ph-warning-circle display-1 text-danger mb-3"></i>
        <h5 class="text-muted">Error al generar el informe</h5>
        <p class="text-danger">${msg}</p>
        <button class="btn btn-primary" onclick="generarReporte()">
            <i class="ph ph-arrow-clockwise me-1"></i> Reintentar
        </button>
    `);
}

// ============================================================
// RENDER ALL SECTIONS
// ============================================================

function renderizarReporte() {
    var data = reporteData;
    if (!data) return;

    $('#reportLoading').hide();
    $('#reportData').show();

    renderizarResumenEjecutivo(data.resumenEjecutivo);
    renderizarTimeline(data.timeline);
    renderizarEvolucion(data.evolucionCultivo);
    renderizarClima(data.analisisClimatico);
    renderizarSuelo(data.analisisSuelo);
    renderizarCostos(data.costosRentabilidad);
    renderizarRendimiento(data.rendimientoCosecha);
    renderizarAlertas(data.alertas);
    renderizarHistorial(data.historialMultiCampania);

    // Load campos for comparativa dropdown
    cargarCamposComparativa(data.resumenEjecutivo ? data.resumenEjecutivo.campo : null);

    // Activate first tab
    $('#reporteTabs button:first').tab('show');

    // Setup comparativa tab listener (render on first click)
    $('#comparativa-tab').off('click.comparativa').on('click.comparativa', function () {
        renderizarComparativa();
    });
}

// ============================================================
// 1. RESUMEN EJECUTIVO
// ============================================================

function renderizarResumenEjecutivo(r) {
    if (!r) return;

    var estadoColor = r.estadoGeneral === 'Excelente' ? '#28a745'
        : r.estadoGeneral === 'Bueno' ? '#17a2b8'
        : r.estadoGeneral === 'Regular' ? '#ffc107'
        : '#dc3545';

    // Build multi-crop display with active/inactive visual hint
    var cultivosHtml = '';
    if (r.cultivos && r.cultivos.length > 0) {
        cultivosHtml = '<div class="d-flex flex-wrap gap-1 mt-1">';
        $.each(r.cultivos, function (i, c) {
            var isActive = c.isActivo === true;
            var bgClass = isActive ? 'bg-success' : 'bg-secondary';
            var textClass = isActive ? 'text-success' : 'text-secondary';
            var iconClass = isActive ? 'ph-tree' : 'ph-tree-evergreen';
            cultivosHtml += `<span class="badge ${bgClass} bg-opacity-10 border ${bgClass} border-opacity-25 ${textClass} px-2 py-1 d-inline-flex align-items-center gap-1" style="font-size:0.75rem;white-space:nowrap;">
                <i class="ph ${iconClass}"></i>
                ${c.nombre}${c.variedad ? ' (' + c.variedad + ')' : ''}
                <span class="badge ${isActive ? 'bg-success' : 'bg-secondary'} text-white ms-1" style="font-size:0.55rem;">${isActive ? 'Activo' : 'Inactivo'}</span>
                <small class="text-muted" style="font-size:0.65rem;">${formatNum(c.superficieHa, 1)} ha</small>
            </span>`;
        });
        cultivosHtml += '</div>';
    } else {
        cultivosHtml = '<span class="text-muted">Sin cultivos registrados</span>';
    }

    var html = `
        <div class="row g-3">
            <div class="col-lg-8">
                <div class="row g-3">
                    <div class="col-md-4 col-6">
                        <div class="kpi-card bg-primary bg-opacity-10 border border-primary border-opacity-25">
                            <div class="d-flex align-items-center gap-3">
                                <div class="kpi-icon bg-primary text-white">
                                    <i class="ph ph-tree"></i>
                                </div>
                                <div>
                                    <small class="text-muted text-uppercase fw-semibold">Cultivos</small>
                                    <h5 class="mb-0 fw-bold">${r.cultivos && r.cultivos.length > 0 ? r.cultivos.length : 'N/A'} <small>cultivo${r.cultivos && r.cultivos.length !== 1 ? 's' : ''}</small></h5>
                                    ${cultivosHtml}
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 col-6">
                        <div class="kpi-card bg-success bg-opacity-10 border border-success border-opacity-25">
                            <div class="d-flex align-items-center gap-3">
                                <div class="kpi-icon bg-success text-white">
                                    <i class="ph ph-ruler"></i>
                                </div>
                                <div>
                                    <small class="text-muted text-uppercase fw-semibold">Superficie</small>
                                    <h5 class="mb-0 fw-bold">${formatNum(r.superficieHa, 1)} <small>ha</small></h5>
                                    <small class="text-muted">${r.campania || ''}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 col-6">
                        <div class="kpi-card bg-info bg-opacity-10 border border-info border-opacity-25">
                            <div class="d-flex align-items-center gap-3">
                                <div class="kpi-icon bg-info text-white">
                                    <i class="ph ph-calendar"></i>
                                </div>
                                <div>
                                    <small class="text-muted text-uppercase fw-semibold">Días desde siembra</small>
                                    <h5 class="mb-0 fw-bold">${r.diasDesdeSiembra || '-'} <small>días</small></h5>
                                    <small class="text-muted">${formatDate(r.fechaSiembra)}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 col-6">
                        <div class="kpi-card bg-warning bg-opacity-10 border border-warning border-opacity-25">
                            <div class="d-flex align-items-center gap-3">
                                <div class="kpi-icon bg-warning text-white">
                                    <i class="ph ph-cloud-rain"></i>
                                </div>
                                <div>
                                    <small class="text-muted text-uppercase fw-semibold">Última lluvia</small>
                                    <h5 class="mb-0 fw-bold">${r.ultimaLluvia || 'Sin datos'}</h5>
                                    <small class="text-muted">NDVI: ${r.ndviPromedio != null ? r.ndviPromedio : '-'}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 col-6">
                        <div class="kpi-card border" style="border-color: ${estadoColor}40 !important;">
                            <div class="d-flex align-items-center gap-3">
                                <div class="kpi-icon text-white" style="background: ${estadoColor};">
                                    <i class="ph ph-shield-check"></i>
                                </div>
                                <div>
                                    <small class="text-muted text-uppercase fw-semibold">Estado General</small>
                                    <h5 class="mb-0 fw-bold" style="color: ${estadoColor}">${r.estadoGeneral}</h5>
                                    <small class="text-muted">Riesgo: ${r.riesgoActual}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 col-6">
                        <div class="kpi-card bg-danger bg-opacity-10 border border-danger border-opacity-25">
                            <div class="d-flex align-items-center gap-3">
                                <div class="kpi-icon bg-danger text-white">
                                    <i class="ph ph-warning"></i>
                                </div>
                                <div>
                                    <small class="text-muted text-uppercase fw-semibold">Riesgo Actual</small>
                                    <h5 class="mb-0 fw-bold">${r.riesgoActual || 'Bajo'}</h5>
                                    <small class="text-muted">Score: ${r.scoreRiesgo || '-'}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-lg-4">
                <div class="card border h-100">
                    <div class="card-body text-center d-flex flex-column">
                        <h6 class="fw-semibold mb-3">Scores del Lote</h6>
                        <div class="row g-2 flex-grow-1 align-items-center justify-content-center">
                            <div class="col-4">
                                ${renderScoreRing(r.scoreProductividad, 'Productividad')}
                            </div>
                            <div class="col-4">
                                ${renderScoreRing(r.scoreSaludCultivo, 'Salud')}
                            </div>
                            <div class="col-4">
                                ${renderScoreRing(r.scoreHumedad, 'Humedad')}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `;

    $('#resumenEjecutivoContent').html(html);
}

function renderScoreRing(score, label) {
    score = score || 0;
    var pct = Math.min(score, 100);
    var cls = pct >= 80 ? 'score-excellent' : pct >= 60 ? 'score-good' : pct >= 40 ? 'score-regular' : 'score-critical';
    return `
        <div class="text-center">
            <div class="score-ring ${cls} mx-auto" style="--pct: ${pct}%;">
                <div class="score-ring-inner">
                    <span class="fw-bold" style="font-size: 16px;">${pct}</span>
                </div>
            </div>
            <small class="text-muted mt-1 d-block">${label}</small>
        </div>
    `;
}

// ============================================================
// 2. TIMELINE AGRONÓMICO (Phosphor icons from backend DTO)
// ============================================================

function renderizarTimeline(timeline) {
    if (!timeline || timeline.length === 0) {
        $('#timelineContent').html('<p class="text-muted text-center py-4">No hay actividades registradas.</p>');
        return;
    }

    var html = '<div class="timeline">';
    $.each(timeline, function (i, evt) {
        var iconClass = evt.icono || 'ph-activity';
        // Ensure icon has ph prefix
        if (iconClass.indexOf('ph-') !== 0) {
            iconClass = 'ph-' + iconClass;
        }
        html += `
            <div class="timeline-item">
                <div class="timeline-icon" style="background: ${evt.color || '#6c757d'};">
                    <i class="ph ${iconClass}"></i>
                </div>
                <div class="timeline-content">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <strong>${evt.tipoActividad}</strong>
                            <p class="mb-0 text-muted small">${evt.descripcion}</p>
                        </div>
                        <small class="text-muted text-nowrap ms-2">${formatDate(evt.fecha)}</small>
                    </div>
                    ${evt.responsable ? `<small class="text-muted"><i class="ph ph-user-circle me-1"></i>${evt.responsable}</small>` : ''}
                    ${evt.lote ? `<small class="text-muted ms-2"><i class="ph ph-map-pin me-1"></i>${evt.lote}</small>` : ''}
                    ${evt.cicloCultivo ? `<small class="text-muted ms-2"><i class="ph ph-arrows-clockwise me-1"></i>${evt.cicloCultivo}</small>` : ''}
                </div>
            </div>
        `;
    });
    html += '</div>';

    $('#timelineContent').html(html);
}

// ============================================================
// 3. EVOLUCIÓN DEL CULTIVO (NDVI curve)
// ============================================================

function renderizarEvolucion(evo) {
    if (!evo) {
        $('#evolucionContent').html('<p class="text-muted text-center py-4">Sin datos de evolución.</p>');
        return;
    }

    var html = '';

    // Comparativa between campaigns
    if (evo.comparativa) {
        var cmp = evo.comparativa;
        html += `
            <div class="row g-3 mb-4">
                <div class="col-md-6">
                    <div class="card bg-success bg-opacity-10 border-success border-opacity-25 h-100">
                        <div class="card-body text-center">
                            <h6 class="fw-semibold">Campaña Actual</h6>
                            <h3 class="fw-bold text-success">${formatNum(cmp.rendimientoActual, 2) || '-'} <small>tn/ha</small></h3>
                            <small class="text-muted">NDVI Prom: ${formatNum(cmp.ndviPromedioActual, 2) || '-'}</small>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card bg-info bg-opacity-10 border-info border-opacity-25 h-100">
                        <div class="card-body text-center">
                            <h6 class="fw-semibold">${cmp.campaniaAnterior || 'Anterior'}</h6>
                            <h3 class="fw-bold text-info">${formatNum(cmp.rendimientoAnterior, 2) || '-'} <small>tn/ha</small></h3>
                            <small class="text-muted">NDVI Prom: ${formatNum(cmp.ndviPromedioAnterior, 2) || '-'}</small>
                        </div>
                    </div>
                </div>
            </div>
        `;
    }

    html += '<div class="row"><div class="col-12"><div class="chart-container"><canvas id="chartEvolucion"></canvas></div></div></div>';
    $('#evolucionContent').html(html);

    // Render NDVI chart
    renderEvolucionChart(evo.evolucion);
}

function renderEvolucionChart(datos) {
    if (!datos || datos.length === 0) return;

    var ctx = document.getElementById('chartEvolucion');
    if (!ctx) return;
    if (charts.evolucion) charts.evolucion.destroy();

    var labels = datos.map(function (d) { return formatDate(d.fecha); });
    var ndvi = datos.map(function (d) { return d.ndvi != null ? d.ndvi : null; });
    var humedad = datos.map(function (d) { return d.humedad != null ? d.humedad : null; });

    charts.evolucion = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'NDVI',
                    data: ndvi,
                    borderColor: 'rgba(40, 167, 69, 1)',
                    backgroundColor: 'rgba(40, 167, 69, 0.1)',
                    fill: true,
                    tension: 0.4,
                    pointRadius: 4,
                    pointBackgroundColor: 'rgba(40, 167, 69, 1)',
                    yAxisID: 'y'
                },
                {
                    label: 'Humedad (%)',
                    data: humedad,
                    borderColor: 'rgba(23, 162, 184, 1)',
                    backgroundColor: 'rgba(23, 162, 184, 0.1)',
                    fill: false,
                    tension: 0.4,
                    pointRadius: 3,
                    borderDash: [5, 5],
                    pointBackgroundColor: 'rgba(23, 162, 184, 1)',
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: { position: 'top' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            if (ctx.dataset.label === 'NDVI') {
                                return 'NDVI: ' + (ctx.parsed.y != null ? ctx.parsed.y.toFixed(3) : '-');
                            }
                            return ctx.dataset.label + ': ' + (ctx.parsed.y != null ? ctx.parsed.y.toFixed(1) + '%' : '-');
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    max: 1,
                    title: { display: true, text: 'NDVI' },
                    ticks: {
                        callback: function (value) { return value.toFixed(2); }
                    }
                },
                y1: {
                    beginAtZero: true,
                    max: 100,
                    position: 'right',
                    grid: { display: false },
                    title: { display: true, text: 'Humedad (%)' }
                }
            }
        }
    });
}

// ============================================================
// 4. ANÁLISIS CLIMÁTICO
// ============================================================

function renderizarClima(clima) {
    if (!clima) {
        $('#climaContent').html('<p class="text-muted text-center py-4">Sin datos climáticos.</p>');
        return;
    }

    var balanceColor = clima.balanceHidrico === 'Normal' ? '#28a745'
        : clima.balanceHidrico === 'Moderado' || clima.balanceHidrico === 'Déficit hídrico' ? '#ffc107'
        : '#dc3545';

    var html = `
        <div class="row g-3 mb-4">
            <div class="col-md-3 col-6">
                <div class="card bg-primary bg-opacity-10 border-primary border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-cloud-rain display-6 text-primary"></i>
                        <h4 class="fw-bold mt-2">${formatNum(clima.lluviaAcumulada, 1) || '-'}</h4>
                        <small class="text-muted">Lluvia Acumulada (mm)</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-warning bg-opacity-10 border-warning border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-sun-dim display-6 text-warning"></i>
                        <h4 class="fw-bold mt-2">${clima.diasSinLluvia != null ? clima.diasSinLluvia + ' días' : '-'}</h4>
                        <small class="text-muted">Sin Lluvia</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-info bg-opacity-10 border-info border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-thermometer display-6 text-info"></i>
                        <h4 class="fw-bold mt-2">${formatNum(clima.tempMinima, 1) || '-'}° / ${formatNum(clima.tempMaxima, 1) || '-'}°</h4>
                        <small class="text-muted">Temp. Mín / Máx</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card text-center h-100" style="border-color: ${balanceColor}40; background: ${balanceColor}10;">
                    <div class="card-body">
                        <i class="ph ph-drop display-6" style="color: ${balanceColor}"></i>
                        <h4 class="fw-bold mt-2" style="color: ${balanceColor}">${clima.balanceHidrico}</h4>
                        <small class="text-muted">Balance Hídrico</small>
                    </div>
                </div>
            </div>
        </div>
        ${clima.cantidadHeladas > 0 ? `
        <div class="alert alert-danger d-flex align-items-center gap-2">
            <i class="ph ph-snowflake fs-5"></i>
            <span>Se registraron <strong>${clima.cantidadHeladas}</strong> eventos de granizo/helada.</span>
        </div>` : ''}
        ${clima.estresHidrico && clima.estresHidrico !== 'Sin estrés' ? `
        <div class="alert alert-warning d-flex align-items-center gap-2">
            <i class="ph ph-warning fs-5"></i>
            <span>Estrés hídrico: <strong>${clima.estresHidrico}</strong></span>
        </div>` : ''}
        <div class="mt-3">
            <h6 class="fw-semibold mb-2">Registros Climáticos</h6>
            <div class="table-responsive">
                <table class="table table-sm table-hover">
                    <thead>
                        <tr>
                            <th>Fecha</th>
                            <th>Precipitación (mm)</th>
                            <th>Tipo</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${(clima.registros || []).slice(0, 30).map(function (r) {
                            return `<tr>
                                <td>${formatDate(r.fecha)}</td>
                                <td>${formatNum(r.precipitacion, 1) || '-'}</td>
                                <td>${r.tipoClima || '-'}</td>
                            </tr>`;
                        }).join('')}
                    </tbody>
                </table>
            </div>
        </div>
    `;

    $('#climaContent').html(html);
}

// ============================================================
// 5. ANÁLISIS DE SUELO (with advertencia instead of recomendacion)
// ============================================================

function renderizarSuelo(suelo) {
    if (!suelo || !suelo.ph) {
        $('#sueloContent').html('<p class="text-muted text-center py-4">No hay análisis de suelo disponibles.</p>');
        return;
    }

    var params = [
        { label: 'pH', data: suelo.ph, maxVal: 14 },
        { label: 'Materia Orgánica (%)', data: suelo.materiaOrganica, maxVal: 10 },
        { label: 'Nitrógeno (ppm)', data: suelo.nitrogeno, maxVal: 50 },
        { label: 'Fósforo (ppm)', data: suelo.fosforo, maxVal: 40 },
        { label: 'Potasio (meq/100g)', data: suelo.potasio, maxVal: 1.5 },
        { label: 'Conductividad Eléctrica (dS/m)', data: suelo.conductividadElectrica, maxVal: 3 },
        { label: 'CIC (meq/100g)', data: suelo.cic, maxVal: 40 }
    ];

    var html = '';
    if (suelo.fechaAnalisis) {
        html += `<div class="mb-3"><strong>Fecha del análisis:</strong> ${formatDate(suelo.fechaAnalisis)}</div>`;
    }
    if (suelo.profundidadCm) {
        html += `<div class="mb-3"><strong>Profundidad:</strong> ${suelo.profundidadCm} cm</div>`;
    }
    if (suelo.textura) {
        html += `<div class="mb-3"><strong>Textura:</strong> ${suelo.textura}</div>`;
    }

    html += '<div class="table-responsive"><table class="table table-hover"><thead><tr><th>Parámetro</th><th>Valor</th><th>Nivel</th><th>Interpretación</th></tr></thead><tbody>';
    $.each(params, function (i, p) {
        if (!p.data) return;
        var nivelClass = p.data.nivel === 'Óptimo' || p.data.nivel === 'Optimo' ? 'success'
            : p.data.nivel === 'Bajo' ? 'danger'
            : p.data.nivel === 'Alto' ? 'warning'
            : 'secondary';
        var pct = p.data.valor != null ? Math.min((p.data.valor / p.maxVal) * 100, 100) : 0;
        var advertencia = p.data.advertencia || '';
        html += `
            <tr>
                <td><strong>${p.label}</strong></td>
                <td>${formatNum(p.data.valor, 2) || '-'} ${p.data.unidad || ''}</td>
                <td><span class="badge bg-${nivelClass}">${p.data.nivel || 'N/A'}</span></td>
                <td>
                    <div class="d-flex align-items-center gap-2">
                        <div class="soil-bar flex-grow-1">
                            <div class="soil-bar-fill soil-${nivelClass === 'success' ? 'optimum' : nivelClass === 'danger' ? 'low' : nivelClass === 'warning' ? 'high' : 'medium'}" style="width: ${pct}%;"></div>
                        </div>
                        <small class="text-muted" style="min-width: 30px;">${pct.toFixed(0)}%</small>
                    </div>
                    <small class="text-muted d-block mt-1">${p.data.interpretacion || ''}</small>
                    ${advertencia ? `<small class="d-block mt-1 text-warning"><i class="ph ph-warning me-1"></i>${advertencia}</small>` : ''}
                </td>
            </tr>
        `;
    });
    html += '</tbody></table></div>';

    // Advertencias (warnings) instead of recomendaciones
    if (suelo.recomendaciones && suelo.recomendaciones.length > 0) {
        html += `
            <div class="mt-3">
                <h6 class="fw-semibold mb-2"><i class="ph ph-warning-circle text-warning me-1"></i> Advertencias</h6>
                <ul class="list-group">
                    ${suelo.recomendaciones.map(function (r) {
                        return `<li class="list-group-item border-0 ps-0"><i class="ph ph-arrow-right text-warning me-1"></i>${r}</li>`;
                    }).join('')}
                </ul>
            </div>
        `;
    }

    $('#sueloContent').html(html);
}

// ============================================================
// 6. COSTOS Y RENTABILIDAD
// ============================================================

function renderizarCostos(costos) {
    if (!costos) {
        $('#costosContent').html('<p class="text-muted text-center py-4">Sin datos de costos.</p>');
        return;
    }

    var d = costos.desglose || {};
    var items = [
        { label: 'Siembra', ars: d.siembraARS || 0, usd: d.siembraUSD || 0, color: '#28a745' },
        { label: 'Fertilización', ars: d.fertilizacionARS || 0, usd: d.fertilizacionUSD || 0, color: '#17a2b8' },
        { label: 'Pulverización', ars: d.pulverizacionARS || 0, usd: d.pulverizacionUSD || 0, color: '#6f42c1' },
        { label: 'Riego', ars: d.riegoARS || 0, usd: d.riegoUSD || 0, color: '#007bff' },
        { label: 'Cosecha', ars: d.cosechaARS || 0, usd: d.cosechaUSD || 0, color: '#fd7e14' },
        { label: 'Monitoreo', ars: d.monitoreoARS || 0, usd: d.monitoreoUSD || 0, color: '#ffc107' },
        { label: 'Análisis de Suelo', ars: d.analisisSueloARS || 0, usd: d.analisisSueloUSD || 0, color: '#20c997' },
        { label: 'Otras Labores', ars: d.otrasLaboresARS || 0, usd: d.otrasLaboresUSD || 0, color: '#6c757d' }
    ];

    var html = `
        <div class="row g-3 mb-4">
            <div class="col-md-3 col-6">
                <div class="card bg-success bg-opacity-10 border-success border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <small class="text-muted">Costo Total</small>
                        <h4 class="fw-bold text-success">${formatMoney(costos.costoTotalARS)}</h4>
                        <small class="text-muted">${formatMoneyUSD(costos.costoTotalUSD)} USD</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-info bg-opacity-10 border-info border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <small class="text-muted">Costo por Hectárea</small>
                        <h4 class="fw-bold text-info">${formatMoney(costos.costoPorHaARS)}</h4>
                        <small class="text-muted">${formatMoneyUSD(costos.costoPorHaUSD)} USD/ha</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-primary bg-opacity-10 border-primary border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <small class="text-muted">Margen Estimado</small>
                        <h4 class="fw-bold text-primary">${costos.margenEstimadoARS != null ? formatMoney(costos.margenEstimadoARS) : '-'}</h4>
                        <small class="text-muted">${costos.margenEstimadoUSD != null ? formatMoneyUSD(costos.margenEstimadoUSD) : '-'} USD</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-warning bg-opacity-10 border-warning border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <small class="text-muted">Rentabilidad Proyectada</small>
                        <h4 class="fw-bold text-warning">${costos.rentabilidadProyectada != null ? costos.rentabilidadProyectada + '%' : '-'}</h4>
                        <small class="text-muted">Retorno estimado</small>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-md-6">
                <h6 class="fw-semibold mb-3">Desglose de Costos (ARS)</h6>
                <div class="card border">
                    <div class="card-body py-2">
                        ${items.map(function (item) {
                            return `<div class="cost-item">
                                <span><span class="badge me-2" style="background: ${item.color};">&nbsp;</span> ${item.label}</span>
                                <span class="fw-semibold">${formatMoney(item.ars)}</span>
                            </div>`;
                        }).join('')}
                    </div>
                </div>
            </div>
            <div class="col-md-6">
                <h6 class="fw-semibold mb-3">Distribución</h6>
                <div class="chart-container"><canvas id="chartCostos"></canvas></div>
            </div>
        </div>
    `;

    $('#costosContent').html(html);
    renderCostosChart(items);
}

function renderCostosChart(items) {
    var ctx = document.getElementById('chartCostos');
    if (!ctx) return;
    if (charts.costos) charts.costos.destroy();

    var hasValues = items.some(function (i) { return i.ars > 0; });
    if (!hasValues) return;

    charts.costos = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: items.map(function (i) { return i.label; }),
            datasets: [{
                data: items.map(function (i) { return i.ars; }),
                backgroundColor: items.map(function (i) { return i.color; }),
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { boxWidth: 12, padding: 12 }
                },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            var total = ctx.dataset.data.reduce(function (a, b) { return a + b; }, 0);
                            var pct = total > 0 ? ((ctx.parsed / total) * 100).toFixed(1) : 0;
                            return ctx.label + ': ' + formatMoney(ctx.parsed) + ' (' + pct + '%)';
                        }
                    }
                }
            }
        }
    });
}

// ============================================================
// 7. RENDIMIENTO Y COSECHA
// ============================================================

function renderizarRendimiento(rend) {
    if (!rend) {
        $('#rendimientoContent').html('<p class="text-muted text-center py-4">Sin datos de rendimiento.</p>');
        return;
    }

    var html = `
        <div class="row g-3 mb-4">
            <div class="col-md-3 col-6">
                <div class="card bg-success bg-opacity-10 border-success border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-trend-up display-6 text-success"></i>
                        <h4 class="fw-bold mt-2">${formatNum(rend.rendimientoTonHa, 2) || '-'}</h4>
                        <small class="text-muted">Rendimiento (tn/ha)</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-primary bg-opacity-10 border-primary border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-cube display-6 text-primary"></i>
                        <h4 class="fw-bold mt-2">${formatNum(rend.produccionTotalTon, 2) || '-'}</h4>
                        <small class="text-muted">Producción Total (tn)</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-info bg-opacity-10 border-info border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-drops display-6 text-info"></i>
                        <h4 class="fw-bold mt-2">${formatNum(rend.humedadCosecha, 1) || '-'}%</h4>
                        <small class="text-muted">Humedad de Cosecha</small>
                    </div>
                </div>
            </div>
            <div class="col-md-3 col-6">
                <div class="card bg-warning bg-opacity-10 border-warning border-opacity-25 text-center h-100">
                    <div class="card-body">
                        <i class="ph ph-calendar-check display-6 text-warning"></i>
                        <h4 class="fw-bold mt-2">${formatDate(rend.fechaCosecha) || '-'}</h4>
                        <small class="text-muted">Fecha de Cosecha</small>
                    </div>
                </div>
            </div>
        </div>
    `;

    if (rend.historico && rend.historico.length > 0) {
        html += `
            <h6 class="fw-semibold mb-3">Comparativa Histórica de Rendimiento</h6>
            <div class="row">
                <div class="col-md-8">
                    <div class="chart-container"><canvas id="chartRendimiento"></canvas></div>
                </div>
                <div class="col-md-4">
                    <div class="table-responsive">
                        <table class="table table-sm table-hover">
                            <thead>
                                <tr><th>Campaña</th><th>Cultivo</th><th>tn/ha</th></tr>
                            </thead>
                            <tbody>
                                ${rend.historico.map(function (h) {
                                    return `<tr>
                                        <td>${h.campania}</td>
                                        <td>${h.cultivo || '-'}</td>
                                        <td class="fw-semibold">${formatNum(h.rendimientoTonHa, 2) || '-'}</td>
                                    </tr>`;
                                }).join('')}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        `;
    }

    $('#rendimientoContent').html(html);
    renderRendimientoChart(rend.historico);
}

function renderRendimientoChart(historico) {
    if (!historico || historico.length === 0) return;
    var ctx = document.getElementById('chartRendimiento');
    if (!ctx) return;
    if (charts.rendimiento) charts.rendimiento.destroy();

    charts.rendimiento = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: historico.map(function (h) { return h.campania; }),
            datasets: [{
                label: 'Rendimiento (tn/ha)',
                data: historico.map(function (h) { return h.rendimientoTonHa || 0; }),
                backgroundColor: 'rgba(40, 167, 69, 0.6)',
                borderColor: 'rgba(40, 167, 69, 1)',
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
                            return ctx.parsed.y.toFixed(2) + ' tn/ha';
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

// ============================================================
// 8. ALERTAS INTELIGENTES
// ============================================================

function renderizarAlertas(alertas) {
    if (!alertas || alertas.length === 0) {
        $('#alertasContent').html(`
            <div class="text-center py-5">
                <i class="ph ph-check-circle display-3 text-success mb-3"></i>
                <h5 class="text-muted">No hay alertas activas</h5>
                <p class="text-muted">El campo se encuentra en condiciones normales.</p>
            </div>
        `);
        return;
    }

    var html = '<div class="row"><div class="col-12">';
    $.each(alertas, function (i, a) {
        var sevClass = a.severidad === 'Alta' ? 'alta' : a.severidad === 'Media' ? 'media' : 'baja';
        var sevIcon = a.icono || (a.severidad === 'Alta' ? 'ph-warning-circle' : a.severidad === 'Media' ? 'ph-warning' : 'ph-info');
        html += `
            <div class="alert-card alert-severity-${sevClass}">
                <div class="d-flex gap-3">
                    <div class="flex-shrink-0">
                        <i class="ph ${sevIcon} fs-3 ${a.severidad === 'Alta' ? 'text-danger' : a.severidad === 'Media' ? 'text-warning' : 'text-info'}"></i>
                    </div>
                    <div class="flex-grow-1">
                        <div class="d-flex justify-content-between">
                            <h6 class="fw-semibold mb-1">${a.tipo}</h6>
                            <span class="badge bg-${a.severidad === 'Alta' ? 'danger' : a.severidad === 'Media' ? 'warning' : 'info'}">${a.severidad}</span>
                        </div>
                        <p class="mb-1">${a.mensaje}</p>
                        ${a.recomendacion ? `<small class="text-muted"><i class="ph ph-lightbulb me-1"></i> ${a.recomendacion}</small>` : ''}
                    </div>
                </div>
            </div>
        `;
    });
    html += '</div></div>';

    $('#alertasContent').html(html);
}

// ============================================================
// 9. HISTORIAL MULTI-CAMPAÑA
// ============================================================

function renderizarHistorial(historial) {
    if (!historial || historial.length === 0) {
        $('#historialContent').html('<p class="text-muted text-center py-4">Sin historial multi-campaña disponible.</p>');
        return;
    }

    var html = '';
    $.each(historial, function (i, h) {
        var expanded = i === 0 ? 'show' : '';
        html += `
            <div class="history-campaign">
                <div class="history-campaign-header" onclick="toggleHistorial(this)">
                    <div class="d-flex align-items-center gap-3">
                        <i class="ph ph-caret-right fs-5 transition-rotate"></i>
                        <div>
                            <h6 class="fw-semibold mb-0">${h.campania}</h6>
                            <small class="text-muted">${h.cultivo || 'Sin cultivo'} · ${h.cantidadLabores} labores</small>
                        </div>
                    </div>
                    <div class="text-end">
                        <small class="text-muted d-block">Rend: ${formatNum(h.rendimientoTonHa, 2) || '-'} tn/ha</small>
                        <small class="text-muted">Costo: ${formatMoney(h.costoTotalARS)}</small>
                    </div>
                </div>
                <div class="history-campaign-body" style="display: ${expanded ? 'block' : 'none'};">
                    <div class="row g-2">
                        <div class="col-md-6">
                            <strong>Labores:</strong> ${h.cantidadLabores}
                        </div>
                        <div class="col-md-3">
                            <strong>Rendimiento:</strong> ${formatNum(h.rendimientoTonHa, 2) || '-'} tn/ha
                        </div>
                        <div class="col-md-3">
                            <strong>Costo Total:</strong> ${formatMoney(h.costoTotalARS)}
                        </div>
                    </div>
                </div>
            </div>
        `;
    });

    $('#historialContent').html(html);
}

function toggleHistorial(el) {
    var body = $(el).next('.history-campaign-body');
    var icon = $(el).find('.ph-caret-right');
    body.slideToggle(200);
    icon.toggleClass('rotate-90');
}

// ============================================================
// 10. COMPARATIVA ENTRE CAMPOS
// ============================================================

function cargarCamposComparativa(campoActualNombre) {
    $.ajax({
        url: '/Campo/GetAll',
        type: 'GET',
        success: function (result) {
            if (result.success) {
                var select = $('#selectCampoComparativa');
                select.empty().append('<option value="">Seleccione un campo...</option>');
                $.each(result.listObject, function (index, campo) {
                    // Skip the current campo (same as principal)
                    if (campoActualNombre && campo.nombre === campoActualNombre) return;
                    select.append($('<option>', {
                        value: campo.id,
                        text: campo.nombre
                    }));
                });
                select.prop('disabled', false);
            }
        }
    });
}

function renderizarComparativa() {
    if (!reporteData) {
        $('#comparativaContent').html('<p class="text-muted text-center py-4">Primero genere el reporte principal.</p>');
        return;
    }

    var campoPrincipal = reporteData.resumenEjecutivo ? reporteData.resumenEjecutivo.campo : 'Campo actual';

    var html = `
        <div class="mb-4">
            <div class="row g-3 align-items-end">
                <div class="col-md-6">
                    <label class="form-label fw-semibold">
                        <i class="ph ph-map-pin me-1"></i> Campo Actual
                    </label>
                    <input type="text" class="form-control" value="${campoPrincipal}" readonly disabled>
                </div>
                <div class="col-md-6">
                    <label for="selectCampoComparativa" class="form-label fw-semibold">
                        <i class="ph ph-arrows-left-right me-1"></i> Campo a Comparar
                    </label>
                    <select class="form-select" id="selectCampoComparativa" disabled>
                        <option value="">Seleccione un campo...</option>
                    </select>
                </div>
            </div>
        </div>
        <div id="comparativaResult">
            <p class="text-muted text-center py-4">
                <i class="ph ph-arrows-left-right display-6 d-block mb-2"></i>
                Seleccione un segundo campo para comparar sus KPIs.
            </p>
        </div>
    `;

    $('#comparativaContent').html(html);

    // Load campos
    cargarCamposComparativa(campoPrincipal);

    // Event: change comparativa campo
    $('#selectCampoComparativa').on('change', function () {
        var idCampoSecundario = parseInt($(this).val());
        if (idCampoSecundario) {
            cargarComparativa(idCampoSecundario);
        } else {
            $('#comparativaResult').html(`
                <p class="text-muted text-center py-4">
                    <i class="ph ph-arrows-left-right display-6 d-block mb-2"></i>
                    Seleccione un segundo campo para comparar sus KPIs.
                </p>
            `);
        }
    });
}

function cargarComparativa(idCampoSecundario) {
    var idCampoPrincipal = currentCampoId;
    var idCampania = parseInt($('#selectCampania').val()) || null;

    $('#comparativaResult').html(`
        <div class="text-center py-4">
            <div class="spinner-border text-primary mb-3" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="text-muted">Cargando datos comparativos...</p>
        </div>
    `);

    $.ajax({
        url: '/Reporte/GetComparativa',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            idCampoPrincipal: idCampoPrincipal,
            idCampoSecundario: idCampoSecundario,
            idCampania: idCampania
        }),
        success: function (result) {
            if (result.success && result.object) {
                renderizarComparativaResult(result.object);
            } else {
                $('#comparativaResult').html(`
                    <div class="alert alert-warning">
                        <i class="ph ph-warning me-1"></i> ${result.message || 'Error al cargar la comparativa'}
                    </div>
                `);
            }
        },
        error: function () {
            $('#comparativaResult').html(`
                <div class="alert alert-danger">
                    <i class="ph ph-warning-circle me-1"></i> Error al conectar con el servidor
                </div>
            `);
        }
    });
}

function renderizarComparativaResult(comp) {
    if (!comp || !comp.campoPrincipal) {
        $('#comparativaResult').html('<p class="text-muted text-center py-4">Sin datos de comparativa.</p>');
        return;
    }

    var p = comp.campoPrincipal;
    var s = comp.campoSecundario;
    var hasSecundario = s && s.nombre;

    var html = '';

    // KPI Comparison Cards
    var kpis = [
        { label: 'Superficie (ha)', icon: 'ph-ruler', pVal: formatNum(p.superficieHa, 1), sVal: hasSecundario ? formatNum(s.superficieHa, 1) : '-',
          pWinner: hasSecundario && p.superficieHa > s.superficieHa, sWinner: hasSecundario && s.superficieHa > p.superficieHa },
        { label: 'Cultivo Principal', icon: 'ph-tree', pVal: p.cultivoPrincipal || 'N/A', sVal: hasSecundario ? (s.cultivoPrincipal || 'N/A') : '-',
          pWinner: false, sWinner: false },
        { label: 'Costo Total (ARS)', icon: 'ph-coins', pVal: formatMoney(p.costoTotalARS), sVal: hasSecundario ? formatMoney(s.costoTotalARS) : '-',
          pWinner: hasSecundario && p.costoTotalARS < s.costoTotalARS, sWinner: hasSecundario && s.costoTotalARS < p.costoTotalARS },
        { label: 'Costo x Ha (ARS)', icon: 'ph-currency-dollar', pVal: formatMoney(p.costoPorHaARS), sVal: hasSecundario ? formatMoney(s.costoPorHaARS) : '-',
          pWinner: hasSecundario && p.costoPorHaARS < s.costoPorHaARS, sWinner: hasSecundario && s.costoPorHaARS < p.costoPorHaARS },
        { label: 'Rendimiento (tn/ha)', icon: 'ph-trend-up', pVal: formatNum(p.rendimientoTonHa, 2) || '-', sVal: hasSecundario ? (formatNum(s.rendimientoTonHa, 2) || '-') : '-',
          pWinner: hasSecundario && (p.rendimientoTonHa || 0) > (s.rendimientoTonHa || 0), sWinner: hasSecundario && (s.rendimientoTonHa || 0) > (p.rendimientoTonHa || 0) },
        { label: 'Rentabilidad (%)', icon: 'ph-percent', pVal: p.rentabilidadProyectada != null ? p.rentabilidadProyectada + '%' : '-', sVal: hasSecundario ? (s.rentabilidadProyectada != null ? s.rentabilidadProyectada + '%' : '-') : '-',
          pWinner: hasSecundario && (p.rentabilidadProyectada || 0) > (s.rentabilidadProyectada || 0), sWinner: hasSecundario && (s.rentabilidadProyectada || 0) > (p.rentabilidadProyectada || 0) },
        { label: 'Labores', icon: 'ph-list-bullets', pVal: p.cantidadLabores, sVal: hasSecundario ? s.cantidadLabores : '-',
          pWinner: false, sWinner: false },
        { label: 'Alertas', icon: 'ph-warning-circle', pVal: p.cantidadAlertas, sVal: hasSecundario ? s.cantidadAlertas : '-',
          pWinner: hasSecundario && p.cantidadAlertas < s.cantidadAlertas, sWinner: hasSecundario && s.cantidadAlertas < p.cantidadAlertas }
    ];

    // Estado general colors
    var pEstadoColor = p.estadoGeneral === 'Excelente' ? '#28a745' : p.estadoGeneral === 'Bueno' ? '#17a2b8' : p.estadoGeneral === 'Regular' ? '#ffc107' : '#dc3545';
    var sEstadoColor = s && s.estadoGeneral === 'Excelente' ? '#28a745' : s && s.estadoGeneral === 'Bueno' ? '#17a2b8' : s && s.estadoGeneral === 'Regular' ? '#ffc107' : '#dc3545';

    html += `
        <div class="d-flex align-items-center gap-2 mb-3">
            <span class="badge bg-primary">${p.nombre}</span>
            <i class="ph ph-arrows-left-right text-muted"></i>
            ${hasSecundario ? `<span class="badge bg-secondary">${s.nombre}</span>` : '<span class="text-muted">(sin comparación)</span>'}
        </div>

        <div class="table-responsive">
            <table class="table table-hover table-bordered">
                <thead class="table-light">
                    <tr>
                        <th style="width: 30%;">Métrica</th>
                        <th style="width: 35%;" class="text-center text-primary">${p.nombre}</th>
                        ${hasSecundario ? `<th style="width: 35%;" class="text-center text-secondary">${s.nombre}</th>` : ''}
                    </tr>
                </thead>
                <tbody>
                    ${kpis.map(function (k) {
                        var pClass = k.pWinner ? 'comparativa-ganador' : '';
                        var sClass = k.sWinner ? 'comparativa-ganador' : '';
                        return `<tr>
                            <td><i class="ph ${k.icon} me-2 text-muted"></i>${k.label}</td>
                            <td class="text-center fw-semibold ${pClass}">${k.pVal}</td>
                            ${hasSecundario ? `<td class="text-center fw-semibold ${sClass}">${k.sVal}</td>` : ''}
                        </tr>`;
                    }).join('')}
                    <tr>
                        <td><i class="ph ph-shield-check me-2 text-muted"></i>Estado General</td>
                        <td class="text-center fw-semibold"><span class="badge" style="background: ${pEstadoColor};">${p.estadoGeneral || 'N/A'}</span></td>
                        ${hasSecundario ? `<td class="text-center fw-semibold"><span class="badge" style="background: ${sEstadoColor};">${s.estadoGeneral || 'N/A'}</span></td>` : ''}
                    </tr>
                </tbody>
            </table>
        </div>
    `;

    // Cost breakdown comparison
    if (hasSecundario) {
        var costItems = [
            { label: 'Siembra', pVal: p.desgloseCostos ? p.desgloseCostos.siembraARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.siembraARS : 0 },
            { label: 'Fertilización', pVal: p.desgloseCostos ? p.desgloseCostos.fertilizacionARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.fertilizacionARS : 0 },
            { label: 'Pulverización', pVal: p.desgloseCostos ? p.desgloseCostos.pulverizacionARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.pulverizacionARS : 0 },
            { label: 'Riego', pVal: p.desgloseCostos ? p.desgloseCostos.riegoARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.riegoARS : 0 },
            { label: 'Cosecha', pVal: p.desgloseCostos ? p.desgloseCostos.cosechaARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.cosechaARS : 0 },
            { label: 'Monitoreo', pVal: p.desgloseCostos ? p.desgloseCostos.monitoreoARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.monitoreoARS : 0 },
            { label: 'Análisis Suelo', pVal: p.desgloseCostos ? p.desgloseCostos.analisisSueloARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.analisisSueloARS : 0 },
            { label: 'Otras Labores', pVal: p.desgloseCostos ? p.desgloseCostos.otrasLaboresARS : 0, sVal: s.desgloseCostos ? s.desgloseCostos.otrasLaboresARS : 0 }
        ];

        html += `
            <h6 class="fw-semibold mt-4 mb-3"><i class="ph ph-coins me-1"></i> Comparativa de Costos (ARS)</h6>
            <div class="table-responsive">
                <table class="table table-sm table-hover">
                    <thead class="table-light">
                        <tr>
                            <th>Rubro</th>
                            <th class="text-end text-primary">${p.nombre}</th>
                            <th class="text-end text-secondary">${s.nombre}</th>
                            <th class="text-end">Diferencia</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${costItems.map(function (item) {
                            var diff = item.pVal - item.sVal;
                            var diffCls = diff > 0 ? 'text-danger' : diff < 0 ? 'text-success' : '';
                            var diffStr = diff === 0 ? '-' : (diff > 0 ? '+' : '') + formatMoney(diff);
                            var pWinner = item.pVal < item.sVal;
                            var sWinner = item.sVal < item.pVal;
                            return `<tr>
                                <td>${item.label}</td>
                                <td class="text-end ${pWinner ? 'comparativa-ganador' : ''}">${formatMoney(item.pVal)}</td>
                                <td class="text-end ${sWinner ? 'comparativa-ganador' : ''}">${formatMoney(item.sVal)}</td>
                                <td class="text-end fw-semibold ${diffCls}">${diffStr}</td>
                            </tr>`;
                        }).join('')}
                    </tbody>
                </table>
            </div>
        `;

        // Rendimiento histórico comparison
        if ((p.rendimientoHistorico && p.rendimientoHistorico.length > 0) ||
            (s.rendimientoHistorico && s.rendimientoHistorico.length > 0)) {
            html += `
                <h6 class="fw-semibold mt-4 mb-3"><i class="ph ph-trend-up me-1"></i> Comparativa de Rendimiento Histórico</h6>
                <div class="row">
                    <div class="col-12">
                        <div class="chart-container"><canvas id="chartComparativaRendimiento" height="250"></canvas></div>
                    </div>
                </div>
            `;
        }
    }

    $('#comparativaResult').html(html);

    // Render rendimiento histórico chart if both have data
    if (hasSecundario && s && s.rendimientoHistorico && s.rendimientoHistorico.length > 0) {
        renderComparativaRendimientoChart(p, s);
    }
}

function renderComparativaRendimientoChart(p, s) {
    var ctx = document.getElementById('chartComparativaRendimiento');
    if (!ctx) return;

    // Merge all campaign labels from both campos
    var allCampanias = [];
    var pData = {};
    var sData = {};

    if (p.rendimientoHistorico) {
        $.each(p.rendimientoHistorico, function (i, r) {
            if (allCampanias.indexOf(r.campania) === -1) allCampanias.push(r.campania);
            pData[r.campania] = r.rendimientoTonHa || 0;
        });
    }
    if (s.rendimientoHistorico) {
        $.each(s.rendimientoHistorico, function (i, r) {
            if (allCampanias.indexOf(r.campania) === -1) allCampanias.push(r.campania);
            sData[r.campania] = r.rendimientoTonHa || 0;
        });
    }

    allCampanias.sort();

    if (charts.comparativaRendimiento) charts.comparativaRendimiento.destroy();

    charts.comparativaRendimiento = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: allCampanias,
            datasets: [
                {
                    label: p.nombre,
                    data: allCampanias.map(function (c) { return pData[c] || 0; }),
                    backgroundColor: 'rgba(13, 110, 253, 0.6)',
                    borderColor: 'rgba(13, 110, 253, 1)',
                    borderWidth: 1,
                    borderRadius: 4
                },
                {
                    label: s.nombre,
                    data: allCampanias.map(function (c) { return sData[c] || 0; }),
                    backgroundColor: 'rgba(108, 117, 125, 0.6)',
                    borderColor: 'rgba(108, 117, 125, 1)',
                    borderWidth: 1,
                    borderRadius: 4
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { position: 'top' },
                tooltip: {
                    callbacks: {
                        label: function (ctx) {
                            return ctx.dataset.label + ': ' + ctx.parsed.y.toFixed(2) + ' tn/ha';
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

// ============================================================
// HELP MODAL
// ============================================================

function mostrarAyudaReporte() {
    var modal = new bootstrap.Modal(document.getElementById('ayudaModal'));
    modal.show();
}

// ============================================================
// EXPORT PDF (all tabs using html2canvas + jsPDF)
// ============================================================

function exportarPDF() {
    if (!reporteData) {
        mostrarMensaje('Primero genere el reporte', 'warning');
        return;
    }

    // Show a loading message
    var btn = document.querySelector('button[onclick="exportarPDF()"]');
    var originalHtml = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Generando PDF...';
    btn.disabled = true;

    // Ensure all tabs are rendered (briefly show each to trigger any chart renders)
    var tabs = document.querySelectorAll('#reporteTabs button');
    var currentActive = document.querySelector('#reporteTabs button.active');

    // We need html2canvas and jsPDF - check they're loaded
    if (typeof html2canvas === 'undefined' || typeof jspdf === 'undefined') {
        mostrarMensaje('Error al cargar las librerías de PDF. Intente nuevamente.', 'error');
        btn.innerHTML = originalHtml;
        btn.disabled = false;
        return;
    }

    // Helper to get a Promise that resolves with a canvas of an element
    function captureElement(element) {
        return html2canvas(element, {
            scale: 2,
            useCORS: true,
            logging: false,
            backgroundColor: '#ffffff',
            windowHeight: element.scrollHeight,
            windowWidth: element.scrollWidth
        });
    }

    // Build PDF: capture the header + each section
    var { jsPDF } = jspdf;
    var pdf = new jsPDF('p', 'mm', 'a4');
    var pageWidth = pdf.internal.pageSize.getWidth();
    var pageHeight = pdf.internal.pageSize.getHeight();

    var yPos = 10;

    // Add title
    pdf.setFontSize(16);
    pdf.setTextColor(0, 102, 204);
    pdf.text('Informe Integral del Campo', pageWidth / 2, yPos, { align: 'center' });
    yPos += 8;

    pdf.setFontSize(10);
    pdf.setTextColor(100);
    pdf.text('Campo: ' + (reporteData.resumenEjecutivo ? reporteData.resumenEjecutivo.campo : ''), pageWidth / 2, yPos, { align: 'center' });
    yPos += 6;

    // 1. Capture Resumen Ejecutivo (always visible)
    var resumenEl = document.getElementById('resumenEjecutivoContent');

    if (!resumenEl) {
        pdf.save('Informe_Campo.pdf');
        btn.innerHTML = originalHtml;
        btn.disabled = false;
        return;
    }

    captureElement(resumenEl).then(function (rCanvas) {
        var rImgData = rCanvas.toDataURL('image/png');
        var rImgWidth = pageWidth - 20;
        var rImgHeight = (rCanvas.height * rImgWidth) / rCanvas.width;

        if (rImgHeight + yPos > pageHeight - 10) {
            pdf.addPage();
            yPos = 10;
        }

        pdf.addImage(rImgData, 'PNG', 10, yPos, rImgWidth, rImgHeight);
        yPos += rImgHeight + 10;

        // 2. Temporarily show all hidden tab-panes for capture, then restore
        var tabContents = document.querySelectorAll('.tab-pane');
        var hiddenStates = [];

        tabContents.forEach(function (tab) {
            var isHidden = tab.classList.contains('fade') && !tab.classList.contains('show') && tab.style.display !== 'block';
            hiddenStates.push({
                element: tab,
                originalDisplay: tab.style.display,
                originalPosition: tab.style.position,
                originalLeft: tab.style.left,
                originalVisibility: tab.style.visibility,
                wasHidden: isHidden
            });
            if (isHidden) {
                tab.style.display = 'block';
                tab.style.position = 'absolute';
                tab.style.left = '-9999px';
                tab.style.visibility = 'hidden';
            }
        });

        // Force layout recalc and wait for charts
        document.body.offsetHeight;
        var waitTime = tabContents.length > 0 ? 300 : 0;

        setTimeout(function () {
            // Now capture each tab content
            var tabPromises = [];
            var tabNames = [];

            tabContents.forEach(function (tab) {
                var tabId = tab.id;
                var tabName = tabId.replace('-content', '').replace('-', ' ');
                tabName = tabName.charAt(0).toUpperCase() + tabName.slice(1);
                tabNames.push(tabName);
                tabPromises.push(captureElement(tab));
            });

            // Restore hidden states immediately after starting captures
            hiddenStates.forEach(function (state) {
                state.element.style.display = state.originalDisplay || '';
                state.element.style.position = state.originalPosition || '';
                state.element.style.left = state.originalLeft || '';
                state.element.style.visibility = state.originalVisibility || '';
            });

            Promise.all(tabPromises).then(function (tabCanvases) {
                for (var i = 0; i < tabCanvases.length; i++) {
                    var tCanvas = tabCanvases[i];
                    var tImgData = tCanvas.toDataURL('image/png');
                    var tImgWidth = pageWidth - 20;
                    var tImgHeight = (tCanvas.height * tImgWidth) / tCanvas.width;

                    // Add page for each tab
                    pdf.addPage();
                    yPos = 10;

                    // Tab title
                    pdf.setFontSize(14);
                    pdf.setTextColor(0, 102, 204);
                    pdf.text(tabNames[i], pageWidth / 2, yPos, { align: 'center' });
                    yPos += 8;

                    // Limit image height per page, split if needed
                    var maxImgHeight = pageHeight - yPos - 10;
                    if (tImgHeight > maxImgHeight) {
                        var scale = maxImgHeight / tImgHeight;
                        tImgWidth = tImgWidth * scale;
                        tImgHeight = tImgHeight * scale;
                    }

                    pdf.addImage(tImgData, 'PNG', 10, yPos, tImgWidth, tImgHeight);
                }

                // Save the PDF
                pdf.save('Informe_Campo_' + (reporteData.resumenEjecutivo ? reporteData.resumenEjecutivo.campo.replace(/\s+/g, '_') : 'reporte') + '.pdf');
                btn.innerHTML = originalHtml;
                btn.disabled = false;
                mostrarMensaje('PDF generado correctamente', 'success');
            }).catch(function (err) {
                console.error('Error capturing tab contents:', err);
                mostrarMensaje('Error al generar el PDF: ' + err.message, 'error');
                btn.innerHTML = originalHtml;
                btn.disabled = false;
            });
        }, waitTime);
    }).catch(function (err) {
        console.error('Error generating PDF:', err);
        mostrarMensaje('Error al generar el PDF: ' + err.message, 'error');
        btn.innerHTML = originalHtml;
        btn.disabled = false;
    });
}

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

function formatNum(val, decimals) {
    if (val == null || isNaN(val)) return '-';
    decimals = decimals || 0;
    return Number(val).toLocaleString('es-AR', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
    });
}

function formatMoney(val) {
    if (val == null || isNaN(val)) return '-';
    return '$ ' + Number(val).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function formatMoneyUSD(val) {
    if (val == null || isNaN(val)) return '-';
    return 'USD ' + Number(val).toLocaleString('es-AR', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function formatDate(dateStr) {
    if (!dateStr) return '-';
    var d = new Date(dateStr);
    if (isNaN(d.getTime())) return dateStr;
    var day = String(d.getDate()).padStart(2, '0');
    var month = String(d.getMonth() + 1).padStart(2, '0');
    var year = d.getFullYear();
    return day + '/' + month + '/' + year;
}

function mostrarMensaje(mensaje, tipo) {
    if (typeof toastr !== 'undefined') {
        if (tipo === 'error') toastr.error(mensaje);
        else if (tipo === 'warning') toastr.warning(mensaje);
        else toastr.info(mensaje);
    } else {
        alert(mensaje);
    }
}
