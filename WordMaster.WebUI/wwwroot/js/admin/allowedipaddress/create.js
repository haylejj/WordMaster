// Allowed IP Address Create Functionality

function openCreateModal() {
    // Form'u temizle
    $('#createAllowedIpAddressForm')[0].reset();
    $('#createIsActive').prop('checked', true);
    
    // Modal'ı göster
    $('#createModal').modal('show');
}

function createAllowedIpAddress() {
    var allowedIpAddressDto = {
        IpAddress: $('#createIpAddress').val(),
        Description: $('#createDescription').val(),
        IsActive: $('#createIsActive').is(':checked')
    };

    if (!allowedIpAddressDto.IpAddress) {
        swal("Hata!", "Lütfen IP adresi alanını doldurun.", "error");
        return;
    }

    $.ajax({
        url: '/Admin/AllowedIpAddress/CreateAllowedIpAddress',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(allowedIpAddressDto),
        success: function (response) {
            if (response.success) {
                $('#createModal').modal('hide');
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

