// Variables globales
var lotesDisponibles = [];
var lotesSeleccionados = [];
var table; // DataTable

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
                    text: '<i class="bi bi-file-earmark-excel me-1"></i>Excel',
                    className: 'btn btn-outline-success btn-sm',
                    title: 'Campanias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="bi bi-file-earmark-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Campanias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="bi bi-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        columnDefs: [
            { orderable: false, targets: [6] },
            { searchable: false, targets: [4, 6] },
            { width: '120px', targets: [1, 4, 6] },
            { className: 'dt-center', targets: [4, 6] }
        ],
        order: [[2, 'desc']],
        pageLength: 25,
        responsive: true
    });

    // Filtrar por estado
    $('#estadoFilter').change(function () {
        var estado = $(this).val();
        if (estado === '0') {
            table.columns(1).search('').draw();
        } else {
            table.columns(1).search(estado).draw();
        }
    });

    // Restablecer filtros
    $('#btnResetFilters').click(function () {
        $('#estadoFilter').val('0');
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
    $('#tblCampanias tbody').on('click', '.btn-view', function () {
        var id = $(this).data('id');
        abrirModalCampania(id, 'ver');
    });

    $('#tblCampanias tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalCampania(id, 'editar');
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
            titulo.html('<i class="bi bi-eye me-2"></i>Detalles de Campaña');
            btnGuardar.hide();
            break;
        case 'editar':
            titulo.html('<i class="bi bi-pencil me-2"></i>Editar Campaña');
            btnGuardar.show();
            break;
        case 'crear':
            titulo.html('<i class="bi bi-plus-circle me-2"></i>Nueva Campaña');
            btnGuardar.show();
            inicializarModalCreacion();
            break;
    }

    modal.show();
}

function eliminarCampania(id) {
    if (!confirm('¿Está seguro de que desea eliminar esta campaña?')) {
        return;
    }

    $.ajax({
        url: '/Campania/Eliminar/' + id,
        type: 'DELETE',
        success: function (response) {
            if (response.success) {
                mostrarExito(response.message || 'Campaña eliminada correctamente');
                table.ajax.reload();
            } else {
                mostrarError(response.message || 'Error al eliminar campaña');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

// ========== FUNCIONES DEL MODAL DE CREACIÓN ==========
function inicializarModalCreacion() {
    var form = $('#formCampania');
    var campoSelect = $('#campoSeleccionado');

    // Elementos de tabs
    var tabs = {
        campos: $('#campos-tab'),
        lotes: $('#lotes-tab'),
        resumen: $('#resumen-tab')
    };

    // Botones de navegación
    var btnSiguienteLotes = $('#btnSiguienteLotes');
    var btnAnteriorCampos = $('#btnAnteriorCampos');
    var btnSiguienteResumen = $('#btnSiguienteResumen');
    var btnAnteriorLotes = $('#btnAnteriorLotes');
    var seleccionarTodos = $('#seleccionarTodos');

    // Inicializar
    cargarCampos();

    // Navegación entre tabs
    btnSiguienteLotes.off('click').on('click', function () {
        if (validarTabCampos()) {
            activarTab('lotes');
            cargarLotesDelCampo();
        }
    });

    btnAnteriorCampos.off('click').on('click', function () {
        activarTab('campos');
    });

    btnSiguienteResumen.off('click').on('click', function () {
        if (validarTabLotes()) {
            activarTab('resumen');
            actualizarResumen();
        }
    });

    btnAnteriorLotes.off('click').on('click', function () {
        activarTab('lotes');
    });

    // Seleccionar todos los lotes
    seleccionarTodos.off('change').on('change', function () {
        $('.lote-checkbox').prop('checked', this.checked);
        actualizarResumenLotes();
    });

    // Cuando cambia el campo seleccionado
    campoSelect.off('change').on('change', function () {
        mostrarInfoCampo($(this).val());
    });

    // Delegación de eventos para checkboxes de lotes
    $('#contenedorLotes').off('change').on('change', '.lote-checkbox', function () {
        actualizarResumenLotes();
    });

    // Validación del formulario
    form.off('submit').on('submit', function (e) {
        e.preventDefault();

        if (!validarFormularioCompleto()) {
            return;
        }

        guardarCampania();
    });

    function activarTab(tabNombre) {
        // Desactivar todos los tabs
        $.each(tabs, function (key, tab) {
            tab.removeClass('active').attr('aria-selected', 'false');
            if (key !== 'campos') {
                tab.prop('disabled', true);
            }
        });

        // Activar el tab solicitado y habilitar los anteriores
        tabs[tabNombre].addClass('active').attr('aria-selected', 'true').prop('disabled', false);

        // Habilitar tabs anteriores
        var tabOrder = ['campos', 'lotes', 'resumen'];
        var currentIndex = tabOrder.indexOf(tabNombre);
        for (var i = 0; i <= currentIndex; i++) {
            tabs[tabOrder[i]].prop('disabled', false);
        }

        // Mostrar el contenido del tab
        $('.tab-pane').removeClass('show active');
        $('#' + tabNombre).addClass('show active');
    }

    function cargarCampos() {
        $.get('/Campania/ObtenerCampos')
            .done(function (data) {
                if (data.success) {
                    campoSelect.empty().append('<option value="">Seleccione un campo...</option>');
                    $.each(data.data, function (index, campo) {
                        campoSelect.append(
                            $('<option></option>')
                                .val(campo.id)
                                .text(campo.nombre)
                                .attr('data-ubicacion', campo.ubicacion || '')
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

    function mostrarInfoCampo(campoId) {
        var infoCampo = $('#infoCampo');
        var campoDetalles = $('#campoDetalles');

        if (!campoId) {
            infoCampo.hide();
            return;
        }

        var option = campoSelect.find('option[value="' + campoId + '"]');
        if (option.length) {
            var ubicacion = option.data('ubicacion') || 'No especificada';
            var superficie = option.data('superficie') || '0';

            campoDetalles.html(
                '<p class="mb-1"><strong>Ubicación:</strong> ' + ubicacion + '</p>' +
                '<p class="mb-0"><strong>Superficie total:</strong> ' + parseFloat(superficie).toFixed(2) + ' Ha</p>'
            );
            infoCampo.show();
        }
    }

    function cargarLotesDelCampo() {
        var campoId = campoSelect.val();
        if (!campoId) return;

        $('#nombreCampoSeleccionado').text(campoSelect.find('option:selected').text());

        $.get('/Campania/ObtenerLotesPorCampo', { campoId: campoId })
            .done(function (data) {
                if (data.success) {
                    lotesDisponibles = data.data;
                    renderizarLotes();
                }
            })
            .fail(function (error) {
                console.error('Error al cargar lotes:', error);
                mostrarError('Error al cargar los lotes');
            });
    }

    function renderizarLotes() {
        var contenedor = $('#contenedorLotes');
        var template = $('#templateLote').html();

        contenedor.empty();

        $.each(lotesDisponibles, function (index, lote) {
            var loteHtml = template
                .replace(/{{id}}/g, lote.id)
                .replace(/{{nombre}}/g, lote.nombre)
                .replace(/{{campoNombre}}/g, lote.campoNombre)
                .replace(/{{superficie}}/g, lote.superficie.toFixed(2));

            contenedor.append(loteHtml);
        });

        actualizarResumenLotes();
    }

    function actualizarResumenLotes() {
        var checkboxes = $('.lote-checkbox:checked');
        var totalLotes = checkboxes.length;
        var totalSuperficie = 0;

        checkboxes.each(function () {
            var loteId = $(this).val();
            var lote = lotesDisponibles.find(function (l) { return l.id == loteId; });
            if (lote) {
                totalSuperficie += lote.superficie;
            }
        });

        $('#totalLotesSeleccionados').text(totalLotes);
        $('#totalSuperficieSeleccionada').text(totalSuperficie.toFixed(2));

        // Actualizar selección de "todos"
        var todosCheckboxes = $('.lote-checkbox');
        seleccionarTodos.prop('checked', totalLotes === todosCheckboxes.length && totalLotes > 0);
        seleccionarTodos.prop('indeterminate', totalLotes > 0 && totalLotes < todosCheckboxes.length);
    }

    function actualizarResumen() {
        $('#resumenNombre').text($('#nombre').val());
        $('#resumenEstado').text($('#estado option:selected').text());
        $('#resumenFechaInicio').text($('#fechaInicio').val());
        $('#resumenFechaFin').text($('#fechaFin').val() || '-');
        $('#resumenCampo').text(campoSelect.find('option:selected').text());

        var totalLotes = $('#totalLotesSeleccionados').text();
        var totalSuperficie = $('#totalSuperficieSeleccionada').text();

        $('#resumenTotalLotes').text(totalLotes);
        $('#resumenTotalSuperficie').text(totalSuperficie);

        // Lista de lotes seleccionados
        var listaResumen = $('#listaResumenLotes');
        var checkboxes = $('.lote-checkbox:checked');

        if (checkboxes.length > 0) {
            var html = '<h6>Lotes incluidos:</h6><ul class="list-group">';
            checkboxes.each(function () {
                var loteId = $(this).val();
                var lote = lotesDisponibles.find(function (l) { return l.id == loteId; });
                if (lote) {
                    html += '<li class="list-group-item d-flex justify-content-between align-items-center">' +
                        lote.nombre +
                        '<span class="badge bg-primary rounded-pill">' + lote.superficie.toFixed(2) + ' Ha</span>' +
                        '</li>';
                }
            });
            html += '</ul>';
            listaResumen.html(html);
        } else {
            listaResumen.html('<div class="alert alert-warning">No se han seleccionado lotes</div>');
        }
    }

    function validarTabCampos() {
        if (!campoSelect.val()) {
            campoSelect.focus();
            mostrarError('Por favor seleccione un campo');
            return false;
        }
        return true;
    }

    function validarTabLotes() {
        if ($('.lote-checkbox:checked').length === 0) {
            mostrarError('Por favor seleccione al menos un lote');
            return false;
        }
        return true;
    }

    function validarFormularioCompleto() {
        // Validar información básica
        if (form[0].checkValidity() === false) {
            form.addClass('was-validated');
            activarTab('campos');
            return false;
        }

        // Validar campos y lotes
        if (!validarTabCampos() || !validarTabLotes()) {
            return false;
        }

        return true;
    }

    function guardarCampania() {
        var formData = form.serializeArray();
        var data = {
            Id: parseInt($('#campaniaId').val() || '0'),
            Nombre: $('#nombre').val(),
            Estado: parseInt($('#estado').val()),
            FechaInicio: $('#fechaInicio').val(),
            FechaFin: $('#fechaFin').val() || null,
            CampoId: parseInt(campoSelect.val()),
            LotesSeleccionados: $('.lote-checkbox:checked').map(function () {
                return parseInt($(this).val());
            }).get()
        };

        // Mostrar loading
        var submitBtn = form.find('button[type="submit"]');
        var originalText = submitBtn.html();
        submitBtn.html('<i class="bi bi-hourglass-split me-1"></i>Creando...').prop('disabled', true);

        // Enviar al servidor
        $.ajax({
            url: '/Campania/Crear',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    mostrarExito('Campaña creada correctamente');
                    var modal = bootstrap.Modal.getInstance($('#modalCampania')[0]);
                    modal.hide();
                    // Recargar la tabla
                    table.ajax.reload();
                } else {
                    mostrarError(result.message || 'Error al crear campaña');
                }
            },
            error: function (error) {
                console.error('Error:', error);
                mostrarError('Error al conectar con el servidor');
            },
            complete: function () {
                submitBtn.html(originalText).prop('disabled', false);
            }
        });
    }
}

// ========== FUNCIONES DE UTILIDAD ==========
function mostrarError(mensaje) {
    if (typeof toastr !== 'undefined') {
        toastr.error(mensaje);
    } else {
        alert(mensaje);
    }
}

function mostrarExito(mensaje) {
    if (typeof toastr !== 'undefined') {
        toastr.success(mensaje);
    } else {
        alert(mensaje);
    }
}