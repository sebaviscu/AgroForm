// Variables globales
var camposConLotes = []; // Array de objetos: { campo, lotes: [] }
var campoSeleccionadoActual = null;
var table;

$(document).ready(function () {
    inicializarDataTable();
    configurarEventosGrilla();
});

// ========== DATATABLE DE CAMPAÑAS ==========
function inicializarDataTable() {
    table = $('#tblCampanias').DataTable({
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
        ajax: {
            url: '/Campania/GetAllDataTable',
            type: 'GET',
            dataType: 'json',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            { data: 'id', width: '80px' },
            {
                data: 'estadoDisplay',
                render: function (data, type, row) {
                    const estadoColors = {
                        'En Curso': 'info',
                        'Finalizada': 'success',
                        'Cancelada': 'danger'
                    };
                    return renderEnumBadge(data, estadoColors);
                }
            },
            { data: 'nombre' },
            {
                data: 'fechaInicio',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString() : '-';
                }
            },
            {
                data: 'fechaFin',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString() : '-';
                }
            },
            {
                data: 'id',
                className: 'text-center',
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-info btn-view"
                                    title="Ver campaña" data-id="${data}">
                                <i class="ph ph-eye"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-delete" 
                                    title="Eliminar campaña" data-id="${data}">
                                <i class="ph ph-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        order: [[0, 'desc']],
        pageLength: 25,
        responsive: true
    });
    function renderEnumBadge(data, colorMap) {
        const color = colorMap[data] || 'secondary';
        return `<span class="badge bg-${color}">${data}</span>`;
    }

    // Filtrar por estado
    $('#estadoFilter').change(function () {
        var estado = $(this).val();
        table.columns(1).search(estado).draw();
    });

    // Restablecer filtros
    $('#btnResetFilters').click(function () {
        $('#estadoFilter').val('');
        table.search('').columns().search('').draw();
    });
}

// ========== FUNCIONES DE LA GRILLA ==========
function configurarEventosGrilla() {
    // Botón nueva campaña
    $('#btnNuevaCampania').click(function () {
        abrirModalCampania(0, 'crear');
    });

    // Delegación de eventos para botones dinámicos
    $('#tblCampanias tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalCampania(id, 'editar');
    });

    $('#tblCampanias tbody').on('click', '.btn-view', function () {
        var id = $(this).data('id');
        abrirModalCampania(id, 'ver');
    });

    $('#tblCampanias tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarCampania(id);
    });
}

function abrirModalCampania(id, accion) {
    var modal = new bootstrap.Modal($('#modalCampania')[0]);
    var titulo = $('#modalCampaniaLabel');
    var btnGuardar = $('#btnGuardarCampania');

    // Configurar según la acción
    switch (accion) {
        case 'ver':
            titulo.html('<i class="ph ph-eye me-2"></i>Detalles de Campaña');
            btnGuardar.hide();
            break;
        case 'editar':
            titulo.html('<i class="ph ph-pencil me-2"></i>Editar Campaña');
            btnGuardar.show();
            // TODO: Cargar datos existentes
            break;
        case 'crear':
            titulo.html('<i class="ph ph-plus-circle me-2"></i>Nueva Campaña');
            btnGuardar.show();
            inicializarModalCreacion();
            break;
    }

    modal.show();
}

