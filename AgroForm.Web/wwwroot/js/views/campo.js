// Variables globales
var table;
let map;
let drawnItems;
let isDrawing = false;
let currentPolygon = null;

let viewMap;
let currentViz = 'normal';
let campoPolygon = null;
let baseLayers = {};
let overlayLayers = {};
let weatherData = null;
let currentFieldData = null;

let historialData = [];


$(document).ready(function () {
    inicializarDataTable();
    configurarEventos();
    configurarMapa();
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
                            <button type="button" class="btn btn-outline-info btn-view"
                                    title="Ver campo y datos satelitales" data-id="${data}">
                                <i class="ph ph-eye"></i>
                            </button>
                            <button type="button" class="btn btn-outline-secondary btn-history"
                                    title="Historial" data-id="${data}">
                                <i class="ph ph-list-dashes"></i>
                            </button>
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

    $('#tblCampos tbody').on('click', '.btn-view', function () {
        var id = $(this).data('id');
        abrirModalVisualizacion(id);
    });

    $('#tblCampos tbody').on('click', '.btn-history', function () {
        var id = $(this).data('id');
        cargarHistorico(id);
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

                // Cargar el polígono si existe
                if (campo.coordenadasPoligono) {
                    setTimeout(() => {
                        cargarPoligonoEnMapa(campo.coordenadasPoligono, campo.latitud, campo.longitud);
                    }, 500);
                }
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
    resetMap();
}

// ========== OPERACIONES CRUD ==========
function guardarCampo() {
    var form = $('#formCampo')[0];

    // Validación del formulario
    if (!form.checkValidity()) {
        form.classList.add('was-validated');
        return;
    }

    var coordenadas = $('#coordenadasPoligono').val();
    if (!coordenadas) {
        $('#status').text('Debes dibujar un campo en el mapa antes de guardar').removeClass('text-success').addClass('text-warning');
        return;
    }

    // Obtener el centro del campo para latitud y longitud
    var centroCoords = $('#center-coords').text();
    var latitud = 0;
    var longitud = 0;

    if (centroCoords !== '-') {
        var coords = centroCoords.split(',');
        latitud = parseFloat(coords[0]);
        longitud = parseFloat(coords[1]);
    }

    var datos = {
        Id: $('#idCampo').val() ? parseInt($('#idCampo').val()) : 0,
        Nombre: $('#nombre').val().trim(),
        Ubicacion: $('#ubicacion').val().trim() || null,
        SuperficieHectareas: $('#superficieHectareas').val() ? parseFloat($('#superficieHectareas').val()) : null,
        CoordenadasPoligono: coordenadas,
        Latitud: latitud,
        Longitud: longitud
    };

    // Validación adicional
    if (!datos.Nombre) {
        mostrarError('El nombre del campo es obligatorio');
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
            mostrarError('Error al conectar con el servidor');
        },
        complete: function () {
            submitBtn.html(originalText).prop('disabled', false);
        }
    });
}

function eliminarCampo(id) {
    mostrarConfirmacion(
        '¿Está seguro de que desea eliminar este campo?',
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
                    cerrarAlertas();

                    if (response.success) {
                        mostrarExito(response.message || 'Campo eliminado correctamente');
                        table.ajax.reload();
                    } else {
                        mostrarError(response.message || 'Error al eliminar el campo');
                    }
                },
                error: function (xhr, status, error) {
                    cerrarAlertas();
                    console.error('Error:', error);
                    mostrarError('Error al conectar con el servidor');
                }
            });
        }
    });
}

// ========== FUNCIONES DEL MAPA ==========
function configurarMapa() {
    // Inicializar el mapa cuando el modal se muestra
    $('#modalCampo').on('shown.bs.modal', function () {
        initMap();
    });

    // Limpiar el mapa cuando el modal se cierra
    $('#modalCampo').on('hidden.bs.modal', function () {
        if (!$('#idCampo').val()) {
            resetMap();
        }
    });

    // Evento para dibujar campo
    $('#btn-draw').click(function () {
        if (!map) return;
        if (isDrawing) return;

        iniciarDibujoPoligono();
    });

    // Evento para limpiar mapa
    $('#btn-clear').click(function () {
        if (!map) return;

        drawnItems.clearLayers();
        isDrawing = false;
        currentPolygon = null;
        map.off('click');

        $('#status').text('Mapa limpiado. Puedes dibujar un nuevo campo.').removeClass('text-success').addClass('text-warning');
        updateFieldInfo(null);
    });

    // Evento para calcular información
    $('#btn-calculate').click(function () {
        if (!map) return;

        const layers = drawnItems.getLayers();
        if (layers.length > 0 && layers[0] instanceof L.Polygon) {
            updateFieldInfo(layers[0]);
        } else {
            $('#status').text('Primero debes dibujar un campo').removeClass('text-success').addClass('text-warning');
        }
    });

    // Validación en tiempo real del nombre
    $('#nombre').on('input', function () {
        if ($(this).val().trim()) {
            $(this).removeClass('is-invalid');
        }
    });
}

function initMap() {
    map = L.map('map').setView([-32.9468, -60.6393], 13);

    // Capa Híbrida (satélite con calles y nombres)
    L.tileLayer('https://{s}.google.com/vt/lyrs=y&x={x}&y={y}&z={z}', {
        maxZoom: 80,
        subdomains: ['mt0', 'mt1', 'mt2', 'mt3'],
        attribution: '© Google'
    }).addTo(map);

    drawnItems = new L.FeatureGroup();
    map.addLayer(drawnItems);

    setTimeout(() => {
        map.invalidateSize();
    }, 100);
}

