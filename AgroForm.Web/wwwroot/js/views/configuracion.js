// Variables de estado
let cultivosData = [];
let catalogosData = [];
let currentFilter = 'all';

// ============================================================================
// INICIALIZACIÓN
// ============================================================================
$(document).ready(function () {
    // Cargar cultivos al mostrar el tab
    $('#cultivos-tab').on('shown.bs.tab', function () {
        cargarCultivosConfig();
    });

    // Cargar catálogos al mostrar el tab
    $('#catalogos-tab').on('shown.bs.tab', function () {
        cargarCatalogosConfig();
    });

    // Cargar cultivos por defecto (primer tab activo)
    cargarCultivosConfig();

    // Evento filtro catálogos
    $('#tipoFilter').on('change', function () {
        currentFilter = $(this).val();
        renderCatalogos();
    });

    // Eventos modal Cultivo (reusable: add / edit)
    $('#btnNuevoCultivo').on('click', function () {
        abrirModalCultivo(); // no id = crear nuevo
    });

    $('#btnGuardarCultivo').on('click', function () {
        guardarCultivo();
    });

    // Eventos modal Catálogo (reusable: add / edit)
    $('#btnNuevoCatalogo').on('click', function () {
        abrirModalCatalogo(); // no id = crear nuevo
    });

    $('#btnGuardarCatalogo').on('click', function () {
        guardarCatalogo();
    });

    $('#btnGuardarEstadoFenologico').on('click', function () {
        guardarEstadoFenologico();
    });

    // Limpiar modales al cerrar
    $('#modalCultivo').on('hidden.bs.modal', function () {
        $('#formCultivo')[0].reset();
        $('#cultivoEditId').val('');
        $('#cultivoColor').val('#4CAF50');
    });

    $('#modalNuevoEstadoFenologico').on('hidden.bs.modal', function () {
        $('#formNuevoEstadoFenologico')[0].reset();
    });

    $('#modalCatalogo').on('hidden.bs.modal', function () {
        $('#formCatalogo')[0].reset();
        $('#catalogoEditId').val('');
    });
});

// ============================================================================
// CULTIVOS
// ============================================================================

function cargarCultivosConfig() {
    $('#cultivosLoading').show();
    $('#cultivosContainer').hide();
    $('#cultivosEmpty').hide();

    $.ajax({
        url: '/Configuracion/GetCultivosConfig',
        method: 'GET',
        success: function (response) {
            $('#cultivosLoading').hide();

            if (response.success && response.listObject && response.listObject.length > 0) {
                // Sort by orden (default to 999 if null so they appear last)
                cultivosData = response.listObject.sort(function (a, b) {
                    var ordenA = a.orden || 999;
                    var ordenB = b.orden || 999;
                    return ordenA - ordenB;
                });
                renderCultivos();
                $('#cultivosContainer').show();
            } else {
                $('#cultivosEmpty').show();
            }
        },
        error: function (xhr) {
            $('#cultivosLoading').hide();
            var msg = 'Error al cargar cultivos';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
            $('#cultivosEmpty').show();
        }
    });
}

function renderCultivos() {
    var $container = $('#cultivosList');
    $container.empty();

    if (cultivosData.length === 0) {
        $container.append('<p class="text-muted small mb-0">No hay cultivos disponibles.</p>');
        return;
    }

    // Build grid container
    var html = '<div class="cultivo-grid">';

    cultivosData.forEach(function (cultivo) {
        var checked = cultivo.isVisible ? 'checked' : '';
        var colorStyle = cultivo.color ? 'style="border-left: 3px solid ' + cultivo.color + ';"' : '';
        var ordenText = cultivo.orden ? '<span class="crop-orden">Ord. ' + cultivo.orden + '</span>' : '';

        // Edit button only for user-added items (isGlobal == false)
        var editBtn = '';
        if (!cultivo.isGlobal) {
            editBtn = '<button class="edit-item-btn" onclick="abrirModalCultivo(' + cultivo.id + ')" title="Editar cultivo">' +
                '<i class="ph ph-pencil"></i></button>';
        }

        html +=
            '<div class="crop-group" data-id="' + cultivo.id + '">' +
            '    <div class="crop-header" data-id="' + cultivo.id + '" ' + colorStyle + '>' +
            '        <i class="ph ph-caret-right expand-icon"></i>' +
            '        <div class="form-check form-check-inline mb-0" onclick="event.stopPropagation();">' +
            '            <input class="form-check-input cultivo-checkbox" type="checkbox" ' + checked +
            '                   data-id="' + cultivo.id + '" data-visible="' + cultivo.isVisible + '">' +
            '        </div>' +
            '        <span class="config-item-label">' + cultivo.nombre + '</span>' +
            ordenText +
            editBtn +
            '    </div>' +
            '    <div class="estados-container" id="estados-' + cultivo.id + '" style="display:none;">' +
            '        <div class="estados-loading text-muted small" style="padding: 0.5rem;">Cargando...</div>' +
            '    </div>' +
            '</div>';
    });

    html += '</div>';
    $container.append(html);

    // Evento toggle cultivo
    $container.find('.cultivo-checkbox').on('change', function () {
        var id = $(this).data('id');
        var wasVisible = $(this).data('visible');
        var newVisible = $(this).is(':checked');
        toggleCultivo(id, newVisible, $(this));
    });

    // Evento expandir/colapsar estados
    $container.find('.crop-header').on('click', function () {
        var id = $(this).data('id');
        var $estadosContainer = $('#estados-' + id);
        var $icon = $(this).find('.expand-icon');

        if ($estadosContainer.is(':visible')) {
            $estadosContainer.slideUp(200);
            $icon.removeClass('expanded');
        } else {
            $icon.addClass('expanded');
            $estadosContainer.slideDown(200);
            cargarEstadosFenologicos(id);
        }
    });
}

