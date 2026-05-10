let chartRendimiento = null;
let chartCostos = null;
let monedaActual = 'ARS';
let datosActuales = [];

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
        url: '/Reporte/GetComparativaCamposData',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(filtros),
        success: function (response) {
            if (response.success) {
                datosActuales = response.listObject || [];
                renderizarDatos(datosActuales);
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
        $('#tablaBody').html('<tr><td colspan="14" class="text-center text-muted py-4">No se encontraron datos con los filtros seleccionados</td></tr>');
        $('#heatmapContainer').html('<div class="text-center text-muted py-3">Sin datos para mostrar</div>');
        limpiarResumen();
        limpiarGraficos();
        return;
    }

    // Renderizar tabla
    renderizarTabla(datos);

    // Renderizar heatmap
    renderizarHeatmap(datos);

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
        if (item.rankingRendimiento === 1) rankingClass = 'ranking-1';
        else if (item.rankingRendimiento === 2) rankingClass = 'ranking-2';
        else if (item.rankingRendimiento === 3) rankingClass = 'ranking-3';

        var rankingHtml = item.rankingRendimiento > 0
            ? '<span class="ranking-badge ' + rankingClass + '">' + item.rankingRendimiento + '</span>'
            : '<span class="text-muted">-</span>';

        var row = '<tr>' +
            '<td class="text-center">' + rankingHtml + '</td>' +
            '<td><span class="campo-name">' + escapeHtml(item.campo) + '</span></td>' +
            '<td><span class="lote-name">' + escapeHtml(item.lote) + '</span></td>' +
            '<td class="text-end">' + formatNum(item.superficieHa, 2) + '</td>' +
            '<td>' + escapeHtml(item.cultivo || '-') + '</td>' +
            '<td>' + formatDate(item.fechaSiembra) + '</td>' +
            '<td>' + formatDate(item.fechaCosecha) + '</td>' +
            '<td class="text-end fw-semibold">' + formatNum(item.rendimientoTonHa, 2) + '</td>' +
            '<td class="text-end">' + formatNum(item.rendimientoTotalTon, 2) + '</td>' +
            '<td class="text-end">' + formatNum(item.totalFertilizantesKgHa, 1) + '</td>' +
            '<td class="text-end">' + formatNum(item.totalPulverizacionesLtsHa, 1) + '</td>' +
            '<td class="text-center">' + (item.cantidadLabores || 0) + '</td>' +
            '<td class="text-end">' +
            '   <span class="valor-ars">' + formatMoney(item.costoTotalARS) + '</span>' +
            '   <span class="valor-usd" style="display: none;">' + formatMoney(item.costoTotalUSD) + '</span>' +
            '</td>' +
            '<td class="text-end">' +
            '   <span class="valor-ars">' + formatMoney(item.costoPorHaARS) + '</span>' +
            '   <span class="valor-usd" style="display: none;">' + formatMoney(item.costoPorHaUSD) + '</span>' +
            '</td>' +
            '</tr>';

        tbody.append(row);
    });

    // Aplicar visibilidad según moneda actual
    aplicarVisibilidadMoneda();
}

function renderizarHeatmap(datos) {
    var container = $('#heatmapContainer');
    container.empty();

    // Encontrar min/max rendimiento para escalar colores
    var conRendimiento = datos.filter(function (d) { return d.rendimientoTonHa != null; });
    if (conRendimiento.length === 0) {
        container.html('<div class="text-center text-muted py-3">Sin datos de rendimiento para heatmap</div>');
        return;
    }

    var maxRend = Math.max.apply(null, conRendimiento.map(function (d) { return d.rendimientoTonHa; }));
    var minRend = Math.min.apply(null, conRendimiento.map(function (d) { return d.rendimientoTonHa; }));
    var rango = maxRend - minRend || 1;

    $.each(datos, function (i, item) {
        var heatClass = 'heatmap-none';
        var label = 'Sin datos';

        if (item.rendimientoTonHa != null) {
            var rel = (item.rendimientoTonHa - minRend) / rango;
            if (rel > 0.75) heatClass = 'heatmap-high';
            else if (rel > 0.5) heatClass = 'heatmap-mid-high';
            else if (rel > 0.25) heatClass = 'heatmap-mid';
            else heatClass = 'heatmap-mid-low';
            label = item.rendimientoTonHa.toFixed(2) + ' tn/ha';
        }

        var html = '<div class="heatmap-cell ' + heatClass + '" title="' +
            escapeHtml(item.lote) + ' (' + escapeHtml(item.campo) + '): ' + label + '" style="min-width: 100px; flex: 1 0 auto;">' +
            '<div class="small fw-bold">' + escapeHtml(item.lote) + '</div>' +
            '<div class="small">' + label + '</div>' +
            '</div>';

        container.append(html);
    });
}

