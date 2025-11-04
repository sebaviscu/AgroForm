$(document).ready(function () {
    var form = $('#formActividadRapida');
    var insumoSelect = $('#idInsumo');
    var cantidadInput = $('#cantidad');
    var unidadMedidaText = $('#unidadMedidaText');
    var loteSelect = $('#IdLote');
    var tipoActividadSelect = $('#tipoidActividad');

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
                    console.error('Error al cargar campos para clima:', result.message);
                    mostrarMensaje('Error al cargar los campos', 'error');
                }
            },
            error: function (error) {
                console.error('Error al cargar campos para clima:', error);
                mostrarMensaje('Error al conectar con el servidor', 'error');
            }
        });
    }

    $('#btnClima').on('click', function () {
        cargarCamposParaClima();
        $('#modalClima').modal('show');
    });

    $('#tipoClima').on('change', function () {
        var tipo = $(this).val();
        if (tipo == "1") { // Granizo
            $('#milimetros').val('').prop('disabled', true);
        } else { // Lluvia
            $('#milimetros').prop('disabled', false);
        }
    });

    // Validación y envío del formulario de clima
    $('#formClima').on('submit', function (e) {
        e.preventDefault();

        if (!$('#campoClima').val()) {
            $('#campoClima').addClass('is-invalid');
            return;
        } else {
            $('#campoClima').removeClass('is-invalid');
        }

        var tipo = $('#tipoClima').val();
        var milimetros = parseFloat($('#milimetros').val());

        if (tipo == "0" && (isNaN(milimetros) || milimetros <= 0)) {
            e.preventDefault();
            $('#milimetros').addClass('is-invalid');
            return;
        } else {
            $('#milimetros').removeClass('is-invalid');
        }

        var data = {
            IdCampo: parseInt($('#campoClima').val()),
            TipoClima: parseInt($('#tipoClima').val()),
            Milimetros: $('#milimetros').val() ? parseFloat($('#milimetros').val()) : 0,
            Fecha: $('#fechaClima').val(),
            Observaciones: $('#observacionesClima').val(),
        };

        var submitBtn = $('#formClima').find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="ph ph-hourglass me-1"></i>Guardando...').prop('disabled', true);

        $.ajax({
            url: '/RegistroClima/Create',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    mostrarMensaje('Registro de clima guardado correctamente', 'success');
                    $('#modalClima').modal('hide');
                    setTimeout(function () {
                        window.location.reload();
                    }, 500);

                } else {
                    mostrarMensaje(result.message || 'Error al guardar el registro de clima', 'error');
                }
            },
            error: function (error) {
                console.error('Error:', error);
                mostrarMensaje('Error al conectar con el servidor', 'error');
            },
            complete: function () {
                submitBtn.html(originalText).prop('disabled', false);
            }
        });
    });

    // Resetear el modal cuando se cierre
    $('#modalClima').on('hidden.bs.modal', function () {
        $('#formClima')[0].reset();
        $('#campoClima').removeClass('is-invalid');
    });

    // Inicializar Select2 para tipo de actividad CON FILTRO DE INSUMOS
    function inicializarSelectConIconos() {
        if ($('#tipoidActividad').hasClass('select2-hidden-accessible')) {
            $('#tipoidActividad').select2('destroy');
        }

        var tipoActividadSelect = $('#tipoidActividad');

        tipoActividadSelect.select2({
            dropdownParent: $('#modalActividadRapida'),
            templateResult: function (option) {
                if (!option.id) return option.text;
                var icono = $(option.element).data('icono');
                var iconoColor = $(option.element).data('icono-color') || '#000';

                if (icono) {
                    var $span = $('<span></span>');
                    $span.append($('<i></i>', {
                        class:'ph '+ icono + ' me-2',
                        style: 'color: ' + iconoColor
                    }));
                    $span.append(option.text);
                    return $span;
                }
                return option.text;
            },
            templateSelection: function (option) {
                var icono = $(option.element).data('icono');
                var iconoColor = $(option.element).data('icono-color') || '#000';

                if (icono && option.id) {
                    var $span = $('<span></span>');
                    $span.append($('<i></i>', {
                        class: 'ph ' + icono + ' me-2',
                        style: 'color: ' + iconoColor
                    }));
                    $span.append(option.text);
                    return $span;
                }
                return option.text;
            },
            escapeMarkup: function (markup) { return markup; },
            width: '100%',
            placeholder: 'Seleccione un tipo...',
            allowClear: false
        });

        // Evento para filtrar insumos cuando cambia el tipo de actividad
        tipoActividadSelect.on('change', function () {
            var idTipoInsumo = $(this).find('option:selected').data('tipo-insumo');
            filtrarInsumosPorTipo(idTipoInsumo);
        });

        tipoActividadSelect.val(null).trigger('change');
    }

    function inicializarSelectLotes() {
        if ($('#IdLote').hasClass('select2-hidden-accessible')) {
            $('#IdLote').select2('destroy');
        }

        loteSelect.select2({
            dropdownParent: $('#modalActividadRapida'),
            placeholder: 'Seleccione uno o más lotes...',
            allowClear: true,
            multiple: true,
            width: '100%',
            closeOnSelect: false
        });
    }

    // NUEVA FUNCIÓN: Cargar todos los insumos
    function cargarTodosLosInsumos() {
        $.ajax({
            url: '/Insumo/GetAll',
            type: 'GET',
            success: function (result) {
                if (result.success && result.listObject) {
                    actualizarSelectInsumo(result.listObject);
                } else {
                    console.error('Error cargando insumos:', result.message);
                    actualizarSelectInsumo([]);
                }
            },
            error: function (error) {
                console.error('Error cargando insumos:', error);
                actualizarSelectInsumo([]);
            }
        });
    }

    // NUEVA FUNCIÓN: Filtrar insumos por tipo
    function filtrarInsumosPorTipo(idTipoInsumo) {
        $.ajax({
            url: '/Insumo/GetByTipoInsumo',
            type: 'GET',
            data: { idTipoInsumo: idTipoInsumo },
            success: function (result) {
                if (result.success && result.listObject) {
                    actualizarSelectInsumo(result.listObject);
                } else {
                    console.error('Error filtrando insumos:', result.message);
                    actualizarSelectInsumo([]);
                }
            },
            error: function (error) {
                console.error('Error filtrando insumos:', error);
                actualizarSelectInsumo([]);
            }
        });
    }

    // NUEVA FUNCIÓN: Actualizar select de insumos
    function actualizarSelectInsumo(insumosData) {
        // Destruir select2 actual
        if (insumoSelect.hasClass('select2-hidden-accessible')) {
            insumoSelect.select2('destroy');
        }

        // Limpiar opciones
        insumoSelect.empty();

        // Agregar placeholder
        insumoSelect.append($('<option>', {
            value: '',
            text: 'Seleccione un insumo...'
        }));

        // Agregar insumos
        $.each(insumosData, function (index, insumo) {
            insumoSelect.append($('<option>', {
                value: insumo.id,
                text: insumo.descripcion,
                'data-unidad': insumo.unidadMedida
            }));
        });

        // Re-inicializar select2
        insumoSelect.select2({
            dropdownParent: $('#modalActividadRapida'),
            placeholder: 'Seleccione un insumo...',
            allowClear: true,
            width: '100%'
        });

        // Limpiar selección y resetear campos relacionados
        insumoSelect.val(null).trigger('change');
        cantidadInput.val('').prop('disabled', true);
        unidadMedidaText.text('-').addClass('text-muted');
    }

    // MODIFICADA: Inicializar Select de Insumo
    function inicializarSelectInsumo() {
        if ($('#idInsumo').hasClass('select2-hidden-accessible')) {
            $('#idInsumo').select2('destroy');
        }

        // Cargar todos los insumos inicialmente
        cargarTodosLosInsumos();
    }

    insumoSelect.on('change', function () {
        var selectedOption = $(this).find('option:selected');
        var unidadMedida = selectedOption.data('unidad') || '-';

        var selectedData = $(this).select2('data');
        if (selectedData && selectedData[0]) {
            unidadMedida = $(selectedData[0].element).data('unidad') || '-';
        }

        unidadMedidaText.text(unidadMedida);

        if ($(this).val()) {
            cantidadInput.prop('required', true).prop('disabled', false);
            unidadMedidaText.removeClass('text-muted');
        } else {
            cantidadInput.prop('required', false).prop('disabled', true).val('');
            unidadMedidaText.text('-').addClass('text-muted');
        }
    });

    // Evento para abrir el modal
    $('#btnAddActividad').on('click', function () {
        $('#modalActividadRapida').modal('show');
    });

    // Inicializar Select2 cuando el modal se muestra
    $('#modalActividadRapida').on('shown.bs.modal', function () {
        inicializarSelectConIconos();
        inicializarSelectLotes();
        inicializarSelectInsumo();
    });

    // Validación del formulario
    form.on('submit', function (e) {
        e.preventDefault();

        // Validar campos requeridos
        var fechaVal = $('#fecha').val();
        var lotesVal = loteSelect.val();
        var tipoActividadVal = tipoActividadSelect.val();

        if (!fechaVal) {
            $('#fecha').addClass('is-invalid');
            e.stopPropagation();
            return;
        } else {
            $('#fecha').removeClass('is-invalid');
        }

        if (!lotesVal || lotesVal.length === 0) {
            loteSelect.next('.select2-container').find('.select2-selection').addClass('is-invalid');
            e.stopPropagation();
            return;
        } else {
            loteSelect.next('.select2-container').find('.select2-selection').removeClass('is-invalid');
        }

        if (!tipoActividadVal) {
            tipoActividadSelect.next('.select2-container').find('.select2-selection').addClass('is-invalid');
            e.stopPropagation();
            return;
        } else {
            tipoActividadSelect.next('.select2-container').find('.select2-selection').removeClass('is-invalid');
        }

        // Resto de validaciones (insumo/cantidad)
        var hasInsumo = insumoSelect.val() !== '' && insumoSelect.val() != null;
        var hasCantidad = cantidadInput.val() !== '' && cantidadInput.val() != null;

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

        guardarActividad();
    });

    // Resetear formulario cuando se cierra el modal
    $('#modalActividadRapida').on('hidden.bs.modal', function () {
        form[0].reset();
        cantidadInput.prop('disabled', true);
        unidadMedidaText.text('-').addClass('text-muted');

        // Destruir todos los Select2
        if (tipoActividadSelect.hasClass('select2-hidden-accessible')) {
            tipoActividadSelect.select2('destroy');
        }
        if (loteSelect.hasClass('select2-hidden-accessible')) {
            loteSelect.select2('destroy');
        }
        if (insumoSelect.hasClass('select2-hidden-accessible')) {
            insumoSelect.select2('destroy');
        }

        // Restablecer fecha actual
        $('#fecha').val(new Date().toISOString().slice(0, 10));
    });

    function guardarActividad() {
        var insumoVal = insumoSelect.val();
        var idInsumo = null;

        if (insumoVal && insumoVal !== "" && insumoVal !== "0") {
            idInsumo = parseInt(insumoVal);
        }

        var data = {
            Fecha: $('#fecha').val(),
            LotesIds: loteSelect.val() ? loteSelect.val().map(function (id) {
                return parseInt(id);
            }) : [],
            TipoidActividad: parseInt($('#tipoidActividad').val()),
            Observacion: $('#observacion').val(),
            Costo: parseFloat($('#costoTotal').val()),
            idInsumo: idInsumo,
            Cantidad: cantidadInput.val() ? parseFloat(cantidadInput.val()) : null
        };

        // Validar consistencia insumo/cantidad
        if ((data.idInsumo && !data.Cantidad) || (!data.idInsumo && data.Cantidad)) {
            mostrarMensaje('Insumo y cantidad deben estar ambos completos o ambos vacíos', 'error');
            return;
        }

        // Mostrar loading en el botón
        var submitBtn = form.find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="ph ph-hourglass me-1"></i>Guardando...').prop('disabled', true);

        // Enviar al servidor
        $.ajax({
            url: '/Actividad/CrearRapida',
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

                    setTimeout(function () {
                        window.location.reload();
                    }, 500);
                } else {
                    mostrarMensaje(result.message || 'Error al crear actividad', 'error');
                    submitBtn.html(originalText).prop('disabled', false);
                }
            },
            error: function (error) {
                console.error('Error:', error);
                mostrarMensaje('Error al conectar con el servidor', 'error');
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