function toggleCultivo(id, visible, $checkbox) {
    $.ajax({
        url: '/Configuracion/ToggleCultivoVisibilidad',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: id, visible: visible }),
        success: function (response) {
            if (response.success) {
                $checkbox.data('visible', visible);
                // Update isVisible in our data array
                cultivosData.forEach(function (c) {
                    if (c.id === id) c.isVisible = visible;
                });
                mostrarMensaje('Cultivo actualizado correctamente', 'success')
            } else {
                // Revertir checkbox
                $checkbox.prop('checked', !visible);
                mostrarError(response.message || 'Error al actualizar visibilidad');
            }
        },
        error: function (xhr) {
            // Revertir checkbox
            $checkbox.prop('checked', !visible);
            var msg = 'Error al actualizar visibilidad';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
        }
    });
}

// ============================================================================
// MODAL CULTIVO (reusable: add / edit)
// ============================================================================

function abrirModalCultivo(id) {
    if (id) {
        // EDIT mode: load data from server
        $.ajax({
            url: '/Configuracion/GetCultivo/' + id,
            method: 'GET',
            success: function (response) {
                if (response.success && response.object) {
                    var c = response.object;
                    $('#cultivoEditId').val(c.id);
                    $('#cultivoNombre').val(c.nombre);
                    $('#cultivoDescripcion').val(c.descripcion || '');
                    $('#cultivoColor').val(c.color || '#4CAF50');
                    $('#cultivoOrden').val(c.orden || 1);

                    $('#modalCultivoTitleText').text('Editar Cultivo');
                    $('#btnGuardarCultivo').html('<i class="ph ph-check-circle me-1"></i> Actualizar');

                    $('#modalCultivo').modal('show');
                } else {
                    mostrarError(response.message || 'Error al cargar datos del cultivo');
                }
            },
            error: function () {
                mostrarError('Error al cargar datos del cultivo');
            }
        });
    } else {
        // CREATE mode: clear form
        $('#formCultivo')[0].reset();
        $('#cultivoEditId').val('');
        $('#cultivoColor').val('#4CAF50');
        $('#cultivoOrden').val(1);

        $('#modalCultivoTitleText').text('Nuevo Cultivo');
        $('#btnGuardarCultivo').html('<i class="ph ph-check-circle me-1"></i> Guardar');

        $('#modalCultivo').modal('show');
    }
}

function guardarCultivo() {
    var nombre = $('#cultivoNombre').val().trim();
    if (!nombre) {
        mostrarError('El nombre del cultivo es requerido');
        return;
    }

    var editId = $('#cultivoEditId').val();
    var isEditing = editId && parseInt(editId) > 0;

    var data = {
        id: isEditing ? parseInt(editId) : 0,
        nombre: nombre,
        descripcion: $('#cultivoDescripcion').val().trim() || null,
        color: $('#cultivoColor').val() || null,
        orden: parseInt($('#cultivoOrden').val()) || 1
    };

    var url = isEditing ? '/Configuracion/UpdateCultivo' : '/Configuracion/CreateCultivo';

    $('#btnGuardarCultivo').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Guardando...');

    $.ajax({
        url: url,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            $('#btnGuardarCultivo').prop('disabled', false).html('<i class="ph ph-check-circle me-1"></i> Guardar');
            $('#modalCultivo').modal('hide');

            if (response.success) {
                mostrarExito(isEditing ? 'Cultivo actualizado correctamente' : 'Cultivo creado correctamente', 'success');
                cargarCultivosConfig();
            } else {
                mostrarError(response.message || 'Error al guardar cultivo');
            }
        },
        error: function (xhr) {
            $('#btnGuardarCultivo').prop('disabled', false).html('<i class="ph ph-check-circle me-1"></i> Guardar');
            var msg = 'Error al guardar cultivo';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
        }
    });
}

