let tableCultivos;
let tableVariedades;

$(document).ready(function () {
    inicializarDataTableCategorias();
    configurarEventosCategorias();
});

function inicializarDataTableCategorias() {
    // Tabla Cultivos
    tableCultivos = $('#tblCultivos').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        ajax: {
            url: '/Cultivo/GetAllDataTable',
            type: 'GET',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            { data: 'nombre', className: 'fw-bold' },
            { data: 'orden', className: 'text-center' },
            { 
                data: 'color',
                render: function(data) {
                    if (!data) return '<span class="text-muted">-</span>';
                    return `<div class="d-flex align-items-center cultivo-color-box">
                        <div class="cultivo-color-display" style="--cultivo-color: ${data}; width: 20px; height: 20px; border-radius: 3px; margin-right: 8px;"></div>
                        <span>${data}</span>
                    </div>`;
                }
            },
            { 
                data: 'activo',
                render: function (data) {
                    return data ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-danger">Inactivo</span>';
                }
            },
            {
                data: 'id',
                className: 'text-center',
                render: function (data) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary btn-edit-cultivo" data-id="${data}"><i class="ph ph-pencil"></i></button>
                            <button class="btn btn-outline-danger btn-delete-cultivo" data-id="${data}"><i class="ph ph-trash"></i></button>
                        </div>`;
                }
            }
        ],
        pageLength: 10
    });

    // Tabla Variedades
    tableVariedades = $('#tblVariedades').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        ajax: {
            url: '/Variedad/GetAllDataTable',
            type: 'GET',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            { 
                data: null,
                render: function (data) {
                    // Intentar obtener el nombre del cultivo de varias formas
                    if (data.cultivo && data.cultivo.nombre) return data.cultivo.nombre;
                    if (data.nombreCultivo) return data.nombreCultivo;
                    return '<span class="text-muted">N/A</span>';
                }
            },
            { data: 'nombre', className: 'fw-bold' },
            { 
                data: 'tipo',
                render: function (data) {
                    const tipos = { 0: 'Variedad', 1: 'Subproducto', 2: 'Descarte' };
                    return tipos[data] || data;
                }
            },
            { 
                data: 'activo',
                render: function (data) {
                    return data ? '<span class="badge bg-success">Activo</span>' : '<span class="badge bg-danger">Inactivo</span>';
                }
            },
            {
                data: 'id',
                className: 'text-center',
                render: function (data) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary btn-edit-variedad" data-id="${data}"><i class="ph ph-pencil"></i></button>
                            <button class="btn btn-outline-danger btn-delete-variedad" data-id="${data}"><i class="ph ph-trash"></i></button>
                        </div>`;
                }
            }
        ],
        pageLength: 10
    });
}

function configurarEventosCategorias() {
    // Cultivos
    $('#btnNuevoCultivo').click(() => abrirModalCultivo());
    $('#tblCultivos tbody').on('click', '.btn-edit-cultivo', function() {
        cargarCultivo($(this).data('id'));
    });
    $('#tblCultivos tbody').on('click', '.btn-delete-cultivo', function() {
        eliminarEntidad('/Cultivo/Delete/', $(this).data('id'), tableCultivos);
    });
    $('#formCultivo').off('submit').on('submit', function(e) {
        e.preventDefault();
        guardarEntidad('/Cultivo/', 'idCultivo', 'formCultivo', tableCultivos, '#modalCultivo');
    });

    // Sincronización entre input de texto y color picker
    $('#colorCultivo').on('input', function() {
        const color = $(this).val();
        if (/^#[0-9A-Fa-f]{6}$/.test(color)) {
            $('#colorCultivoPicker').val(color);
        }
    });

    $('#colorCultivoPicker').on('input', function() {
        $('#colorCultivo').val($(this).val());
    });

    // Variedades
    $('#btnNuevaVariedad').click(() => abrirModalVariedad());
    $('#tblVariedades tbody').on('click', '.btn-edit-variedad', function() {
        cargarVariedad($(this).data('id'));
    });
    $('#tblVariedades tbody').on('click', '.btn-delete-variedad', function() {
        eliminarEntidad('/Variedad/Delete/', $(this).data('id'), tableVariedades);
    });
    $('#formVariedad').off('submit').on('submit', function(e) {
        e.preventDefault();
        guardarEntidad('/Variedad/', 'idVariedad', 'formVariedad', tableVariedades, '#modalVariedad');
    });
}

