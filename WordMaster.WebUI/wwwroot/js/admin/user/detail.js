// User Detail Functionality

function openDetailModal(id, event) {
    event.preventDefault();

    // Kullanıcı detaylarını getir
    $.ajax({
        url: '/Admin/User/GetUserDetail',
        type: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                var detail = response.detail;

                // Kişisel Bilgiler
                $('#detailId').text(detail.id);
                $('#detailUserName').text(detail.userName);
                $('#detailEmail').text(detail.email);
                $('#detailPhone').text(detail.phone);
                $('#detailBirthDate').text(detail.birthDate);
                $('#detailCity').text(detail.city);
                $('#detailGender').text(detail.gender);

                // Giriş İstatistikleri
                $('#detailTotalLoginAttempts').text(detail.totalLoginAttempts);
                $('#detailSuccessfulLogins').text(detail.successfulLogins);
                $('#detailFailedLogins').text(detail.failedLogins);
                $('#detailLastLoginDate').text(detail.lastLoginDate);

                // Kelime İstatistikleri
                $('#detailWordCount').text(detail.wordCount);
                $('#detailFavoriteCount').text(detail.favoriteCount);
                $('#detailUnknowsCount').text(detail.unknowsCount);
                $('#detailLastPracticeDate').text(detail.lastPracticeDate);

                // Modal'ı göster
                $('#detailModal').modal('show');
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "Kullanıcı detayları alınamadı.", "error");
        }
    });
}

