$(document).ready(function () {

    $('.campaña-item').on('click', function (e) {
        e.preventDefault();

        var campañaId = $(this).data('campaña-id');

        $.ajax({
            url: '/Campania/SetCurrent/' + campañaId,
            type: 'POST',
            contentType: 'application/json',
            success: function (data) {
                if (data.success) {
                    $('#campañaActualText').text(
                        $(e.currentTarget).find('.fw-semibold').text()
                    );
                    if (data.message != "")
                        location.reload();
                } else {
                    alert('Error al cambiar campaña: ' + data.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                alert('Error al cambiar campaña');
            }
        });
    });

});