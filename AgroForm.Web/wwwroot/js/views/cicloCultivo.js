$(document).ready(function () {
    // Inicializar DataTable
    var tabla = $('#tablaCiclos').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/2.2.2/i18n/es-AR.json'
        },
        order: [[5, 'desc']],
        pageLength: 25,
        columnDefs: [
            { orderable: false, targets: [8] }
        ]
    });

    // Botón "Nuevo Ciclo"
    $('#btnNuevoCicloGestion').on('click', function () {
        abrirModalCrearCiclo();
    });

    // Botones "Ver Detalle"
    $(document).on('click', '.btnVerDetalle', function () {
        var row = $(this).closest('tr');
        var id = row.data('id');
        verDetalleCiclo(id);
    });

    // Botones "Cerrar Ciclo"
    $(document).on('click', '.btnCerrarCiclo', function () {
        var row = $(this).closest('tr');
        var id = row.data('id');
        var cultivo = row.find('td:eq(2)').text();
        mostrarConfirmacion('¿Está seguro de cerrar el ciclo de ' + cultivo + '?', 'Cerrar Ciclo')
            .then(function (result) {
                if (result.isConfirmed) {
                    cerrarCicloGestion(id, row);
                }
            });
    });
});

function abrirModalCrearCiclo() {
    // Verificar si el modal ya fue creado
    if ($('#modalCrearCicloGestion').length === 0) {
        var modalHtml =
            '<div class="modal fade" id="modalCrearCicloGestion" tabindex="-1" aria-hidden="true">' +
            '    <div class="modal-dialog">' +
            '        <div class="modal-content">' +
            '            <div class="modal-header bg-success text-white">' +
            '                <h5 class="modal-title"><i class="ph ph-seedling me-2"></i>Nuevo Ciclo de Cultivo</h5>' +
            '                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>' +
            '            </div>' +
            '            <div class="modal-body">' +
            '                <div class="mb-3">' +
            '                    <label for="gestCicloIdLote" class="form-label">Lote *</label>' +
            '                    <select class="form-select" id="gestCicloIdLote" required></select>' +
            '                </div>' +
            '                <div class="mb-3">' +
            '                    <label for="gestCicloIdCultivo" class="form-label">Cultivo *</label>' +
            '                    <select class="form-select" id="gestCicloIdCultivo" required></select>' +
            '                </div>' +
                        '                <div class="mb-3">' +
            '                    <label for="gestCicloEpoca" class="form-label">Época</label>' +
            '                    <select class="form-select" id="gestCicloEpoca">' +
            '                        <option value="">Sin especificar</option>' +
            '                        <option value="0">Primera</option>' +
            '                        <option value="1">Segunda</option>' +
            '                        <option value="2">Tercera</option>' +
            '                    </select>' +
            '                </div>' +
            '            </div>' +
            '            <div class="modal-footer">' +
            '                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancelar</button>' +
            '                <button type="button" class="btn btn-success" id="btnGuardarCicloGestion">' +
            '                    <i class="ph ph-check-circle me-1"></i>Crear Ciclo' +
            '                </button>' +
            '            </div>' +
            '        </div>' +
            '    </div>' +
            '</div>';
        $('body').append(modalHtml);
    }

    // Cargar lotes
    var selectLote = $('#gestCicloIdLote');
    selectLote.empty().append('<option value="">Cargando lotes...</option>');
    $.ajax({
        url: '/Lote/GetAll',
        type: 'GET',
        success: function (result) {
            selectLote.empty().append('<option value="">Seleccione un lote...</option>');
            if (result.success && result.listObject) {
                $.each(result.listObject, function (i, l) {
                    selectLote.append($('<option>', {
                        value: l.id,
                        text: l.nombre + (l.campo ? ' (' + l.campo.nombre + ')' : '')
                    }));
                });
            }
        }
    });

    // Cargar cultivos
    var selectCultivo = $('#gestCicloIdCultivo');
    selectCultivo.empty().append('<option value="">Cargando cultivos...</option>');
    $.ajax({
        url: '/Cultivo/GetVisible',
        type: 'GET',
        success: function (result) {
            selectCultivo.empty().append('<option value="">Seleccione un cultivo...</option>');
            if (result.success && result.listObject) {
                $.each(result.listObject, function (i, c) {
                    selectCultivo.append($('<option>', {
                        value: c.id,
                        text: c.nombre
                    }));
                });
            }
        }
    });

    
    // Guardar ciclo
    $('#btnGuardarCicloGestion').off('click').on('click', function () {
        var loteId = parseInt($('#gestCicloIdLote').val());
        var cultivoId = parseInt($('#gestCicloIdCultivo').val());

        if (!loteId) {
            mostrarMensaje('Debe seleccionar un lote', 'error');
            return;
        }
        if (!cultivoId) {
            mostrarMensaje('Debe seleccionar un cultivo', 'error');
            return;
        }

        var data = {
            idLote: loteId,
            idCultivo: cultivoId,
            epoca: parseInt($('#gestCicloEpoca').val()) || null
        };

        $.ajax({
            url: '/CicloCultivo/Crear',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                if (result.success) {
                    $('#modalCrearCicloGestion').modal('hide');
                    mostrarMensaje('Ciclo creado correctamente', 'success');
                    setTimeout(function () {
                        location.reload();
                    }, 500);
                } else {
                    mostrarError(result.message || 'Error al crear ciclo');
                }
            },
            error: function () {
                mostrarError('Error al conectar con el servidor');
            }
        });
    });

    $('#modalCrearCicloGestion').modal('show');
}