function abrirModalCultivo() {
    $('#idCultivo').val('');
    $('#formCultivo')[0].reset();
    $('#activoCultivo').prop('checked', true);
    $('#colorCultivoPicker').val('#FF5733');
    $('#modalCultivo').modal('show');
}

function cargarCultivo(id) {
    $.get('/Cultivo/GetById/' + id, (res) => {
        const data = res.object || res.data;
        if(res.success && data) {
            $('#idCultivo').val(data.id);
            $('#nombreCultivo').val(data.nombre);
            $('#ordenCultivo').val(data.orden);
            $('#colorCultivo').val(data.color || '');
            $('#colorCultivoPicker').val(data.color || '#FF5733');
            $('#activoCultivo').prop('checked', data.activo);
            $('#modalCultivo').modal('show');
        } else {
            mostrarError(res.message || 'Error al cargar cultivo');
        }
    });
}

async function abrirModalVariedad() {
    $('#idVariedad').val('');
    $('#formVariedad')[0].reset();
    $('#activoVariedad').prop('checked', true);
    await cargarComboCultivos();
    $('#modalVariedad').modal('show');
}

async function cargarComboCultivos() {
    try {
        const res = await $.get('/Cultivo/GetAll');
        const list = res.listObject || res.ListObject || res.data || res.object;
        
        if(res.success && list && Array.isArray(list)) {
            const select = $('#idCultivoVariedad');
            select.empty().append('<option value="">Seleccione un cultivo</option>');
            list.forEach(c => {
                select.append(`<option value="${c.id}">${c.nombre}</option>`);
            });
        }
    } catch (err) {
        console.error('Error loading crops:', err);
    }
}

function cargarVariedad(id) {
    $.get('/Variedad/GetById/' + id, async (res) => {
        const data = res.object || res.data;
        if(res.success && data) {
            await cargarComboCultivos();
            $('#idVariedad').val(data.id);
            $('#idCultivoVariedad').val(data.idCultivo);
            $('#nombreVariedad').val(data.nombre);
            $('#tipoVariedad').val(data.tipo);
            $('#descripcionVariedad').val(data.descripcion);
            $('#activoVariedad').prop('checked', data.activo);
            $('#modalVariedad').modal('show');
        } else {
            mostrarError(res.message || 'Error al cargar variedad');
        }
    });
}

function guardarEntidad(baseUrl, idField, formId, table, modalSelector) {
    const id = $('#' + idField).val();
    const formData = $(`#${formId}`).serializeArray();
    const data = {};
    
    formData.forEach(x => {
        data[x.name] = x.value;
    });
    
    // Forzar tipos numéricos
    if (data.IdCultivo) data.IdCultivo = parseInt(data.IdCultivo);
    if (data.Tipo) data.Tipo = parseInt(data.Tipo);
    if (data.Orden) data.Orden = parseInt(data.Orden);
    
    // Manejo de checkbox para booleanos
    data.Activo = $(`#${formId} [name="Activo"]`).is(':checked');

    const url = id ? baseUrl + 'Update' : baseUrl + 'Create';
    const method = id ? 'PUT' : 'POST';
    
    if (id) data.Id = parseInt(id);

    $.ajax({
        url: url,
        type: method,
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: function(res) {
            if(res.success) {
                $(modalSelector).modal('hide');
                table.ajax.reload();
                mostrarMensaje('Guardado correctamente');
            } else {
                mostrarError(res.message || 'Error al guardar');
            }
        },
        error: function(xhr) {
            mostrarError('Error en la comunicación con el servidor');
        }
    });
}

function eliminarEntidad(url, id, table) {
    mostrarConfirmacion('¿Desea eliminar este registro?').then(result => {
        if(result.isConfirmed) {
            $.ajax({
                url: url + id,
                type: 'DELETE',
                success: function(res) {
                    if(res.success) {
                        table.ajax.reload();
                        mostrarExito('Eliminado correctamente');
                    } else {
                        mostrarError(res.message || 'Error al eliminar');
                    }
                }
            });
        }
    });
}
