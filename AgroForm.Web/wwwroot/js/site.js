$(document).ready(function () {
    const $sidebar = $('#sidebar');
    const $content = $('#content');
    const $sidebarCollapse = $('#sidebarCollapse');
    const $sidebarCollapseMobile = $('#sidebarCollapseMobile');
    const $sidebarLogoText = $('.sidebar-logo-text');
    const $sidebarLogoIcon = $('.sidebar-logo-icon');
    const $sidebarOverlay = $('#sidebarOverlay');

    // Estado inicial - verificar si está en móvil
    function checkMobile() {
        return window.innerWidth < 992;
    }

    // Inicializar estado - EN MÓVIL EMPEZAR CERRADO
    function initializeSidebar() {
        if (checkMobile()) {
            $sidebar.removeClass('mobile-active'); // Asegurar que esté cerrado en móvil
            $sidebarLogoText.show();
            if ($sidebarLogoIcon.length) {
                $sidebarLogoIcon.show();
            }
        } else {
            // En desktop, estado normal
            if (!$sidebar.hasClass('active')) {
                $sidebarLogoText.show();
                if ($sidebarLogoIcon.length) {
                    $sidebarLogoIcon.show();
                }
            }
        }
    }

    // Toggle sidebar for desktop
    $sidebarCollapse.on('click', function (e) {
        e.stopPropagation();
        toggleSidebar();
    });

    // Toggle sidebar for mobile
    $sidebarCollapseMobile.on('click', function (e) {
        e.stopPropagation();
        if (checkMobile()) {
            toggleMobileSidebar();
        } else {
            toggleSidebar();
        }
    });

    // Overlay click para móvil
    $sidebarOverlay.on('click', function () {
        if (checkMobile()) {
            $sidebar.removeClass('mobile-active');
        }
    });

    function toggleSidebar() {
        $sidebar.toggleClass('active');
        $content.toggleClass('active');

        // Ocultar/mostrar logo text e icono
        if ($sidebar.hasClass('active')) {
            $sidebarLogoText.hide();
            if ($sidebarLogoIcon.length) {
                $sidebarLogoIcon.hide();
            }
        } else {
            $sidebarLogoText.show();
            if ($sidebarLogoIcon.length) {
                $sidebarLogoIcon.show();
            }
        }
    }

    function toggleMobileSidebar() {
        $sidebar.toggleClass('mobile-active');

        // En móvil, siempre mostrar el logo cuando está activo
        if ($sidebar.hasClass('mobile-active')) {
            $sidebarLogoText.show();
            if ($sidebarLogoIcon.length) {
                $sidebarLogoIcon.show();
            }
        }
    }

    // Close sidebar when clicking outside on mobile
    $(document).on('click', function (event) {
        const isMobile = checkMobile();
        const isClickInsideSidebar = $sidebar.has(event.target).length > 0 || $sidebar.is(event.target);
        const isClickInsideToggle = $sidebarCollapseMobile.has(event.target).length > 0 || $sidebarCollapseMobile.is(event.target);
        const isClickInsideDesktopToggle = $sidebarCollapse.has(event.target).length > 0 || $sidebarCollapse.is(event.target);

        if (isMobile && !isClickInsideSidebar && !isClickInsideToggle && $sidebar.hasClass('mobile-active')) {
            $sidebar.removeClass('mobile-active');
        }
    });

    // Close mobile sidebar when clicking on a link
    $sidebar.on('click', 'a', function (event) {
        if (checkMobile()) {
            $sidebar.removeClass('mobile-active');
        }
    });

    // Handle window resize
    $(window).on('resize', function () {
        if (window.innerWidth >= 992) {
            // En desktop, asegurar que no tenga clase mobile-active
            $sidebar.removeClass('mobile-active');

            // Restaurar estado normal en desktop
            if (!$sidebar.hasClass('active')) {
                $sidebarLogoText.show();
                if ($sidebarLogoIcon.length) {
                    $sidebarLogoIcon.show();
                }
            }
        } else {
            // En móvil, asegurar que esté cerrado por defecto
            if (!$sidebar.hasClass('mobile-active')) {
                $sidebarLogoText.show();
                if ($sidebarLogoIcon.length) {
                    $sidebarLogoIcon.show();
                }
            }
        }
    });

    // Inicializar estado al cargar
    initializeSidebar();

});

// ========== FUNCIONES DE UTILIDAD MEJORADAS ==========
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

// Función de loading
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

// Cerrar cualquier SweetAlert abierto
function cerrarAlertas() {
    Swal.close();
}