function eliminarCampania(id) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar esta campaña? Esta acción no se puede deshacer.',
        'Eliminar Campaña'
    ).then((result) => {
        if (result.isConfirmed) {
            mostrarLoading('Eliminando campaña...');

            $.ajax({
                url: '/Campania/Delete/' + id,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas();
                    if (response.success) {
                        mostrarExito(response.message || 'Campaña eliminada correctamente');
                        table.ajax.reload();
                    } else {
                        mostrarError(response.message || 'Error al eliminar campaña');
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

// ========== FUNCIONES DEL MODAL DE CREACIÓN ==========
function inicializarModalCreacion() {
    // Resetear variables
    camposConLotes = [];
    campoSeleccionadoActual = null;

    // Limpiar formulario
    $('#formCampania')[0].reset();
    $('#formCampania').removeClass('was-validated');

    // Resetear tabs
    activarTab('campos');

    // Cargar datos iniciales
    cargarCampos();
    configurarEventosModal();
}

function configurarEventosModal() {
    // Navegación entre tabs
    $('#btnSiguienteLotes').off('click').on('click', function () {
        if (validarTabCampos()) {
            activarTab('lotes');
        }
    });

    $('#btnAnteriorCampos').off('click').on('click', function () {
        activarTab('campos');
    });

    $('#btnSiguienteResumen').off('click').on('click', function () {
        if (validarTabLotes()) {
            activarTab('resumen');
            actualizarResumen();
        }
    });

    $('#btnAnteriorLotes').off('click').on('click', function () {
        activarTab('lotes');
    });

    // Agregar campo
    //$('#btnAgregarCampo').off('click').on('click', function () {
    //    agregarCampo();
    //});

    // Agregar lote
    $('#btnAgregarLote').off('click').on('click', function () {
        agregarLote();
    });

    // Enter en los campos de nuevo lote
    $('#nuevoLoteNombre, #nuevoLoteSuperficie').off('keypress').on('keypress', function (e) {
        if (e.which === 13) {
            agregarLote();
        }
    });

    // Cuando cambia el campo seleccionado
    $('#campoSeleccionadoLotes').off('change').on('change', function () {
        var idCampo = $(this).val();
        campoSeleccionadoActual = idCampo;
        mostrarInfoCampoLotes(idCampo);
    });

    $(document).off('click', '.btn-eliminar-campo').on('click', '.btn-eliminar-campo', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var idCampo = $(this).data('campo-id');
        console.log('Eliminar campo clickeado:', idCampo); // Para debug
        eliminarCampo(idCampo);
    });

    // Eliminar lote (delegación de eventos)
    $(document).off('click', '.btn-eliminar-lote').on('click', '.btn-eliminar-lote', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var idCampo = $(this).data('campo-id');
        var IdLote = $(this).data('lote-id');
        console.log('Eliminar lote clickeado:', idCampo, IdLote); // Para debug
        eliminarLote(idCampo, IdLote);
    });

    // Validación del formulario
    $('#formCampania').off('submit').on('submit', function (e) {
        e.preventDefault();
        guardarCampania();
    });
}

function activarTab(tabNombre) {
    // Desactivar todos los tabs
    $('.nav-tabs .nav-link').removeClass('active').attr('aria-selected', 'false');
    $('.tab-pane').removeClass('show active');

    // Activar tab seleccionado
    $(`#${tabNombre}-tab`).addClass('active').attr('aria-selected', 'true');
    $(`#${tabNombre}-pane`).addClass('show active');

    // Mostrar/ocultar botones según el tab activo
    $('#btnAnteriorCampos').hide();
    $('#btnSiguienteLotes').hide();
    $('#btnAnteriorLotes').hide();
    $('#btnSiguienteResumen').hide();
    $('#btnGuardarCampania').hide();

    switch (tabNombre) {
        case 'campos':
            $('#btnSiguienteLotes').show();
            break;
        case 'lotes':
            $('#btnAnteriorCampos').show();
            $('#btnSiguienteResumen').show();
            break;
        case 'resumen':
            $('#btnAnteriorLotes').show();
            $('#btnGuardarCampania').show();
            break;
    }
}

function cargarCampos() {
    $.get('/Campo/GetAll')
        .done(function (response) {
            if (response.success) {
                var campoSelect = $('#campoSeleccionadoLotes');
                campoSelect.empty().append('<option value="">Seleccione un campo...</option>');
                var campos = response.listObject || response.data || [];

                campos.forEach(function (campo) {
                    campoSelect.append(
                        $('<option></option>')
                            .val(campo.id)
                            .text(campo.nombre)
                            .attr('data-ubicacion', campo.ubicacion || 'No especificada')
                            .attr('data-superficie', campo.superficieHectareas || '0')
                    );
                });
            }
        })
        .fail(function (error) {
            console.error('Error al cargar campos:', error);
            mostrarError('Error al cargar los campos');
        });
}

function mostrarInfoCampoLotes(idCampo) {
    var infoSuperficieCampo = $('#infoSuperficieCampo');
    var formAgregarLote = $('#formAgregarLote');

    if (!idCampo) {
        infoSuperficieCampo.hide();
        formAgregarLote.hide();
        return;
    }

    var option = $('#campoSeleccionadoLotes').find('option[value="' + idCampo + '"]');
    if (option.length) {
        var superficieTotal = parseFloat(option.data('superficie') || '0');

        // Calcular superficie disponible
        var campoExistente = camposConLotes.find(item => item.campo.id == idCampo);
        var superficieUsada = campoExistente ? campoExistente.lotes.reduce((total, lote) => total + lote.superficie, 0) : 0;
        var superficieDisponible = superficieTotal - superficieUsada;

        // Actualizar información de superficie
        $('#campoSuperficieTotal').text(superficieTotal.toFixed(2));
        $('#campoSuperficieDisponible').text(superficieDisponible.toFixed(2));

        // Colorear según disponibilidad
        var disponibleElement = $('#campoSuperficieDisponible');
        if (superficieDisponible <= 0) {
            disponibleElement.addClass('text-danger').removeClass('text-success');
        } else {
            disponibleElement.addClass('text-success').removeClass('text-danger');
        }

        $('#nombreCampoSeleccionado').text(option.text());
        infoSuperficieCampo.show();
        formAgregarLote.show();
    }
}

//function agregarCampo() {
//    var idCampo = $('#campoSeleccionadoLotes').val();
//    if (!idCampo) {
//        mostrarError('Por favor seleccione un campo');
//        return;
//    }

//    // Verificar si el campo ya fue agregado
//    if (camposConLotes.some(item => item.campo.id == idCampo)) {
//        mostrarError('Este campo ya ha sido agregado a la campaña');
//        return;
//    }

//    var option = $('#campoSeleccionadoLotes').find('option[value="' + idCampo + '"]');
//    var campo = {
//        id: idCampo,
//        nombre: option.text(),
//        ubicacion: option.data('ubicacion'),
//        superficieTotal: parseFloat(option.data('superficie') || '0')
//    };

//    camposConLotes.push({
//        campo: campo,
//        lotes: []
//    });

//    renderizarCamposConLotes();
//    $('#campoSeleccionadoLotes').val('');
//    $('#infoSuperficieCampo').hide();
//    $('#formAgregarLote').hide();
//    campoSeleccionadoActual = null;
//}

function eliminarCampo(idCampo) {
    mostrarConfirmacion('¿Está seguro de que desea eliminar este campo y todos sus lotes?', 'Eliminar Campo')
        .then((result) => {
            if (result.isConfirmed) {
                camposConLotes = camposConLotes.filter(item => item.campo.id != idCampo);
                renderizarCamposConLotes();

                // Si el campo eliminado era el seleccionado, limpiar la selección
                if (campoSeleccionadoActual == idCampo) {
                    $('#campoSeleccionadoLotes').val('');
                    $('#infoSuperficieCampo').hide();
                    $('#formAgregarLote').hide();
                    campoSeleccionadoActual = null;
                }
            }
        });
}

function agregarLote() {
    var idCampo = $('#campoSeleccionadoLotes').val();
    if (!idCampo) {
        mostrarError('Por favor seleccione un campo primero');
        return;
    }

    var nombre = $('#nuevoLoteNombre').val().trim();
    var superficie = parseFloat($('#nuevoLoteSuperficie').val());

    // Validaciones
    if (!nombre) {
        mostrarError('Por favor ingrese el nombre del lote');
        $('#nuevoLoteNombre').focus();
        return;
    }

    if (!superficie || superficie <= 0) {
        mostrarError('Por favor ingrese una superficie válida');
        $('#nuevoLoteSuperficie').focus();
        return;
    }

    // Encontrar el campo
    var campoIndex = camposConLotes.findIndex(item => item.campo.id == idCampo);
    if (campoIndex === -1) {
        // Si el campo no está en la campaña, agregarlo automáticamente
        var option = $('#campoSeleccionadoLotes').find('option[value="' + idCampo + '"]');
        if (option.length) {
            var campo = {
                id: idCampo,
                nombre: option.text(),
                ubicacion: option.data('ubicacion'),
                superficieTotal: parseFloat(option.data('superficie') || '0')
            };

            camposConLotes.push({
                campo: campo,
                lotes: []
            });
            campoIndex = camposConLotes.length - 1;
        } else {
            mostrarError('Campo no encontrado');
            return;
        }
    }

    // Verificar superficie disponible
    var superficieUsada = camposConLotes[campoIndex].lotes.reduce((total, lote) => total + lote.superficie, 0);
    var superficieDisponible = camposConLotes[campoIndex].campo.superficieTotal - superficieUsada;

    if (superficie > superficieDisponible) {
        mostrarError(`Superficie insuficiente. Disponible: ${superficieDisponible.toFixed(2)} Ha, Intenta: ${superficie.toFixed(2)} Ha`);
        return;
    }

    // Crear nuevo lote
    var nuevoLote = {
        id: Date.now(), // ID temporal
        nombre: nombre,
        superficie: superficie,
        idCampo: idCampo
    };

    camposConLotes[campoIndex].lotes.push(nuevoLote);
    renderizarCamposConLotes();

    // Limpiar campos y actualizar superficie
    $('#nuevoLoteNombre').val('');
    $('#nuevoLoteSuperficie').val('');
    $('#nuevoLoteNombre').focus();

    // Actualizar información de superficie del campo seleccionado
    mostrarInfoCampoLotes(idCampo);
}

function eliminarLote(idCampo, IdLote) {
    mostrarConfirmacion('¿Está seguro de que desea eliminar este lote?', 'Eliminar Lote')
        .then((result) => {
            if (result.isConfirmed) {
                var campoIndex = camposConLotes.findIndex(item => item.campo.id == idCampo);
                if (campoIndex !== -1) {
                    camposConLotes[campoIndex].lotes = camposConLotes[campoIndex].lotes.filter(lote => lote.id != IdLote);
                    renderizarCamposConLotes();

                    // Actualizar información de superficie si este campo está seleccionado
                    if (campoSeleccionadoActual == idCampo) {
                        mostrarInfoCampoLotes(idCampo);
                    }
                }
            }
        });
}

function renderizarCamposConLotes() {
    var contenedor = $('#camposConLotes');
    var templateCampo = $('#templateCampoConLotes').html();
    var templateLote = $('#templateLote').html();

    contenedor.empty();

    if (camposConLotes.length === 0) {
        contenedor.html(`
            <div class="text-center py-4 text-muted">
                <i class="ph ph-tray display-4"></i>
                <p class="mt-2">No hay campos agregados</p>
            </div>
        `);
    } else {
        camposConLotes.forEach(function (item) {
            var superficieUsada = item.lotes.reduce((total, lote) => total + lote.superficie, 0);
            var superficieDisponible = item.campo.superficieTotal - superficieUsada;

            var campoHtml = templateCampo
                .replace(/{{idCampo}}/g, item.campo.id)
                .replace(/{{campoNombre}}/g, item.campo.nombre)
                .replace(/{{campoUbicacion}}/g, item.campo.ubicacion)
/*                .replace(/{{campoSuperficieTotal}}/g, item.campo.superficieTotal.toFixed(2))*/
                ;

            contenedor.append(campoHtml);

            // Renderizar lotes de este campo
            var lotesContainer = $(`#lotes-${item.campo.id}`);
            if (item.lotes.length === 0) {
                lotesContainer.html('<div class="text-center py-2 text-muted"><small>No hay lotes agregados</small></div>');
            } else {
                item.lotes.forEach(function (lote) {
                    var loteHtml = templateLote
                        .replace(/{{IdLote}}/g, lote.id)
                        .replace(/{{idCampo}}/g, item.campo.id)
                        .replace(/{{nombre}}/g, lote.nombre)
                        .replace(/{{superficie}}/g, lote.superficie.toFixed(2));

                    lotesContainer.append(loteHtml);
                });
            }

            // Actualizar contadores del campo
            $(`.lotes-count-${item.campo.id}`).text(item.lotes.length);
            $(`.superficie-total-${item.campo.id}`).text(superficieUsada.toFixed(2));
            $(`.superficie-disponible-${item.campo.id}`).text(superficieDisponible.toFixed(2));

            // Colorear según disponibilidad
            var disponibleElement = $(`.superficie-disponible-${item.campo.id}`);
            if (superficieDisponible <= 0) {
                disponibleElement.addClass('text-danger').removeClass('text-success');
            } else {
                disponibleElement.addClass('text-success').removeClass('text-danger');
            }
        });
    }

    actualizarResumenGeneral();
}

function actualizarResumenGeneral() {
    var totalCampos = camposConLotes.length;
    var totalLotes = camposConLotes.reduce((total, item) => total + item.lotes.length, 0);
    var totalSuperficie = camposConLotes.reduce((total, item) => {
        return total + item.lotes.reduce((subTotal, lote) => subTotal + lote.superficie, 0);
    }, 0);

    $('#totalCamposAgregados').text(totalCampos);
    $('#totalLotesAgregados').text(totalLotes);
    $('#totalSuperficieAgregada').text(totalSuperficie.toFixed(2));
}

function actualizarResumen() {
    $('#resumenNombre').text($('#nombre').val());
    $('#resumenEstado').text($('#estado option:selected').text());
    $('#resumenFechaInicio').text($('#fechaInicio').val());
    $('#resumenFechaFin').text($('#fechaFin').val() || '-');
    $('#resumenTotalCampos').text(camposConLotes.length);
    $('#resumenTotalLotes').text(camposConLotes.reduce((total, item) => total + item.lotes.length, 0));
    $('#resumenTotalSuperficie').text(camposConLotes.reduce((total, item) => {
        return total + item.lotes.reduce((subTotal, lote) => subTotal + lote.superficie, 0);
    }, 0).toFixed(2));

    // Detalle por campo
    var listaCampos = $('#listaResumenCampos');
    if (camposConLotes.length > 0) {
        var html = '';
        camposConLotes.forEach(function (item) {
            var superficieCampo = item.lotes.reduce((total, lote) => total + lote.superficie, 0);
            var superficieDisponible = item.campo.superficieTotal - superficieCampo;
            html += `
                <div class="card mb-2">
                    <div class="card-body py-2">
                        <h6 class="mb-1">${item.campo.nombre}</h6>
                        <div class="row small">
                            <div class="col-3"><strong>Lotes:</strong> ${item.lotes.length}</div>
                            <div class="col-3"><strong>Usado:</strong> ${superficieCampo.toFixed(2)} Ha</div>
                            <div class="col-3"><strong>Disponible:</strong> ${superficieDisponible.toFixed(2)} Ha</div>
                            <div class="col-3"><strong>Total:</strong> ${item.campo.superficieTotal.toFixed(2)} Ha</div>
                        </div>
                    </div>
                </div>
            `;
        });
        listaCampos.html(html);
    } else {
        listaCampos.html('<div class="alert alert-warning">No se han agregado campos</div>');
    }
}

function validarTabCampos() {
    var nombre = $('#nombre').val().trim();
    var estado = $('#estado').val();
    var fechaInicio = $('#fechaInicio').val();

    if (!nombre) {
        mostrarError('Por favor ingrese el nombre de la campaña');
        $('#nombre').focus();
        return false;
    }

    if (!estado) {
        mostrarError('Por favor seleccione un estado');
        $('#estado').focus();
        return false;
    }

    if (!fechaInicio) {
        mostrarError('Por favor ingrese la fecha de inicio');
        $('#fechaInicio').focus();
        return false;
    }

    return true;
}

function validarTabLotes() {
    if (camposConLotes.length === 0) {
        mostrarError('Por favor agregue al menos un campo a la campaña');
        return false;
    }

    // Verificar que cada campo tenga al menos un lote
    var camposSinLotes = camposConLotes.filter(item => item.lotes.length === 0);
    if (camposSinLotes.length > 0) {
        mostrarError('Todos los campos agregados deben tener al menos un lote');
        return false;
    }

    return true;
}

function guardarCampania() {
    var data = {
        Nombre: $('#nombre').val(),
        Estado: $('#estado').val(),
        FechaInicio: $('#fechaInicio').val(),
        FechaFin: $('#fechaFin').val() || null,
        Lotes: camposConLotes.flatMap(item =>
            item.lotes.map(lote => ({
                idCampo: parseInt(item.campo.id),
                Nombre: lote.nombre,
                SuperficieHectareas: lote.superficie
            }))
        )
    };

    mostrarLoading('Creando campaña...');

    $.ajax({
        url: '/Campania/Create',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            cerrarAlertas();
            if (response.success) {
                mostrarExito(response.message || 'Campaña creada correctamente');
                $('#modalCampania').modal('hide');
                table.ajax.reload();
            } else {
                mostrarError(response.message || 'Error al crear campaña');
            }
        },
        error: function (error) {
            cerrarAlertas();
            console.error('Error:', error);
            mostrarError('Error al conectar con el servidor');
        }
    });
}