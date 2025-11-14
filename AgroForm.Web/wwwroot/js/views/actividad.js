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

// Evento para editar actividad desde la grilla
$(document).on('click', '.btn-edit-actividad', function () {
    var id = $(this).data('id');
    var idTipoActividad = $(this).data('idtipoactividad');

    cargarActividadParaEditar(id, idTipoActividad);
});

// Función para cargar los datos de la actividad a editar
function cargarActividadParaEditar(id, idTipoActividad) {
    $.ajax({
        url: '/Actividad/GetBy?id=' + id + '&idTipoActividad=' + idTipoActividad,
        type: 'GET',
        success: function (result) {
            if (result.success) {
                abrirModalParaEdicion(result.object, result.object.tipoActividad.nombre);
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
function abrirModalParaEdicion(actividad, tipoActividadNombre) {
    // Abrir el modal
    $('#modalActividadRapida').modal('show');

    // Esperar a que el modal esté completamente visible
    $('#modalActividadRapida').one('shown.bs.modal', function () {
        $('#actividadId').val(null)
        // Configurar modo edición
        configurarModoEdicion(actividad, tipoActividadNombre);
    });
}

// Función para configurar el modal en modo edición
function configurarModoEdicion(actividad, tipoActividadNombre) {
    // 1. Cambiar el título del modal
    $('#modalActividadRapidaLabel').html('<i class="ph ph-pencil me-2"></i>Editar Labor');

    // 2. Cargar datos básicos
    $('#fecha').val(actividad.fecha.split('T')[0]); // Formatear fecha
    $('#observacion').val(actividad.observacion || '');

    // 3. Seleccionar el tipo de actividad (pero bloquearlo)
    $('#tipoidActividad').val(actividad.idTipoActividad).trigger('change');
    $('#tipoidActividad').prop('disabled', true);

    // 4. Seleccionar los lotes (pero bloquearlos)
    if (actividad.idLote) {
        $('#IdLote').val(actividad.idLote).trigger('change');
    }
    $('#IdLote').prop('disabled', true);

    // 5. Cargar campos específicos después de que se cargue el template

    setTimeout(function () {
        cargarDatosEspecificosEditar(actividad, tipoActividadNombre);
        cerrarAlertas();
    }, 500);

    // 6. Cambiar el texto del botón de guardar
    $('button[type="submit"]').html('<i class="ph ph-check-circle me-1"></i>Actualizar Labor');

    // 7. Agregar hidden field para el ID de la actividad
    if (!$('#actividadId').length) {
        $('<input>').attr({
            type: 'hidden',
            id: 'actividadId',
            name: 'actividadId'
        }).val(actividad.id).appendTo('#formActividadRapida');
    } else {
        $('#actividadId').val(actividad.id);
    }
}

// Función para cargar datos específicos según el tipo de actividad
function cargarDatosEspecificosEditar(datosEspecificos, tipoActividadNombre) {
    if (!datosEspecificos) return;

    switch (tipoActividadNombre) {
        case 'Siembra':
            if (datosEspecificos.superficieHa != null) $('#superficieHa').val(datosEspecificos.superficieHa);
            if (datosEspecificos.densidadSemillaKgHa != null) $('#densidadSemillaKgHa').val(datosEspecificos.densidadSemillaKgHa);
            if (datosEspecificos.costo != null) $('#costoSiembra').val(datosEspecificos.costo);
            if (datosEspecificos.idCultivo != null) $('#idCultivo').val(datosEspecificos.idCultivo).trigger('change');
            if (datosEspecificos.idVariedad != null) setTimeout(() => $('#idVariedad').val(datosEspecificos.idVariedad).trigger('change'), 200);
            if (datosEspecificos.idMetodoSiembra != null) $('#idMetodoSiembra').val(datosEspecificos.idMetodoSiembra).trigger('change');
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoSiembra').prop('checked', !!datosEspecificos.rsDolar).trigger('change');
            break;

        case 'Riego':
            if (datosEspecificos.horasRiego != null) $('#horasRiego').val(datosEspecificos.horasRiego);
            if (datosEspecificos.volumenAguaM3 != null) $('#volumenAguaM3').val(datosEspecificos.volumenAguaM3);
            if (datosEspecificos.costo != null) $('#costoRiegoTotal').val(datosEspecificos.costo);
            if (datosEspecificos.idMetodoRiego != null) $('#idMetodoRiego').val(datosEspecificos.idMetodoRiego).trigger('change');
            if (datosEspecificos.idFuenteAgua != null) $('#idFuenteAgua').val(datosEspecificos.idFuenteAgua).trigger('change');
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoRiego').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 'Fertilizado':
            if (datosEspecificos.cantidadKgHa != null) $('#cantidadKgHa').val(datosEspecificos.cantidadKgHa);
            if (datosEspecificos.dosisKgHa != null) $('#dosisKgHa').val(datosEspecificos.dosisKgHa);
            if (datosEspecificos.costo != null) $('#costoFertilizado').val(datosEspecificos.costo);
            if (datosEspecificos.idNutriente != null) $('#idNutriente').val(datosEspecificos.idNutriente).trigger('change');
            if (datosEspecificos.idTipoFertilizante != null) $('#idTipoFertilizante').val(datosEspecificos.idTipoFertilizante).trigger('change');
            if (datosEspecificos.idMetodoAplicacion != null) $('#idMetodoAplicacion').val(datosEspecificos.idMetodoAplicacion).trigger('change');
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoFertilizacion').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 'Pulverizacion':
            if (datosEspecificos.volumenLitrosHa != null) $('#volumenLitrosHa').val(datosEspecificos.volumenLitrosHa);
            if (datosEspecificos.dosis != null) $('#dosisPulverizacion').val(datosEspecificos.dosis);
            if (datosEspecificos.condicionesClimaticas != null) $('#condicionesClimaticas').val(datosEspecificos.condicionesClimaticas);
            if (datosEspecificos.idProductoAgroquimico != null) $('#idProductoAgroquimico').val(datosEspecificos.idProductoAgroquimico).trigger('change');
            if (datosEspecificos.costo != null) $('#costoPulverizacionTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoPulverizacion').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 'Monitoreo':
            if (datosEspecificos.IdMonitoreo != null) $('#idMonitoreo').val(datosEspecificos.idMonitoreo).trigger('change');
            if (datosEspecificos.idTipoMonitoreo != null) $('#idTipoMonitoreo').val(datosEspecificos.idTipoMonitoreo).trigger('change');
            if (datosEspecificos.idEstadoFenologico != null) $('#idEstadoFenologico').val(datosEspecificos.idEstadoFenologico).trigger('change');
            if (datosEspecificos.costo != null) $('#costoMonitoreoTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoMonitoreo').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 'AnalisisSuelo':
            if (datosEspecificos.profundidadCm != null) $('#profundidadCm').val(datosEspecificos.profundidadCm);
            if (datosEspecificos.ph != null) $('#ph').val(datosEspecificos.ph);
            if (datosEspecificos.materiaOrganica != null) $('#materiaOrganica').val(datosEspecificos.materiaOrganica);
            if (datosEspecificos.nitrogeno != null) $('#nitrogeno').val(datosEspecificos.nitrogeno);
            if (datosEspecificos.fosforo != null) $('#fosforo').val(datosEspecificos.fosforo);
            if (datosEspecificos.potasio != null) $('#potasio').val(datosEspecificos.potasio);
            if (datosEspecificos.conductividadElectrica != null) $('#conductividadElectrica').val(datosEspecificos.conductividadElectrica);
            if (datosEspecificos.cic != null) $('#cic').val(datosEspecificos.cic);
            if (datosEspecificos.textura != null) $('#textura').val(datosEspecificos.textura).trigger('change');
            if (datosEspecificos.idLaboratorio != null) $('#idLaboratorio').val(datosEspecificos.idLaboratorio).trigger('change');
            if (datosEspecificos.costo != null) $('#costoAnalisisSueloTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoAnalisisSuelo').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 'Cosecha':
            if (datosEspecificos.rendimientoTonHa != null) $('#rendimientoTonHa').val(datosEspecificos.rendimientoTonHa);
            if (datosEspecificos.humedadGrano != null) $('#humedadGrano').val(datosEspecificos.humedadGrano);
            if (datosEspecificos.superficieCosechadaHa != null) $('#superficieCosechadaHa').val(datosEspecificos.superficieCosechadaHa);
            if (datosEspecificos.idCultivo != null) $('#idCultivoCosecha').val(datosEspecificos.idCultivo).trigger('change');
            if (datosEspecificos.costo != null) $('#costoCosechaTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoCosecha').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;

        case 'OtraLabor':
            if (datosEspecificos.costo != null) $('#costoOtraLaborTotal').val(datosEspecificos.costo);
            if (datosEspecificos.esDolar != null) $('#switchMonedaCostoOtraLabor').prop('checked', !!datosEspecificos.esDolar).trigger('change');
            break;
    }
}


//// Función para resetear el modal cuando se cierre
//function resetearModal() {
//    $('#modalActividadRapidaLabel').html('<i class="ph ph-tractor me-2"></i>Labor');
//    $('#tipoidActividad').prop('disabled', false);
//    $('#IdLote').prop('disabled', false);
//    $('button[type="submit"]').html('<i class="ph ph-check-circle me-1"></i>Guardar Actividad');
//    $('#actividadId').remove();
//}

//// Modificar el evento hidden del modal para incluir el reset
//$('#modalActividadRapida').on('hidden.bs.modal', function () {
//    resetearModal();
//    // Tu código existente de reset...
//});