$(document).ready(function () {

    inicializarDataTable();
    configurarEventosGrilla();
});

function inicializarDataTable() {
    var table = $('#tblActividades').DataTable({
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
                    title: 'Campanias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Campanias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        order: [[0, 'desc']],
        pageLength: 25,
        responsive: true
    });

    $('#campoFilter').on('change', function () {
        var valor = $(this).val();

        if (valor === "TODOS" || valor === null) {
            table.column(2).search('').draw();
        } else {
            table.column(2).search('^' + valor + '$', true, false).draw();
        }
    });
}

function configurarEventosGrilla() {

    $('#tblActividades tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        var idTipoActividad = $(this).data('idTipoActividad');
        //abrirModalActividad(id, idTipoActividad);
    });

    $('#tblActividades tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        var idTipoActividad = $(this).data('idtipoactividad');
        eliminarActividad(id, idTipoActividad);
    });
}

function eliminarActividad(id, idTipoActividad) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar esta labor? Esta acción no se puede deshacer.',
        'Eliminar labor'
    ).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: '/Actividad/Delete?id=' + id + '&idTipoActividad=' + idTipoActividad,
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
                        mostrarError(response.message || 'Error al eliminar labor');
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