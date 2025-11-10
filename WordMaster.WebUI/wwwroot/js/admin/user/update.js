// User Update Functionality

function openUpdateModal(id, event) {
    event.preventDefault();

    // Kullanıcıyı getir
    $.ajax({
        url: '/Admin/User/GetUser',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                $('#updateUserId').val(response.user.id);
                $('#updateUserName').val(response.user.userName);
                $('#updateEmail').val(response.user.email);
                $('#updatePhone').val(response.user.phone);
                $('#updateCity').val(response.user.city);
                $('#updateBirthDate').val(response.user.birthDate);
                $('#updateGender').val(response.user.gender);

                // Modal'ı göster
                $('#updateModal').modal('show');
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "Kullanıcı bilgisi alınamadı.", "error");
        }
    });
}

function updateUser() {
    var userDto = {
        Id: $('#updateUserId').val(),
        UserName: $('#updateUserName').val(),
        Email: $('#updateEmail').val(),
        Phone: $('#updatePhone').val() || null,
        City: $('#updateCity').val() || null,
        BirthDate: $('#updateBirthDate').val() || null,
        Gender: $('#updateGender').val() ? parseInt($('#updateGender').val()) : null
    };

    if (!userDto.UserName || !userDto.Email) {
        swal("Hata!", "Lütfen kullanıcı adı ve email alanlarını doldurun.", "error");
        return;
    }

    $.ajax({
        url: '/Admin/User/UpdateUser',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(userDto),
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

