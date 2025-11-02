// Variables globales
var table;

$(document).ready(function () {
    inicializarDataTable();
    configurarEventos();
});

// ========== DATATABLE DE CAMPOS ==========
function inicializarDataTable() {
    table = $('#tblCampos').DataTable({
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
                    title: 'Campos_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Campos_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        ajax: {
            url: '/Campo/GetAllDataTable',
            type: 'GET',
            dataType: 'json',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            {
                data: 'nombre',
                className: 'fw-semibold'
            },
            {
                data: 'ubicacion',
                render: function (data, type, row) {
                    return data || '<span class="text-muted">No especificada</span>';
                }
            },
            {
                data: 'superficieHectareas',
                className: 'text-end',
                render: function (data, type, row) {
                    if (data) {
                        return parseFloat(data).toLocaleString('es-ES', {
                            minimumFractionDigits: 2,
                            maximumFractionDigits: 2
                        }) + ' Ha';
                    }
                    return '<span class="text-muted">-</span>';
                }
            },
            {
                data: 'id',
                className: 'text-center',
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-primary btn-edit" 
                                    title="Editar campo" data-id="${data}">
                                <i class="ph ph-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-delete" 
                                    title="Eliminar campo" data-id="${data}">
                                <i class="ph ph-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        columnDefs: [
            { width: '35%', targets: 0 },
            { width: '35%', targets: 1 },
            { width: '15%', targets: 2 },
            { width: '15%', targets: 3 }
        ],
        order: [[0, 'asc']],
        pageLength: 25,
        responsive: true,
        drawCallback: function (settings) {
            // Actualizar contadores después de dibujar la tabla
            var api = this.api();
            var total = api.rows({ search: 'applied' }).count();
            $('#totalCampos').text(total);
        }
    });
}

// ========== CONFIGURACIÓN DE EVENTOS ==========
function configurarEventos() {
    // Botón nuevo campo
    $('#btnNuevoCampo').click(function () {
        abrirModalCampo(0, 'crear');
    });

    // Delegación de eventos para botones dinámicos
    $('#tblCampos tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalCampo(id, 'editar');
    });

    $('#tblCampos tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarCampo(id);
    });

    // Validación del formulario
    $('#formCampo').on('submit', function (e) {
        e.preventDefault();
        guardarCampo();
    });

    // Resetear formulario al cerrar modal
    $('#modalCampo').on('hidden.bs.modal', function () {
        limpiarFormulario();
    });
}

// ========== FUNCIONES DEL MODAL ==========
function abrirModalCampo(id, accion) {
    var modal = new bootstrap.Modal($('#modalCampo')[0]);
    var titulo = $('#modalCampoLabel');
    var btnGuardar = $('#btnGuardarCampo');

    switch (accion) {
        case 'editar':
            titulo.text('Editar Campo');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Actualizar');
            cargarDatosCampo(id);
            break;
        case 'crear':
            titulo.text('Nuevo Campo');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Guardar');
            limpiarFormulario();
            break;
    }

    modal.show();
}

function cargarDatosCampo(id) {
    $.ajax({
        url: '/Campo/GetById/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var campo = response.object;
                $('#idCampo').val(campo.id);
                $('#nombre').val(campo.nombre);
                $('#ubicacion').val(campo.ubicacion || '');
                $('#superficieHectareas').val(campo.superficieHectareas || '');
            } else {
                mostrarError(response.message || 'Error al cargar los datos del campo');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function limpiarFormulario() {
    $('#formCampo')[0].reset();
    $('#idCampo').val('');
    $('#formCampo').removeClass('was-validated');
}

// ========== OPERACIONES CRUD ==========
function guardarCampo() {
    var form = $('#formCampo')[0];

    // Validación del formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    var datos = {
        Id: $('#idCampo').val() ? parseInt($('#idCampo').val()) : 0,
        Nombre: $('#nombre').val().trim(),
        Ubicacion: $('#ubicacion').val().trim() || null,
        SuperficieHectareas: $('#superficieHectareas').val() ? parseFloat($('#superficieHectareas').val()) : null
    };

    // Validación adicional
    if (!datos.Nombre) {
        mostrarError('El nombre del campo es obligatorio');
        return;
    }

    // Validación adicional
    if (!datos.SuperficieHectareas) {
        mostrarError('La superficie del campo es obligatorio');
        return;
    }

    // Mostrar loading
    var submitBtn = $('#btnGuardarCampo');
    var originalText = submitBtn.html();
    submitBtn.html('<i class="ph ph-hourglass me-1"></i>Guardando...').prop('disabled', true);

    var url = datos.Id ? '/Campo/Update' : '/Campo/Create';
    var metodo = datos.Id ? 'PUT' : 'POST';

    $.ajax({
        url: url,
        type: metodo,
        contentType: 'application/json',
        data: JSON.stringify(datos),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                mostrarExito(response.message || (datos.Id ? 'Campo actualizado correctamente' : 'Campo creado correctamente'));
                $('#modalCampo').modal('hide');
                table.ajax.reload();
            } else {
                mostrarError(response.message || 'Error al guardar el campo');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error:', error);
            mostrarError('Error al conectar con el servidor');
        },
        complete: function () {
            submitBtn.html(originalText).prop('disabled', false);
        }
    });
}

function eliminarCampo(id) {

    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este campo? Esta acción no se puede deshacer.',
        'Eliminar Campo'
    ).then((result) => {
        if (result.isConfirmed) {

            mostrarLoading('Eliminando campo...');

            $.ajax({
                url: '/Campo/Eliminar/' + id,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas(); // Cerrar loading

                    if (response.success) {
                        mostrarExito(response.message || 'Campo eliminado correctamente');
                        table.ajax.reload();
                    } else {
                        mostrarError(response.message || 'Error al eliminar el campo');
                    }
                },
                error: function (xhr, status, error) {
                    cerrarAlertas(); // Cerrar loading
                    console.error('Error:', error);
                    mostrarError('Error al conectar con el servidor');
                }
            });
        }
    });
}
