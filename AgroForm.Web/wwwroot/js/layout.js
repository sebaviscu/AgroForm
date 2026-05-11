// ===== FUNCIONES GLOBALES DEL LAYOUT =====

// Inicialización de configuración de moneda para la vista de usuario
$(document).ready(function () {
    // El manejo del tema ahora está centralizado en dark-theme.js (DarkTheme)
    // Cargar configuración de moneda al abrir el modal
    $('#configModal').on('show.bs.modal', function () {
        cargarConfiguracionMonedaModal();
    });
});

// Función para cargar configuración de moneda en el modal
function cargarConfiguracionMonedaModal() {
    $.ajax({
        url: '/Usuario/GetConfiguracionMoneda',
        type: 'GET',
        success: function(response) {
            if (response.success) {
                // Cargar dropdown de tipos de dólar
                var select = $('#tipoDolarModal');
                select.empty();
                select.append('<option value="">Seleccione un tipo de dólar</option>');
                
                response.object.monedasDisponibles.forEach(function(moneda) {
                    var selected = moneda.id === response.object.idMonedaReferencia ? 'selected' : '';
                    select.append(`<option value="${moneda.id}" ${selected}>${moneda.nombre}</option>`);
                });
            } else {
                mostrarMensaje(response.message || 'Error al cargar configuración', 'error');
            }
        },
        error: function() {
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}

// Función para guardar configuración de moneda desde el modal
function guardarConfiguracionMoneda() {
    var idMoneda = $('#tipoDolarModal').val();
    
    if (!idMoneda) {
        mostrarMensaje('Por favor seleccione un tipo de dólar', 'error');
        return;
    }

    mostrarLoading('Actualizando configuración...');

    $.ajax({
        url: '/Usuario/ActualizarMonedaReferencia',
        type: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify(parseInt(idMoneda)),
        success: function(response) {
            cerrarAlertas();
            if (response.success) {
                mostrarMensaje(response.message, 'success');
                // Cerrar el modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('configModal'));
                modal.hide();
                
                // Recargar la página después de un breve delay para actualizar el dashboard
                setTimeout(function() {
                    location.reload();
                }, 1000);
            } else {
                mostrarMensaje(response.message || 'Error al actualizar configuración', 'error');
            }
        },
        error: function() {
            cerrarAlertas();
            mostrarMensaje('Error al conectar con el servidor', 'error');
        }
    });
}

// Función para abrir modal de configuración
function abrirModalConfiguracion(event) {
    event.preventDefault();
    const modal = new bootstrap.Modal(document.getElementById('configModal'));
    modal.show();
}

// ===== FUNCIONES DE SIMULACIÓN =====

// Función para detener simulación
function detenerSimulacion() {
    mostrarConfirmacion(
        '¿Está seguro de que desea detener la simulación? Volverá a tener vista de Super Admin.',
        'Detener Simulación'
    ).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Simulacion/DetenerSimulacion',
                type: 'POST',
                headers: {
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        actualizarEstadoSimulacion();
                        // Volver a la vista de administrador
                        setTimeout(() => {
                            window.location.href = response.redirectUrl || '/Administrador/Index';
                        }, 800);
                    } else {
                        toastr.error(response.message);
                    }
                },
                error: function (xhr, status, error) {
                    toastr.error('Error al detener simulación: ' + (xhr.responseJSON?.message || error));
                }
            });
        }
    });
}

// Función para actualizar estado de simulación
function actualizarEstadoSimulacion() {
    $.ajax({
        url: '/Simulacion/GetEstadoSimulacion',
        type: 'GET',
        success: function (response) {
            if (response.isSimulating) {
                // Mostrar banner de simulación
                $('#bannerSimulacionContainer').removeClass('d-none');
                $('#licenciaSimulada').text(response.licenciaActual);
                $('#sidebar').addClass('simulating');
            } else {
                // Ocultar banner de simulación
                $('#bannerSimulacionContainer').addClass('d-none');
                $('#sidebar').removeClass('simulating');
            }
        },
        error: function () {
            console.error('Error al obtener estado de simulación');
        }
    });
}

// Verificar estado de simulación al cargar la página
$(document).ready(function() {
    actualizarEstadoSimulacion();
});
