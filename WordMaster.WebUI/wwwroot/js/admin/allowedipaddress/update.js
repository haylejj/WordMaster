// Allowed IP Address Update Functionality

function openUpdateModal(id, event) {
    event.preventDefault();

    // IP adresini getir
    $.ajax({
        url: '/Admin/AllowedIpAddress/GetAllowedIpAddress',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                $('#updateAllowedIpAddressId').val(response.allowedIpAddress.id);
                $('#updateIpAddress').val(response.allowedIpAddress.ipAddress);
                $('#updateDescription').val(response.allowedIpAddress.description || '');
                $('#updateIsActive').prop('checked', response.allowedIpAddress.isActive);

                // Modal'ı göster
                $('#updateModal').modal('show');
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "IP adresi bilgisi alınamadı.", "error");
        }
    });
}

function updateAllowedIpAddress() {
    var allowedIpAddressDto = {
        Id: parseInt($('#updateAllowedIpAddressId').val()),
        IpAddress: $('#updateIpAddress').val(),
        Description: $('#updateDescription').val(),
        IsActive: $('#updateIsActive').is(':checked')
    };

    if (!allowedIpAddressDto.IpAddress) {
        swal("Hata!", "Lütfen IP adresi alanını doldurun.", "error");
        return;
    }

    $.ajax({
        url: '/Admin/AllowedIpAddress/UpdateAllowedIpAddress',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(allowedIpAddressDto),
        success: function (response) {
            if (response.success) {
                $('#updateModal').modal('hide');
                swal("Başarılı!", response.message, "success")
                    .then(() => {
                        location.reload();
                    });
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "Bir hata oluştu. Lütfen tekrar deneyin.", "error");
        }
    });
}

