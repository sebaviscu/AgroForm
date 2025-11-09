// Variables globales
var table;
let map;
let drawnItems;
let isDrawing = false;
let currentPolygon = null;

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

    var url = datos.Id ? '/Campo/Update/' + datos.Id : '/Campo/Create';
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

    // Solo agregar marcador si no estamos en modo edición/carga
    const existingMarkers = drawnItems.getLayers().filter(layer => layer instanceof L.Marker);
    //if (existingMarkers.length === 0) {
    //    L.marker([center.lat, center.lng])
    //        .addTo(map)
    //        .bindPopup('Centro aprox. del campo')
    //        .openPopup();
    //}
}