function calcularResumen(datos) {
    var totalLotes = datos.length;
    var conRendimiento = datos.filter(function (d) { return d.rendimientoTonHa != null; });
    var conCostoARS = datos.filter(function (d) { return d.costoPorHaARS != null; });
    var conCostoUSD = datos.filter(function (d) { return d.costoPorHaUSD != null; });

    // Rendimiento promedio
    var rendProm = null;
    var rendMax = null;
    var rendMin = null;
    var mejorLote = null;
    var peorLote = null;

    if (conRendimiento.length > 0) {
        var sum = conRendimiento.reduce(function (a, b) { return a + b.rendimientoTonHa; }, 0);
        rendProm = sum / conRendimiento.length;
        rendMax = Math.max.apply(null, conRendimiento.map(function (d) { return d.rendimientoTonHa; }));
        rendMin = Math.min.apply(null, conRendimiento.map(function (d) { return d.rendimientoTonHa; }));

        var mejor = conRendimiento.reduce(function (a, b) { return a.rendimientoTonHa > b.rendimientoTonHa ? a : b; });
        mejorLote = mejor.lote + ' (' + mejor.campo + ')';

        var peor = conRendimiento.reduce(function (a, b) { return a.rendimientoTonHa < b.rendimientoTonHa ? a : b; });
        peorLote = peor.lote + ' (' + peor.campo + ')';
    }

    // Costo promedio ARS
    var costoPromARS = null;
    if (conCostoARS.length > 0) {
        var sumCostoARS = conCostoARS.reduce(function (a, b) { return a + b.costoPorHaARS; }, 0);
        costoPromARS = sumCostoARS / conCostoARS.length;
    }

    // Costo promedio USD
    var costoPromUSD = null;
    if (conCostoUSD.length > 0) {
        var sumCostoUSD = conCostoUSD.reduce(function (a, b) { return a + b.costoPorHaUSD; }, 0);
        costoPromUSD = sumCostoUSD / conCostoUSD.length;
    }

    // Actualizar UI
    $('#resumen-totalLotes').text(totalLotes);
    $('#resumen-rendimientoPromedio').text(rendProm != null ? rendProm.toFixed(2) + ' tn/ha' : '-');
    $('#resumen-mejorLote').text(mejorLote || '-').attr('title', mejorLote || '');
    $('#resumen-mejorRendimiento').text(rendMax != null ? rendMax.toFixed(2) + ' tn/ha' : '');
    $('#resumen-costoPromedio-ars').text(costoPromARS != null ? '$ ' + costoPromARS.toFixed(0) : '-');
    $('#resumen-costoPromedio-usd').text(costoPromUSD != null ? 'US$ ' + costoPromUSD.toFixed(2) : '-');
}

function limpiarResumen() {
    $('#resumen-totalLotes').text('0');
    $('#resumen-rendimientoPromedio').text('-');
    $('#resumen-mejorLote').text('-');
    $('#resumen-mejorRendimiento').text('');
    $('#resumen-costoPromedio-ars').text('-');
    $('#resumen-costoPromedio-usd').text('-');
}

