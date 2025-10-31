$(document).ready(function () {
    var form = $('#formActividadRapida');
    var insumoSelect = $('#insumoId');
    var cantidadInput = $('#cantidad');

    // Mostrar/ocultar cantidad basado en selección de insumo
    insumoSelect.on('change', function () {
        if ($(this).val()) {
            cantidadInput.prop('required', true).prop('disabled', false);
        } else {
            cantidadInput.prop('required', false).prop('disabled', true).val('');
        }
    });

    // Evento para abrir el modal desde tu botón
    $('#btnAddActividad').on('click', function () {
        // Usar el método de Bootstrap directamente
        $('#modalActividadRapida').modal('show');
    });

    // Validación del formulario
    form.on('submit', function (e) {
        e.preventDefault();

        // Validar que si hay insumo, haya cantidad y viceversa
        var hasInsumo = insumoSelect.val() !== '';
        var hasCantidad = cantidadInput.val() !== '';

        if (hasInsumo && !hasCantidad) {
            cantidadInput.focus();
            mostrarMensaje('Si selecciona un insumo, debe ingresar la cantidad', 'error');
            return;
        }

        if (!hasInsumo && hasCantidad) {
            insumoSelect.focus();
            mostrarMensaje('Si ingresa cantidad, debe seleccionar un insumo', 'error');
            return;
        }

        if (!form[0].checkValidity()) {
            e.stopPropagation();
            form.addClass('was-validated');
            return;
        }

        guardarActividad();
    });

    // Resetear formulario cuando se cierra el modal
    $('#modalActividadRapida').on('hidden.bs.modal', function () {
        form[0].reset();
        form.removeClass('was-validated');
        cantidadInput.prop('disabled', true).prop('required', false);

        // Restablecer fecha actual
        $('#fecha').val(new Date().toISOString().slice(0, 16));
    });

    function guardarActividad() {
        var formData = form.serializeArray();
        var data = {
            Fecha: $('#fecha').val(),
            TipoActividadId: parseInt($('#tipoActividadId').val()),
            Observacion: $('#observacion').val(),
            InsumoId: insumoSelect.val() ? parseInt(insumoSelect.val()) : null,
            Cantidad: cantidadInput.val() ? parseFloat(cantidadInput.val()) : null
        };

        // Validar consistencia insumo/cantidad
        if ((data.InsumoId && !data.Cantidad) || (!data.InsumoId && data.Cantidad)) {
            mostrarMensaje('Insumo y cantidad deben estar ambos completos o ambos vacíos', 'error');
            return;
        }

        // Mostrar loading en el botón
        var submitBtn = form.find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="bi bi-hourglass-split me-1"></i>Guardando...').prop('disabled', true);

        // Enviar al servidor
        $.ajax({
            url: '@Url.Action("CrearRapida", "Actividad")',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    mostrarMensaje('Actividad creada correctamente', 'success');
                    $('#modalActividadRapida').modal('hide');

                    // Recargar la página o actualizar la tabla después de un tiempo
                    setTimeout(function () {
                        window.location.reload();
                    }, 1500);
                } else {
                    mostrarMensaje(result.message || 'Error al crear actividad', 'error');
                }
            },
            error: function (error) {
                console.error('Error:', error);
                mostrarMensaje('Error al conectar con el servidor', 'error');
            },
            complete: function () {
                // Restaurar botón
                submitBtn.html(originalText).prop('disabled', false);
            }
        });
    }

    function mostrarMensaje(mensaje, tipo) {
        if (typeof toastr !== 'undefined') {
            if (tipo === 'success') {
                toastr.success(mensaje);
            } else {
                toastr.error(mensaje);
            }
        } else {
            alert(mensaje);
        }
    }
});