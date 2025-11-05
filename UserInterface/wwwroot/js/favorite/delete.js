// Favorite Delete Functionality

function deleteFavorite(id, event) {
    event.preventDefault();

    swal({
        title: "Emin misiniz?",
        text: "Bu favoriyi silmek istediğinizden emin misiniz?",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                $.ajax({
                    url: '/Favorite/DeleteFavorite',
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