function resetMap() {
    if (map) {
        drawnItems.clearLayers();
        isDrawing = false;
        currentPolygon = null;
        updateFieldInfo(null);
    }
}

function iniciarDibujoPoligono() {
    isDrawing = true;
    $('#status').text('Haz clic en el mapa para dibujar tu campo. Haz clic en el primer punto para cerrar el polígono.').removeClass('text-success').addClass('text-warning');

    // Limpiar solo si no hay un campo cargado para editar
    if (!$('#idCampo').val()) {
        drawnItems.clearLayers();
    }

    let polygonPoints = [];
    currentPolygon = null;

    function onMapClick(e) {
        polygonPoints.push(e.latlng);

        // Dibujar puntos temporales
        L.circleMarker(e.latlng, {
            radius: 5,
            color: '#3498db',
            fillColor: '#2980b9',
            fillOpacity: 0.8
        }).addTo(map);

        // Si hay al menos 2 puntos, dibujar líneas
        if (polygonPoints.length >= 2) {
            if (currentPolygon) {
                map.removeLayer(currentPolygon);
            }

            currentPolygon = L.polygon(polygonPoints, {
                color: '#0d6efd',
                fillColor: '#0d6efd',
                fillOpacity: 0.2,
                weight: 3
            }).addTo(map);
        }

        // Verificar si el usuario hizo clic cerca del primer punto para cerrar el polígono
        if (polygonPoints.length > 2) {
            const firstPoint = polygonPoints[0];
            const distance = Math.sqrt(
                Math.pow(e.latlng.lat - firstPoint.lat, 2) +
                Math.pow(e.latlng.lng - firstPoint.lng, 2)
            );

            if (distance < 0.0005) {
                // Cerrar el polígono
                map.off('click', onMapClick);
                isDrawing = false;

                drawnItems.addLayer(currentPolygon);
                updateFieldInfo(currentPolygon);

                $('#status').text('Polígono cerrado. Puedes calcular la información.').removeClass('text-warning').addClass('text-success');
            }
        }
    }

    map.on('click', onMapClick);
}

// ========== FUNCIONES PARA CARGAR POLÍGONO EXISTENTE ==========
function cargarPoligonoEnMapa(coordenadasJson, latitud, longitud) {
    try {
        const coordenadas = JSON.parse(coordenadasJson);

        if (coordenadas && coordenadas.length >= 3) {
            // Convertir las coordenadas al formato de Leaflet
            const latlngs = coordenadas.map(coord => [coord.lat, coord.lng]);

            // Limpiar cualquier polígono existente
            drawnItems.clearLayers();

            // Crear y agregar el polígono
            const polygon = L.polygon(latlngs, {
                color: '#0d6efd',
                fillColor: '#0d6efd',
                fillOpacity: 0.2,
                weight: 3
            }).addTo(map);

            drawnItems.addLayer(polygon);

            // Actualizar la información del campo
            updateFieldInfo(polygon);

            // Centrar el mapa en el polígono
            map.fitBounds(polygon.getBounds());

            // Agregar marcador en el centro si hay coordenadas
            //if (latitud && longitud) {
            //    L.marker([latitud, longitud])
            //        .addTo(map)
            //        .bindPopup('Centro del campo')
            //        .openPopup();
            //}

            $('#status').text('Campo cargado correctamente').removeClass('text-warning').addClass('text-success');
        }
    } catch (error) {
        console.error('Error al cargar el polígono:', error);
        mostrarError('Error al cargar la geometría del campo');
    }
}

// ========== FUNCIONES DE CÁLCULO ==========
function calculateCenter(latlngs) {
    let latSum = 0;
    let lngSum = 0;
    const count = latlngs.length;

    latlngs.forEach(latlng => {
        latSum += latlng.lat;
        lngSum += latlng.lng;
    });

    return {
        lat: latSum / count,
        lng: lngSum / count
    };
}

function calculateAreaTurf(latlngs) {
    if (latlngs.length < 3) return 0;

    // Crear el polígono en formato GeoJSON
    const coordinates = latlngs.map(latlng => [latlng.lng, latlng.lat]);
    coordinates.push(coordinates[0]); // Cerrar el polígono

    const polygon = turf.polygon([coordinates]);
    const area = turf.area(polygon); // Área en metros cuadrados
    return area;
}

function calculatePerimeterTurf(latlngs) {
    if (latlngs.length < 3) return 0;

    const coordinates = latlngs.map(latlng => [latlng.lng, latlng.lat]);
    coordinates.push(coordinates[0]); // Cerrar el polígono

    const polygon = turf.polygon([coordinates]);
    const perimeter = turf.length(polygon, { units: 'meters' }); // Perímetro en metros
    return perimeter;
}

function calculateArea(latlngs) {
    let area = 0;
    const n = latlngs.length;

    for (let i = 0; i < n; i++) {
        const j = (i + 1) % n;
        area += latlngs[i].lng * latlngs[j].lat;
        area -= latlngs[j].lng * latlngs[i].lat;
    }

    area = Math.abs(area) / 2.0;

    // Conversión aproximada a hectáreas
    const areaHectareas = area * 10000;

    return areaHectareas;
}

function calculatePerimeter(latlngs) {
    let perimeter = 0;
    const n = latlngs.length;

    for (let i = 0; i < n; i++) {
        const j = (i + 1) % n;
        const dx = latlngs[j].lng - latlngs[i].lng;
        const dy = latlngs[j].lat - latlngs[i].lat;
        perimeter += Math.sqrt(dx * dx + dy * dy) * 111319.9;
    }

    return perimeter;
}