// ============================================================================
// ESTADOS FENOLÓGICOS
// ============================================================================

function cargarEstadosFenologicos(idCultivo) {
    var $container = $('#estados-' + idCultivo);
    $container.find('.estados-loading').show();

    $.ajax({
        url: '/Configuracion/GetEstadosFenologicos/' + idCultivo,
        method: 'GET',
        success: function (response) {
            $container.find('.estados-loading').hide();

            if (response.success && response.listObject) {
                renderEstadosFenologicos(idCultivo, response.listObject);
            } else {
                $container.html('<p class="text-muted small mb-0">No hay estados fenológicos.</p>');
            }
        },
        error: function () {
            $container.find('.estados-loading').hide();
            $container.html('<p class="text-danger small mb-0">Error al cargar estados.</p>');
        }
    });
}

function renderEstadosFenologicos(idCultivo, estados) {
    var $container = $('#estados-' + idCultivo);
    $container.empty();

    if (estados.length === 0) {
        $container.html('<p class="text-muted small mb-0">No hay estados fenológicos para este cultivo.</p>');
        // Botón añadir estado
        $container.append(
            '<button class="btn btn-sm btn-outline-primary add-estado-btn" onclick="abrirModalEstadoFenologico(' + idCultivo + ')">' +
            '<i class="ph ph-plus-circle me-1"></i>Nuevo Estado Fenológico</button>'
        );
        return;
    }

    estados.forEach(function (estado) {
        var checked = estado.activo ? 'checked' : '';
        var html =
            '<div class="estado-item">' +
            '    <div class="form-check form-check-inline mb-0">' +
            '        <input class="form-check-input estado-checkbox" type="checkbox" ' + checked +
            '               data-id="' + estado.id + '">' +
            '    </div>' +
            '    <span class="estado-item-label">' +
            '        <strong>' + estado.codigo + '</strong> - ' + estado.nombre +
            (estado.descripcion ? '<br><small class="text-muted">' + estado.descripcion + '</small>' : '') +
            '    </span>' +
            '</div>';
        $container.append(html);
    });

    // Botón añadir estado
    $container.append(
        '<button class="btn btn-sm btn-outline-primary add-estado-btn" onclick="abrirModalEstadoFenologico(' + idCultivo + ')">' +
        '<i class="ph ph-plus-circle me-1"></i>Nuevo Estado Fenológico</button>'
    );

    // Evento toggle estado
    $container.find('.estado-checkbox').on('change', function () {
        var id = $(this).data('id');
        toggleEstadoFenologico(id, $(this));
    });
}

function toggleEstadoFenologico(id, $checkbox) {
    var wasChecked = $checkbox.is(':checked');

    $.ajax({
        url: '/Configuracion/ToggleEstadoFenologico',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: id }),
        success: function (response) {
            if (response.success) {
                // No need to update data, state toggles server-side
            } else {
                $checkbox.prop('checked', !wasChecked);
                mostrarError(response.message || 'Error al actualizar estado');
            }
        },
        error: function () {
            $checkbox.prop('checked', !wasChecked);
            mostrarError('Error al actualizar estado fenológico');
        }
    });
}

function abrirModalEstadoFenologico(idCultivo) {
    // Buscar el nombre del cultivo
    var cultivo = null;
    cultivosData.forEach(function (c) {
        if (c.id === idCultivo) cultivo = c;
    });

    if (!cultivo) return;

    $('#estadoIdCultivo').val(idCultivo);
    $('#estadoCultivoNombre').text(cultivo.nombre);
    $('#modalNuevoEstadoFenologico').modal('show');
}

