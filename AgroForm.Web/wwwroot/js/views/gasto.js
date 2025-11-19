let table;
let graficoTorta;
let graficoBarras;
let monedaActual = 'ARS';
let datosGastosIndex = []; // Datos para los cuadros superiores

$(document).ready(function () {
    cargarDatosGastosIndex(); // Cargar datos para los cuadros
    inicializarDataTable();    // Cargar tabla principal
    configurarEventosGrilla();
});

function cargarDatosGastosIndex() {
    $.ajax({
        url: '/Gasto/GetGatosIndex',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            if (response.success) {
                datosGastosIndex = response.listObject;
                actualizarEstadisticas();
                // Inicializar gráficos solo si no están inicializados
                if (!graficoTorta || !graficoBarras) {
                    inicializarGraficos();
                }
            }
        },
        error: function () {
            console.error('Error al cargar datos de gastos para índices');
        }
    });
}

function actualizarEstadisticas() {
    actualizarTablaResumen();
    actualizarGraficoTorta();
    actualizarGraficoBarras();
}

function actualizarTablaResumen() {
    const tbody = $('#tbodyResumenGastos');
    tbody.empty();

    if (!datosGastosIndex || datosGastosIndex.length === 0) {
        tbody.append('<tr><td colspan="2" class="text-center text-muted">No hay datos</td></tr>');
        return;
    }

    // Agrupar gastos por descripción y sumar costos
    const gastosAgrupados = {};

    datosGastosIndex.forEach(gasto => {
        const descripcion = gasto.descripcion || 'Sin descripción';
        const costo = monedaActual === 'ARS' ? (gasto.costoARS || 0) : (gasto.costoUSD || 0);

        if (!gastosAgrupados[descripcion]) {
            gastosAgrupados[descripcion] = 0;
        }
        gastosAgrupados[descripcion] += costo;
    });

    // Ordenar por monto descendente
    const gastosOrdenados = Object.entries(gastosAgrupados)
        .sort(([, a], [, b]) => b - a);

    // Llenar tabla
    gastosOrdenados.forEach(([descripcion, total]) => {
        const fila = `
            <tr>
                <td class="small">${descripcion}</td>
                <td class="text-end fw-semibold small">
                    ${formatearMoneda(total, monedaActual)}
                </td>
            </tr>
        `;
        tbody.append(fila);
    });

    // Agregar total general
    const totalGeneral = gastosOrdenados.reduce((sum, [, monto]) => sum + monto, 0);
    tbody.append(`
        <tr class="table-primary">
            <td class="fw-bold">TOTAL</td>
            <td class="text-end fw-bold">${formatearMoneda(totalGeneral, monedaActual)}</td>
        </tr>
    `);
}

function inicializarGraficos() {
    // Gráfico de torta
    graficoTorta = new ApexCharts(document.querySelector("#graficoTorta"), {
        series: [],
        chart: {
            type: 'pie',
            height: 280
        },
        labels: [],
        colors: ['#008FFB', '#00E396', '#FEB019', '#FF4560', '#775DD0', '#546E7A', '#26a69a', '#D10CE8'],
        legend: {
            position: 'bottom',
            fontSize: '12px'
        },
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    height: 250
                },
                legend: {
                    position: 'bottom'
                }
            }
        }]
    });
    graficoTorta.render();

    // Gráfico de barras
    graficoBarras = new ApexCharts(document.querySelector("#graficoBarras"), {
        series: [{
            name: 'Gastos',
            data: []
        }],
        chart: {
            type: 'bar',
            height: 280
        },
        plotOptions: {
            bar: {
                borderRadius: 4,
                horizontal: false,
            }
        },
        dataLabels: {
            enabled: false
        },
        xaxis: {
            categories: []
        },
        colors: ['#00E396'],
        yaxis: {
            title: {
                text: 'Monto'
            },
            labels: {
                formatter: function (value) {
                    return formatearMonedaCompacta(value, monedaActual);
                }
            }
        }
    });
    graficoBarras.render();

    // Actualizar con datos iniciales
    actualizarGraficoTorta();
    actualizarGraficoBarras();
}