function updateFieldInfo(polygon) {
    if (!polygon) {
        $('#status').text('No se ha dibujado ningún campo').removeClass('text-success').addClass('text-warning');
        $('#center-coords').text('-');
        $('#area').text('-');
        $('#perimeter').text('-');
        $('#superficieHectareas').val('');
        $('#coordenadasPoligono').val('');
        return;
    }

    const latlngs = polygon.getLatLngs()[0];

    if (latlngs.length < 3) {
        $('#status').text('Se necesitan al menos 3 puntos para formar un polígono').removeClass('text-success').addClass('text-warning');
        return;
    }

    const center = calculateCenter(latlngs);
    const areaM2 = calculateAreaTurf(latlngs);
    const areaHectareas = areaM2 / 10000;
    const perimeter = calculatePerimeterTurf(latlngs);

    // Actualizar la interfaz
    $('#status').text('Campo dibujado correctamente').removeClass('text-warning').addClass('text-success');
    $('#center-coords').text(`${center.lat.toFixed(6)}, ${center.lng.toFixed(6)}`);
    $('#area').text(`${areaHectareas.toFixed(4)} Ha`);
    $('#perimeter').text(`${perimeter.toFixed(2)} metros`);
    $('#superficieHectareas').val(areaHectareas.toFixed(2));

    // Guardar coordenadas en campo oculto para el formulario
    const coordenadas = latlngs.map(latlng => ({
        lat: latlng.lat,
        lng: latlng.lng
    }));
    $('#coordenadasPoligono').val(JSON.stringify(coordenadas));

}


// Función para abrir el modal de visualización
function abrirModalVisualizacion(id) {
    var modal = new bootstrap.Modal($('#modalVisualizacion')[0]);

    // Cargar datos del campo
    cargarDatosParaVisualizacion(id);

    modal.show();
}

// Función MEJORADA para cargar datos del campo
function cargarDatosParaVisualizacion(id) {
    $.ajax({
        url: '/Campo/GetById/' + id,
        type: 'GET',
        success: function (response) {
            if (response.success) {
                currentFieldData = response.object;

                // Actualizar información del campo
                $('#view-nombre').text(currentFieldData.nombre);
                $('#view-ubicacion').text(currentFieldData.ubicacion || 'No especificada');
                $('#view-superficie').text(currentFieldData.superficieHectareas ?
                    parseFloat(currentFieldData.superficieHectareas).toLocaleString('es-ES', {
                        minimumFractionDigits: 2,
                        maximumFractionDigits: 2
                    }) + ' Ha' : '-');
                $('#view-centro').text(currentFieldData.latitud && currentFieldData.longitud ?
                    `${parseFloat(currentFieldData.latitud).toFixed(6)}, ${parseFloat(currentFieldData.longitud).toFixed(6)}` : '-');

                // Inicializar mapa
                inicializarMapaVisualizacion();

                // INICIALIZAR FECHA
                inicializarFecha();

                // Cargar polígono del campo
                if (currentFieldData.coordenadasPoligono) {
                    setTimeout(() => {
                        cargarPoligonoVisualizacion(currentFieldData.coordenadasPoligono, currentFieldData.latitud, currentFieldData.longitud);
                        // Cargar datos y análisis
                        cargarDatosYAnalisis();
                    }, 500);
                }

            } else {
                mostrarError(response.message || 'Error al cargar los datos del campo');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

// Función para inicializar la fecha
function inicializarFecha() {
    const hoy = new Date().toISOString().split('T')[0];
    $('#layer-date').val(hoy);
    console.log('📅 Fecha inicializada:', hoy);
}

// Función para cargar datos y análisis
async function cargarDatosYAnalisis() {
    if (!currentFieldData) return;

    const lat = currentFieldData.latitud;
    const lon = currentFieldData.longitud;
    const fecha = $('#layer-date').val();

    if (lat && lon) {
        await cargarDatosMeteorologicos(lat, lon, fecha);
        actualizarAnalisisCompleto();
    }
}

// Función para cargar datos meteorológicos
async function cargarDatosMeteorologicos(lat, lon, fecha = null) {
    try {
        const hoy = new Date().toISOString().split('T')[0];
        const esFechaActual = !fecha || fecha === hoy;

        let url;

        if (esFechaActual) {
            // Datos en tiempo real y pronóstico
            url = `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&current=temperature_2m,relative_humidity_2m,precipitation,soil_temperature_0cm,soil_moisture_0_1cm&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=auto`;
        } else {
            // Datos históricos
            url = `https://archive-api.open-meteo.com/v1/archive?latitude=${lat}&longitude=${lon}&start_date=${fecha}&end_date=${fecha}&daily=temperature_2m_max,temperature_2m_min,precipitation_sum&timezone=auto`;
        }

        console.log(`🌤️ Cargando datos para fecha: ${fecha || 'hoy'}`);
        const response = await fetch(url);

        if (response.ok) {
            const data = await response.json();
            weatherData = data;
            weatherData.fechaConsulta = fecha || hoy;
            weatherData.esTiempoReal = esFechaActual;

            console.log('✅ Datos meteorológicos cargados:', {
                fecha: fecha || 'hoy',
                tipo: esFechaActual ? 'tiempo_real' : 'historico',
                datos: data
            });
        } else {
            console.warn('❌ No se pudieron cargar datos meteorológicos');
            weatherData = null;
        }
    } catch (error) {
        console.error('Error cargando datos meteorológicos:', error);
        weatherData = null;
    }
}

// Función para inicializar el mapa
function inicializarMapaVisualizacion() {
    // Destruir mapa anterior si existe
    if (viewMap) {
        viewMap.remove();
    }

    viewMap = L.map('view-map').setView([-34.6037, -58.3816], 13);

    // Capas base
    baseLayers = {
        "Google Satélite": L.tileLayer('https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}', {
            attribution: '© Google Maps',
            maxZoom: 20
        }),
        "OpenStreetMap": L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap',
            maxZoom: 19
        })
    };

    // Agregar capa base por defecto
    baseLayers["Google Satélite"].addTo(viewMap);

    // Configurar capas overlay
    configurarCapasOverlay();

    // Agregar control de capas
    L.control.layers(baseLayers, overlayLayers, {
        collapsed: true,
        position: 'topright'
    }).addTo(viewMap);

    // Agregar control de escala
    L.control.scale({ imperial: false }).addTo(viewMap);

    console.log('🗺️ Mapa de visualización inicializado');
}

// Función para configurar capas overlay
// Función MEJORADA para configurar capas overlay que SÍ funcionan
function configurarCapasOverlay() {
    overlayLayers = {
        // ESRI World Imagery - SIEMPRE funciona
        "ESRI Satélite": L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            attribution: '© ESRI',
            maxZoom: 19,
            opacity: 0.9
        }),

        // Google Híbrido - SIEMPRE funciona
        "Google Híbrido": L.tileLayer('https://mt1.google.com/vt/lyrs=s,h&x={x}&y={y}&z={z}', {
            attribution: '© Google Maps',
            maxZoom: 20,
            opacity: 0.9
        }),

        // OpenTopoMap - SIEMPRE funciona
        "Mapa Topográfico": L.tileLayer('https://{s}.tile.opentopomap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenTopoMap',
            maxZoom: 17,
            opacity: 0.8
        }),

        // Mapa de Suelos (simulado)
        "Mapa de Suelos": L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap - Datos de suelo simulados',
            opacity: 0.6
        })
    };
}

