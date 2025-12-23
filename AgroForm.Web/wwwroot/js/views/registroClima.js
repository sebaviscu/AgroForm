$(document).ready(function () {

    configurarEventosGrilla();
});
function configurarEventosGrilla() {

    $('#tblClima tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        cargarRegistroClimaParaEditar(id);

    });

    $('#tblClima tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarClima(id);
    });
}

function eliminarClima(id) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este registro de clima?',
        'Eliminar Registro de Clima'
    ).then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: '/RegistroClima/Delete/' + id,
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
                        mostrarError(response.message || 'Error al eliminar registro de clima');
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

function cargarRegistroClimaParaEditar(id) {
    $.ajax({
        url: '/RegistroClima/GetById/' + id,
        type: 'GET',
        success: function (result) {
            if (result.success) {
                abrirModalParaEdicion(result.object);
            } else {
                mostrarMensaje('Error al cargar la registro de clima: ' + result.message, 'error');
            }
        },
        error: function (error) {
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}

function abrirModalParaEdicion(registroClima) {
    $('#modalClima').modal('show');
    cargarCamposParaClima();

    $('#modalClima').one('shown.bs.modal', function () {
        $('#registroClimaId').val(registroClima.id)
        configurarModoEdicion(registroClima);
    });
}

function configurarModoEdicion(registroClima) {

    $('#fechaClima').val(registroClima.fecha.split('T')[0]); // Formatear fecha
    $('#observacionesClima').val(registroClima.observacion || '');
    $('#tipoClima').val(registroClima.tipoClima);
    $('#campoClima').val(registroClima.idCampo);

    if (registroClima.tipoClima == 0) {
        $('#milimetros').val(registroClima.milimetros);
    }
    else {
        $('#milimetros').prop('disabled', true);
    }

    $('#modalClimaLabel').html('<i class="ph ph-cloud-rain me-2"></i>Editar Registro de Clima');
    $('#btnGuardarClima').html('<i class="ph ph-check-circle me-1"></i>Actualizar');

}

function cargarCamposParaClima() {
    $.ajax({
        url: '/Campo/GetAll',
        type: 'GET',
        success: function (result) {
            if (result.success) {
                var select = $('#campoClima');
                select.empty().append('<option value="" selected disabled>Seleccione un campo...</option>');
                $.each(result.listObject, function (index, lote) {
                    select.append($('<option>', {
                        value: lote.id,
                        text: lote.nombre
                    }));
                });
            } else {
                mostrarMensaje('Error al cargar los campos', 'error');
            }
        },
        error: function (error) {
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}