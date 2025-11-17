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
                        'Iniciada': 'secondary',
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

                    const estado = row.estadoDisplay; // ← muy importante

                    let botones = `<div class="btn-group btn-group-sm">`;

                    // ----- ESTADO: INICIADA -----
                    if (estado === 'Iniciada') {
                        botones += `
                        <button type="button" class="btn btn-outline-primary btn-edit"
                                title="Editar" data-id="${data}">
                            <i class="ph ph-pencil"></i>
                        </button>
                        <button type="button" class="btn btn-outline-danger btn-delete"
                                title="Eliminar" data-id="${data}">
                            <i class="ph ph-trash"></i>
                        </button>
                    `;
                    }

                    // ----- ESTADO: EN CURSO -----
                    //if (estado === 'En Curso') {
                    //    botones += `
                    //    <button type="button" class="btn btn-outline-primary btn-edit"
                    //            title="Editar" data-id="${data}">
                    //        <i class="ph ph-pencil"></i>
                    //    </button>

                    //    <button type="button" class="btn btn-warning btn-finalizar ms-3"
                    //            title="Cerrar campaña" data-id="${data}" data-nombre="${row.nombre}">
                    //        <i class="ph ph-lock-key"></i>
                    //        <span class="d-none d-sm-inline">Finalizar</span>
                    //    </button>
                    //`;
                    //}

                    if (estado === 'En Curso') {
                        botones += `
                        <button type="button" class="btn btn-outline-primary btn-edit"
                                title="Editar" data-id="${data}">
                            <i class="ph ph-pencil"></i>
                        </button>
                    `;
                    }

                    // ----- ESTADO: FINALIZADA o CANCELADA → SOLO VER -----
                    if (estado === 'Finalizada' || estado === 'Cancelada') {
                        botones += `
                        <button type="button" class="btn btn-outline-secondary btn-ver"
                                title="Ver campaña" data-id="${data}">
                            <i class="ph ph-eye"></i>
                        </button>
                    `;
                    }

                    botones += `</div>`;
                    return botones;
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

    $('#tblCampanias tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarCampania(id);
    });

    $('#tblCampanias tbody').on('click', '.btn-finalizar', function () {
        var id = $(this).data('id');
        var nombre = $(this).data('nombre');
        finalizarCampania(id, nombre);
    });
}