// Función para cargar el polígono en el mapa
function cargarPoligonoVisualizacion(coordenadasJson, latitud, longitud) {
    try {
        const coordenadas = JSON.parse(coordenadasJson);

        if (coordenadas && coordenadas.length >= 3) {
            // Limpiar polígono anterior si existe
            if (campoPolygon) {
                viewMap.removeLayer(campoPolygon);
            }

            // Convertir coordenadas al formato de Leaflet
            const latlngs = coordenadas.map(coord => [coord.lat, coord.lng]);

            // Crear polígono con estilo normal
            campoPolygon = L.polygon(latlngs, {
                color: '#0d6efd',
                fillColor: '#0d6efd',
                fillOpacity: 0.2,
                weight: 3,
                opacity: 0.8
            }).addTo(viewMap);

            // Agregar popup informativo
            campoPolygon.bindPopup(`
                <div class="text-center">
                    <h6>${$('#view-nombre').text()}</h6>
                    <p class="mb-1"><small>Superficie: ${$('#view-superficie').text()}</small></p>
                    <p class="mb-0"><small>Ubicación: ${$('#view-ubicacion').text()}</small></p>
                </div>
            `);

            // Centrar el mapa en el polígono
            viewMap.fitBounds(campoPolygon.getBounds().pad(0.1));

            console.log('✅ Polígono del campo cargado correctamente');
        }
    } catch (error) {
        console.error('Error al cargar el polígono:', error);
        mostrarError('Error al cargar la geometría del campo');
    }
}

// Función para cambiar visualización
// Función MEJORADA para cambiar visualización
function cambiarVisualizacion(vizType) {
    console.log(`🎨 Cambiando visualización: ${vizType}`);

    // PRIMERO: Restaurar capa base por defecto (Google Satélite)
    Object.values(baseLayers).forEach(layer => {
        if (viewMap.hasLayer(layer)) {
            viewMap.removeLayer(layer);
        }
    });
    baseLayers["Google Satélite"].addTo(viewMap);

    // LUEGO: Remover capas overlay
    Object.values(overlayLayers).forEach(layer => {
        if (viewMap.hasLayer(layer)) {
            viewMap.removeLayer(layer);
        }
    });

    // Ocultar todas las leyendas
    $('#ndvi-legend').hide();
    $('#humidity-legend').hide();
    $('#esri-legend').hide(); 

    // Remover clase active de todos los botones
    $('.btn-viz').removeClass('active');
    // Agregar clase active al botón clickeado
    $(`.btn-viz[data-viz="${vizType}"]`).addClass('active');

    let vizName = '';

    // Aplicar visualización según el tipo
    switch (vizType) {
        case 'normal':
            aplicarVisualizacionNormal();
            vizName = 'Vista Normal';
            break;

        case 'ndvi':
            aplicarVisualizacionNDVI();
            vizName = 'Color por Vegetación';
            $('#ndvi-legend').show();
            break;

        case 'humidity':
            aplicarVisualizacionHumedad();
            vizName = 'Color por Humedad';
            $('#humidity-legend').show();
            break;

        case 'sentinel':
            aplicarVisualizacionSentinel();
            vizName = 'Vista ESRI Satélite';
            $('#esri-legend').show();
            break;

        default:
            aplicarVisualizacionNormal();
            vizName = 'Vista Normal';
            break;
    }

    // Actualizar texto de visualización actual
    $('#current-viz').text(vizName);
    currentViz = vizType;
}

