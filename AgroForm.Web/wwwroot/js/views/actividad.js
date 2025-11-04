$(document).ready(function () {
    // Inicializar DataTable
    var table = $('#tblActividades').DataTable({
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
                    title: 'Campanias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'pdf',
                    text: '<i class="ph ph-file-pdf me-1"></i>PDF',
                    className: 'btn btn-outline-danger btn-sm',
                    title: 'Campanias_' + new Date().toISOString().slice(0, 10).replace(/-/g, '')
                },
                {
                    extend: 'print',
                    text: '<i class="ph ph-printer me-1"></i>Imprimir',
                    className: 'btn btn-outline-info btn-sm'
                }
            ]
        },
        columnDefs: [
            { orderable: false, targets: [7] },
            { searchable: false, targets: [5, 6, 7] },
            { className: 'dt-center', targets: [7] },
            { width: '100px', targets: [3, 6, 7] }
        ],
        order: [[0, 'desc']],
        pageLength: 25,
        responsive: true
    });

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
