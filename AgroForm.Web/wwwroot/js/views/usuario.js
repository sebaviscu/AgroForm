$(document).ready(function () {
    inicializarDataTable();
    configurarEventos();
});

function inicializarDataTable() {
    table = $('#tblUsuarios').DataTable({
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
                    title: 'Usuarios_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Usuarios_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        ajax: {
            url: '/Usuario/GetAllDataTable',
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
                data: 'rol',
                render: function (data) {
                    // Convertir valor numérico del rol a texto
                    const roles = {
                        0: 'Administrador',
                        1: 'Usuario',
                        2: 'Invitado'
                    };
                    return roles[data] || data;
                }
            },
            {
                data: 'email'
            },
            {
                data: 'activo',
                render: function (data) {
                    return data ?
                        '<span class="badge bg-success">Activo</span>' :
                        '<span class="badge bg-danger">Inactivo</span>';
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
                                    title="Editar usuario" data-id="${data}">
                                <i class="ph ph-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-delete" 
                                    title="Eliminar usuario" data-id="${data}">
                                <i class="ph ph-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        order: [[0, 'asc']],
        pageLength: 25,
        responsive: true
    });
}

function configurarEventos() {

    $('#btnNuevoUsuario').click(function () {
        abrirModalUsuario(0, 'crear');
    });

    $('#tblUsuarios tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalUsuario(id, 'editar');
    });

    $('#tblUsuarios tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarUsuario(id);
    });

    $('#formUsuario').on('submit', function (e) {
        e.preventDefault();
        guardarUsuario();
    });

    $('#modalUsuario').on('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    // Validación de contraseñas
    $('#confirmarContrasena').on('input', function () {
        validarContrasenas();
    });

    $('#contrasenaUsuario').on('input', function () {
        validarContrasenas();
    });
}

function validarContrasenas() {
    const contrasena = $('#contrasenaUsuario').val();
    const confirmar = $('#confirmarContrasena').val();
    const confirmarInput = $('#confirmarContrasena')[0];

    if (confirmar && contrasena !== confirmar) {
        confirmarInput.setCustomValidity('Las contraseñas no coinciden');
    } else {
        confirmarInput.setCustomValidity('');
    }
}

function abrirModalUsuario(id, accion) {
    var modal = new bootstrap.Modal($('#modalUsuario')[0]);
    var titulo = $('#modalUsuarioLabel');
    var btnGuardar = $('#formUsuario button[type="submit"]');

    switch (accion) {
        case 'editar':
            titulo.html('<i class="ph ph-user me-2"></i>Editar Usuario');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Actualizar');
            cargarDatosUsuario(id);
            break;
        case 'crear':
            titulo.html('<i class="ph ph-user me-2"></i>Nuevo Usuario');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Guardar');
            limpiarFormulario();
            break;
    }

    modal.show();
}

function cargarDatosUsuario(id) {
    $.ajax({
        url: '/Usuario/GetById/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var usuario = response.data || response.object;

                // Ocultar campos de contraseña en edición
                $('#contrasenaGroup, #confirmarContrasenaGroup').hide();
                $('#contrasenaUsuario, #confirmarContrasena').removeAttr('required');

                // Llenar formulario con datos del usuario
                $('#nombreUsuario').val(usuario.nombre);
                $('#emailUsuario').val(usuario.email);
                $('#telefonoUsuario').val(usuario.phoneNumber || '');
                $('#rolUsuario').val(usuario.rol);
                $('#estadoUsuario').val(usuario.activo.toString());

                // Guardar ID en campo oculto si no existe
                if (!$('#idUsuario').length) {
                    $('<input>').attr({
                        type: 'hidden',
                        id: 'idUsuario',
                        name: 'Id'
                    }).appendTo('#formUsuario');
                }
                $('#idUsuario').val(usuario.id);

            } else {
                mostrarError(response.message || 'Error al cargar los datos del usuario');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function limpiarFormulario() {
    $('#formUsuario')[0].reset();
    $('#idUsuario').remove();
    $('#formUsuario').removeClass('was-validated');

    // Mostrar campos de contraseña en creación
    $('#contrasenaGroup, #confirmarContrasenaGroup').show();
    $('#contrasenaUsuario, #confirmarContrasena').attr('required', 'required');

    // Limpiar validaciones personalizadas
    $('#confirmarContrasena')[0].setCustomValidity('');
}

// ========== OPERACIONES CRUD ==========
function guardarUsuario() {
    var form = $('#formUsuario')[0];

    // Validación del formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    // Validar que las contraseñas coincidan (solo en creación)
    if (!$('#idUsuario').length || $('#idUsuario').val() === '') {
        const contrasena = $('#contrasenaUsuario').val();
        const confirmar = $('#confirmarContrasena').val();
        if (contrasena !== confirmar) {
            $('#confirmarContrasena')[0].setCustomValidity('Las contraseñas no coinciden');
            form.classList.add('was-validated');
            return;
        }
    }

    var datos = {
        Id: $('#idUsuario').val() ? parseInt($('#idUsuario').val()) : 0,
        Nombre: $('#nombreUsuario').val().trim(),
        Email: $('#emailUsuario').val().trim(),
        PhoneNumber: $('#telefonoUsuario').val().trim() || null,
        Rol: parseInt($('#rolUsuario').val()),
        Activo: $('#estadoUsuario').val() === 'true'
    };

    // Solo incluir contraseña si es un nuevo usuario
    if (!datos.Id) {
        datos.Password = $('#contrasenaUsuario').val();
    }

    // Mostrar loading
    var submitBtn = $('#formUsuario button[type="submit"]');
    var originalText = submitBtn.html();
    submitBtn.html('<i class="ph ph-hourglass me-1"></i>' + (datos.Id ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);

    var url = datos.Id ? '/Usuario/Update' : '/Usuario/Create';
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
                mostrarExito(response.message || (datos.Id ? 'Usuario actualizado correctamente' : 'Usuario creado correctamente'));
                $('#modalUsuario').modal('hide');
                table.ajax.reload();
            } else {
                mostrarError(response.message || 'Error al guardar el usuario');
            }
        },
        error: function (xhr, status, error) {
            mostrarError('Error al conectar con el servidor: ' + (xhr.responseJSON?.message || error));
        },
        complete: function () {
            submitBtn.html(originalText).prop('disabled', false);
        }
    });
}

function eliminarUsuario(id) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este usuario?',
        'Eliminar Usuario'
    ).then((result) => {
        if (result.isConfirmed) {
            mostrarLoading('Eliminando usuario...');

            $.ajax({
                url: '/Usuario/Delete/' + id,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas();

                    if (response.success) {
                        mostrarExito(response.message || 'Usuario eliminado correctamente');
                        table.ajax.reload();
                    } else {
                        mostrarError(response.message || 'Error al eliminar el usuario');
                    }
                },
                error: function (xhr, status, error) {
                    cerrarAlertas();
                    console.error('Error:', error);
                    mostrarError('Error al conectar con el servidor: ' + (xhr.responseJSON?.message || error));
                }
            });
        }
    });
}
