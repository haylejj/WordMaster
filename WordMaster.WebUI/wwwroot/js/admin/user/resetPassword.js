// User Password Reset Functionality

function resetPassword(id, event) {
    event.preventDefault();

    swal({
        title: "Emin misiniz?",
        text: "Bu kullanıcının şifresini sıfırlamak istediğinizden emin misiniz? Yeni şifre kullanıcıya email olarak gönderilecektir.",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willReset) => {
            if (willReset) {
                $.ajax({
                    url: '/Admin/User/ResetPassword',
                    type: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (response.success) {
                            swal("Başarılı!", response.message, "success");
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

