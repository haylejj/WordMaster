function openUpdateModal(id, e) {
    e.stopPropagation();
    $.get('/Word/GetWord', { id: id })
        .done(function (res) {
            if (!res.success) {
                swal('Hata', res.message || 'Kelime getirilemedi.', 'error');
                return;
            }
            $('#updateWordId').val(res.word.id);
            $('#updateEnglishWord').val(res.word.englishWord);
            $('#updateTurkishWord').val(res.word.turkishWord);
            var modal = new bootstrap.Modal(document.getElementById('updateModal'));
            modal.show();
        })
        .fail(function () {
            swal('Hata', 'Sunucu hatası.', 'error');
        });
}

function updateWord() {
    var data = {
        Id: $('#updateWordId').val(),
        EnglishWord: $('#updateEnglishWord').val(),
        TurkishWord: $('#updateTurkishWord').val()
    };
    $.post('/Word/UpdateWord', data)
        .done(function (res) {
            if (!res.success) {
                swal('Hata', res.message || 'Güncelleme başarısız.', 'error');
                return;
            }
            swal('Başarılı', 'Kelime güncellendi.', 'success')
                .then(() => window.location.reload());
        })
        .fail(function () {
            swal('Hata', 'Sunucu hatası.', 'error');
        });
}
