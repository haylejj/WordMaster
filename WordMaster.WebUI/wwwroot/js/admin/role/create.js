// Role Create Functionality

function openCreateModal() {
    // Form'u temizle
    $('#createRoleForm')[0].reset();
    
    // Modal'ı göster
    $('#createModal').modal('show');
}

function createRole() {
    var roleDto = {
        Name: $('#createRoleName').val()
    };

    if (!roleDto.Name) {
        swal("Hata!", "Lütfen rol adı alanını doldurun.", "error");
        return;
    }

    $.ajax({
        url: '/Admin/Role/CreateRole',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(roleDto),
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

