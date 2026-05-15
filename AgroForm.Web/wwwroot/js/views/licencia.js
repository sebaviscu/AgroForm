let pagosTable;

$(document).ready(function () {
    cargarDatosLicencia();
});

/**
 * Carga los datos de la licencia del usuario actual vía API.
 */
function cargarDatosLicencia() {
    // Mostrar loading, ocultar contenido y error
    $('#licenciaLoading').show();
    $('#licenciaContent').hide();
    $('#licenciaError').hide();

    $.ajax({
        url: '/Licencia/GetMyLicencia',
        type: 'GET',
        dataType: 'json',
        success: function (response) {
            $('#licenciaLoading').hide();

            if (response.success && response.object) {
                renderizarDatosLicencia(response.object);
                $('#licenciaContent').show();
            } else {
                mostrarErrorLicencia(response.message || 'No se pudieron obtener los datos de la licencia.');
            }
        },
        error: function (xhr, status, error) {
            $('#licenciaLoading').hide();
            var mensaje = 'Error al conectar con el servidor';
            try {
                var resp = JSON.parse(xhr.responseText);
                if (resp && resp.message) mensaje = resp.message;
            } catch (e) {}
            mostrarErrorLicencia(mensaje);
        }
    });
}

/**
 * Muestra la vista de error con un mensaje personalizado.
 */
function mostrarErrorLicencia(mensaje) {
    $('#licenciaErrorMessage').text(mensaje);
    $('#licenciaError').show();
    $('#licenciaContent').hide();
}

/**
 * Renderiza los datos de la licencia en las cards informativas.
 */
function renderizarDatosLicencia(licencia) {
    // Tipo de licencia
    var tipoTexto = licencia.tipoLicencia === 1 ? 'Premium' : 'Básica';
    var tipoBadge = licencia.tipoLicencia === 1
        ? '<span class="badge bg-warning text-dark badge-license">Premium</span>'
        : '<span class="badge bg-secondary badge-license">Básica</span>';
    $('#lblTipoLicencia').html(tipoBadge);

    // Estado
    var activo = licencia.activo === true;
    var estadoBadge = activo
        ? '<span class="badge bg-success badge-license">Activa</span>'
        : '<span class="badge bg-danger badge-license">Inactiva</span>';
    $('#lblEstado').html(estadoBadge);

    // Razón Social
    $('#lblRazonSocial').text(licencia.razonSocial || '-');

    // Contacto
    $('#lblContacto').text(licencia.nombreContacto || '-');

    // Fecha de Registro
    if (licencia.registrationDate) {
        var fechaReg = new Date(licencia.registrationDate);
        $('#lblFechaRegistro').text(fechaReg.toLocaleDateString('es-ES', {
            year: 'numeric', month: 'long', day: 'numeric'
        }));
    } else {
        $('#lblFechaRegistro').text('-');
    }

    // Vencimiento
    if (licencia.esPrueba && licencia.fechaFinPrueba) {
        var fechaFin = new Date(licencia.fechaFinPrueba);
        var hoy = new Date();
        var expirada = fechaFin < hoy;
        var vencimientoText = fechaFin.toLocaleDateString('es-ES', {
            year: 'numeric', month: 'long', day: 'numeric'
        });
        var badgeClass = expirada ? 'bg-danger' : 'bg-warning text-dark';
        $('#lblVencimiento').html(
            '<span class="badge ' + badgeClass + ' badge-license">' + vencimientoText + '</span>'
        );
    } else if (!licencia.esPrueba) {
        $('#lblVencimiento').html('<span class="badge bg-info badge-license">Sin vencimiento</span>');
    } else {
        $('#lblVencimiento').text('-');
    }

    // Teléfono
    $('#lblTelefono').text(licencia.numeroContacto || '-');

    // Es Prueba
    if (licencia.esPrueba) {
        $('#lblEsPrueba').html('<span class="badge bg-warning text-dark badge-license">Sí</span>');
    } else {
        $('#lblEsPrueba').html('<span class="badge bg-secondary badge-license">No</span>');
    }

    // Inicializar DataTable con los pagos
    inicializarTablaPagos(licencia.pagoLicencias || []);
}

/**
 * Inicializa la DataTable de pagos con los datos proporcionados.
 */
function inicializarTablaPagos(pagos) {
    // Destruir instancia previa si existe
    if ($.fn.DataTable.isDataTable('#tblPagosLicencia')) {
        $('#tblPagosLicencia').DataTable().destroy();
        $('#tblPagosLicencia tbody').empty();
    }

    pagosTable = $('#tblPagosLicencia').DataTable({
        data: pagos,
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
                    title: 'Pagos_Licencia_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Pagos_Licencia_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        columns: [
            {
                data: 'fecha',
                render: function (data) {
                    if (!data) return '-';
                    return new Date(data).toLocaleDateString('es-ES', {
                        year: 'numeric', month: 'short', day: 'numeric'
                    });
                }
            },
            {
                data: 'tipoPagoLicencia',
                className: 'fw-semibold',
                render: function (data) {
                    var tipos = {
                        0: 'Mantenimiento'
                    };
                    return tipos[data] || 'Desconocido';
                }
            },
            {
                data: 'precio',
                className: 'fw-bold',
                render: function (data) {
                    if (data == null) return '$0.00';
                    return '$ ' + parseFloat(data).toLocaleString('es-AR', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    });
                }
            },
            {
                data: 'registrationDate',
                render: function (data) {
                    if (!data) return '-';
                    return new Date(data).toLocaleDateString('es-ES', {
                        year: 'numeric', month: 'short', day: 'numeric',
                        hour: '2-digit', minute: '2-digit'
                    });
                }
            }
        ],
        order: [[0, 'desc']],
        pageLength: 10,
        lengthMenu: [[5, 10, 25, 50, -1], [5, 10, 25, 50, 'Todos']],
        responsive: true,
        columnDefs: [
            { responsivePriority: 1, targets: [0, 2] },
            { responsivePriority: 2, targets: [1] }
        ],
        // Mensaje cuando no hay datos
        info: true,
        infoEmpty: 'No hay pagos registrados',
        zeroRecords: 'No se encontraron pagos',
        emptyTable: 'No hay pagos registrados'
    });
}