// Función para aplicar visualización normal
function aplicarVisualizacionNormal() {
    if (campoPolygon) {
        campoPolygon.setStyle({
            fillColor: '#0d6efd',
            fillOpacity: 0.2,
            color: '#0d6efd',
            weight: 3
        });
    }
}

// Función para aplicar visualización NDVI
function aplicarVisualizacionNDVI() {
    if (!campoPolygon) return;

    let ndviValue = 0.5;
    let color = '#0d6efd';

    if (weatherData) {
        if (weatherData.esTiempoReal && weatherData.current) {
            // Cálculo con datos en tiempo real (código existente)
            const temp = weatherData.current.temperature_2m;
            const humidity = weatherData.current.relative_humidity_2m;
            const soilMoisture = weatherData.current.soil_moisture_0_1cm;

            ndviValue = 0.3 + (temp / 40) * 0.3 + (humidity / 100) * 0.2 + soilMoisture * 2;
        } else if (weatherData.daily) {
            // Cálculo con datos históricos
            const daily = weatherData.daily;
            const index = 0;
            const tempMax = daily.temperature_2m_max ? daily.temperature_2m_max[index] : 20;
            const tempMin = daily.temperature_2m_min ? daily.temperature_2m_min[index] : 10;
            const precip = daily.precipitation_sum ? daily.precipitation_sum[index] : 0;

            const tempPromedio = (tempMax + tempMin) / 2;
            const humedadEstimada = Math.min(80, Math.max(30, 50 + (precip * 10)));

            ndviValue = 0.3 + (tempPromedio / 40) * 0.3 + (humedadEstimada / 100) * 0.2 + (precip * 0.5);
        }

        ndviValue = Math.min(Math.max(ndviValue, 0.1), 0.9);

        // Determinar color según NDVI (igual para ambos casos)
        if (ndviValue > 0.6) {
            color = "#1a9850";
        } else if (ndviValue > 0.3) {
            color = "#fee08b";
        } else {
            color = "#d73027";
        }
    }

    campoPolygon.setStyle({
        fillColor: color,
        fillOpacity: 0.6,
        color: color,
        weight: 3
    });
}

// Función para aplicar visualización humedad
function aplicarVisualizacionHumedad() {
    if (!campoPolygon) return;

    let color = '#0d6efd';

    if (weatherData && weatherData.current) {
        const soilMoisture = weatherData.current.soil_moisture_0_1cm;

        // Determinar color según humedad
        if (soilMoisture < 0.1) {
            color = "#d73027"; // Seco
        } else if (soilMoisture < 0.2) {
            color = "#fdae61"; // Ligeramente húmedo
        } else if (soilMoisture < 0.3) {
            color = "#fee08b"; // Húmedo
        } else {
            color = "#1a9850"; // Muy húmedo
        }
    }

    campoPolygon.setStyle({
        fillColor: color,
        fillOpacity: 0.6,
        color: color,
        weight: 3
    });
}

// Función MEJORADA para aplicar visualización Sentinel/ESRI
// Función MEJORADA para aplicar visualización ESRI Satélite
function aplicarVisualizacionSentinel() {
    // Remover capa base actual
    if (viewMap && viewMap.hasLayer(baseLayers["Google Satélite"])) {
        viewMap.removeLayer(baseLayers["Google Satélite"]);
    }

    // Agregar ESRI World Imagery
    if (overlayLayers["ESRI Satélite"]) {
        overlayLayers["ESRI Satélite"].addTo(viewMap);

        // Agregar información contextual al polígono
        if (campoPolygon) {
            campoPolygon.setStyle({
                fillColor: '#ff0000',
                fillOpacity: 0.2,
                color: '#ff0000',
                weight: 3
            });

            // Actualizar el popup con información específica de vista satelital
            campoPolygon.bindPopup(`
                <div class="text-center">
                    <h6>${$('#view-nombre').text()}</h6>
                    <p class="small mb-2">📷 <strong>Vista ESRI Satélite</strong></p>
                    <p class="small mb-1">Superficie: ${$('#view-superficie').text()}</p>
                    <p class="small mb-2">Ubicación: ${$('#view-ubicacion').text()}</p>
                    <div class="alert alert-info p-1 small">
                        <strong>💡 Consejo:</strong> Observa los patrones de color para identificar tipos de vegetación y suelo
                    </div>
                </div>
            `);
        }

        console.log('✅ Vista ESRI Satélite activada con leyenda');
    } else {
        console.warn('❌ Capa ESRI no disponible');
        // Fallback a Google Híbrido
        if (overlayLayers["Google Híbrido"]) {
            overlayLayers["Google Híbrido"].addTo(viewMap);
        }
    }
}