function actualizarGraficoTorta() {
    if (!graficoTorta || !datosGastosIndex || datosGastosIndex.length === 0) return;

    // Agrupar gastos por descripción
    const gastosAgrupados = {};

    datosGastosIndex.forEach(gasto => {
        const descripcion = gasto.descripcion || 'Sin descripción';
        const costo = monedaActual === 'ARS' ? (gasto.costoARS || 0) : (gasto.costoUSD || 0);

        if (!gastosAgrupados[descripcion]) {
            gastosAgrupados[descripcion] = 0;
        }
        gastosAgrupados[descripcion] += costo;
    });

    // Preparar datos para el gráfico
    const series = [];
    const labels = [];

    Object.entries(gastosAgrupados)
        .sort(([, a], [, b]) => b - a)
        .forEach(([descripcion, monto]) => {
            if (monto > 0) {
                series.push(monto);
                labels.push(descripcion);
            }
        });

    // Si no hay datos, mostrar mensaje
    if (series.length === 0) {
        series.push(1);
        labels.push('Sin datos');
    }

    graficoTorta.updateOptions({
        series: series,
        labels: labels,
        tooltip: {
            y: {
                formatter: function (value) {
                    return formatearMoneda(value, monedaActual);
                }
            }
        }
    });
}

function actualizarGraficoBarras() {
    if (!graficoBarras || !datosGastosIndex || datosGastosIndex.length === 0) return;

    // Agrupar gastos por mes
    const gastosPorMes = {};

    datosGastosIndex.forEach(gasto => {
        const fecha = new Date(gasto.fecha);
        const mesAnio = `${fecha.getFullYear()}-${(fecha.getMonth() + 1).toString().padStart(2, '0')}`;
        const nombreMes = fecha.toLocaleDateString('es-ES', { month: 'short', year: 'numeric' });
        const costo = monedaActual === 'ARS' ? (gasto.costoARS || 0) : (gasto.costoUSD || 0);

        if (!gastosPorMes[mesAnio]) {
            gastosPorMes[mesAnio] = {
                nombre: nombreMes,
                total: 0
            };
        }
        gastosPorMes[mesAnio].total += costo;
    });

    // Ordenar por mes
    const mesesOrdenados = Object.entries(gastosPorMes)
        .sort(([a], [b]) => a.localeCompare(b))
        .slice(-6); // Últimos 6 meses

    const categorias = mesesOrdenados.map(([, data]) => data.nombre);
    const datos = mesesOrdenados.map(([, data]) => data.total);

    // Si no hay datos, mostrar algo
    if (datos.length === 0) {
        categorias.push('Sin datos');
        datos.push(0);
    }

    graficoBarras.updateOptions({
        series: [{
            name: 'Gastos',
            data: datos
        }],
        xaxis: {
            categories: categorias
        },
        yaxis: {
            labels: {
                formatter: function (value) {
                    return formatearMonedaCompacta(value, monedaActual);
                }
            }
        },
        tooltip: {
            y: {
                formatter: function (value) {
                    return formatearMoneda(value, monedaActual);
                }
            }
        }
    });
}

function formatearMoneda(monto, moneda) {
    if (!monto) return '0.00';

    const opciones = {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    };

    const simbolo = moneda === 'ARS' ? '$' : 'US$';
    return simbolo + ' ' + monto.toLocaleString('es-AR', opciones);
}

function formatearMonedaCompacta(monto, moneda) {
    if (!monto) return '0';

    const simbolo = moneda === 'ARS' ? '$' : 'US$';

    if (monto >= 1000000) {
        return simbolo + (monto / 1000000).toFixed(1) + 'M';
    } else if (monto >= 1000) {
        return simbolo + (monto / 1000).toFixed(1) + 'K';
    } else {
        return simbolo + monto.toFixed(0);
    }
}

function cambiarMoneda() {
    const selector = document.getElementById('selectorMoneda');
    monedaActual = selector.value;

    // Actualizar tabla principal
    document.querySelectorAll('.valor-ars').forEach(el => {
        el.style.display = monedaActual === 'ARS' ? 'inline' : 'none';
    });

    document.querySelectorAll('.valor-usd').forEach(el => {
        el.style.display = monedaActual === 'USD' ? 'inline' : 'none';
    });

    // Actualizar estadísticas y gráficos (usando datos de GetGatosIndex)
    actualizarEstadisticas();
}

