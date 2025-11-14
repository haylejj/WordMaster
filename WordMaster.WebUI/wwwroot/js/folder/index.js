// Folder Update and Delete Functionality

function openUpdateModal(id, event) {
    event.preventDefault();
    event.stopPropagation();

    // Klasörü getir
    $.ajax({
        url: '/Folder/GetFolder',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                $('#updateFolderId').val(response.folder.id);
                $('#updateFolderName').val(response.folder.name);
                $('#updateModal').modal('show');
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "Klasör bilgisi alınamadı.", "error");
        }
    });
}

function updateFolder() {
    var folderId = parseInt($('#updateFolderId').val());
    var folderName = $('#updateFolderName').val().trim();

    if (!folderName) {
        swal("Hata!", "Lütfen klasör adını girin.", "error");
        return;
    }

    const token = $('input[name="__RequestVerificationToken"]').first().val();

    $.ajax({
        url: '/Folder/Update',
        type: 'POST',
        data: {
            id: folderId,
            name: folderName,
            __RequestVerificationToken: token
        },
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

function deleteFolder(id, event) {
    event.preventDefault();
    event.stopPropagation();

    swal({
        title: "Emin misiniz?",
        text: "Bu klasörü silmek istediğinizden emin misiniz?",
        icon: "warning",
        buttons: true,
        dangerMode: true,
    })
        .then((willDelete) => {
            if (willDelete) {
                const token = $('input[name="__RequestVerificationToken"]').first().val();
                $.ajax({
                    url: '/Folder/Delete',
                    type: 'POST',
                    data: {
                        id: id,
                        __RequestVerificationToken: token
                    },
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

