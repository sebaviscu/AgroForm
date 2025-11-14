let table;
let tablePagos;
let tableCatalogos;

$(document).ready(function () {
    inicializarDataTable();
    configurarEventos();

    inicializarDataTableCatalogos();
    configurarEventosCatalogos();
});

function inicializarDataTable() {
    table = $('#tblLicencias').DataTable({
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
                    title: 'Licencias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Licencias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        ajax: {
            url: '/Licencia/GetAllDataTable',
            type: 'GET',
            dataType: 'json',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            {
                data: 'razonSocial',
                className: 'fw-semibold'
            },
            {
                data: null,
                render: function (data) {
                    return `
                        ${data.nombreContacto || 'N/A'}<br>
                        <small class="text-muted">${data.numeroContacto || ''}</small>
                    `;
                }
            },
            {
                data: 'tipoLicencia',
                render: function (data) {
                    const tipos = {
                        0: '<span class="badge bg-secondary">Básica</span>',
                        1: '<span class="badge bg-warning">Premium</span>'
                    };
                    return tipos[data] || data;
                }
            },
            {
                data: 'activo',
                render: function (data) {
                    return data ?
                        '<span class="badge bg-success">Activa</span>' :
                        '<span class="badge bg-danger">Inactiva</span>';
                }
            },
            {
                data: 'esPrueba',
                render: function (data) {
                    return data ?
                        '<span class="badge bg-info">Prueba</span>' :
                        '<span class="badge bg-light text-dark">Completa</span>';
                }
            },
            {
                data: 'fechaFinPrueba',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('es-ES') : 'N/A';
                }
            },
            {
                data: null,
                render: function (data) {
                    if (data.ultimoPago) {
                        return `
                            ${new Date(data.ultimoPago.fecha).toLocaleDateString('es-ES')}<br>
                            <small class="text-muted">$${data.ultimoPago.precio}</small>
                        `;
                    }
                    return 'Sin pagos';
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
                                    title="Editar licencia" data-id="${data}">
                                <i class="ph ph-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-info btn-pagos" 
                                    title="Ver pagos" data-id="${data}">
                                <i class="ph ph-currency-dollar"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-delete" 
                                    title="Eliminar licencia" data-id="${data}">
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

    $('#btnNuevaLicencia').click(function () {
        abrirModalLicencia(0, 'crear');
    });

    $('#tblLicencias tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalLicencia(id, 'editar');
    });

    $('#tblLicencias tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarLicencia(id);
    });

    $('#tblLicencias tbody').on('click', '.btn-pagos', function () {
        var id = $(this).data('id');
        verPagosLicencia(id);
    });

    $('#formLicencia').on('submit', function (e) {
        e.preventDefault();
        guardarLicencia();
    });

    $('#modalLicencia').on('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    // Mostrar/ocultar fecha de prueba
    $('#esPrueba').change(function () {
        if ($(this).is(':checked')) {
            $('#fechaPruebaGroup').show();
            $('#fechaFinPrueba').attr('required', 'required');
        } else {
            $('#fechaPruebaGroup').hide();
            $('#fechaFinPrueba').removeAttr('required');
        }
    });
}

function abrirModalLicencia(id, accion) {
    var modal = new bootstrap.Modal($('#modalLicencia')[0]);
    var titulo = $('#modalLicenciaLabel');
    var btnGuardar = $('#formLicencia button[type="submit"]');

    switch (accion) {
        case 'editar':
            titulo.html('<i class="ph ph-license me-2"></i>Editar Licencia');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Actualizar');
            // Ocultar sección de usuario en edición
            $('#usuarioSection').hide();
            cargarDatosLicencia(id);
            break;
        case 'crear':
            titulo.html('<i class="ph ph-license me-2"></i>Nueva Licencia');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Guardar');
            // Mostrar sección de usuario en creación
            $('#usuarioSection').show();
            limpiarFormulario();
            break;
    }

    modal.show();
}

function cargarDatosLicencia(id) {
    $.ajax({
        url: '/Administrador/GetLicenciaById/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var licencia = response.object;

                // Llenar formulario con datos de la licencia
                $('#razonSocial').val(licencia.razonSocial || '');
                $('#nombreContacto').val(licencia.nombreContacto || '');
                $('#numeroContacto').val(licencia.numeroContacto || '');
                $('#tipoLicencia').val(licencia.tipoLicencia);
                $('#esPrueba').prop('checked', licencia.esPrueba);

                if (licencia.esPrueba && licencia.fechaFinPrueba) {
                    $('#fechaFinPrueba').val(licencia.fechaFinPrueba.split('T')[0]);
                    $('#fechaPruebaGroup').show();
                }

                // Guardar ID en campo oculto
                if (!$('#idLicencia').length) {
                    $('<input>').attr({
                        type: 'hidden',
                        id: 'idLicencia',
                        name: 'Id'
                    }).appendTo('#formLicencia');
                }
                $('#idLicencia').val(licencia.id);

            } else {
                mostrarError(response.message || 'Error al cargar los datos de la licencia');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function limpiarFormulario() {
    $('#formLicencia')[0].reset();
    $('#idLicencia').remove();
    $('#formLicencia').removeClass('was-validated');
    $('#fechaPruebaGroup').hide();
    $('#fechaFinPrueba').removeAttr('required');
    $('#usuarioSection').show();
}

function guardarLicencia() {
    var form = $('#formLicencia')[0];

    // Validación del formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    var datos = {
        Id: $('#idLicencia').val() ? parseInt($('#idLicencia').val()) : 0,
        RazonSocial: $('#razonSocial').val().trim(),
        NombreContacto: $('#nombreContacto').val().trim() || null,
        NumeroContacto: $('#numeroContacto').val().trim() || null,
        TipoLicencia: parseInt($('#tipoLicencia').val()),
        EsPrueba: $('#esPrueba').is(':checked'),
        FechaFinPrueba: $('#esPrueba').is(':checked') ? $('#fechaFinPrueba').val() : null
    };

    // Solo incluir datos de usuario si es creación
    if (!datos.Id) {
        datos.Usuario = {
            Nombre: $('#nombreUsuario').val().trim(),
            Email: $('#emailUsuario').val().trim(),
            PhoneNumber: $('#telefonoUsuario').val().trim() || null,
            Rol: 0,
            Activo: true
        };
    }

    // Mostrar loading
    var submitBtn = $('#formLicencia button[type="submit"]');
    var originalText = submitBtn.html();
    submitBtn.html('<i class="ph ph-hourglass me-1"></i>' + (datos.Id ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);

    var url = datos.Id ? '/Administrador/Update/' + datos.Id : '/Administrador/Create';
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
                mostrarExito(response.message || (datos.Id ? 'Licencia actualizada correctamente' : 'Licencia creada correctamente'));
                $('#modalLicencia').modal('hide');
                table.ajax.reload();
            } else {
                mostrarError(response.message || 'Error al guardar la licencia');
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

function eliminarLicencia(id) {

    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar esta licencia?',
        'Eliminar Licencia')
    .then((result) => {
        if (result.isConfirmed) {

            $.ajax({
                url: '/Administrador/Delete/' + id,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas();

                    if (response.success) {
                        mostrarExito(response.message || 'Licencia eliminada correctamente');
                        table.ajax.reload();
                    } else {
                        mostrarError(response.message || 'Error al eliminar la licencia');
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


function verPagosLicencia(id) {
    cargarDatosLicenciaParaPagos(id);
}

function cargarDatosLicenciaParaPagos(id) {
    $.ajax({
        url: '/Administrador/GetLicenciaById/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var licencia = response.data || response.object;
                mostrarModalPagos(licencia);
            } else {
                mostrarError(response.message || 'Error al cargar los datos de la licencia');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function mostrarModalPagos(licencia) {
    // Actualizar información de la licencia
    $('#infoRazonSocial').text(licencia.razonSocial || 'N/A');
    $('#infoTipoLicencia').html(getBadgeTipoLicencia(licencia.tipoLicencia));
    $('#infoEstado').html(licencia.activo ?
        '<span class="badge bg-success">Activa</span>' :
        '<span class="badge bg-danger">Inactiva</span>'
    );

    // Calcular total pagado
    const totalPagado = licencia.pagoLicencias ?
        licencia.pagoLicencias.reduce((sum, pago) => sum + pago.precio, 0) : 0;
    $('#infoTotalPagado').text('$' + totalPagado.toFixed(2));

    // Guardar ID de licencia en el formulario
    if (!$('#idLicenciaPago').length) {
        $('<input>').attr({
            type: 'hidden',
            id: 'idLicenciaPago',
            name: 'IdLicencia'
        }).appendTo('#formPagoLicencia');
    }
    $('#idLicenciaPago').val(licencia.id);

    // Inicializar tabla de pagos
    inicializarTablaPagos(licencia.pagoLicencias);

    // Mostrar modal
    var modal = new bootstrap.Modal($('#modalPagosLicencia')[0]);
    modal.show();
}

function getBadgeTipoLicencia(tipo) {
    const tipos = {
        0: '<span class="badge bg-secondary">Básica</span>',
        1: '<span class="badge bg-warning">Premium</span>'
    };
    return tipos[tipo] || tipo;
}

function inicializarTablaPagos(pagos) {
    // Destruir tabla existente si hay una
    if ($.fn.DataTable.isDataTable('#tblPagosLicencia')) {
        tablePagos.destroy();
    }

    tablePagos = $('#tblPagosLicencia').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        data: pagos || [],
        columns: [
            {
                data: 'tipoPagoLicencia',
                render: function (data) {
                    const tipos = {
                        0: '<span class="badge bg-primary">Mantenimiento</span>'
                    };
                    return tipos[data] || data;
                }
            },
            {
                data: 'fecha',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('es-ES') : 'N/A';
                }
            },
            {
                data: 'precio',
                render: function (data) {
                    return `<span class="fw-bold text-success">$${parseFloat(data).toFixed(2)}</span>`;
                }
            },
            {
                data: 'fechaCreacion',
                render: function (data) {
                    return data ? new Date(data).toLocaleDateString('es-ES') : 'N/A';
                }
            },
            {
                data: 'id',
                className: 'text-center',
                orderable: false,
                render: function (data, type, row) {
                    return `
                        <div class="btn-group btn-group-sm">
                            <button type="button" class="btn btn-outline-danger btn-eliminar-pago" 
                                    title="Eliminar pago" data-id="${data}">
                                <i class="ph ph-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        order: [[1, 'desc']], // Ordenar por fecha descendente
        pageLength: 10,
        responsive: true,
        drawCallback: function (settings) {
            // Actualizar contador de pagos
            const count = this.api().data().count();
            $('#contadorPagos').text(count + ' pago' + (count !== 1 ? 's' : ''));
        }
    });

    // Configurar eventos de la tabla de pagos
    configurarEventosPagos();
}

function configurarEventosPagos() {
    // Eliminar pago
    $('#tblPagosLicencia tbody').on('click', '.btn-eliminar-pago', function () {
        var idPago = $(this).data('id');
        eliminarPago(idPago);
    });

    // Submit del formulario de pago
    $('#formPagoLicencia').on('submit', function (e) {
        e.preventDefault();
        agregarPago();
    });
}

function agregarPago() {
    var form = $('#formPagoLicencia')[0];

    // Validación del formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    var datos = {
        TipoPagoLicencia: parseInt($('#tipoPago').val()),
        Precio: parseFloat($('#precioPago').val()),
        Fecha: $('#fechaPago').val(),
        IdLicencia: parseInt($('#idLicenciaPago').val())
    };

    // Mostrar loading
    var submitBtn = $('#formPagoLicencia button[type="submit"]');
    var originalText = submitBtn.html();
    submitBtn.html('<i class="ph ph-hourglass me-1"></i>Agregando...').prop('disabled', true);

    $.ajax({
        url: '/Administrador/CreatePagoLicencia',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(datos),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                mostrarExito(response.message || 'Pago agregado correctamente');
                $('#formPagoLicencia')[0].reset();
                $('#formPagoLicencia').removeClass('was-validated');

                // Recargar los datos de la licencia para actualizar la tabla
                const idLicencia = $('#idLicenciaPago').val();
                cargarDatosLicenciaParaPagos(idLicencia);

            } else {
                mostrarError(response.message || 'Error al agregar el pago');
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

function eliminarPago(idPago) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este pago?',
        'Eliminar Pago'
    ).then((result) => {
        if (result.isConfirmed) {
            mostrarLoading('Eliminando pago...');

            $.ajax({
                url: '/Administrador/DeletePagoLicencia/' + idPago,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas();

                    if (response.success) {
                        mostrarExito(response.message || 'Pago eliminado correctamente');

                        // Recargar los datos de la licencia para actualizar la tabla
                        const idLicencia = $('#idLicenciaPago').val();
                        cargarDatosLicenciaParaPagos(idLicencia);

                    } else {
                        mostrarError(response.message || 'Error al eliminar el pago');
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

// Actualizar la función configurarEventos para incluir el modal de pagos
function configurarEventos() {

    $('#btnNuevaLicencia').click(function () {
        abrirModalLicencia(0, 'crear');
    });

    $('#tblLicencias tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalLicencia(id, 'editar');
    });

    $('#tblLicencias tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarLicencia(id);
    });

    $('#tblLicencias tbody').on('click', '.btn-pagos', function () {
        var id = $(this).data('id');
        verPagosLicencia(id);
    });

    $('#formLicencia').on('submit', function (e) {
        e.preventDefault();
        guardarLicencia();
    });

    $('#modalLicencia').on('hidden.bs.modal', function () {
        limpiarFormulario();
    });

    $('#modalPagosLicencia').on('hidden.bs.modal', function () {
        // Limpiar tabla de pagos al cerrar el modal
        if ($.fn.DataTable.isDataTable('#tblPagosLicencia')) {
            tablePagos.destroy();
        }
        $('#tblPagosLicencia tbody').empty();
    });

    // Mostrar/ocultar fecha de prueba
    $('#esPrueba').change(function () {
        if ($(this).is(':checked')) {
            $('#fechaPruebaGroup').show();
            $('#fechaFinPrueba').attr('required', 'required');
        } else {
            $('#fechaPruebaGroup').hide();
            $('#fechaFinPrueba').removeAttr('required');
        }
    });
}










// CATALOGOS

function inicializarDataTableCatalogos() {
    tableCatalogos = $('#tblCatalogos').DataTable({
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
                    title: 'Catalogos_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Catalogos_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        ajax: {
            url: '/Catalogo/GetAllDataTable',
            type: 'GET',
            dataType: 'json',
            dataSrc: function (response) {
                return response.success ? response.data : [];
            }
        },
        columns: [
            {
                data: 'tipo',
                render: function (data) {
                    const tipos = {
                        10: '<span class="badge bg-danger">Plaga</span>',
                        11: '<span class="badge bg-warning">Maleza</span>',
                        12: '<span class="badge bg-info">Enfermedad</span>',
                        20: '<span class="badge bg-success">Tipo Fertilizante</span>',
                        21: '<span class="badge bg-primary">Nutriente</span>',
                        22: '<span class="badge bg-secondary">Producto Agroquímico</span>',
                        30: '<span class="badge bg-success">Método Siembra</span>',
                        31: '<span class="badge bg-info">Método Riego</span>',
                        32: '<span class="badge bg-warning">Método Aplicación</span>',
                        40: '<span class="badge bg-dark">Maquinaria</span>',
                        41: '<span class="badge bg-primary">Fuente Agua</span>',
                        50: '<span class="badge bg-secondary">Laboratorio</span>',
                        99: '<span class="badge bg-light text-dark">Otro</span>'
                    };
                    return tipos[data] || data;
                }
            },
            {
                data: 'nombre',
                className: 'fw-semibold'
            },
            {
                data: 'descripcion',
                render: function (data) {
                    return data || '<span class="text-muted">Sin descripción</span>';
                }
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
                                    title="Editar catálogo" data-id="${data}">
                                <i class="ph ph-pencil"></i>
                            </button>
                            <button type="button" class="btn btn-outline-danger btn-delete" 
                                    title="Eliminar catálogo" data-id="${data}">
                                <i class="ph ph-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        order: [[1, 'asc']],
        pageLength: 25,
        responsive: true
    });
}

function configurarEventosCatalogos() {

    $('#btnNuevoCatalogo').click(function () {
        abrirModalCatalogo(0, 'crear');
    });

    $('#tblCatalogos tbody').on('click', '.btn-edit', function () {
        var id = $(this).data('id');
        abrirModalCatalogo(id, 'editar');
    });

    $('#tblCatalogos tbody').on('click', '.btn-delete', function () {
        var id = $(this).data('id');
        eliminarCatalogo(id);
    });

    $('#formCatalogo').on('submit', function (e) {
        e.preventDefault();
        guardarCatalogo();
    });

    $('#modalCatalogo').on('hidden.bs.modal', function () {
        limpiarFormularioCatalogo();
    });
}

function abrirModalCatalogo(id, accion) {
    var modal = new bootstrap.Modal($('#modalCatalogo')[0]);
    var titulo = $('#modalCatalogoLabel');
    var btnGuardar = $('#formCatalogo button[type="submit"]');

    switch (accion) {
        case 'editar':
            titulo.html('<i class="ph ph-list-checks me-2"></i>Editar Catálogo');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Actualizar');
            cargarDatosCatalogo(id);
            break;
        case 'crear':
            titulo.html('<i class="ph ph-list-checks me-2"></i>Nuevo Catálogo');
            btnGuardar.html('<i class="ph ph-check-circle me-1"></i> Guardar');
            limpiarFormularioCatalogo();
            break;
    }

    modal.show();
}

function cargarDatosCatalogo(id) {
    $.ajax({
        url: '/Catalogo/GetById/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                var catalogo = response.data || response.object;

                // Llenar formulario con datos del catálogo
                $('#tipoCatalogo').val(catalogo.tipo);
                $('#nombreCatalogo').val(catalogo.nombre);
                $('#descripcionCatalogo').val(catalogo.descripcion || '');
                $('#estadoCatalogo').val(catalogo.activo.toString());

                $('#idCatalogo').val(catalogo.id);

            } else {
                mostrarError(response.message || 'Error al cargar los datos del catálogo');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function limpiarFormularioCatalogo() {
    $('#formCatalogo')[0].reset();
    $('#idCatalogo').remove();
    $('#formCatalogo').removeClass('was-validated');
}

// ========== OPERACIONES CRUD ==========
function guardarCatalogo() {
    var form = $('#formCatalogo')[0];

    // Validación del formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    var datos = {
        Id: $('#idCatalogo').val() ? parseInt($('#idCatalogo').val()) : 0,
        Tipo: parseInt($('#tipoCatalogo').val()),
        Nombre: $('#nombreCatalogo').val().trim(),
        Descripcion: $('#descripcionCatalogo').val().trim() || null,
        Activo: $('#estadoCatalogo').val() === 'true'
    };

    // Mostrar loading
    var submitBtn = $('#formCatalogo button[type="submit"]');
    var originalText = submitBtn.html();
    submitBtn.html('<i class="ph ph-hourglass me-1"></i>' + (datos.Id ? 'Actualizando...' : 'Guardando...')).prop('disabled', true);

    var url = datos.Id ? '/Catalogo/Update/' + datos.Id : '/Catalogo/Create';
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
                mostrarExito(response.message || (datos.Id ? 'Catálogo actualizado correctamente' : 'Catálogo creado correctamente'));
                $('#modalCatalogo').modal('hide');
                tableCatalogos.ajax.reload();
            } else {
                mostrarError(response.message || 'Error al guardar el catálogo');
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

function eliminarCatalogo(id) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este catálogo?',
        'Eliminar Catálogo'
    ).then((result) => {
        if (result.isConfirmed) {
            mostrarLoading('Eliminando catálogo...');

            $.ajax({
                url: '/Catalogo/Delete/' + id,
                type: 'DELETE',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    cerrarAlertas();

                    if (response.success) {
                        mostrarExito(response.message || 'Catálogo eliminado correctamente');
                        tableCatalogos.ajax.reload();
                    } else {
                        mostrarError(response.message || 'Error al eliminar el catálogo');
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







function mostrarConfirmacion(mensaje, titulo = 'Confirmar') {
    return Swal.fire({
        title: titulo,
        text: mensaje,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText: '<i class="ph ph-check-circle me-1"></i> Sí, confirmar',
        cancelButtonText: '<i class="ph ph-x-circle me-1"></i> Cancelar',
        reverseButtons: true,
        customClass: {
            confirmButton: 'btn btn-danger',
            cancelButton: 'btn btn-secondary'
        }
    });
}

function mostrarExito(mensaje, titulo = 'Éxito') {
    Swal.fire({
        title: titulo,
        text: mensaje,
        icon: 'success',
        confirmButtonColor: '#198754',
        confirmButtonText: '<i class="ph ph-check-circle me-1"></i> Aceptar',
        timer: 2000,
        timerProgressBar: true
    });
}

function mostrarError(mensaje, titulo = 'Error') {
    Swal.fire({
        title: titulo,
        text: mensaje,
        icon: 'error',
        confirmButtonColor: '#dc3545',
        confirmButtonText: '<i class="ph ph-warning me-1"></i> Entendido'
    });
}

function mostrarInfo(mensaje, titulo = 'Información') {
    Swal.fire({
        title: titulo,
        text: mensaje,
        icon: 'info',
        confirmButtonColor: '#0dcaf0',
        confirmButtonText: '<i class="ph ph-info me-1"></i> Entendido'
    });
}

function mostrarLoading(mensaje = 'Procesando...') {
    Swal.fire({
        title: mensaje,
        allowOutsideClick: false,
        allowEscapeKey: false,
        allowEnterKey: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

function cerrarAlertas() {
    Swal.close();
}