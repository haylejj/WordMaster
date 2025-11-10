// User Roles Update Functionality

function openUpdateRolesModal(id, event) {
    event.preventDefault();

    // Kullanıcı ID'yi set et
    $('#updateRolesUserId').val(id);

    // Rolleri getir
    $.ajax({
        url: '/Admin/User/GetUserRoles',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                // Rolleri container'a yükle
                var rolesContainer = $('#rolesContainer');
                rolesContainer.empty();

                if (response.roles && response.roles.length > 0) {
                    response.roles.forEach(function (role) {
                        var roleCheckbox = $('<div class="form-check mb-2">')
                            .append($('<input>')
                                .attr({
                                    'type': 'checkbox',
                                    'class': 'form-check-input role-checkbox',
                                    'id': 'role_' + role.id,
                                    'data-role-id': role.id,
                                    'data-role-name': role.name,
                                    'checked': role.exist
                                }))
                            .append($('<label>')
                                .attr({
                                    'class': 'form-check-label fw-bold',
                                    'for': 'role_' + role.id,
                                    'style': 'font-weight: bold !important;'
                                })
                                .text(role.name));

                        rolesContainer.append(roleCheckbox);
                    });
                } else {
                    rolesContainer.append('<p class="text-muted">Rol bulunamadı.</p>');
                }

                // Modal'ı göster
                $('#updateRolesModal').modal('show');
            } else {
                swal("Hata!", response.message || "Roller alınamadı.", "error");
            }
        },
        error: function () {
            swal("Hata!", "Roller alınırken bir hata oluştu.", "error");
        }
    });
}

function updateUserRoles() {
    var userId = $('#updateRolesUserId').val();

    if (!userId) {
        swal("Hata!", "Kullanıcı ID bulunamadı.", "error");
        return;
    }

    // Seçili rolleri topla
    var roles = [];
    $('.role-checkbox').each(function () {
        var checkbox = $(this);
        roles.push({
            id: checkbox.data('role-id'),
            name: checkbox.data('role-name'),
            exist: checkbox.is(':checked')
        });
    });

    if (roles.length === 0) {
        swal("Hata!", "En az bir rol seçmelisiniz.", "error");
        return;
    }

    var requestData = {
        userId: userId,
        roles: roles
    };

    $.ajax({
        url: '/Admin/User/UpdateUserRoles',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function (response) {
            if (response.success) {
                $('#updateRolesModal').modal('hide');
                swal("Başarılı!", response.message, "success")
                    .then(() => {
                        location.reload();
                    });
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function (xhr) {
            var errorMessage = "Bir hata oluştu. Lütfen tekrar deneyin.";
            if (xhr.responseJSON && xhr.responseJSON.message) {
                errorMessage = xhr.responseJSON.message;
            }
            swal("Hata!", errorMessage, "error");
        }
    });
}