// Función para actualizar análisis completo
// Función MEJORADA para actualizar análisis completo
function actualizarAnalisisCompleto() {
    if (!currentFieldData) return;

    const fechaSeleccionada = $('#layer-date').val();
    const hoy = new Date().toISOString().split('T')[0];
    const esFechaActual = fechaSeleccionada === hoy;

    console.log(`📊 Actualizando análisis para fecha: ${fechaSeleccionada} (${esFechaActual ? 'hoy' : 'histórica'})`);

    let contenido = `
        <div class="row">
            <div class="col-md-6">
                <h6 class="border-bottom pb-2">
                    ${esFechaActual ? '📊 Datos en Tiempo Real' : '📊 Datos Históricos'}
                    ${!esFechaActual ? `<br><small class="text-muted">${fechaSeleccionada}</small>` : ''}
                </h6>
    `;

    // Datos Meteorológicos - Manejar tanto tiempo real como histórico
    if (weatherData) {
        const ahora = new Date().toLocaleTimeString('es-AR');

        if (weatherData.esTiempoReal && weatherData.current) {
            // DATOS EN TIEMPO REAL
            const current = weatherData.current;

            contenido += `
                <div class="mb-3">
                    <small class="text-muted">Actualizado: ${ahora}</small>
                    <div class="row small mt-1">
                        <div class="col-6 mb-2">
                            <strong>🌡️ Temperatura:</strong><br>
                            ${current.temperature_2m}°C
                        </div>
                        <div class="col-6 mb-2">
                            <strong>💧 Humedad Aire:</strong><br>
                            ${current.relative_humidity_2m}%
                        </div>
                        <div class="col-6 mb-2">
                            <strong>🌧️ Precipitación:</strong><br>
                            ${current.precipitation} mm
                        </div>
                        <div class="col-6 mb-2">
                            <strong>💧 Humedad Suelo:</strong><br>
                            ${current.soil_moisture_0_1cm} /m³  
                        </div>
                        <div class="col-6 mb-2">
                            <strong>🌡️ Temp. Suelo:</strong><br>
                            ${current.soil_temperature_0cm}°C
                        </div>
                        <div class="col-6 mb-2">
                            <strong>📅 Fecha:</strong><br>
                            ${fechaSeleccionada}
                        </div>
                    </div>
                </div>
            `;
        } else if (weatherData.daily) {
            // DATOS HISTÓRICOS
            const daily = weatherData.daily;
            const index = 0; // Primer día (la fecha seleccionada)

            contenido += `
                <div class="mb-3">
                    <small class="text-muted">Datos históricos - ${fechaSeleccionada}</small>
                    <div class="row small mt-1">
                        <div class="col-6 mb-2">
                            <strong>🌡️ Temp. Máx:</strong><br>
                            ${daily.temperature_2m_max ? daily.temperature_2m_max[index] + '°C' : 'N/D'}
                        </div>
                        <div class="col-6 mb-2">
                            <strong>🌡️ Temp. Mín:</strong><br>
                            ${daily.temperature_2m_min ? daily.temperature_2m_min[index] + '°C' : 'N/D'}
                        </div>
                        <div class="col-6 mb-2">
                            <strong>🌧️ Precipitación:</strong><br>
                            ${daily.precipitation_sum ? daily.precipitation_sum[index] + ' mm' : 'N/D'}
                        </div>
                        <div class="col-6 mb-2">
                            <strong>💧 Humedad Suelo:</strong><br>
                            <em class="text-muted">-</em>
                        </div>
                        <div class="col-6 mb-2">
                            <strong>🌡️ Temp. Suelo:</strong><br>
                            <em class="text-muted">-</em>
                        </div>
                        <div class="col-6 mb-2">
                            <strong>📊 Tipo:</strong><br>
                            Datos Históricos
                        </div>
                    </div>
                </div>
            `;
        }
    } else {
        contenido += `
            <div class="alert alert-warning p-2 small">
                <i class="ph ph-warning-circle me-1"></i>
                No se pudieron cargar los datos para esta fecha
            </div>
        `;
    }

    contenido += `
            </div>
            <div class="col-md-6">
                <h6 class="border-bottom pb-2">🌱 Análisis y Recomendaciones</h6>
    `;

    // Análisis de Vegetación - Adaptado para datos históricos
    let ndviValue = 0.5;
    let estadoVegetacion = "Media";
    let recomendacionVegetacion = "Monitorear crecimiento regularmente";
    let baseAnalisis = "";

    if (weatherData) {
        if (weatherData.esTiempoReal && weatherData.current) {
            // ANÁLISIS CON DATOS EN TIEMPO REAL
            const temp = weatherData.current.temperature_2m;
            const humidity = weatherData.current.relative_humidity_2m;
            const soilMoisture = weatherData.current.soil_moisture_0_1cm;

            ndviValue = 0.3 + (temp / 40) * 0.3 + (humidity / 100) * 0.2 + soilMoisture * 2;
            ndviValue = Math.min(Math.max(ndviValue, 0.1), 0.9);
            baseAnalisis = `Basado en condiciones actuales (${temp}°C, ${humidity}% humedad)`;

        } else if (weatherData.daily) {
            // ANÁLISIS CON DATOS HISTÓRICOS
            const daily = weatherData.daily;
            const index = 0;
            const tempMax = daily.temperature_2m_max ? daily.temperature_2m_max[index] : 20;
            const tempMin = daily.temperature_2m_min ? daily.temperature_2m_min[index] : 10;
            const precip = daily.precipitation_sum ? daily.precipitation_sum[index] : 0;

            const tempPromedio = (tempMax + tempMin) / 2;
            const humedadEstimada = Math.min(80, Math.max(30, 50 + (precip * 10)));

            ndviValue = 0.3 + (tempPromedio / 40) * 0.3 + (humedadEstimada / 100) * 0.2 + (precip * 0.5);
            ndviValue = Math.min(Math.max(ndviValue, 0.1), 0.9);
            baseAnalisis = `Basado en datos históricos (${tempPromedio.toFixed(1)}°C promedio, ${precip}mm lluvia)`;
        }

        // Determinar estado (común para ambos casos)
        if (ndviValue > 0.6) {
            estadoVegetacion = "Alta";
            recomendacionVegetacion = "Condiciones óptimas para el crecimiento";
        } else if (ndviValue > 0.3) {
            estadoVegetacion = "Media";
            recomendacionVegetacion = "Monitorear crecimiento regularmente";
        } else {
            estadoVegetacion = "Baja";
            recomendacionVegetacion = "Evaluar necesidad de riego y nutrientes";
        }
    }

    contenido += `
        <div class="mb-3">
            <strong>Estado de Vegetación:</strong>
            <div class="d-flex justify-content-between align-items-center mt-1">
                <span>${estadoVegetacion}</span>
                <small class="text-muted">NDVI: ${ndviValue.toFixed(3)}</small>
            </div>
            <div class="mt-1 small text-muted">
                ${baseAnalisis}<br>
                ${recomendacionVegetacion}
            </div>
        </div>
    `;

    // Análisis de Suelo - Adaptado para datos históricos
    let estadoSuelo = "Óptimo";
    let recomendacionSuelo = "Condiciones adecuadas";
    let humedadSueloTexto = "N/D";

    if (weatherData) {
        if (weatherData.esTiempoReal && weatherData.current) {
            // SUELO CON DATOS EN TIEMPO REAL
            const soilMoisture = weatherData.current.soil_moisture_0_1cm;
            const soilTemp = weatherData.current.soil_temperature_0cm;
            humedadSueloTexto = `${soilMoisture} /m³`;

            if (soilMoisture < 0.1) {
                estadoSuelo = "Seco";
                recomendacionSuelo = "Considerar programa de riego";
            } else if (soilMoisture < 0.2) {
                estadoSuelo = "Ligeramente húmedo";
                recomendacionSuelo = "Monitorear humedad regularmente";
            } else if (soilMoisture < 0.3) {
                estadoSuelo = "Húmedo";
                recomendacionSuelo = "Condiciones adecuadas";
            } else {
                estadoSuelo = "Muy húmedo";
                recomendacionSuelo = "Condiciones óptimas";
            }

            if (soilTemp < 10) {
                recomendacionSuelo += ". Temperatura baja para siembra";
            } else if (soilTemp > 25) {
                recomendacionSuelo += ". Temperatura alta, monitorear estrés";
            }

        } else if (weatherData.daily) {
            // SUELO CON DATOS HISTÓRICOS
            const daily = weatherData.daily;
            const index = 0;
            const precip = daily.precipitation_sum ? daily.precipitation_sum[index] : 0;

            // Estimación basada en precipitación
            if (precip > 10) {
                estadoSuelo = "Muy húmedo";
                humedadSueloTexto = "Estimada (alta)";
                recomendacionSuelo = "Condiciones óptimas de humedad";
            } else if (precip > 5) {
                estadoSuelo = "Húmedo";
                humedadSueloTexto = "Estimada (media)";
                recomendacionSuelo = "Condiciones adecuadas";
            } else if (precip > 1) {
                estadoSuelo = "Ligeramente húmedo";
                humedadSueloTexto = "Estimada (baja)";
                recomendacionSuelo = "Monitorear humedad regularmente";
            } else {
                estadoSuelo = "Seco";
                humedadSueloTexto = "Estimada (muy baja)";
                recomendacionSuelo = "Considerar programa de riego";
            }
        }
    }

    contenido += `
        <div class="mb-3">
            <strong>Estado del Suelo:</strong>
            <div class="d-flex justify-content-between align-items-center mt-1">
                <span>${estadoSuelo}</span>
                <small class="text-muted">Humedad: ${humedadSueloTexto}</small>
            </div>
            <div class="mt-1 small text-muted">
                ${recomendacionSuelo}
            </div>
        </div>
    `;

    contenido += `
            </div>
        </div>
    `;

    $('#analisis-completo').html(contenido);

    // Actualizar visualización si está activa
    if (currentViz === 'ndvi' || currentViz === 'humidity') {
        cambiarVisualizacion(currentViz);
    }
}

