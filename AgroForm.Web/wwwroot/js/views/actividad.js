$(document).ready(function () {

    inicializarDataTable();
    configurarEventosGrilla();
});

function inicializarDataTable() {
    var table = $('#tblActividades').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/2.2.2/i18n/es-ES.json'
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

    $('#tblActividades tbody').off('click.actividades').on('click.actividades', '.btn-edit', function () {
        mostrarLoading();
        var id = $(this).data('id');
        var idTipoActividad = $(this).data('idtipoactividad');
        cargarActividadParaEditar(id, idTipoActividad);

    });

    $('#tblActividades tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        var idTipoActividad = $(this).data('idtipoactividad');
        eliminarActividad(id, idTipoActividad);
    });
}

function eliminarActividad(id, idTipoActividad) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar esta labor?',
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

// Función para cargar los datos de la actividad a editar
function cargarActividadParaEditar(id, idTipoActividad) {
    $.ajax({
        url: '/Actividad/GetBy?id=' + id + '&idTipoActividad=' + idTipoActividad,
        type: 'GET',
        success: function (result) {
            if (result.success) {
                abrirModalParaEdicionActividad(result.object, result.object.tipoActividad.nombre);
            } else {
                mostrarMensaje('Error al cargar la actividad: ' + result.message, 'error');
            }
        },
        error: function (error) {
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}

// Función para abrir el modal y cargar los datos
function abrirModalParaEdicionActividad(actividad, tipoActividadNombre) {
    // Abrir el modal
    $('#modalActividadRapida').modal('show');

    mostrarLoading();
    setTimeout(() => {
        configurarModoEdicionActividad(actividad, tipoActividadNombre);
        cerrarAlertas();

    }, 1000);
}

// Función para configurar el modal en modo edición
function configurarModoEdicionActividad(actividad, tipoActividadNombre) {
    $('#modalActividadRapidaLabel').html('<i class="ph ph-pencil me-2"></i>Editar Labor');

    $('#fecha').val(actividad.fecha.split('T')[0]); // Formatear fecha
    $('#observacion').val(actividad.observacion || '');

    if (actividad.idLote) {
        loteChoicesInstance.setChoiceByValue(actividad.idLote.toString());
    }
    loteChoicesInstance.disable()

    $('#tipoidActividad').val(actividad.idTipoActividad).trigger('change');

    $('#tipoidActividad').prop('disabled', true);

    cargarDatosEspecificosEditar(actividad, actividad.idTipoActividad);

    $('#btnGuardarLabor').html('<i class="ph ph-check-circle me-1"></i>Actualizar Labor');

    if (!$('#actividadId').length) {
        $('<input>').attr({
            type: 'hidden',
            id: 'actividadId',
            name: 'actividadId'
        }).val(actividad.id).appendTo('#formActividadRapida');
    } else {
        $('#actividadId').val(actividad.id);
    }

    // Cargar los ciclos del lote y seleccionar el ciclo asociado a esta labor
    if (actividad.idLote) {
        setTimeout(function () {
            cargarCiclosPorLote(actividad.idLote, actividad.idCicloCultivo);
        }, 500);
    }
}

// Función para cargar datos específicos según el tipo de actividad
function cargarDatosEspecificosEditar(datosEspecificos, tipoActividadNombre) {

    switch (tipoActividadNombre) {
        case 2:
            if (datosEspecificos.superficieHa != null) $('#superficieHa').val(datosEspecificos.superficieHa);
            if (datosEspecificos.densidadSemillaKgHa != null) $('#densidadSemillaKgHa').val(datosEspecificos.densidadSemillaKgHa);
            if (datosEspecificos.costo != null) $('#costoSiembra').val(datosEspecificos.costo);
            if (datosEspecificos.idMetodoSiembra != null) setSelectWhenReady('#idMetodoSiembra', datosEspecificos.idMetodoSiembra);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoSiembra').prop('checked', !!datosEspecificos.esDolar).trigger('change');

            if (datosEspecificos.idCultivo != null) setSelectWhenReady('#idCultivo', datosEspecificos.idCultivo);
            $('#idCultivo').trigger('change');

            break;

        case 5:
            if (datosEspecificos.horasRiego != null) $('#horasRiego').val(datosEspecificos.horasRiego);
            // Entity returns VolumenAgua, display in volumenAguaM3 input
            if (datosEspecificos.volumenAgua != null) $('#volumenAguaM3').val(datosEspecificos.volumenAgua);
            if (datosEspecificos.costo != null) $('#costoRiegoTotal').val(datosEspecificos.costo);
            if (datosEspecificos.idMetodoRiego != null) setSelectWhenReady('#idMetodoRiego', datosEspecificos.idMetodoRiego);
            if (datosEspecificos.idFuenteAgua != null) setSelectWhenReady('#idFuenteAgua', datosEspecificos.idFuenteAgua);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoRiego').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            // Unidad selector for el volumen de agua
            if (datosEspecificos.idUnidadVolumenAgua != null) {
                setUnidadSelectorValue('volumenAguaM3', datosEspecificos.idUnidadVolumenAgua);
            }
            break;

        case 4:
            // Entity returns Cantidad (not CantidadKgHa), Dosis (not DosisKgHa)
            if (datosEspecificos.cantidad != null) $('#cantidadKgHa').val(datosEspecificos.cantidad);
            if (datosEspecificos.dosis != null) $('#dosisKgHa').val(datosEspecificos.dosis);
            if (datosEspecificos.costo != null) $('#costoFertilizado').val(datosEspecificos.costo);
            if (datosEspecificos.idNutriente != null) setSelectWhenReady('idNutriente', datosEspecificos.idNutriente);
            if (datosEspecificos.idTipoFertilizante != null) setSelectWhenReady('idTipoFertilizante', datosEspecificos.idTipoFertilizante);
            if (datosEspecificos.idMetodoAplicacion != null) setSelectWhenReady('idMetodoAplicacion', datosEspecificos.idMetodoAplicacion);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoFertilizacion').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            // Unidad selectors for cantidad and dosis
            if (datosEspecificos.idUnidadCantidad != null) {
                setUnidadSelectorValue('cantidadKgHa', datosEspecificos.idUnidadCantidad);
            }
            if (datosEspecificos.idUnidadDosis != null) {
                setUnidadSelectorValue('dosisKgHa', datosEspecificos.idUnidadDosis);
            }
            break;

        case 3:
            if (datosEspecificos.volumen != null) $('#volumenLitrosHa').val(datosEspecificos.volumen);
            if (datosEspecificos.dosis != null) $('#dosisPulverizacion').val(datosEspecificos.dosis);
            if (datosEspecificos.condicionesClimaticas != null) $('#condicionesClimaticas').val(datosEspecificos.condicionesClimaticas);
            if (datosEspecificos.idProductoAgroquimico != null) setSelectWhenReady('#idProductoAgroquimico', datosEspecificos.idProductoAgroquimico);
            if (datosEspecificos.costo != null) $('#costoPulverizacionTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoPulverizacion').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 6:
            // Set monitoreo type using native val()+change to trigger AJAX load of idTipoMonitoreo options
            if (datosEspecificos.idMonitoreo != null) {
                var $monSel = $('#idMonitoreo');
                $monSel.val(datosEspecificos.idMonitoreo);
                // Dispatch native change event so cargarCatalogos loads idTipoMonitoreo options
                $monSel[0].dispatchEvent(new Event('change'));
            }

            // Poll until idTipoMonitoreo options are loaded (cargarCatalogos AJAX is async)
            if (datosEspecificos.idTipoMonitoreo != null) {
                (function(tipoValue) {
                    var retryTipo = setInterval(function() {
                        var el = document.getElementById('idTipoMonitoreo');
                        if (el && el._choicesInstance) {
                            var activeItems = el._choicesInstance._store ? el._choicesInstance._store.activeItems : null;
                            if (activeItems && activeItems.length > 0) {
                                el._choicesInstance.removeActiveItems();
                                el._choicesInstance.setChoiceByValue(String(tipoValue));
                                clearInterval(retryTipo);
                            }
                        }
                    }, 100);
                    setTimeout(function() { clearInterval(retryTipo); }, 8000);
                })(datosEspecificos.idTipoMonitoreo);
            }

            // Poll until idEstadoFenologico options are loaded (cargarEstadosFenologicos AJAX is async, depends on ciclo load)
            if (datosEspecificos.idEstadoFenologico != null) {
                (function(efValue) {
                    var retryEF = setInterval(function() {
                        var el = document.getElementById('idEstadoFenologico');
                        if (el && el._choicesInstance) {
                            var activeItems = el._choicesInstance._store ? el._choicesInstance._store.activeItems : null;
                            if (activeItems && activeItems.length > 0) {
                                el._choicesInstance.removeActiveItems();
                                el._choicesInstance.setChoiceByValue(String(efValue));
                                clearInterval(retryEF);
                            }
                        }
                    }, 100);
                    setTimeout(function() { clearInterval(retryEF); }, 8000);
                })(datosEspecificos.idEstadoFenologico);
            }

            if (datosEspecificos.costo != null) $('#costoMonitoreoTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoMonitoreo').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 1:
            if (datosEspecificos.profundidadCm != null) $('#profundidadCm').val(datosEspecificos.profundidadCm);
            if (datosEspecificos.ph != null) $('#ph').val(datosEspecificos.ph);
            if (datosEspecificos.materiaOrganica != null) $('#materiaOrganica').val(datosEspecificos.materiaOrganica);
            if (datosEspecificos.nitrogeno != null) $('#nitrogeno').val(datosEspecificos.nitrogeno);
            if (datosEspecificos.fosforo != null) $('#fosforo').val(datosEspecificos.fosforo);
            if (datosEspecificos.potasio != null) $('#potasio').val(datosEspecificos.potasio);
            if (datosEspecificos.conductividadElectrica != null) $('#conductividadElectrica').val(datosEspecificos.conductividadElectrica);
            if (datosEspecificos.cic != null) $('#cic').val(datosEspecificos.cic);
            if (datosEspecificos.textura != null) $('#textura').val(datosEspecificos.textura).trigger('change');
            if (datosEspecificos.idLaboratorio != null) setSelectWhenReady('#idLaboratorio', datosEspecificos.idLaboratorio);
            if (datosEspecificos.costo != null) $('#costoAnalisisSueloTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoAnalisisSuelo').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 7:
            // Entity returns Rendimiento (not RendimientoTonHa), SuperficieCosechada (not SuperficieCosechadaHa)
            if (datosEspecificos.rendimiento != null) $('#rendimientoTonHa').val(datosEspecificos.rendimiento);
            if (datosEspecificos.humedadGrano != null) $('#humedadGrano').val(datosEspecificos.humedadGrano);
            if (datosEspecificos.superficieCosechada != null) $('#superficieCosechadaHa').val(datosEspecificos.superficieCosechada);
            if (datosEspecificos.costo != null) $('#costoCosechaTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoCosecha').prop('checked', !!datosEspecificos.esDolar).trigger('change');

            if (datosEspecificos.idCultivo != null) setSelectWhenReady('#idCultivoCosecha', datosEspecificos.idCultivo);

            break;

        case 8:
            if (datosEspecificos.costo != null) $('#costoOtraLaborTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoOtraLabor').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 9: // SiloBolsa
            if (datosEspecificos.codigo != null) $('#codigoSiloBolsa').val(datosEspecificos.codigo);
            if (datosEspecificos.longitud != null) $('#longitudSiloBolsa').val(datosEspecificos.longitud);
            if (datosEspecificos.capacidadTotalTn != null) $('#capacidadTotalTn').val(datosEspecificos.capacidadTotalTn);
            if (datosEspecificos.humedadGrano != null) $('#humedadGrano').val(datosEspecificos.humedadGrano);
            if (datosEspecificos.costo != null) $('#costoSiloBolsaTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoSiloBolsa').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;
    }
}

/**
 * Sets the value of a unidad selector (Choices.js instance) by finding it
 * as the sibling .unidad-selector within the same .input-group as the given inputId.
 * @param {string} inputId - The ID of the numeric input field (without #)
 * @param {number|string} value - The value to set on the unidad selector
 */
function setUnidadSelectorValue(inputId, value) {
    if (value == null) return;
    var selector = $('#' + inputId).closest('.input-group').find('.unidad-selector');
    if (selector.length === 0) return;
    var element = selector[0];
    var strValue = value.toString();
    if (element._choicesInstance) {
        try {
            element._choicesInstance.setChoiceByValue(strValue);
        } catch (e) {
            // Fallback: set native value directly
            selector.val(strValue);
        }
    } else {
        selector.val(strValue);
    }
}


function cambiarMoneda() {
    const selector = document.getElementById('selectorMoneda');
    const monedaActual = selector.value;

    document.querySelectorAll('.valor-ars').forEach(el => {
        el.style.display = monedaActual === 'ARS' ? 'inline' : 'none';
    });

    document.querySelectorAll('.valor-usd').forEach(el => {
        el.style.display = monedaActual === 'USD' ? 'inline' : 'none';
    });
}