function guardarEstadoFenologico() {
    var idCultivo = parseInt($('#estadoIdCultivo').val());
    var codigo = $('#estadoCodigo').val().trim();
    var nombre = $('#estadoNombre').val().trim();

    if (!codigo) {
        mostrarError('El código del estado fenológico es requerido');
        return;
    }
    if (!nombre) {
        mostrarError('El nombre del estado fenológico es requerido');
        return;
    }

    var data = {
        idCultivo: idCultivo,
        codigo: codigo,
        nombre: nombre,
        descripcion: $('#estadoDescripcion').val().trim() || null
    };

    $('#btnGuardarEstadoFenologico').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Guardando...');

    $.ajax({
        url: '/Configuracion/CreateEstadoFenologico',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            $('#btnGuardarEstadoFenologico').prop('disabled', false).html('<i class="ph ph-check-circle me-1"></i> Guardar');
            $('#modalNuevoEstadoFenologico').modal('hide');

            if (response.success) {
                mostrarExito('Estado fenológico creado correctamente');
                // Recargar estados del cultivo
                cargarEstadosFenologicos(idCultivo);
            } else {
                mostrarError(response.message || 'Error al crear estado fenológico');
            }
        },
        error: function (xhr) {
            $('#btnGuardarEstadoFenologico').prop('disabled', false).html('<i class="ph ph-check-circle me-1"></i> Guardar');
            var msg = 'Error al crear estado fenológico';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
        }
    });
}

// ============================================================================
// CATÁLOGOS
// ============================================================================

function cargarCatalogosConfig() {
    $('#catalogosLoading').show();
    $('#catalogosContainer').hide();
    $('#catalogosEmpty').hide();

    $.ajax({
        url: '/Configuracion/GetCatalogosConfig',
        method: 'GET',
        success: function (response) {
            $('#catalogosLoading').hide();

            if (response.success && response.listObject && response.listObject.length > 0) {
                catalogosData = response.listObject;
                renderCatalogos();
                $('#catalogosContainer').show();
            } else {
                $('#catalogosEmpty').show();
            }
        },
        error: function (xhr) {
            $('#catalogosLoading').hide();
            var msg = 'Error al cargar catálogos';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
            $('#catalogosEmpty').show();
        }
    });
}

function renderCatalogos() {
    var filtered = catalogosData;

    // Aplicar filtro de tipo
    if (currentFilter !== 'all') {
        var tipoNum = parseInt(currentFilter);
        filtered = filtered.filter(function (c) { return c.tipo === tipoNum; });
    }

    // Sort by tipo, then within each tipo alphabetically by nombre
    filtered.sort(function (a, b) {
        if (a.tipo !== b.tipo) return a.tipo - b.tipo;
        if (a.nombre < b.nombre) return -1;
        if (a.nombre > b.nombre) return 1;
        return 0;
    });

    renderCatalogosLista(filtered);

    if (filtered.length === 0) {
        $('#catalogosEmpty').show();
        $('#catalogosContainer').hide();
    }
}

function renderCatalogosLista(catalogos) {
    var $container = $('#catalogosList');
    $container.empty();

    if (catalogos.length === 0) {
        $container.html('<p class="text-muted small mb-0">No hay catálogos' +
            (currentFilter !== 'all' ? ' para este tipo' : '') + '.</p>');
        return;
    }

    // Agrupar por tipo
    var grouped = {};
    catalogos.forEach(function (cat) {
        if (!grouped[cat.tipo]) {
            grouped[cat.tipo] = [];
        }
        grouped[cat.tipo].push(cat);
    });

    // Ordenar tipos
    var sortedTipos = Object.keys(grouped).sort(function (a, b) {
        return parseInt(a) - parseInt(b);
    });

    sortedTipos.forEach(function (tipoKey) {
        var items = grouped[tipoKey];
        var tipoDisplay = items[0] ? items[0].tipoDisplay : 'Tipo ' + tipoKey;

        var html = '<div class="catalogo-tipo-group">' +
            '    <div class="catalogo-tipo-title">' +
            '        <i class="ph ph-tag me-1"></i>' + tipoDisplay +
            '    </div>' +
            '    <div class="catalogo-grid">';

        items.forEach(function (cat) {
            var checked = cat.isVisible ? 'checked' : '';

            // Edit button only for user-added items (isGlobal == false)
            var editBtn = '';
            if (!cat.isGlobal) {
                editBtn = '<button class="edit-item-btn" onclick="abrirModalCatalogo(' + cat.id + ')" title="Editar catálogo">' +
                    '<i class="ph ph-pencil"></i></button>';
            }

            html +=
                '<div class="catalogo-item">' +
                '    <div class="form-check form-check-inline mb-0">' +
                '        <input class="form-check-input catalogo-checkbox" type="checkbox" ' + checked +
                '               data-id="' + cat.id + '" data-visible="' + cat.isVisible + '">' +
                '    </div>' +
                '    <span class="catalogo-item-label">' + cat.nombre + '</span>' +
                editBtn +
                '</div>';
        });

        html += '</div></div>';
        $container.append(html);
    });

    // Eventos toggle
    $container.find('.catalogo-checkbox').on('change', function () {
        var id = $(this).data('id');
        var wasVisible = $(this).data('visible');
        var newVisible = $(this).is(':checked');
        toggleCatalogo(id, newVisible, $(this));
    });
}