// Eventos
$(document).on('click', '.btn-viz', function () {
    const vizType = $(this).data('viz');
    cambiarVisualizacion(vizType);
});

// Evento para cambios de fecha
$('#layer-date').on('change', function () {
    const fechaSeleccionada = $(this).val();
    console.log('📅 Fecha cambiada:', fechaSeleccionada);

    // Mostrar loading
    $('#analisis-completo').html(`
        <div class="text-center text-muted">
            <i class="ph ph-hourglass me-1"></i>Cargando datos para ${fechaSeleccionada}...
        </div>
    `);

    // Recargar datos con la nueva fecha
    setTimeout(() => {
        cargarDatosYAnalisis();
    }, 500);
});

// Evento para el modal
$('#modalVisualizacion').on('shown.bs.modal', function () {
    if (viewMap) {
        setTimeout(() => {
            viewMap.invalidateSize();
            // Aplicar visualización por defecto
            cambiarVisualizacion('normal');
        }, 300);
    }
});

// Evento para exportar análisis
$('#btn-export-analysis').on('click', function () {
    // Implementar exportación si es necesario
    mostrarExito('Función de exportación en desarrollo');
});




























function cargarHistorico(id) {
    var modal = new bootstrap.Modal($('#modalHistorial')[0]);

    mostrarLoading();

    $.ajax({
        url: '/Campo/GetHistorialById/' + id,
        type: 'GET',
        success: function (response) {
            cerrarAlertas();
            if (response.success) {
                modal.show();
                historialData = response.listObject || [];
                mostrarHistorial(historialData);
                modal.show();
            } else {
                mostrarError(response.message || 'Error al cargar el historial');
            }
        },
        error: function () {
            cerrarAlertas();
            mostrarError('Error al conectar con el servidor');
        }
    });
}

