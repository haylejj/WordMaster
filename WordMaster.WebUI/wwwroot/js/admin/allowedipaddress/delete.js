// Allowed IP Address Delete Functionality

function deleteAllowedIpAddress(id, event) {
    event.preventDefault();

    swal({
        title: "Emin misiniz?",
        text: "Bu IP adresini silmek istediğinizden emin misiniz?",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: '/Admin/AllowedIpAddress/DeleteAllowedIpAddress',
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (response.success) {
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
        });
}

