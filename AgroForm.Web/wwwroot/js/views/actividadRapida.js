let loteChoicesInstance = null;
let idCultivoSembrado = null;
let cicloSeleccionadoCultivoId = null;

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

            $('.form-select', camposEspecificosContainer).each(function () {
                const element = this;
                // Los selects de cultivo y variedad se manejan manualmente con cargarCultivos()/cargarVariedades(),
                // no deben tener Choices.js porque interfiere con la recarga de opciones
                if (element.id === 'idCultivo' || element.id === 'idCultivoCosecha' || element.id === 'idVariedad') return;
                if (element._choicesInstance) {
                    element._choicesInstance.destroy();
                }
                const instance = new Choices(element, {
                    searchEnabled: false,
                    placeholder: true,
                    shouldSort: false,
                    placeholderValue: 'Seleccione una opción...',
                    itemSelectText: '',
                    removeItemButton: true
                });
                element._choicesInstance = instance;
            });

            // Si hay un ciclo seleccionado con cultivo, auto-completar en Siembra o Cosecha
            autoCompletarCultivoDesdeCiclo(tipoActividadNombre);
        }
    }

    function cargarSwitchMoneda(idSwitch, idLabel) {
        $("#" + idSwitch).on("change", function () {
            const esUSD = $(this).is(":checked");
            $("#" + idLabel).text(esUSD ? "US$" : "$");
            $(this).next("label").text(esUSD ? "USD" : "ARS");
        });
    }

    // Auto-completar cultivo desde ciclo seleccionado
    function autoCompletarCultivoDesdeCiclo(tipoActividadNombre) {
        if (!cicloSeleccionadoCultivoId) return;

        setTimeout(function () {
            if (tipoActividadNombre === 'Siembra') {
                var $cultivoSelect = $('#idCultivo');
                if ($cultivoSelect.length && $cultivoSelect.find('option[value="' + cicloSeleccionadoCultivoId + '"]').length) {
                    $cultivoSelect.val(cicloSeleccionadoCultivoId).trigger('change');
                    $cultivoSelect.prop('disabled', true);
                    // Mostrar hint de que el cultivo viene del ciclo
                    if ($('#hintCultivoCiclo').length === 0) {
                        $('<small id="hintCultivoCiclo" class="text-success d-block mt-1">' +
                            '<i class="ph ph-seedling me-1"></i>Cultivo del ciclo activo</small>')
                            .insertAfter($cultivoSelect.closest('.mb-3'));
                    }
                }
            } else if (tipoActividadNombre === 'Cosecha') {
                var $cultivoCosechaSelect = $('#idCultivoCosecha');
                if ($cultivoCosechaSelect.length && $cultivoCosechaSelect.find('option[value="' + cicloSeleccionadoCultivoId + '"]').length) {
                    $cultivoCosechaSelect.val(cicloSeleccionadoCultivoId).trigger('change');
                }
            }
        }, 300);
    }

    // FUNCIÓN: Cargar datos para selects específicos
    async function cargarDatosParaSelects(tipoActividadNombre) {
        switch (tipoActividadNombre) {
            case 'Siembra':
                cargarSwitchMoneda("switchMonedaCostoSiembra", "labelMonedaCostoSiembra");
                cargarCultivos('Siembra');
                break;
            case 'Cosecha':
                cargarSwitchMoneda("switchMonedaCostoCosecha", "labelMonedaCostoCosecha");
                cargarCultivos('Cosecha');
                break;
            case 'Riego':
                cargarSwitchMoneda("switchMonedaCostoRiego", "labelMonedaCostoRiego");
                break;
            case 'Fertilizado':
                cargarSwitchMoneda("switchMonedaCostoFertilizado", "labelMonedaCostoFertilizado");
                break;
            case 'Pulverizacion':
                cargarSwitchMoneda("switchMonedaCostoPulverizacion", "labelMonedaCostoPulverizacion");
                break;
            case 'Monitoreo':
                cargarEstadosFenologicos();
                cargarSwitchMoneda("switchMonedaCostoMonitoreo", "labelMonedaCostoMonitoreo");
                break;
            case 'Analisis de suelo':
                cargarSwitchMoneda("switchMonedaCostoAnalisisSuelo", "labelMonedaCostoAnalisisSuelo");
                break;
            case 'Otras labores':
                cargarSwitchMoneda("switchMonedaCostoOtraLabor", "labelMonedaCostoOtraLabor");
                break;
        }
    }

    // FUNCIÓN: Cargar cultivos en todos los selectores de cultivo
    function cargarCultivos(tipoActividadOrigen) {
        $.ajax({
            url: '/Cultivo/GetAll',
            type: 'GET',
            success: function (result) {
                if (result.success && result.listObject) {
                    var selectCultivo = $('#idCultivo, #idCultivoCosecha, #nuevoCicloIdCultivo');
                    selectCultivo.empty();
                    selectCultivo.append($('<option>', {
                        value: '',
                        text: 'Seleccione un cultivo'
                    }));
                    $.each(result.listObject, function (index, cultivo) {
                        selectCultivo.append($('<option>', {
                            value: cultivo.id,
                            text: cultivo.nombre
                        }));
                    });
                    selectCultivo.val('').trigger('change');

                    // Si se cargó por cambio de actividad, auto-completar desde ciclo
                    if (tipoActividadOrigen) {
                        autoCompletarCultivoDesdeCiclo(tipoActividadOrigen);
                    }
                }
            },
            error: function (error) {
                console.error('Error cargando cultivos:', error);
            }
        });
    }

    function cargarTodosCatalogos() {
        $.ajax({
            url: '/Catalogo/GetAllActive',
            type: 'GET',
            success: function (result) {
                if (result.success && result.listObject) {
                    cargarDatosSelect2(result.listObject);
                }
            },
            error: function (error) {
                console.error('Error cargando catálogos:', error);
            }
        });
    }

    async function cargarDatosSelect2(todosCatalogos) {
        if (!todosCatalogos) return;
        cargarCultivos();
        llenarSelectConCatalogo(30, 'MetodoSiembra', todosCatalogos);
        cargarSwitchMoneda("switchMonedaCostoCosecha", "labelMonedaCostoCosecha");
        llenarSelectConCatalogo(31, 'MetodoRiego', todosCatalogos);
        llenarSelectConCatalogo(41, 'FuenteAgua', todosCatalogos);
        llenarSelectConCatalogo(21, 'Nutriente', todosCatalogos);
        llenarSelectConCatalogo(20, 'TipoFertilizante', todosCatalogos);
        llenarSelectConCatalogo(32, 'MetodoAplicacion', todosCatalogos);
        llenarSelectConCatalogo(22, 'ProductoAgroquimico', todosCatalogos);
        cargarEstadosFenologicos();
        llenarSelectConCatalogo(50, 'Laboratorio', todosCatalogos);
    }

    function llenarSelectConCatalogo(tipoCatalogo, idSelect2, todosCatalogos) {
        if (!todosCatalogos) return;
        const catalogosFiltrados = todosCatalogos.filter(catalogo => catalogo.tipo === tipoCatalogo);
        var selectId = 'id' + idSelect2;
        var $select = $('#' + selectId);
        $select.empty();
        $select.append($('<option>', { value: '', text: 'Seleccione una opción' }));
        $.each(catalogosFiltrados, function (index, catalogo) {
            $select.append($('<option>', {
                value: catalogo.id,
                text: catalogo.nombre
            }));
        });
        $select.val('').trigger('change');
    }

    // FUNCIÓN: Cargar estados fenológicos basados en el cultivo del ciclo activo
    function cargarEstadosFenologicos() {
        var cultivoId = cicloSeleccionadoCultivoId;
        if (cultivoId === null) return;

        $.ajax({
            url: '/EstadoFenologico/GetByCultivo/' + cultivoId,
            type: 'GET',
            success: function (result) {
                var selectElement = document.getElementById("idEstadoFenologico");
                if (selectElement && selectElement._choicesInstance) {
                    selectElement._choicesInstance.clearChoices();
                    selectElement._choicesInstance.removeActiveItems();
                    var choicesArray = result.listObject.map(function (catalogo) {
                        return {
                            value: catalogo.id.toString(),
                            label: catalogo.nombre + " (" + catalogo.codigo + ")",
                            selected: false,
                            disabled: false
                        };
                    });
                    try {
                        selectElement._choicesInstance.setChoices(choicesArray, 'value', 'label', false);
                        selectElement._choicesInstance.removeActiveItems();
                    } catch (error) {
                        console.error('Error actualizando Choices:', error);
                    }
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
        }
    });

    $(document).on('change', '#idMonitoreo', function () {
        var idTipoCatalogo = $(this).val();
        if (idTipoCatalogo) {
            cargarCatalogos(idTipoCatalogo, 'idTipoMonitoreo');
        } else {
            $('#idTipoMonitoreo').empty().val(null).trigger('change');
        }
    });

    function cargarCatalogos(tipoCatalogo, idSelect) {
        $.ajax({
            url: '/Catalogo/GetByTipo',
            type: 'GET',
            data: { tipo: tipoCatalogo },
            success: function (result) {
                if (result.success && result.listObject) {
                    var selectElement = document.getElementById(idSelect);
                    if (selectElement && selectElement._choicesInstance) {
                        selectElement._choicesInstance.clearChoices();
                        selectElement._choicesInstance.removeActiveItems();
                        var choicesArray = result.listObject.map(function (catalogo) {
                            return {
                                value: catalogo.id.toString(),
                                label: catalogo.nombre,
                                selected: false,
                                disabled: false
                            };
                        });
                        try {
                            selectElement._choicesInstance.setChoices(choicesArray, 'value', 'label', false);
                            selectElement._choicesInstance.removeActiveItems();
                        } catch (error) {
                            console.error('Error actualizando Choices:', error);
                        }
                    }
                }
            },
            error: function (error) {
                console.error('Error cargando catálogos:', error);
            }
        });
    }

    // Inicializar Select2 para tipo de actividad
    function inicializarSelectConIconos() {
        var tipoActividadSelect = $('#tipoidActividad');
        if (!tipoActividadSelect.hasClass('select2-hidden-accessible')) {
            tipoActividadSelect.select2({
                minimumResultsForSearch: Infinity,
                dropdownParent: $('#modalActividadRapida'),
                templateResult: function (option) {
                    if (!option.id) return option.text;
                    var icono = $(option.element).data('icono');
                    var iconoColor = $(option.element).data('icono-color') || '#000';
                    if (icono) {
                        var $span = $('<span></span>');
                        $span.append($('<i></i>', { class: 'ph ' + icono + ' me-2', style: 'color: ' + iconoColor }));
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
                        $span.append($('<i></i>', { class: 'ph ' + icono + ' me-2', style: 'color: ' + iconoColor }));
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

            tipoActividadSelect.off('change.selectActividad').on('change.selectActividad', function () {
                var selectedOption = $(this).find('option:selected');
                var tipoActividadNombre = selectedOption.data('tipo-actividad');
                if (tipoActividadNombre) {
                    cargarCamposEspecificos(tipoActividadNombre);
                    cargarDatosParaSelects(tipoActividadNombre);
                    $('#observacionesContainer').removeClass('d-none');
                } else {
                    camposEspecificosContainer.empty();
                    $('#observacionesContainer').addClass('d-none');
                }
            });
        }
        tipoActividadSelect.show();
    }

    function inicializarSelectLotes() {
        if (loteChoicesInstance) {
            loteChoicesInstance.destroy();
        }
        const loteSelectElement = document.getElementById('IdLote');
        loteChoicesInstance = new Choices(loteSelectElement, {
            searchEnabled: false,
            placeholder: true,
            placeholderValue: 'Seleccione un lote...',
            removeItemButton: true,
            shouldSort: false,
            searchPlaceholder: 'Buscar lote...',
            itemSelectText: '',
            allowHTML: false,
            position: 'auto',
            renderSelectedChoices: 'always',
            callbackOnInit: function () {
                this.removeActiveItems();
                this.setChoiceByValue('');
                camposEspecificosContainer.empty();
            }
        });

        loteSelectElement.addEventListener('change', function () { handleLoteChange(); }, false);
        loteSelectElement.addEventListener('choice', function () { handleLoteChange(); }, false);
        loteChoicesInstance.enable();

        function handleLoteChange() {
            camposEspecificosContainer.empty();
            cicloSeleccionadoCultivoId = null;

            const selectedValue = loteChoicesInstance.getValue(true);
            const selectElement = document.getElementById('IdLote');
            const selectedOption = selectElement.options[selectElement.selectedIndex];

            if (selectedValue) {
                cargarCiclosPorLote(parseInt(selectedValue));
            } else {
                var cicloSelect = $('#idCicloCultivo');
                cicloSelect.empty().append($('<option>', { value: '', text: 'Seleccione un ciclo...' }));
                actualizarBotonesCiclo(false);
            }

            if (!selectedOption) {
                $('#tipoidActividad').prop('disabled', true);
                $('#idCicloCultivo').prop('disabled', true);
                $('#btnNuevoCiclo').prop('disabled', true);
                $('#btnCerrarCiclo').prop('disabled', true);
                return;
            }

            // Obtener superficie disponible para sembrar
            const superficieParaSembrar = parseFloat(selectedOption.getAttribute('data-superficie-para-sembrar'));
            const siembraACosechar = selectedOption.getAttribute('data-siembra-a-cosechar');
            const superficieSembrada = parseFloat(selectedOption.getAttribute('data-superficie-sembrada'));

            // Mostrar info de superficie sembrada (si aplica)
            if (siembraACosechar && superficieSembrada > 0) {
                $('#info-cosecha').text(`Sembrado: ${siembraACosechar} ${superficieSembrada} Ha.`);
            } else {
                $('#info-cosecha').text("");
            }

            // Configurar límite de superficie para siembra
            if (superficieParaSembrar > 0) {
                const inputSuperficieMaxima = $('#superficieHa');
                inputSuperficieMaxima.attr('max', superficieParaSembrar);
                inputSuperficieMaxima.attr('placeholder', `Máximo: ${superficieParaSembrar} ha`);
                inputSuperficieMaxima.attr('title', `Superficie máxima permitida: ${superficieParaSembrar} ha`);
            }

            // Habilitar selects de actividad y ciclo
            $('#tipoidActividad').prop('disabled', false);
            $('#idCicloCultivo').prop('disabled', false);
            $('#btnNuevoCiclo').prop('disabled', false);
            $('#btnCerrarCiclo').prop('disabled', false);
            $(selectElement).trigger('change');
        }
    }

    // Evento para abrir el modal
    $('#btnAddActividad').on('click', function () {
        $('#modalActividadRapida').modal('show');
    });

    // Inicializar cuando el modal se muestra
    $('#modalActividadRapida').on('shown.bs.modal', function () {
        $('#modalActividadRapidaLabel').html('<i class="ph ph-tractor me-2"></i>Crear Labor');
        $('#btnGuardarLabor').html('<i class="ph ph-check-circle me-1"></i>Guardar Labor');
        inicializarSelectConIconos();
        inicializarSelectLotes();
        cargarTodosCatalogos();
        $('#tipoidActividad').prop('disabled', true);
        $('#idCicloCultivo').prop('disabled', true);
        $('#btnNuevoCiclo').prop('disabled', true);
        $('#btnCerrarCiclo').prop('disabled', true);
        $('#IdLote').prop('disabled', false);
        $('#observacionesContainer').addClass('d-none');
        $('#tipoidActividad').trigger('change.select2');
    });

    // Validación del formulario
    form.on('submit', function (e) {
        e.preventDefault();

        var fechaVal = $('#fecha').val();
        var lotesVal = loteSelect.val();

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

        var tipoActividadNombre = tipoActividadSelect.find('option:selected').data('tipo-actividad');
        if (!validarCamposEspecificos(tipoActividadNombre)) {
            return;
        }

        guardarActividad();
    });

    // Validar campos específicos
    function validarCamposEspecificos(tipoActividadNombre) {
        var isValid = true;
        var errorMessage = '';

        switch (tipoActividadNombre) {
            case 'Siembra':
                if (!$('#idCultivo').val()) {
                    errorMessage = 'Debe seleccionar un cultivo';
                    isValid = false;
                }
                if (!$('#idVariedad').val()) {
                    errorMessage = 'Debe seleccionar una variedad de cultivo';
                    isValid = false;
                }
                var superficieMaxString = $('#superficieHa').attr('max');
                var superficieValString = $('#superficieHa').val();
                if (superficieMaxString && superficieValString) {
                    var superficieMax = parseFloat(superficieMaxString);
                    var superficieVal = parseFloat(superficieValString);
                    if (superficieVal > superficieMax) {
                        $('#superficieHa').addClass('is-invalid');
                        mostrarMensaje(`La superficie no puede superar ${superficieMax}`, 'error');
                        return;
                    } else {
                        $('#superficieHa').removeClass('is-invalid');
                    }
                }
                break;

            case 'Cosecha':
                if (!$('#idCultivoCosecha').val()) {
                    errorMessage = 'Debe seleccionar un cultivo';
                    isValid = false;
                }
                var superficieMaxString = $('#superficieCosechadaHa').attr('max');
                var superficieValString = $('#superficieCosechadaHa').val();
                if (superficieMaxString && superficieValString) {
                    var superficieMax = parseFloat(superficieMaxString);
                    var superficieVal = parseFloat(superficieValString);
                    if (superficieVal > superficieMax) {
                        $('#superficieCosechadaHa').addClass('is-invalid');
                        mostrarMensaje(`La superficie no puede superar ${superficieMax}`, 'error');
                        return;
                    } else {
                        $('#superficieCosechadaHa').removeClass('is-invalid');
                    }
                }
                break;

            case 'Monitoreo':
                if (!$('#idTipoMonitoreo').val()) {
                    errorMessage = 'Debe seleccionar un tipo de monitoreo';
                    isValid = false;
                }
                break;
        }
        if (!isValid)
            mostrarMensaje(errorMessage);
        return isValid;
    }

    $('#modalActividadRapida').on('show.bs.modal', function () {
        $('#tipoidActividad').val('').trigger('change');
        camposEspecificosContainer.empty();
        $('#nuevoCicloInline').addClass('d-none');
        $('#observacionesContainer').addClass('d-none');
    });

    $('#modalActividadRapida').on('hidden.bs.modal', function () {
        $('#actividadId').val(null);
        form[0].reset();
        cantidadInput.prop('disabled', true);
        unidadMedidaText.text('-').addClass('text-muted');
        camposEspecificosContainer.empty();
        cicloSeleccionadoCultivoId = null;
        $('#observacionesContainer').addClass('d-none');

        if (tipoActividadSelect.hasClass('select2-hidden-accessible')) {
            tipoActividadSelect.select2('destroy');
        }
        if (loteSelect.hasClass('select2-hidden-accessible')) {
            loteSelect.select2('destroy');
        }
        $('#fecha').val(new Date().toISOString().slice(0, 10));
    });

    // Guardar actividad
    function guardarActividad() {
        var tipoActividadNombre = tipoActividadSelect.find('option:selected').data('tipo-actividad');
        var idTipoActividadNombre = tipoActividadSelect.find('option:selected').data('id-tipo-actividad');
        var dataEspecifica = obtenerDatosEspecificos(idTipoActividadNombre);

        var actividadId = $('#actividadId').val();
        var esEdicion = actividadId && actividadId > 0;
        const loteId = parseInt(loteSelect.val());
        const loteArray = [loteId];
        var idCiclo = parseInt($('#idCicloCultivo').val());

        var data = {
            fecha: $('#fecha').val(),
            lotesIds: loteArray,
            tipoidActividad: idTipoActividadNombre,
            observacion: $('#observacion').val(),
            tipoActividad: tipoActividadNombre,
            datosEspecificos: dataEspecifica,
            idLabor: esEdicion ? parseInt(actividadId) : null,
            idLote: esEdicion ? parseInt($('#IdLote').val()[0]) : null,
            idCicloCultivo: idCiclo > 0 ? idCiclo : null
        };

        var submitBtn = form.find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="ph ph-hourglass me-1"></i>' + (esEdicion ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);
        var url = esEdicion ? '/Actividad/EditarLabor' : '/Actividad/CrearLabor';
        var mensajeExito = esEdicion ? 'Actividad actualizada correctamente' : 'Actividad creada correctamente';

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
                    setTimeout(function () { window.location.reload(); }, 500);
                } else {
                    mostrarError(result.message || (esEdicion ? 'Error al actualizar actividad' : 'Error al crear actividad'));
                    submitBtn.html(originalText).prop('disabled', false);
                }
            },
            error: function () {
                mostrarMensaje('Error al conectar con el servidor', 'error');
                submitBtn.html(originalText).prop('disabled', false);
            }
        });
    }

    // Obtener datos específicos según tipo de actividad
    function obtenerDatosEspecificos(tipoActividadNombre) {
        var datos = {};
        switch (tipoActividadNombre) {
            case 2: // Siembra
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
            case 5: // Riego
                datos = {
                    HorasRiego: parseFloat($('#horasRiego').val()) || 0,
                    VolumenAguaM3: parseFloat($('#volumenAguaM3').val()) || 0,
                    IdMetodoRiego: parseInt($('#idMetodoRiego').val()),
                    IdFuenteAgua: $('#idFuenteAgua').val() ? parseInt($('#idFuenteAgua').val()) : null,
                    Costo: parseFloat($('#costoRiegoTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoRiego').is(':checked')
                };
                break;
            case 4: // Fertilizado
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
            case 3: // Pulverizacion
                datos = {
                    VolumenLitrosHa: parseFloat($('#volumenLitrosHa').val()) || 0,
                    Dosis: parseFloat($('#dosisPulverizacion').val()) || 0,
                    CondicionesClimaticas: $('#condicionesClimaticas').val() || '',
                    IdProductoAgroquimico: parseInt($('#idProductoAgroquimico').val()),
                    Costo: parseFloat($('#costoPulverizacionTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoPulverizacion').is(':checked')
                };
                break;
            case 6: // Monitoreo
                datos = {
                    IdTipoMonitoreo: parseInt($('#idTipoMonitoreo').val()),
                    IdMonitoreo: parseInt($('#idMonitoreo').val()),
                    IdEstadoFenologico: $('#idEstadoFenologico').val() ? parseInt($('#idEstadoFenologico').val()) : null,
                    Costo: parseFloat($('#costoMonitoreoTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoMonitoreo').is(':checked')
                };
                break;
            case 1: // AnalisisSuelo
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
            case 7: // Cosecha
                datos = {
                    RendimientoTonHa: parseFloat($('#rendimientoTonHa').val()) || 0,
                    HumedadGrano: parseFloat($('#humedadGrano').val()) || 0,
                    SuperficieCosechadaHa: parseFloat($('#superficieCosechadaHa').val()) || 0,
                    IdCultivo: parseInt($('#idCultivoCosecha').val()),
                    Costo: parseFloat($('#costoCosechaTotal').val()) || 0,
                    EsDolar: $('#switchMonedaCostoCosecha').is(':checked')
                };
                break;
            case 8: // OtraLabor
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

    // CÓDIGO EXISTENTE PARA GASTOS
    $('#btnGasto').on('click', function () {
        $('#modalGasto').modal('show');
        $('#modalGastoLabel').html('<i class="ph ph-receipt me-2"></i>Crear Gasto');
        $('#btnGuardarLabor').html('<i class="ph ph-check-circle me-1"></i>Guardar');
        cargarSwitchMoneda("switchMonedaCostoGasto", "labelMonedaCostoGasto");
    });

    $('#formGasto').on('submit', function (e) {
        e.preventDefault();
        var gastoId = $('#gastoId').val();
        var esEdicion = gastoId && gastoId > 0;
        var data = {
            id: esEdicion ? parseInt(gastoId) : 0,
            tipoGasto: parseInt($('#tipoGasto').val()),
            fecha: $('#fechaGasto').val(),
            observacion: $('#observacionesGasto').val(),
            costo: parseFloat($('#costoGasto').val()) || 0,
            esDolar: $('#switchMonedaCostoGasto').is(':checked')
        };
        $('#gastoId').val(null);
        var submitBtn = $('#formGasto').find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' + (esEdicion ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);
        var url = esEdicion ? '/Gasto/Update' : '/Gasto/Create';
        var mensajeExito = esEdicion ? 'Gasto actualizado correctamente' : 'Gasto creado correctamente';
        $.ajax({
            url: url,
            type: esEdicion ? 'PUT' : 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                if (result.success) {
                    mostrarMensaje(mensajeExito, 'success');
                    $('#modalGasto').modal('hide');
                    setTimeout(function () { window.location.reload(); }, 500);
                } else {
                    mostrarMensaje(result.message || 'Error al guardar gasto', 'error');
                }
            },
            error: function () { mostrarMensaje('Error al conectar con el servidor', 'error'); },
            complete: function () { submitBtn.html(originalText).prop('disabled', false); }
        });
    });

    $('#modalGasto').on('hidden.bs.modal', function () { $('#formGasto')[0].reset(); });

    // --- GESTIÓN DE CICLOS ---

    // Botón: Nuevo ciclo inline (sin modal)
    $('#btnNuevoCiclo').on('click', function () {
        var loteId = parseInt($('#IdLote').val());
        if (!loteId) {
            mostrarMensaje('Debe seleccionar un lote primero', 'error');
            return;
        }
        // Ocultar el botón + y mostrar el inline
        $(this).hide();
        $('#nuevoCicloInline').removeClass('d-none');
        // Cargar cultivos en el select inline si no tiene opciones
        var $selectCultivo = $('#nuevoCicloIdCultivo');
        if ($selectCultivo.find('option').length <= 1) {
            cargarCultivos();
        }
    });

    // Cargar variedades cuando cambia el cultivo en el inline
    $(document).on('change', '#nuevoCicloIdCultivo', function () {
        var cultivoId = parseInt($(this).val());
        if (cultivoId) {
            cargarVariedadesInline(cultivoId);
        } else {
            $('#nuevoCicloIdVariedad').empty().append($('<option>', { value: '', text: 'Sin variedad...' }));
        }
    });

    function cargarVariedadesInline(idCultivo) {
        var selectVariedad = $('#nuevoCicloIdVariedad');
        selectVariedad.empty().append($('<option>', { value: '', text: 'Sin variedad...' }));
        $.ajax({
            url: '/Variedad/GetByCultivo?idCultivo=' + idCultivo,
            type: 'GET',
            success: function (result) {
                if (result.success && result.listObject) {
                    $.each(result.listObject, function (i, v) {
                        selectVariedad.append($('<option>', {
                            value: v.id,
                            text: v.nombre
                        }));
                    });
                }
            }
        });
    }

    // Botón: Guardar ciclo inline
    $('#btnGuardarNuevoCicloInline').on('click', function () {
        var loteId = parseInt($('#IdLote').val());
        var cultivoId = parseInt($('#nuevoCicloIdCultivo').val());
        if (!cultivoId) {
            mostrarMensaje('Debe seleccionar un cultivo', 'error');
            return;
        }
        var data = {
            idLote: loteId,
            idCultivo: cultivoId,
            idVariedad: parseInt($('#nuevoCicloIdVariedad').val()) || null,
            epoca: parseInt($('#nuevoCicloEpoca').val()) || null
        };

        var btn = $(this);
        var originalText = btn.html();
        btn.html('<span class="spinner-border spinner-border-sm me-1"></span>Creando...').prop('disabled', true);

        $.ajax({
            url: '/CicloCultivo/Crear',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            success: function (result) {
                if (result.success && result.object) {
                    mostrarExito('Ciclo creado correctamente');
                    // Ocultar inline y mostrar botón +
                    $('#nuevoCicloInline').addClass('d-none');
                    $('#btnNuevoCiclo').show();
                    // Recargar ciclos y seleccionar el nuevo
                    cargarCiclosPorLote(loteId, result.object.id);
                } else {
                    mostrarError(result.message || 'Error al crear ciclo');
                    btn.html(originalText).prop('disabled', false);
                }
            },
            error: function () {
                mostrarError('Error al conectar con el servidor');
                btn.html(originalText).prop('disabled', false);
            }
        });
    });

    // Botón: Cerrar ciclo
    $('#btnCerrarCiclo').on('click', function () {
        var cicloId = parseInt($('#idCicloCultivo').val());
        if (!cicloId) {
            mostrarMensaje('Debe seleccionar un ciclo activo para cerrar', 'error');
            return;
        }
        mostrarConfirmacion('¿Está seguro de cerrar este ciclo de cultivo?', 'Cerrar Ciclo')
            .then(function (result) {
                if (result.isConfirmed) {
                    cerrarCiclo(cicloId);
                }
            });
    });

    // Evento: cambio en el selector de ciclo
    $('#idCicloCultivo').on('change', function () {
        var selectedOption = $(this).find('option:selected');
        var idCultivo = parseInt(selectedOption.data('id-cultivo'));
        var esActivo = selectedOption.data('activo') === true || selectedOption.data('activo') === 'true';
        var tieneCicloValido = $(this).val() && $(this).val() !== '';

        cicloSeleccionadoCultivoId = idCultivo > 0 ? idCultivo : null;

        // Actualizar badge de ciclo activo
        if (esActivo && idCultivo > 0) {
            $('#cicloActivoBadge').removeClass('d-none');
        } else {
            $('#cicloActivoBadge').addClass('d-none');
        }

        // Actualizar botones
        actualizarBotonesCiclo(tieneCicloValido);

        // Si no hay ciclo seleccionado, deshabilitar selector de labor
        if (!tieneCicloValido) {
            $('#tipoidActividad').prop('disabled', true);
        } else if ($('#IdLote').val()) {
            // Si hay ciclo y hay lote, habilitar labor
            $('#tipoidActividad').prop('disabled', false);
        }

        // Si hay un template visible de Siembra o Cosecha, actualizar el cultivo
        var tipoActividadNombre = tipoActividadSelect.find('option:selected').data('tipo-actividad');
        if (tipoActividadNombre && cicloSeleccionadoCultivoId) {
            if (tipoActividadNombre === 'Siembra') {
                var $cultivoSelect = $('#idCultivo');
                if ($cultivoSelect.length && !$cultivoSelect.prop('disabled')) {
                    $cultivoSelect.val(cicloSeleccionadoCultivoId).trigger('change');
                    $cultivoSelect.prop('disabled', true);
                    // Mostrar hint
                    if ($('#hintCultivoCiclo').length === 0) {
                        $('<small id="hintCultivoCiclo" class="text-success d-block mt-1">' +
                            '<i class="ph ph-seedling me-1"></i>Cultivo del ciclo activo</small>')
                            .insertAfter($cultivoSelect.closest('.mb-3'));
                    }
                }
            } else if (tipoActividadNombre === 'Cosecha') {
                var $cultivoCosechaSelect = $('#idCultivoCosecha');
                if ($cultivoCosechaSelect.length) {
                    $cultivoCosechaSelect.val(cicloSeleccionadoCultivoId).trigger('change');
                }
            } else if (tipoActividadNombre === 'Monitoreo') {
                cargarEstadosFenologicos();
            }
        }
    });
});

// Función para actualizar botones de ciclo según estado
function actualizarBotonesCiclo(hayCicloSeleccionado) {
    if (hayCicloSeleccionado) {
        $('#btnNuevoCiclo').hide();
        $('#btnCerrarCiclo').prop('disabled', false);
    } else {
        $('#btnNuevoCiclo').show();
        $('#btnCerrarCiclo').prop('disabled', true);
    }
}

// FUNCIONES PARA GESTIÓN DE CICLOS DE CULTIVO
function cargarCiclosPorLote(idLote, seleccionarId) {
    $.ajax({
        url: '/CicloCultivo/GetByLote?idLote=' + idLote,
        type: 'GET',
        success: function (result) {
            var select = $('#idCicloCultivo');
            select.empty();
            select.append($('<option>', { value: '', text: 'Seleccione un ciclo...' }));

            var tieneActivo = false;

            if (result.success && result.listObject && result.listObject.length > 0) {
                $.each(result.listObject, function (i, ciclo) {
                    var label = ciclo.cultivoConEpoca;
                    if (ciclo.estaActivo) {
                        label += ' [Activo]';
                        tieneActivo = true;
                    } else {
                        label += ' [Cerrado]';
                    }
                    select.append($('<option>', {
                        value: ciclo.id,
                        text: label,
                        'data-activo': ciclo.estaActivo,
                        'data-id-cultivo': ciclo.idCultivo || 0,
                        disabled: !ciclo.estaActivo
                    }));
                });
            } else {
                select.append($('<option>', {
                    value: '',
                    text: 'Sin ciclos - Use + para crear',
                    disabled: true
                }));
            }

            // Si hay un ID para seleccionar, seleccionarlo
            if (seleccionarId) {
                select.val(seleccionarId).trigger('change');
            } else if (tieneActivo) {
                // Seleccionar el primer ciclo activo
                var optionActivo = select.find('option[data-activo="true"]').first();
                if (optionActivo.length) {
                    select.val(optionActivo.val()).trigger('change');
                } else {
                    select.val('').trigger('change');
                }
            } else {
                select.val('').trigger('change');
            }

            // Actualizar botones según si hay ciclo seleccionado/activo
            var selectedVal = select.val();
            actualizarBotonesCiclo(selectedVal && selectedVal !== '');
        },
        error: function () {
            mostrarMensaje('Error al cargar ciclos de cultivo', 'error');
        }
    });
}

function cerrarCiclo(idCicloCultivo) {
    var data = { idCicloCultivo: idCicloCultivo };
    $.ajax({
        url: '/CicloCultivo/Cerrar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (result) {
            if (result.success) {
                mostrarExito('Ciclo cerrado correctamente');
                var loteId = parseInt($('#IdLote').val());
                if (loteId) {
                    cargarCiclosPorLote(loteId);
                }
            } else {
                mostrarError(result.message || 'Error al cerrar ciclo');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function cargarVariedades(idCultivo) {
    var selectVariedad = $('#nuevoCicloIdVariedad, #idVariedad');
    selectVariedad.empty().append($('<option>', { value: '', text: 'Sin variedad...' }));
    $.ajax({
        url: '/Variedad/GetByCultivo?idCultivo=' + idCultivo,
        type: 'GET',
        success: function (result) {
            if (result.success && result.listObject) {
                $.each(result.listObject, function (i, v) {
                    selectVariedad.append($('<option>', {
                        value: v.id,
                        text: v.nombre
                    }));
                });
            }
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
    $('#modalClimaLabel').html('<i class="ph ph-cloud-rain me-2"></i>Crear Registro de Clima');
    $('#btnGuardarClima').html('<i class="ph ph-check-circle me-1"></i>Guardar');
});

$('#tipoClima').on('change', function () {
    var tipo = $(this).val();
    if (tipo == "1") {
        $('#milimetros').val('').prop('disabled', true);
    } else {
        $('#milimetros').prop('disabled', false);
    }
});

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
    submitBtn.html('<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>' + (esEdicion ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);
    var url = esEdicion ? '/RegistroClima/Update' : '/RegistroClima/Create';
    var mensajeExito = esEdicion ? 'Registro de Clima actualizado correctamente' : 'Registro de Clima creado correctamente';
    $.ajax({
        url: url,
        type: esEdicion ? 'PUT' : 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
        success: function (result) {
            if (result.success) {
                mostrarMensaje(mensajeExito, 'success');
                $('#modalClima').modal('hide');
                setTimeout(function () { window.location.reload(); }, 500);
            } else {
                mostrarMensaje(result.message || 'Error al guardar el registro de clima', 'error');
            }
        },
        error: function () { mostrarMensaje('Error al conectar con el servidor', 'error'); },
        complete: function () { submitBtn.html(originalText).prop('disabled', false); }
    });
});

$('#modalClima').on('hidden.bs.modal', function () {
    $('#formClima')[0].reset();
    $('#campoClima').removeClass('is-invalid');
});

