
$(document).ready(function () {
    cargarCampos();
});

function cargarCampos() {
    $.ajax({
        url: '/Campo/GetAll',
        type: 'GET',
        success: function (result) {
            if (result.success) {
                var select = $('#selectCampos');
                select.empty().append('<option value="" selected disabled>Seleccione una opción...</option>');
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