// Función para mostrar el historial
function mostrarHistorial(datos) {
    if (!datos || datos.length === 0) {
        $('#contenido-historial').html(`
            <div class="text-center text-muted py-4">
                <i class="ph ph-clipboard-text me-1"></i>
                No hay actividades registradas
            </div>
        `);
        $('#historial-total').text('0 actividades');
        return;
    }

    // Ordenar por fecha (más reciente primero)
    datos.sort((a, b) => new Date(b.fecha) - new Date(a.fecha));

    // Actualizar información general
    $('#historial-total').text(`${datos.length} actividades`);

    // Obtener información del campo
    if (datos.length > 0) {
        const primerRegistro = datos[0];
        $('#historial-nombre').text(primerRegistro.campo || 'Campo');
        $('#historial-superficie').text(primerRegistro.lote ? `Lote: ${primerRegistro.lote}` : '');
    }

    // Agrupar por año
    const actividadesPorAnio = agruparPorAnio(datos);

    // Llenar filtro de años
    llenarFiltroAnios(actividadesPorAnio);

    // Mostrar el historial
    mostrarHistorialSimple(actividadesPorAnio);
}

// Función para agrupar actividades por año
function agruparPorAnio(datos) {
    const agrupado = {};

    datos.forEach(actividad => {
        const fecha = new Date(actividad.fecha);
        const anio = fecha.getFullYear();

        if (!agrupado[anio]) {
            agrupado[anio] = [];
        }

        agrupado[anio].push(actividad);
    });

    return agrupado;
}

// Función para llenar el filtro de años
function llenarFiltroAnios(actividadesPorAnio) {
    $('#filtro-anio').empty().append('<option value="todos">Todos los años</option>');

    // Años ordenados descendente
    const anios = Object.keys(actividadesPorAnio).sort((a, b) => b - a);
    anios.forEach(anio => {
        const cantidad = actividadesPorAnio[anio].length;
        $('#filtro-anio').append(`<option value="${anio}">${anio} (${cantidad})</option>`);
    });
}

// Función para mostrar historial simple
function mostrarHistorialSimple(actividadesPorAnio) {
    let contenido = '';
    const anios = Object.keys(actividadesPorAnio).sort((a, b) => b - a);

    anios.forEach(anio => {
        const actividades = actividadesPorAnio[anio];

        contenido += `
            <div class="card mb-3" data-anio="${anio}">
                <div class="card-header bg-light py-2">
                    <h6 class="mb-0 fw-bold">
                        <i class="ph ph-calendar-blank me-1"></i>${anio}
                        <span class="badge bg-secondary ms-2">${actividades.length}</span>
                    </h6>
                </div>
                <div class="card-body p-0">
                    <div class="list-group list-group-flush">
        `;

        actividades.forEach(actividad => {
            const fecha = new Date(actividad.fecha);
            const fechaFormateada = fecha.toLocaleDateString('es-AR');
            const icono = actividad.iconoTipoActividad || 'ph-activity';
            const color = actividad.iconoColorTipoActividad || '#6c757d';

            contenido += `
    <div class="list-group-item border-0 py-2" data-anio="${anio}">
        <div class="row align-items-center">
            <div class="col-auto">
                <div class="actividad-icono me-2 flex-shrink-0">
                    <i class="ph ${icono} fs-5" style="color: ${color};"></i>
                </div>
            </div>
            <div class="col">
                <div class="d-flex justify-content-between align-items-center">
                    <div class="flex-grow-1">
                        <div class="d-flex align-items-center mb-1">
                            <h6 class="mb-0 fw-semibold me-2">${actividad.tipoActividad || 'Actividad'}</h6>
                            <small class="text-muted">${actividad.detalle || 'Sin detalles'}</small>
                        </div>
                        ${actividad.campania ? `<small class="text-muted d-block">${actividad.campania}</small>` : ''}
                    </div>
                    <div class="text-end ms-3">
                        <small class="text-muted">${fechaFormateada}</small>
                    </div>
                </div>
            </div>
        </div>
    </div>
`;
        });

        contenido += `
                    </div>
                </div>
            </div>
        `;
    });

    $('#contenido-historial').html(contenido);
}

// Función para aplicar filtro
function aplicarFiltro() {
    const anioSeleccionado = $('#filtro-anio').val();

    if (anioSeleccionado === 'todos') {
        // Mostrar todo
        const actividadesPorAnio = agruparPorAnio(historialData);
        mostrarHistorialSimple(actividadesPorAnio);
    } else {
        // Filtrar por año específico
        const actividadesFiltradas = historialData.filter(a =>
            new Date(a.fecha).getFullYear().toString() === anioSeleccionado
        );

        if (actividadesFiltradas.length === 0) {
            $('#contenido-historial').html(`
                <div class="text-center text-muted py-4">
                    <i class="ph ph-magnifying-glass me-1"></i>
                    No hay actividades para el año ${anioSeleccionado}
                </div>
            `);
        } else {
            const actividadesPorAnio = { [anioSeleccionado]: actividadesFiltradas };
            mostrarHistorialSimple(actividadesPorAnio);
        }
    }
}

// Filtro
$('#filtro-anio').on('change', aplicarFiltro);

// Reiniciar modal cuando se cierra
$('#modalHistorial').on('hidden.bs.modal', function () {
    historialData = [];
    $('#filtro-anio').empty().append('<option value="todos">Todos los años</option>');
});