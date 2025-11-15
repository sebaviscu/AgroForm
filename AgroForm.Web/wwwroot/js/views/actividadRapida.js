$(document).ready(function () {
    var form = $('#formActividadRapida');
    var cantidadInput = $('#cantidad');
    var unidadMedidaText = $('#unidadMedidaText');
    var loteSelect = $('#IdLote');
    var tipoActividadSelect = $('#tipoidActividad');
    var camposEspecificosContainer = $('#camposEspecificos');

    // Mapeo de tipos de actividad a sus templates
    var tipoActividadTemplates = {
        'Siembra': '#templateSiembra',
        'Riego': '#templateRiego',
        'Fertilizado': '#templateFertilizado',
        'Pulverizacion': '#templatePulverizacion',
        'Monitoreo': '#templateMonitoreo',
        'Analisis de suelo': '#templateAnalisisSuelo',
        'Otras labores': '#templateOtrasLabores',
        'Cosecha': '#templateCosecha'
    };


    // FUNCIÓN: Cargar campos específicos según tipo de actividad
    function cargarCamposEspecificos(tipoActividadNombre) {
        camposEspecificosContainer.empty();

        var templateId = tipoActividadTemplates[tipoActividadNombre];
        if (templateId) {
            var template = $(templateId).html();
            camposEspecificosContainer.html(template);

            // Inicializar Select2 para los nuevos selects
            setTimeout(function () {
                $('.form-select', camposEspecificosContainer).each(function () {
                    if (!$(this).hasClass('select2-hidden-accessible')) {
                        $(this).select2({
                            dropdownParent: $('#modalActividadRapida'),
                            width: '100%'
                        });
                    }
                });
            }, 100);
        }
    }

    function cargarSwitchMoneda(idSwitch, idLabel) {
        $("#" + idSwitch).on("change", function () {
            const esUSD = $(this).is(":checked");
            $("#" + idLabel).text(esUSD ? "US$" : "$");
            $(this).next("label").text(esUSD ? "USD" : "ARS");
        });
    }
    // FUNCIÓN: Cargar datos para selects específicos
    function cargarDatosParaSelects(tipoActividadNombre) {
        switch (tipoActividadNombre) {
            case 'Siembra':
                cargarCultivos();
                cargarCatalogos(30, 'MetodoSiembra');
                cargarSwitchMoneda("switchMonedaCostoSiembra", "labelMonedaCostoSiembra");
            case 'Cosecha':
                cargarCultivos();
                cargarSwitchMoneda("switchMonedaCostoCosecha", "labelMonedaCostoCosecha");
                break;
            case 'Riego':
                cargarCatalogos(31, 'MetodoRiego');
                cargarCatalogos(41, 'FuenteAgua');
                cargarSwitchMoneda("switchMonedaCostoRiego", "labelMonedaCostoRiego");
                break;
            case 'Fertilizado':
                cargarCatalogos(21, 'Nutriente');
                cargarCatalogos(20, 'TipoFertilizante');
                cargarCatalogos(32, 'MetodoAplicacion');
                cargarSwitchMoneda("switchMonedaCostoFertilizacion", "labelMonedaCostoFertilizacion");
                break;
            case 'Pulverizacion':
                cargarCatalogos(22, 'ProductoAgroquimico');
                cargarSwitchMoneda("switchMonedaCostoPulverizacion", "labelMonedaCostoPulverizacion");
                break;
            case 'Monitoreo':
                //cargarEstadosFenologicos();
                cargarSwitchMoneda("switchMonedaCostoMonitoreo", "labelMonedaCostoMonitoreo");
                break;
            case 'AnalisisSuelo':
                cargarCatalogos(50, 'Laboratorio');
                cargarSwitchMoneda("switchMonedaCostoAnalisisSuelo", "labelMonedaCostoAnalisisSuelo");
                break;
            case 'OtrasLabores':
                cargarSwitchMoneda("switchMonedaCostoOtraLabor", "labelMonedaCostoOtraLabor");
                break;
        }
    }

    // FUNCIÓN: Cargar cultivos
    function cargarCultivos() {
        $.ajax({
            url: '/Cultivo/GetAll',
            type: 'GET',
            success: function (result) {
                if (result.success && result.listObject) {
                    var selectCultivo = $('#idCultivo, #idCultivoCosecha');
                    $.each(result.listObject, function (index, cultivo) {
                        selectCultivo.append($('<option>', {
                            value: cultivo.id,
                            text: cultivo.nombre
                        }));
                    });
                    $('#idCultivo').val(null).trigger('change');
                    $('#idCultivoCosecha').val(null).trigger('change');
                }
            },
            error: function (error) {
                console.error('Error cargando cultivos:', error);
            }
        });
    }

    // FUNCIÓN: Cargar catalogos por tipo
    function cargarCatalogos(tipoCatalogo, idSelect2) {
        $.ajax({
            url: '/Catalogo/GetByTipo',
            type: 'GET',
            data: { tipo: tipoCatalogo },
            success: function (result) {
                if (result.success && result.listObject) {
                    var selectId = 'id' + idSelect2;
                    $('#' + selectId).empty();
                    $.each(result.listObject, function (index, catalogo) {
                        $('#' + selectId).append($('<option>', {
                            value: catalogo.id,
                            text: catalogo.nombre
                        }));
                    });
                    $('#' + selectId).val(null).trigger('change');
                }
            },
            error: function (error) {
                console.error('Error cargando catálogos:', error);
            }
        });
    }

    // FUNCIÓN: Cargar estados fenológicos
    function cargarEstadosFenologicos() {
        $.ajax({
            url: '/EstadoFenologico/GetAll',
            type: 'GET',
            success: function (result) {
                if (result.success && result.listObject) {
                    var select = $('#idEstadoFenologico');
                    select.empty().append('<option value="">Seleccionar estado</option>');
                    $.each(result.listObject, function (index, estado) {
                        select.append($('<option>', {
                            value: estado.id,
                            text: estado.nombre + ' (' + estado.codigo + ')'
                        }));
                    });
                }
            },
            error: function (error) {
                console.error('Error cargando estados fenológicos:', error);
            }
        });
    }

    // Evento cuando cambia el cultivo (para variedades)
    $(document).on('change', '#idCultivo', function () {
        var cultivoId = $(this).val();
        if (cultivoId) {
            cargarVariedades(cultivoId);
        } else {
            $('#idVariedad').empty().val(null).trigger('change');
        }
    });

    $(document).on('change', '#idMonitoreo', function () {
        var idTipoCatalogo = $(this).val();
        if (idTipoCatalogo) {
            cargarCatalogos(idTipoCatalogo, 'TipoMonitoreo');
        } else {
            $('#IdTipoMonitoreo').empty().val(null).trigger('change');
        }
    });

    // FUNCIÓN: Cargar variedades por cultivo
    function cargarVariedades(cultivoId) {
        $.ajax({
            url: '/Variedad/GetByCultivo',
            type: 'GET',
            data: { idCultivo: cultivoId },
            success: function (result) {
                if (result.success && result.listObject) {
                    var select = $('#idVariedad');
                    select.empty();
                    $.each(result.listObject, function (index, variedad) {
                        select.append($('<option>', {
                            value: variedad.id,
                            text: variedad.nombre
                        }));
                    });
                    select.val(null).trigger('change');
                }
            },
            error: function (error) {
                console.error('Error cargando variedades:', error);
            }
        });
    }

    // CÓDIGO EXISTENTE PARA CLIMA (sin cambios)
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

    $('#btnClima').on('click', function () {
        cargarCamposParaClima();
        $('#modalClima').modal('show');
        $('#modalClimaLabel').html('<i class="ph ph-pencil me-2"></i>Crear Registro de Clima');
        $('button[type="submit"]').html('<i class="ph ph-check-circle me-1"></i>Guardar');
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
        var registroClimaId = $('#registroClimaId').val();
        var esEdicion = registroClimaId && registroClimaId > 0;

        var data = {
            IdCampo: parseInt($('#campoClima').val()),
            TipoClima: parseInt($('#tipoClima').val()),
            Milimetros: $('#milimetros').val() ? parseFloat($('#milimetros').val()) : 0,
            Fecha: $('#fechaClima').val(),
            Observaciones: $('#observacionesClima').val(),
            id: esEdicion ? parseInt(registroClimaId) : 0,
        };

        var submitBtn = $('#formClima').find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="ph ph-hourglass me-1"></i>Guardando...').prop('disabled', true);

        submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' + (esEdicion ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);
        var url = esEdicion ? '/RegistroClima/Update' : '/RegistroClima/Create';
        var mensajeExito = esEdicion ? 'Registro de Clima actualizado correctamente' : 'Registro de Clima creado correctamente';

        $.ajax({
            url: url,
            type: esEdicion ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    mostrarMensaje(mensajeExito, 'success');
                    $('#modalClima').modal('hide');
                    setTimeout(function () {
                        window.location.reload();
                    }, 500);

                } else {
                    mostrarMensaje(result.message || 'Error al guardar el registro de clima', 'error');
                }
            },
            error: function (error) {
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

    // MODIFICADA: Inicializar Select2 para tipo de actividad
    function inicializarSelectConIconos() {
        // Destruir select2 existente si hay uno
        if ($('#tipoidActividad').hasClass('select2-hidden-accessible')) {
            $('#tipoidActividad').select2('destroy');
        }

        var tipoActividadSelect = $('#tipoidActividad');

        // Primero, asegurarnos de que el select esté visible
        tipoActividadSelect.show();

        tipoActividadSelect.select2({
            dropdownParent: $('#modalActividadRapida'), // IMPORTANTE: Referencia al modal
            templateResult: function (option) {
                if (!option.id) return option.text;
                var icono = $(option.element).data('icono');
                var iconoColor = $(option.element).data('icono-color') || '#000';
                var tipoActividad = $(option.element).data('tipo-actividad');

                if (icono) {
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

        // **CORRECCIÓN: Usar el evento change de Select2 correctamente**
        tipoActividadSelect.on('change', function () {

            var selectedOption = $(this).find('option:selected');
            var tipoActividadNombre = selectedOption.data('tipo-actividad');

            if (tipoActividadNombre) {
                cargarCamposEspecificos(tipoActividadNombre);
                cargarDatosParaSelects(tipoActividadNombre);
            } else {
                camposEspecificosContainer.empty();
            }
        });

        // Inicialmente limpiar campos específicos
        camposEspecificosContainer.empty();
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


    // Evento para abrir el modal
    $('#btnAddActividad').on('click', function () {
        $('#modalActividadRapida').modal('show');
    });

    // Inicializar Select2 cuando el modal se muestra
    $('#modalActividadRapida').on('shown.bs.modal', function () {
        $('#modalActividadRapidaLabel').html('<i class="ph ph-tractor me-2"></i>Crear Labor');
        $('button[type="submit"]').html('<i class="ph ph-check-circle me-1"></i>Guardar Labor');
        inicializarSelectConIconos();
        inicializarSelectLotes();
        $('#tipoidActividad').prop('disabled', false);
        $('#IdLote').prop('disabled', false);

        $('#tipoidActividad').trigger('change.select2');
    });

    // MODIFICADA: Validación del formulario
    form.on('submit', function (e) {
        e.preventDefault();

        // Validar campos requeridos base
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
            mostrarMensaje('Debe seleccionar al menos un lote', 'error');
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

        // Validar campos específicos según tipo de actividad
        var tipoActividadNombre = tipoActividadSelect.find('option:selected').data('tipo-actividad');
        if (!validarCamposEspecificos(tipoActividadNombre)) {
            return;
        }

        guardarActividad();
    });

    // FUNCIÓN: Validar campos específicos
    function validarCamposEspecificos(tipoActividadNombre) {
        var isValid = true;

        switch (tipoActividadNombre) {
            case 'Siembra':
                //if (!$('#superficieHa').val()) {
                //    $('#superficieHa').addClass('is-invalid');
                //    isValid = false;
                //}
                //if (!$('#densidadSemillaKgHa').val()) {
                //    $('#densidadSemillaKgHa').addClass('is-invalid');
                //    isValid = false;
                //}
                if (!$('#idCultivo').val()) {
                    $('#idCultivo').next('.select2-container').find('.select2-selection').addClass('is-invalid');
                    isValid = false;
                }
                //if (!$('#idMetodoSiembra').val()) {
                //    $('#idMetodoSiembra').next('.select2-container').find('.select2-selection').addClass('is-invalid');
                //    isValid = false;
                //}
                break;

            case 'Cosecha':
                if (!$('#idCultivoCosecha').val()) {
                    $('#idCultivoCosecha').next('.select2-container').find('.select2-selection').addClass('is-invalid');
                    isValid = false;
                }
                break;

            case 'Riego':
                //if (!$('#horasRiego').val()) {
                //    $('#horasRiego').addClass('is-invalid');
                //    isValid = false;
                //}
                //if (!$('#volumenAguaM3').val()) {
                //    $('#volumenAguaM3').addClass('is-invalid');
                //    isValid = false;
                //}
                //if (!$('#idMetodoRiego').val()) {
                //    $('#idMetodoRiego').next('.select2-container').find('.select2-selection').addClass('is-invalid');
                //    isValid = false;
                //}
                break;

        }

        return isValid;
    }

    $('#modalActividadRapida').on('show.bs.modal', function () {
        $('#tipoidActividad').val('').trigger('change');
        camposEspecificosContainer.empty();

    });
    // Resetear formulario cuando se cierra el modal
    $('#modalActividadRapida').on('hidden.bs.modal', function () {
        form[0].reset();
        cantidadInput.prop('disabled', true);
        unidadMedidaText.text('-').addClass('text-muted');
        camposEspecificosContainer.empty();

        // Destruir todos los Select2
        if (tipoActividadSelect.hasClass('select2-hidden-accessible')) {
            tipoActividadSelect.select2('destroy');
        }
        if (loteSelect.hasClass('select2-hidden-accessible')) {
            loteSelect.select2('destroy');
        }

        // Restablecer fecha actual
        $('#fecha').val(new Date().toISOString().slice(0, 10));
    });

    // MODIFICADA: Función guardar actividad
    function guardarActividad() {
        var tipoActividadNombre = tipoActividadSelect.find('option:selected').data('tipo-actividad');
        var dataEspecifica = obtenerDatosEspecificos(tipoActividadNombre);

        var actividadId = $('#actividadId').val();
        var esEdicion = actividadId && actividadId > 0;

        var data = {
            fecha: $('#fecha').val(),
            lotesIds: loteSelect.val() ? loteSelect.val().map(function (id) {
                return parseInt(id);
            }) : [],
            tipoidActividad: parseInt($('#tipoidActividad').val()),
            observacion: $('#observacion').val(),
            tipoActividad: tipoActividadNombre,
            datosEspecificos: dataEspecifica,
            idLabor: esEdicion ? parseInt(actividadId) : null,
            idLote: esEdicion ? parseInt($('#IdLote').val()[0]) : null
        };

        // Mostrar loading en el botón
        var submitBtn = form.find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="ph ph-hourglass me-1"></i>' + (esEdicion ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);
        var url = esEdicion ? '/Actividad/EditarLabor' : '/Actividad/CrearLabor';
        var mensajeExito = esEdicion ? 'Actividad actualizada correctamente' : 'Actividad creada correctamente';

        // Enviar al servidor
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    $('#modalActividadRapida').modal('hide');
                    mostrarExito(mensajeExito);

                    setTimeout(function () {
                        window.location.reload();
                    }, 500);
                } else {
                    mostrarError(result.message || (esEdicion ? 'Error al actualizar actividad' : 'Error al crear actividad'));
                    submitBtn.html(originalText).prop('disabled', false);
                }
            },
            error: function (error) {
                mostrarMensaje('Error al conectar con el servidor', 'error');
                submitBtn.html(originalText).prop('disabled', false);
            }
        });
    }

    // FUNCIÓN: Obtener datos específicos según tipo de actividad
    function obtenerDatosEspecificos(tipoActividadNombre) {
        var datos = {};

        switch (tipoActividadNombre) {
            case 'Siembra':
                datos = {
                    SuperficieHa: parseFloat($('#superficieHa').val()) || 0,
                    DensidadSemillaKgHa: parseFloat($('#densidadSemillaKgHa').val()) || 0,
                    Costo: parseFloat($('#costoSiembra').val()) || 0,
                    IdCultivo: parseInt($('#idCultivo').val()),
                    IdVariedad: $('#idVariedad').val() ? parseInt($('#idVariedad').val()) : null,
                    IdMetodoSiembra: parseInt($('#idMetodoSiembra').val()),
                    EsDolar: $('#switchMonedaCostoSiembra').is(':checked')
                };
                break;
                
            case 'Riego':
                datos = {
                    HorasRiego: parseFloat($('#horasRiego').val()) || 0,
                    VolumenAguaM3: parseFloat($('#volumenAguaM3').val()) || 0,
                    IdMetodoRiego: parseInt($('#idMetodoRiego').val()),
                    IdFuenteAgua: $('#idFuenteAgua').val() ? parseInt($('#idFuenteAgua').val()) : null,
                    Costo: parseFloat($('#costoRiegoTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoRiego').is(':checked')
                };
                break;

            case 'Fertilizado':
                datos = {
                    CantidadKgHa: parseFloat($('#cantidadKgHa').val()) || 0,
                    DosisKgHa: parseFloat($('#dosisKgHa').val()) || 0,
                    Costo: parseFloat($('#costoFertilizado').val()) || 0,
                    IdNutriente: parseInt($('#idNutriente').val()),
                    IdTipoFertilizante: parseInt($('#idTipoFertilizante').val()),
                    IdMetodoAplicacion: parseInt($('#idMetodoAplicacion').val()),
                    EsDolar: $('#switchMonedaCostoFertilizacion').is(':checked')
                }; 
                break;

            case 'Pulverizacion':
                datos = {
                    VolumenLitrosHa: parseFloat($('#volumenLitrosHa').val()) || 0,
                    Dosis: parseFloat($('#dosisPulverizacion').val()) || 0,
                    CondicionesClimaticas: $('#condicionesClimaticas').val() || '',
                    IdProductoAgroquimico: parseInt($('#idProductoAgroquimico').val()),
                    Costo: parseFloat($('#costoPulverizacionTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoPulverizacion').is(':checked')
                };
                break;

            case 'Monitoreo':
                datos = {
                    IdTipoMonitoreo: parseInt($('#idTipoMonitoreo').val()),
                    IdMonitoreo: parseInt($('#idMonitoreo').val()),
                    IdEstadoFenologico: $('#idEstadoFenologico').val() ? parseInt($('#idEstadoFenologico').val()) : null,
                    Costo: parseFloat($('#costoMonitoreoTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoMonitoreo').is(':checked')
                };
                break;

            case 'AnalisisSuelo':
                datos = {
                    ProfundidadCm: $('#profundidadCm').val() ? parseFloat($('#profundidadCm').val()) : null,
                    PH: $('#ph').val() ? parseFloat($('#ph').val()) : null,
                    MateriaOrganica: $('#materiaOrganica').val() ? parseFloat($('#materiaOrganica').val()) : null,
                    Nitrogeno: $('#nitrogeno').val() ? parseFloat($('#nitrogeno').val()) : null,
                    Fosforo: $('#fosforo').val() ? parseFloat($('#fosforo').val()) : null,
                    Potasio: $('#potasio').val() ? parseFloat($('#potasio').val()) : null,
                    ConductividadElectrica: $('#conductividadElectrica').val() ? parseFloat($('#conductividadElectrica').val()) : null,
                    CIC: $('#cic').val() ? parseFloat($('#cic').val()) : null,
                    Textura: $('#textura').val() || '',
                    IdLaboratorio: $('#idLaboratorio').val() ? parseInt($('#idLaboratorio').val()) : null,
                    Costo: parseFloat($('#costoAnalisisSueloTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoAnalisisSuelo').is(':checked')
                };
                break;

            case 'Cosecha':
                datos = {
                    RendimientoTonHa: parseFloat($('#rendimientoTonHa').val()) || 0,
                    HumedadGrano: parseFloat($('#humedadGrano').val()) || 0,
                    SuperficieCosechadaHa: parseFloat($('#superficieCosechadaHa').val()) || 0,
                    IdCultivo: parseInt($('#idCultivoCosecha').val()),
                    Costo: parseFloat($('#costoCosechaTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoCosecha').is(':checked')
                };
                break;

            case 'OtraLabor':
                datos = {
                    Costo: parseFloat($('#costoOtraLaborTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoOtraLabor').is(':checked')
                };
                break;
        }

        return datos;
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