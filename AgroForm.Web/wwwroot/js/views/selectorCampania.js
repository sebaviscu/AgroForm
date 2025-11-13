$(document).ready(function () {
    //if (window.innerWidth < 992) {
    //    $('#tblCampanias').removeClass('w-100').addClass('w-75')
    //}

    //$('.campaña-item').on('click', function (e) {
    //    e.preventDefault();

    //    var campañaId = $(this).data('campaña-id');

    //    $.ajax({
    //        url: '@Url.Action("SetCurrent", "Campañas")',
    //        type: 'POST',
    //        contentType: 'application/json',
    //        data: JSON.stringify({ campañaId: parseInt(campañaId) }),
    //        success: function (data) {
    //            if (data.success) {
    //                $('#campañaActualText').text(
    //                    $(e.currentTarget).find('.fw-semibold').text()
    //                );
    //                location.reload();
    //            } else {
    //                alert('Error al cambiar campaña: ' + data.message);
    //            }
    //        },
    //        error: function (xhr, status, error) {
    //            console.error('Error:', error);
    //            alert('Error al cambiar campaña');
    //        }
    //    });
    //});
});