function verDetalleCiclo(id) {
    $.ajax({
        url: '/CicloCultivo/GetById/' + id,
        type: 'GET',
        success: function (result) {
            if (result.success && result.object) {
                var ciclo = result.object;
                var html = '';

                // Información general
                html += '<div class="row mb-3">';
                html += '    <div class="col-md-4"><strong>Lote:</strong> ' + (ciclo.lote?.nombre || '-') + '</div>';
                html += '    <div class="col-md-4"><strong>Cultivo:</strong> ' + (ciclo.cultivo?.nombre || '-') + '</div>';
                html += '</div>';
                html += '<div class="row mb-3">';
                html += '    <div class="col-md-4"><strong>Época:</strong> ' + ciclo.epocaDisplay + '</div>';
                html += '    <div class="col-md-4"><strong>Inicio:</strong> ' + (ciclo.fechaInicio ? new Date(ciclo.fechaInicio).toLocaleDateString() : '-') + '</div>';
                html += '    <div class="col-md-4"><strong>Fin:</strong> ' + (ciclo.fechaFin ? new Date(ciclo.fechaFin).toLocaleDateString() : '<span class="badge bg-success">Activo</span>') + '</div>';
                html += '</div>';

                // Resumen de labores
                var totalLabores = 0;
                var laboresHtml = '';
                var tipos = [
                    { key: 'siembras', title: 'Siembras', icon: 'ph-seedling', color: 'info' },
                    { key: 'riegos', title: 'Riegos', icon: 'ph-drop', color: 'primary' },
                    { key: 'fertilizaciones', title: 'Fertilizaciones', icon: 'ph-flask', color: 'warning' },
                    { key: 'pulverizaciones', title: 'Pulverizaciones', icon: 'ph-spray', color: 'danger' },
                    { key: 'monitoreos', title: 'Monitoreos', icon: 'ph-binoculars', color: 'secondary' },
                    { key: 'analisisSuelos', title: 'Análisis de Suelo', icon: 'ph-test-tube', color: 'dark' },
                    { key: 'cosechas', title: 'Cosechas', icon: 'ph-harvest', color: 'success' },
                    { key: 'otrasLabores', title: 'Otras Labores', icon: 'ph-wrench', color: 'secondary' }
                ];

                tipos.forEach(function (tipo) {
                    var items = ciclo[tipo.key] || [];
                    if (items.length > 0) {
                        totalLabores += items.length;
                        laboresHtml += '<div class="mb-2">';
                        laboresHtml += '    <span class="badge bg-' + tipo.color + ' me-1"><i class="ph ' + tipo.icon + ' me-1"></i>' + tipo.title + ': ' + items.length + '</span>';
                        laboresHtml += '</div>';
                    }
                });

                html += '<hr>';
                html += '<h6 class="fw-bold">Labores del ciclo (' + totalLabores + ')</h6>';
                html += laboresHtml || '<p class="text-muted">Sin labores registradas</p>';

                $('#detalleCicloBody').html(html);
                $('#modalDetalleCiclo').modal('show');
            } else {
                mostrarError('Error al obtener detalle del ciclo');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}

function cerrarCicloGestion(id, row) {
    var data = { idCicloCultivo: id };
    $.ajax({
        url: '/CicloCultivo/Cerrar',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(data),
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (result) {
            if (result.success) {
                mostrarMensaje('Ciclo cerrado correctamente', 'success');
                setTimeout(function () {
                    location.reload();
                }, 500);
            } else {
                mostrarError(result.message || 'Error al cerrar ciclo');
            }
        },
        error: function () {
            mostrarError('Error al conectar con el servidor');
        }
    });
}
