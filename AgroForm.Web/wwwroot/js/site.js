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

    // Inicializar sidebar
    function initializeSidebar() {
        if (checkMobile()) {
            // En móvil: empezar cerrado
            $sidebar.removeClass('active mobile-active');
            $content.removeClass('active');
            $sidebarOverlay.removeClass('active');
            // Asegurar que el logo se vea
            $sidebarLogoText.show();
            $sidebarLogoIcon.show();
        } else {
            // En desktop: empezar abierto
            $sidebar.removeClass('mobile-active');
            $content.removeClass('active');
            $sidebarOverlay.removeClass('active');
            $sidebarLogoText.show();
            $sidebarLogoIcon.show();
        }
    }

    // Toggle sidebar for desktop
    $sidebarCollapse.on('click', function (e) {
        e.stopPropagation();
        if (!checkMobile()) {
            toggleSidebar();
        }
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
            closeMobileSidebar();
        }
    });

    // Close button en móvil
    $('#sidebarClose').on('click', function () {
        if (checkMobile()) {
            closeMobileSidebar();
        }
    });

    function toggleSidebar() {
        console.log('Toggling desktop sidebar');
        $sidebar.toggleClass('active');
        $content.toggleClass('active');

        // Ocultar/mostrar logo text cuando está colapsado
        if ($sidebar.hasClass('active')) {
            $sidebarLogoText.hide();
            $sidebarLogoIcon.hide();
        } else {
            $sidebarLogoText.show();
            $sidebarLogoIcon.show();
        }
    }

    function toggleMobileSidebar() {
        console.log('Toggling mobile sidebar');
        if ($sidebar.hasClass('mobile-active')) {
            closeMobileSidebar();
        } else {
            openMobileSidebar();
        }
    }

    function openMobileSidebar() {
        $sidebar.addClass('mobile-active');
        $sidebarOverlay.addClass('active');
        $('body').addClass('sidebar-open');
        // Asegurar que el logo se vea en móvil
        $sidebarLogoText.show();
        $sidebarLogoIcon.show();
    }

    function closeMobileSidebar() {
        $sidebar.removeClass('mobile-active');
        $sidebarOverlay.removeClass('active');
        $('body').removeClass('sidebar-open');
    }

    // Close sidebar when clicking outside on mobile
    $(document).on('click', function (event) {
        if (!checkMobile()) return;

        const isClickInsideSidebar = $sidebar.has(event.target).length > 0 || $sidebar.is(event.target);
        const isClickInsideToggle = $sidebarCollapseMobile.has(event.target).length > 0 || $sidebarCollapseMobile.is(event.target);
        const isClickInsideClose = $('#sidebarClose').has(event.target).length > 0 || $('#sidebarClose').is(event.target);

        if (!isClickInsideSidebar && !isClickInsideToggle && !isClickInsideClose && $sidebar.hasClass('mobile-active')) {
            closeMobileSidebar();
        }
    });

    // Close mobile sidebar when clicking on a link
    $sidebar.on('click', 'a', function (event) {
        if (checkMobile()) {
            // Si el enlace abre un submenu, no cerrar el sidebar
            if ($(this).hasClass('dropdown-toggle')) {
                event.stopPropagation();
                return;
            }
            closeMobileSidebar();
        }
    });


    // Handle window resize
    $(window).on('resize', function () {
        if (window.innerWidth >= 992) {
            // En desktop: cerrar modo móvil si está activo
            closeMobileSidebar();

            // Restaurar estado normal en desktop
            if (!$sidebar.hasClass('active')) {
                $sidebarLogoText.show();
                $sidebarLogoIcon.show();
            }
        } else {
            // En móvil: cerrar modo desktop si está activo
            $sidebar.removeClass('active');
            $content.removeClass('active');
            $sidebarLogoText.show();
            $sidebarLogoIcon.show();
        }
    });

    // Cerrar con ESC key
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && checkMobile() && $sidebar.hasClass('mobile-active')) {
            closeMobileSidebar();
        }
    });

    // Inicializar estado al cargar
    initializeSidebar();
});

// ========== FUNCIONES DE UTILIDAD (se mantienen igual) ==========
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

function mostrarMensaje(mensaje, tipo) {
    if (tipo === 'success') {
        toastr.success(mensaje);
    } else {
        toastr.error(mensaje);
    }
}


function setSelect2WhenReady(selector, value) {
    return new Promise(resolve => {

        let lastCount = -1;
        let stableFor = 0;

        const interval = setInterval(() => {
            const count = $(selector).find("option").length;

            // Si el número de opciones sigue cambiando
            if (count !== lastCount) {
                lastCount = count;
                stableFor = 0;
            } else {
                // Sigue igual → sumamos tiempo estable
                stableFor += 100;
            }

            // Cuando estuvo 300ms sin cambiar
            if (stableFor >= 300 && count > 0) {
                clearInterval(interval);

                $(selector).val(value).trigger('change');

                resolve(true);
            }

        }, 100);

        // Timeout 5s
        setTimeout(() => {
            clearInterval(interval);
            resolve(false);
        }, 5000);
    });
}
