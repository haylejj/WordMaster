$(function () {
    const $select = $('#selectWord');
    // Fetch once, then initialize Select2 for local search
    $.get('/Folder/UserWords', function (data) {
        const items = (data && data.results) ? data.results : [];
        let optionsHtml = '<option value=\"\">Kelime seçin...</option>';
        for (let i = 0; i < items.length; i++) {
            optionsHtml += '<option value=\"' + items[i].id + '\">' + items[i].text + '</option>';
        }
        $select.html(optionsHtml);

        if ($.fn && $.fn.select2) {
            $select.select2({
                placeholder: 'Kelime seçin...',
                allowClear: true,
                width: 'resolve',
                minimumInputLength: 0,
                minimumResultsForSearch: 0
            });
        } else {
            console.warn('Select2 yüklenemedi; arama devre dışı.');
        }
    }, 'json');
});

function toggleFlip(cardEl) {
    // stop click bubbling if inner button clicked
    if (event && (event.target.closest('button') || event.target.closest('form'))) {
        return;
    }
    cardEl.classList.toggle('flipped');
}

function removeWordFromFolder(folderId, wordId, e) {
    e.stopPropagation();
    swal({
        title: 'Emin misiniz?',
        text: 'Bu kelime klasörden kaldırılacak.',
        icon: 'warning',
        buttons: ['İptal', 'Kaldır'],
        dangerMode: true
    }).then(function (willDelete) {
        if (!willDelete) return;
        const token = $('input[name="__RequestVerificationToken"]').first().val();
        $.ajax({
            url: '/Folder/RemoveWord',
            method: 'POST',
            data: { folderId: folderId, wordId: wordId, __RequestVerificationToken: token }
        }).done(function () {
            swal('Kaldırıldı', 'Kelime klasörden kaldırıldı.', 'success');
            // Remove the card smoothly
            const btn = e.target.closest('.btn');
            const cardCol = btn.closest('.col');
            $(cardCol).fadeOut(200, function () { $(this).remove(); });
        }).fail(function () {
            swal('Hata', 'İşlem gerçekleştirilemedi.', 'error');
        });
    });
}

