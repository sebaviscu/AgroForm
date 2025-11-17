$(document).ready(function () {
    inicializarDataTable();
    configurarEventosGrilla();
});

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
                data: 'fecha'
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
                        ${costoARS > 0 ? '$' + parseFloat(costoARS).toLocaleString('es-AR', { maximumFractionDigits: 0 }) : '-'}
                    </span>
                    <span class="valor-usd" style="display: none;">
                        ${costoUSD > 0 ? 'US$' + parseFloat(costoUSD).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : '-'}
                    </span>
                `;
                }
            },
            {
                data: 'observaciones'
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

                        setTimeout(function () {
                            window.location.reload();
                        }, 500);
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
    $('#costoGasto').val(datosEspecificos.costo);
    $('#switchMonedaCostoGasto').prop('checked', registroGasto.esDolar).trigger('change');

    $('#modalGastoLabel').html('<i class="ph ph-pencil me-2"></i>Editar Registro de Gasto');
    $('button[type="submit"]').html('<i class="ph ph-check-circle me-1"></i>Actualizar');

}


function cambiarMoneda() {
    const selector = document.getElementById('selectorMoneda');
    const monedaActual = selector.value;

    // Ocultar todos y mostrar solo los de la moneda seleccionada
    document.querySelectorAll('.valor-ars').forEach(el => {
        el.style.display = monedaActual === 'ARS' ? 'inline' : 'none';
    });

    document.querySelectorAll('.valor-usd').forEach(el => {
        el.style.display = monedaActual === 'USD' ? 'inline' : 'none';
    });

}