function abrirModalCampania(id, accion) {
    var modal = new bootstrap.Modal($('#modalCampania')[0]);
    var titulo = $('#modalCampaniaLabel');
    var btnGuardar = $('#btnGuardarCampania');

    // Configurar según la acción
    switch (accion) {
        case 'editar':
            titulo.html('<i class="ph ph-pencil me-2"></i>Editar Campaña');
            btnGuardar.show();
            cargarDatosCampania(id);
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
        '¿Está seguro de que desea eliminar esta campaña?',
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

    // Limpiar formulario y validaciones
    $('#formCampania')[0].reset();
    limpiarValidaciones();

    // Resetear tabs
    activarTab('campos');

    // Establecer fecha mínima como hoy
    var hoy = new Date();
    var fecha = hoy.toISOString().split('T')[0];
    $('#fechaInicio').val(fecha);
    $('#fechaInicio').attr('min', fecha);

    // Cargar datos iniciales
    cargarCampos().then(function () {
        configurarEventosModal();
        // Renderizar con template CON botones eliminar
        renderizarCamposConLotes(false); // <- Pasar false para modo creación

        const añoActual = new Date().getFullYear().toString().slice(-2)
        const añoSiguiente = (parseInt(añoActual) + 1).toString().padStart(2, '0')
        $('#nombre').val(`Campaña ${añoActual}/${añoSiguiente}`)
    });
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
        eliminarCampo(idCampo);
    });

    // Eliminar lote (delegación de eventos)
    $(document).off('click', '.btn-eliminar-lote').on('click', '.btn-eliminar-lote', function (e) {
        e.preventDefault();
        e.stopPropagation();
        var idCampo = $(this).data('campo-id');
        var IdLote = $(this).data('lote-id');
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
    return new Promise(function (resolve, reject) {
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
                    resolve();
                } else {
                    reject('Error al cargar campos');
                }
            })
            .fail(function (error) {
                console.error('Error al cargar campos:', error);
                reject('Error al cargar los campos');
            });
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

function eliminarCampo(idCampo) {
    // Encontrar el campo
    var campo = camposConLotes.find(item => item.campo.id == idCampo);

    // Si el campo es existente, no permitir eliminarlo
    if (campo && campo.campo.esExistente) {
        mostrarError('No se puede eliminar un campo existente de la campaña');
        return;
    }

    mostrarConfirmacion('¿Está seguro de que desea eliminar este campo y todos sus lotes?', 'Eliminar Campo')
        .then((result) => {
            if (result.isConfirmed) {
                camposConLotes = camposConLotes.filter(item => item.campo.id != idCampo);

                // Renderizar según el modo
                var esEdicion = $('#idCampania').val() !== '';
                renderizarCamposConLotes(esEdicion);

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
                superficieTotal: parseFloat(option.data('superficie') || '0'),
                esExistente: false // <- CAMPO NUEVO EN EDICIÓN
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
        idCampo: idCampo,
        esExistente: false // <- LOTE NUEVO
    };

    camposConLotes[campoIndex].lotes.push(nuevoLote);

    // Renderizar según el modo (edición o creación)
    var esEdicion = $('#idCampania').val() !== '';
    renderizarCamposConLotes(esEdicion);

    // Limpiar campos y actualizar superficie
    $('#nuevoLoteNombre').val('');
    $('#nuevoLoteSuperficie').val('');
    $('#nuevoLoteNombre').focus();

    // Actualizar información de superficie del campo seleccionado
    mostrarInfoCampoLotes(idCampo);
}

function eliminarLote(idCampo, IdLote) {
    // Encontrar el lote
    var campoIndex = camposConLotes.findIndex(item => item.campo.id == idCampo);
    if (campoIndex !== -1) {
        var lote = camposConLotes[campoIndex].lotes.find(l => l.id == IdLote);

        // Si el lote es existente, no permitir eliminarlo
        if (lote && lote.esExistente) {
            mostrarError('No se puede eliminar un lote existente de la campaña');
            return;
        }
    }

    mostrarConfirmacion('¿Está seguro de que desea eliminar este lote?', 'Eliminar Lote')
        .then((result) => {
            if (result.isConfirmed) {
                var campoIndex = camposConLotes.findIndex(item => item.campo.id == idCampo);
                if (campoIndex !== -1) {
                    camposConLotes[campoIndex].lotes = camposConLotes[campoIndex].lotes.filter(lote => lote.id != IdLote);

                    // Renderizar según el modo
                    var esEdicion = $('#idCampania').val() !== '';
                    renderizarCamposConLotes(esEdicion);

                    // Actualizar información de superficie si este campo está seleccionado
                    if (campoSeleccionadoActual == idCampo) {
                        mostrarInfoCampoLotes(idCampo);
                    }
                }
            }
        });
}

function renderizarCamposConLotes(esEdicion = false) {
    var contenedor = $('#camposConLotes');
    var templateCampoConBoton = $('#templateCampoConLotes').html();
    var templateCampoSinBoton = $('#templateCampoConLotesExistente').html();
    var templateLote = $('#templateLote').html();
    var templateLoteExistente = $('#templateLoteExistente').html();

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

            // Elegir template según si el campo es existente o nuevo
            var templateCampo = item.campo.esExistente ? templateCampoSinBoton : templateCampoConBoton;

            var campoHtml = templateCampo
                .replace(/{{idCampo}}/g, item.campo.id)
                .replace(/{{campoNombre}}/g, item.campo.nombre)
                .replace(/{{campoUbicacion}}/g, item.campo.ubicacion);

            contenedor.append(campoHtml);

            // Renderizar lotes de este campo
            var lotesContainer = $(`#lotes-${item.campo.id}`);
            if (item.lotes.length === 0) {
                lotesContainer.html('<div class="text-center py-2 text-muted"><small>No hay lotes agregados</small></div>');
            } else {
                item.lotes.forEach(function (lote) {
                    // Elegir template según si el lote es existente o nuevo
                    var loteTemplate = lote.esExistente ? templateLoteExistente : templateLote;
                    var loteHtml = loteTemplate
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
    $('#resumenEstado').text("Inicializado");
    $('#resumenFechaInicio').text($('#fechaInicio').val());
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
    var form = $('#formCampania')[0];
    var nombreInput = $('#nombre')[0];
    var fechaInput = $('#fechaInicio')[0];

    // Limpiar validaciones anteriores
    form.classList.remove('was-validated');
    nombreInput.classList.remove('is-invalid', 'is-valid');
    fechaInput.classList.remove('is-invalid', 'is-valid');

    // Validar individualmente cada campo
    var nombreValido = nombreInput.checkValidity();
    var fechaValida = fechaInput.checkValidity();

    if (!nombreValido || !fechaValida) {
        // Mostrar validación visual
        form.classList.add('was-validated');

        // Enfocar el primer campo inválido
        if (!nombreValido) {
            nombreInput.focus();
        } else if (!fechaValida) {
            fechaInput.focus();
        }

        return false;
    }

    // Si todo está válido, agregar clases de éxito
    nombreInput.classList.add('is-valid');
    fechaInput.classList.add('is-valid');

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
    var idCampania = $('#idCampania').val();
    var esEdicion = !!idCampania;

    var data = {
        Nombre: $('#nombre').val(),
        EstadosCampania: 0,
        FechaInicio: $('#fechaInicio').val(),
        Lotes: []
    };

    // Si es edición, incluir el ID
    if (esEdicion) {
        data.Id = parseInt(idCampania);
    }

    // Agregar solo los lotes nuevos (los existentes ya están en la BD)
    camposConLotes.forEach(function (item) {
        item.lotes.forEach(function (lote) {
            if (!lote.esExistente) { // Solo agregar lotes nuevos
                data.Lotes.push({
                    idCampo: parseInt(item.campo.id),
                    Nombre: lote.nombre,
                    SuperficieHectareas: lote.superficie,
                    IdCampania: idCampania != '' ? parseInt(idCampania) : 0
                });
            }
        });
    });

    mostrarLoading(esEdicion ? 'Actualizando campaña...' : 'Creando campaña...');

    var url = esEdicion ? '/Campania/Update' : '/Campania/Create';
    var metodo = esEdicion ? 'PUT' : 'POST';

    $.ajax({
        url: url,
        type: metodo,
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            cerrarAlertas();
            if (response.success) {
                mostrarExito(response.message || (esEdicion ? 'Campaña actualizada correctamente' : 'Campaña creada correctamente'));
                $('#modalCampania').modal('hide');
                table.ajax.reload();
            } else {
                mostrarError(response.message || (esEdicion ? 'Error al actualizar campaña' : 'Error al crear campaña'));
            }
        },
        error: function (error) {
            cerrarAlertas();
            console.error('Error:', error);
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function finalizarCampania(id, nombre) {
    mostrarConfirmacion(
        'Esta acción generará un reporte final y cerrará la campaña. ¿Continuar?',
        'Cerrar ' + nombre
    ).then((result) => {
        if (result.isConfirmed) {

            mostrarLoading();
            $.ajax({
                url: '/Campania/Finalizar/' + id,
                type: 'PUT',
                success: function (response) {
                    cerrarAlertas();
                    if (response.success) {

                        generarPdf(response.object, nombre);

                    } else {
                        mostrarError(response.message || 'Error cerrar la campaña');
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

function generarPdf(id, nombre) {

    mostrarLoading();
    $.ajax({
        url: '/Reportes/CierreCampania/' + id,
        type: 'GET',
        success: function (response) {
            cerrarAlertas();
            if (response.success) {

                abrirPDF(response.object, 'Reporte_Campania_' + nombre + '.pdf');

            } else {
                mostrarError(response.message || 'Error al cerrar la campaña');
            }
        },
        error: function () {
            cerrarAlertas();
            mostrarError('Error al conectar con el servidor');
        }
    });
}
function abrirPDF(base64String, filename) {
    try {
        const binaryString = atob(base64String);

        const bytes = new Uint8Array(binaryString.length);
        for (let i = 0; i < binaryString.length; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }

        const blob = new Blob([bytes], { type: 'application/pdf' });

        const url = window.URL.createObjectURL(blob);

        window.open(url, '_blank');

    } catch (error) {
        console.error('Error al abrir PDF:', error);
        mostrarError('Error al abrir el PDF: ' + error.message);
    }
}

function cargarDatosCampania(id) {
    mostrarLoading('Cargando campaña...');

    $.ajax({
        url: '/Campania/GetById/' + id,
        type: 'GET',
        success: function (response) {
            cerrarAlertas();
            if (response.success && response.object) {
                var campania = response.object;
                llenarFormularioCampania(campania);
            } else {
                mostrarError(response.message || 'Error al cargar los datos de la campaña');
            }
        },
        error: function () {
            cerrarAlertas();
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function llenarFormularioCampania(campania) {
    // Resetear variables
    camposConLotes = [];
    campoSeleccionadoActual = null;

    // Llenar datos básicos
    $('#idCampania').val(campania.id); // Necesitarás agregar este campo hidden
    $('#nombre').val(campania.nombre);
    $('#fechaInicio').val(campania.fechaInicio ? campania.fechaInicio.split('T')[0] : '');

    // Cargar campos disponibles
    cargarCampos().then(function () {
        // Cargar lotes existentes
        if (campania.lotes && campania.lotes.length > 0) {
            cargarLotesExistentes(campania.lotes);
        }

        // Configurar eventos del modal
        configurarEventosModal();
        activarTab('campos');
    });
}

function cargarLotesExistentes(lotes) {
    // Agrupar lotes por campo
    var lotesPorCampo = {};

    lotes.forEach(function (lote) {
        if (!lotesPorCampo[lote.idCampo]) {
            lotesPorCampo[lote.idCampo] = [];
        }
        lotesPorCampo[lote.idCampo].push(lote);
    });

    // Para cada campo con lotes, crear la estructura
    Object.keys(lotesPorCampo).forEach(function (idCampo) {
        var lotesCampo = lotesPorCampo[idCampo];
        var option = $('#campoSeleccionadoLotes').find('option[value="' + idCampo + '"]');

        if (option.length) {
            var campo = {
                id: idCampo,
                nombre: option.text(),
                ubicacion: option.data('ubicacion'),
                superficieTotal: parseFloat(option.data('superficie') || '0'),
                esExistente: true // <- MARCAR CAMPO COMO EXISTENTE
            };

            // Agregar el campo con sus lotes existentes
            camposConLotes.push({
                campo: campo,
                lotes: lotesCampo.map(function (lote) {
                    return {
                        id: lote.id, // ID real de la base de datos
                        nombre: lote.nombre,
                        superficie: lote.superficieHectareas,
                        idCampo: parseInt(idCampo),
                        esExistente: true // Marcar como lote existente
                    };
                })
            });
        }
    });

    renderizarCamposConLotes(true); // <- Pasar true para modo edición
}

function limpiarValidaciones() {
    try {
        var form = $('#formCampania')[0];
        if (!form) return;

        // Remover clase was-validated del formulario
        $(form).removeClass('was-validated');

        // Limpiar todos los campos del formulario
        $(form).find('input, select, textarea').each(function () {
            var campo = $(this);
            campo.removeClass('is-valid is-invalid');
            campo[0].setCustomValidity('');

            // Ocultar mensajes de feedback específicos de este campo
            campo.next('.valid-feedback').hide();
            campo.next('.invalid-feedback').hide();

            // También buscar mensajes que puedan estar en otros lugares
            campo.siblings('.valid-feedback').hide();
            campo.siblings('.invalid-feedback').hide();
        });

        console.log('Validaciones limpiadas correctamente');
    } catch (error) {
        console.error('Error al limpiar validaciones:', error);
    }
}