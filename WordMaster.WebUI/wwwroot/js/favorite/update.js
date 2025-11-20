// Favorite Update Functionality

function openUpdateModal(wordId, event) {
    event.preventDefault();

    $.ajax({
        url: '/Favorite/GetWord',
        type: 'GET',
        data: { favoriteId: wordId },
        success: function (response) {
            if (response.success) {
                $('#updateWordId').val(response.word.id);
                $('#updateEnglishWord').val(response.word.englishWord);
                $('#updateTurkishWord').val(response.word.turkishWord);
                $('#updateModal').modal('show');
            } else {
                swal("Hata!", response.message, "error");
            }
        },
        error: function () {
            swal("Hata!", "Kelime bilgisi alınamadı.", "error");
        }
    });
}

function updateWord() {
    var wordDto = {
        Id: parseInt($('#updateWordId').val()),
        EnglishWord: $('#updateEnglishWord').val(),
        TurkishWord: $('#updateTurkishWord').val()
    };

    if (!wordDto.EnglishWord || !wordDto.TurkishWord) {
        swal("Hata!", "Lütfen tüm alanları doldurun.", "error");
        return;
    }

    $.ajax({
        url: '/Favorite/UpdateWord',
        type: 'POST',
        data: wordDto,
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