function renderizarGraficos(datos) {
    var conRendimiento = datos.filter(function (d) { return d.rendimientoTonHa != null; });
    var labels = conRendimiento.map(function (d) { return d.lote; });
    var rendimientos = conRendimiento.map(function (d) { return d.rendimientoTonHa; });
    var colores = conRendimiento.map(function (d) {
        if (d.rankingRendimiento === 1) return '#ffd700';
        if (d.rankingRendimiento === 2) return '#c0c0c0';
        if (d.rankingRendimiento === 3) return '#cd7f32';
        return '#4CAF50';
    });

    // Gráfico de rendimiento (barras)
    var ctx1 = document.getElementById('chartRendimiento').getContext('2d');
    if (chartRendimiento) chartRendimiento.destroy();

    chartRendimiento = new Chart(ctx1, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Rendimiento (tn/ha)',
                data: rendimientos,
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
                        text: 'tn/ha'
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

    // Gráfico de costos (torta/donut) - usa la moneda actual
    actualizarGraficoCostos(datos);
}

function actualizarGraficoCostos(datos) {
    var esARS = monedaActual === 'ARS';
    var campoTotal = esARS ? 'costoTotalARS' : 'costoTotalUSD';
    var simbolo = esARS ? 'ARS' : 'USD';

    var conCostos = datos.filter(function (d) { return d[campoTotal] != null && d[campoTotal] > 0; });
    var labelsCostos = conCostos.map(function (d) { return d.lote; });
    var costos = conCostos.map(function (d) { return d[campoTotal]; });
    var coloresCostos = [
        '#FF6384', '#36A2EB', '#FFCE56', '#4BC0C0', '#9966FF',
        '#FF9F40', '#C9CBCF', '#7BC8A4', '#E7E9ED', '#F7464A'
    ];

    var ctx2 = document.getElementById('chartCostos').getContext('2d');
    if (chartCostos) chartCostos.destroy();

    chartCostos = new Chart(ctx2, {
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
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            var value = context.parsed || 0;
                            var prefix = esARS ? '$' : 'US$';
                            return ' ' + prefix + ' ' + value.toLocaleString('es-AR', {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 2
                            });
                        }
                    }
                }
            }
        }
    });

    // Actualizar título del gráfico
    $('#chartCostosTitle').text('(' + simbolo + ')');
}

function limpiarGraficos() {
    if (chartRendimiento) { chartRendimiento.destroy(); chartRendimiento = null; }
    if (chartCostos) { chartCostos.destroy(); chartCostos = null; }
}

function exportarExcel() {
    var table = document.getElementById('tblComparativa');
    if (!table) return;

    // Clonar la tabla para no modificar la original
    var clone = table.cloneNode(true);
    // Quitar la columna de ranking (primera columna)
    $(clone).find('tr').each(function () {
        $(this).find('th:first, td:first').remove();
    });

    var wb = XLSX.utils.table_to_book(clone, { sheet: 'Comparativa' });
    XLSX.writeFile(wb, 'Comparativa_Campos_Lotes.xlsx');
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
    var esARS = monedaActual === 'ARS';
    var prefix = esARS ? '$' : 'US$';
    var decimals = esARS ? 0 : 2;
    return prefix + ' ' + Number(val).toLocaleString('es-AR', {
        minimumFractionDigits: decimals,
        maximumFractionDigits: decimals
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
    // Se puede implementar un spinner si se desea
}

// --- Funciones de cambio de moneda ---

function cambiarMoneda() {
    var selector = document.getElementById('selectorMoneda');
    monedaActual = selector.value;

    // Actualizar visibilidad de columnas de costo en la tabla
    aplicarVisibilidadMoneda();

    // Actualizar resumen de costo promedio
    actualizarResumenMoneda();

    // Actualizar gráfico de costos
    if (datosActuales && datosActuales.length > 0) {
        actualizarGraficoCostos(datosActuales);
    }
}

function aplicarVisibilidadMoneda() {
    var mostrarARS = monedaActual === 'ARS';

    document.querySelectorAll('.valor-ars').forEach(function (el) {
        el.style.display = mostrarARS ? 'inline' : 'none';
    });

    document.querySelectorAll('.valor-usd').forEach(function (el) {
        el.style.display = mostrarARS ? 'none' : 'inline';
    });
}

function actualizarResumenMoneda() {
    // Las tarjetas de resumen ya tienen ambos valores almacenados,
    // solo cambiamos la visibilidad
    var mostrarARS = monedaActual === 'ARS';

    var arsEl = document.getElementById('resumen-costoPromedio-ars');
    var usdEl = document.getElementById('resumen-costoPromedio-usd');

    if (arsEl) arsEl.style.display = mostrarARS ? 'inline' : 'none';
    if (usdEl) usdEl.style.display = mostrarARS ? 'none' : 'inline';
}