// Función para recargar solo los cuadros superiores
function recargarCuadrosSuperiores() {
    cargarDatosGastosIndex();
}

// Resto del código de la tabla principal (sin cambios)
function inicializarDataTable() {
    table = $('#tblGasto').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        dom: '<"row"<"col-md-6"B><"col-md-6"f>>rt<"row"<"col-md-6"l><"col-md-6"p>>',
        buttons: {
            dom: {
                button: {
                    className: 'btn'
                }
            },
            buttons: [
                {
                    extend: 'excel',
                    text: '<i class="ph ph-file-xls me-1"></i>Excel',
                    className: 'btn btn-outline-success btn-sm',
                    title: 'Gastos_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Gastos_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        ajax: {
            url: '/Gasto/GetAllDataTable',
            type: 'GET',
            dataType: 'json',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            {
                data: 'fecha',
                render: function (data) {
                    return new Date(data).toLocaleDateString('es-ES');
                }
            },
            {
                data: 'tipoGastoString',
                className: 'fw-semibold'
            },
            {
                data: null,
                render: function (data, type, row) {
                    const costoARS = row.costoARS || 0;
                    const costoUSD = row.costoUSD || 0;

                    return `
                    <span class="valor-ars">
                        ${costoARS > 0 ? '$' + parseFloat(costoARS).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '-'}
                    </span>
                    <span class="valor-usd" style="display: none;">
                        ${costoUSD > 0 ? 'US$' + parseFloat(costoUSD).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '-'}
                    </span>
                `;
                }
            },
            {
                data: 'observacion'
            },
            {
                data: 'id',
                className: 'text-center',
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-primary btn-edit" 
                                    title="Editar gasto" data-id="${data}">
                                <i class="ph ph-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-delete" 
                                    title="Eliminar gasto" data-id="${data}">
                                <i class="ph ph-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        order: [[0, 'asc']],
        pageLength: 25,
        responsive: true
    });
}

function configurarEventosGrilla() {
    $('#tblGasto tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        cargarRegistroGastoParaEditar(id);
    });

    $('#tblGasto tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarGasto(id);
    });
}

function eliminarGasto(id) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este gasto?',
        'Eliminar Gasto'
    ).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Gasto/Delete/' + id,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas();
                    if (response.success) {
                        mostrarExito(response.message);
                        // Recargar tabla principal y cuadros superiores
                        table.ajax.reload(null, false);
                        recargarCuadrosSuperiores(); // Recargar solo los cuadros
                    } else {
                        mostrarError(response.message || 'Error al eliminar registro de gasto');
                    }
                },
                error: function () {
                    cerrarAlertas();
                    mostrarError('Error al conectar con el servidor');
                }
            });
        }
    });
}

function cargarRegistroGastoParaEditar(id) {
    $.ajax({
        url: '/Gasto/GetById/' + id,
        type: 'GET',
        success: function (result) {
            if (result.success) {
                abrirModalParaEdicion(result.object);
            } else {
                mostrarMensaje('Error al cargar la registro de gasto: ' + result.message, 'error');
            }
        },
        error: function (error) {
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}

function abrirModalParaEdicion(registroGasto) {
    $('#modalGasto').modal('show');
    $('#modalGasto').one('shown.bs.modal', function () {
        $('#gastoId').val(registroGasto.id)
        configurarModoEdicion(registroGasto);
    });
}

function configurarModoEdicion(registroGasto) {
    $('#fechaGasto').val(registroGasto.fecha.split('T')[0]);
    $('#observacionesGasto').val(registroGasto.observacion || '');
    $('#tipoGasto').val(registroGasto.tipoGasto);
    $('#costoGasto').val(registroGasto.costo);
    $('#switchMonedaCostoGasto').prop('checked', registroGasto.esDolar).trigger('change');
    $('#modalGastoLabel').html('<i class="ph ph-pencil me-2"></i>Editar Registro de Gasto');
    $('button[type="submit"]').html('<i class="ph ph-check-circle me-1"></i>Actualizar');
}