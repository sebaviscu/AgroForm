$(document).ready(function () {
    // Inicializar DataTable
    var table = $('#tblActividades').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json'
        },
        dom: '<"row"<"col-md-6"B><"col-md-6"f>>rt<"row"<"col-md-6"l><"col-md-6"p>>',
        buttons: [
            { extend: 'excel', text: '<i class="ph ph-file-xls me-1"></i>Excel', className: 'btn btn-outline-success btn-sm', title: 'Actividades_' + new Date().toISOString().slice(0, 10) },
            { extend: 'pdf', text: '<i class="ph ph-file-pdf me-1"></i>PDF', className: 'btn btn-outline-danger btn-sm', title: 'Actividades_' + new Date().toISOString().slice(0, 10) },
            { extend: 'print', text: '<i class="ph ph-printer me-1"></i>Imprimir', className: 'btn btn-outline-info btn-sm' }
        ],
        columnDefs: [
            { orderable: false, targets: [8] },
            { searchable: false, targets: [5, 6, 8] },
            { className: 'dt-center', targets: [8] },
            { width: '100px', targets: [0, 6, 8] }
        ],
        order: [[2, 'desc']],
        pageLength: 25,
        responsive: true
    });

    // Filtrar por campo
    $('#campoFilter').change(function () {
        var idCampo = $(this).val();
        filtrarPorCampo(idCampo);
    });

    // Restablecer filtros
    $('#btnResetFilters').click(function () {
        $('#campoFilter').val('0');
        filtrarPorCampo(0);
    });

    function filtrarPorCampo(idCampo) {
        $.post('@Url.Action("FiltrarPorCampo","Actividad")', { idCampo: idCampo })
            .done(function (response) {
                if (response.success) {
                    actualizarDataTable(response.data);
                } else {
                    mostrarError(response.message || 'Error al filtrar actividades');
                }
            })
            .fail(function () {
                mostrarError('Error al conectar con el servidor');
            });
    }

    function actualizarDataTable(data) {
        table.clear();
        if (data && data.length) {
            $.each(data, function (i, actividad) {
                var duracion = actividad.duracion ? '<span>' + actividad.duracion.substring(0, 5) + 'h</span>' : '<span class="text-muted">-</span>';
                var costo = actividad.costo ? '<span class="fw-semibold">$' + parseFloat(actividad.costo).toFixed(2) + '</span>' : '<span class="text-muted">-</span>';
                var estadoBadge = getEstadoBadge(actividad.estado);
                var estadoOrder = getEstadoOrder(actividad.estado);

                table.row.add([
                    '<span class="badge bg-primary">' + actividad.tipoActividad + '</span>',
                    actividad.campo || '-',
                    '<span data-order="' + new Date(actividad.fecha).toISOString() + '">' + formatFecha(actividad.fecha) + '</span>',
                    actividad.descripcion || '-',
                    actividad.responsable || '-',
                    '<span data-order="' + (actividad.duracionMinutos || 0) + '">' + duracion + '</span>',
                    '<span data-order="' + (actividad.costo || 0) + '">' + costo + '</span>',
                    '<span class="badge ' + estadoBadge + '" data-order="' + estadoOrder + '">' + actividad.estado + '</span>',
                    '<div class="btn-group btn-group-sm">' +
                    '<button type="button" class="btn btn-outline-primary btn-view" data-id="' + actividad.id + '"><i class="ph ph-eye"></i></button>' +
                    '<button type="button" class="btn btn-outline-secondary btn-edit" data-id="' + actividad.id + '"><i class="ph ph-pencil"></i></button>' +
                    '<button type="button" class="btn btn-outline-danger btn-delete" data-id="' + actividad.id + '"><i class="ph ph-trash"></i></button>' +
                    '</div>'
                ]);
            });
        }
        table.draw();
    }

    function formatFecha(fechaStr) {
        return new Date(fechaStr).toLocaleDateString('es-ES');
    }

    function getEstadoBadge(estado) {
        switch (estado) {
            case 'Completada': return 'bg-success';
            case 'En Progreso': return 'bg-warning';
            case 'Pendiente': return 'bg-secondary';
            case 'Cancelada': return 'bg-danger';
            default: return 'bg-secondary';
        }
    }

    function getEstadoOrder(estado) {
        switch (estado) {
            case 'Pendiente': return 1;
            case 'En Progreso': return 2;
            case 'Completada': return 3;
            case 'Cancelada': return 4;
            default: return 0;
        }
    }

    function mostrarError(msg) { if (typeof toastr !== 'undefined') { toastr.error(msg); } else alert(msg); }
    function mostrarExito(msg) { if (typeof toastr !== 'undefined') { toastr.success(msg); } else alert(msg); }

    // Botones dinámicos
    $('#tblActividades tbody').on('click', '.btn-view', function () { verDetalles($(this).data('id')); });
    $('#tblActividades tbody').on('click', '.btn-edit', function () { window.location.href = '@Url.Action("Editar","Actividad")/' + $(this).data('id'); });
    $('#tblActividades tbody').on('click', '.btn-delete', function () { eliminarActividad($(this).data('id')); });

    function verDetalles(id) {
        $.get('@Url.Action("Detalles","Actividad")/' + id)
            .done(function (response) {
                if (response.success) { mostrarModalDetalles(response.data); }
                else { mostrarError(response.message || 'Error al cargar detalles'); }
            })
            .fail(function () { mostrarError('Error al conectar con el servidor'); });
    }

    function eliminarActividad(id) {
        if (!confirm('¿Está seguro de que desea eliminar esta actividad?')) return;
        $.post('@Url.Action("Eliminar","Actividad")', { id: id })
            .done(function (response) {
                if (response.success) { mostrarExito(response.message); filtrarPorCampo($('#campoFilter').val()); }
                else { mostrarError(response.message || 'Error al eliminar actividad'); }
            })
            .fail(function () { mostrarError('Error al conectar con el servidor'); });
    }

    function mostrarModalDetalles(actividad) {
        console.log('Detalles:', actividad);
        alert('Detalles de: ' + actividad.descripcion);
    }
});