function toggleCatalogo(id, visible, $checkbox) {
    $.ajax({
        url: '/Configuracion/ToggleCatalogoVisibilidad',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ id: id, visible: visible }),
        success: function (response) {
            if (response.success) {
                $checkbox.data('visible', visible);
                // Update isVisible in our data array
                catalogosData.forEach(function (c) {
                    if (c.id === id) c.isVisible = visible;
                });
                mostrarMensaje('Catalogo actualizado correctamente', 'success')
            } else {
                $checkbox.prop('checked', !visible);
                mostrarError(response.message || 'Error al actualizar visibilidad');
            }
        },
        error: function () {
            $checkbox.prop('checked', !visible);
            mostrarError('Error al actualizar visibilidad');
        }
    });
}

// ============================================================================
// MODAL CATÁLOGO (reusable: add / edit)
// ============================================================================

function abrirModalCatalogo(id) {
    if (id) {
        // EDIT mode: load data from server
        $.ajax({
            url: '/Configuracion/GetCatalogo/' + id,
            method: 'GET',
            success: function (response) {
                if (response.success && response.object) {
                    var c = response.object;
                    $('#catalogoEditId').val(c.id);
                    $('#catalogoTipo').val(c.tipo);
                    $('#catalogoNombre').val(c.nombre);
                    $('#catalogoDescripcion').val(c.descripcion || '');

                    $('#modalCatalogoTitleText').text('Editar Catálogo');
                    $('#btnGuardarCatalogo').html('<i class="ph ph-check-circle me-1"></i> Actualizar');

                    $('#modalCatalogo').modal('show');
                } else {
                    mostrarError(response.message || 'Error al cargar datos del catálogo');
                }
            },
            error: function () {
                mostrarError('Error al cargar datos del catálogo');
            }
        });
    } else {
        // CREATE mode: clear form
        $('#formCatalogo')[0].reset();
        $('#catalogoEditId').val('');

        $('#modalCatalogoTitleText').text('Nuevo Catálogo');
        $('#btnGuardarCatalogo').html('<i class="ph ph-check-circle me-1"></i> Guardar');

        $('#modalCatalogo').modal('show');
    }
}

function guardarCatalogo() {
    var tipo = parseInt($('#catalogoTipo').val());
    var nombre = $('#catalogoNombre').val().trim();

    if (!$('#catalogoTipo').val()) {
        mostrarError('Debe seleccionar un tipo de catálogo');
        return;
    }
    if (!nombre) {
        mostrarError('El nombre del catálogo es requerido');
        return;
    }

    var editId = $('#catalogoEditId').val();
    var isEditing = editId && parseInt(editId) > 0;

    var data = {
        id: isEditing ? parseInt(editId) : 0,
        tipo: tipo,
        nombre: nombre,
        descripcion: $('#catalogoDescripcion').val().trim() || null
    };

    var url = isEditing ? '/Configuracion/UpdateCatalogo' : '/Configuracion/CreateCatalogo';

    $('#btnGuardarCatalogo').prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Guardando...');

    $.ajax({
        url: url,
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function (response) {
            $('#btnGuardarCatalogo').prop('disabled', false).html('<i class="ph ph-check-circle me-1"></i> Guardar');
            $('#modalCatalogo').modal('hide');

            if (response.success) {
                mostrarMensaje(isEditing ? 'Catálogo actualizado correctamente' : 'Catálogo creado correctamente', 'success');
                cargarCatalogosConfig();
            } else {
                mostrarError(response.message || 'Error al guardar catálogo');
            }
        },
        error: function (xhr) {
            $('#btnGuardarCatalogo').prop('disabled', false).html('<i class="ph ph-check-circle me-1"></i> Guardar');
            var msg = 'Error al guardar catálogo';
            try {
                var resp = JSON.parse(xhr.responseText);
                msg = resp.message || msg;
            } catch (e) { }
            mostrarError(msg);
        }
    });
}
