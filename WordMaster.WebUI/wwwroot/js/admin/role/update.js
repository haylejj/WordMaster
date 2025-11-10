// Role Update Functionality

function openUpdateModal(id, event) {
    event.preventDefault();

    // Rolü getir
    $.ajax({
        url: '/Admin/Role/GetRole',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                $('#updateRoleId').val(response.role.id);
                $('#updateRoleName').val(response.role.name);

                // Modal'ı göster
                $('#updateModal').modal('show');
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "Rol bilgisi alınamadı.", "error");
        }
    });
}

function updateRole() {
    var roleDto = {
        Id: $('#updateRoleId').val(),
        Name: $('#updateRoleName').val()
    };

    if (!roleDto.Name) {
        swal("Hata!", "Lütfen rol adı alanını doldurun.", "error");
        return;
    }

    $.ajax({
        url: '/Admin/Role/UpdateRole',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(roleDto),
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

