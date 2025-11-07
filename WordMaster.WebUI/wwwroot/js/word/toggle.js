// Word Toggle Functionality (Favorite and Unknows checkboxes)

// Favorite checkbox toggle
$(document).on('change', '.favorite-checkbox', function () {
    var wordId = $(this).data('word-id');
    var isChecked = $(this).is(':checked');
    var checkbox = $(this);

    $.ajax({
        url: '/Favorite/ToggleFavorite',
        type: 'POST',
        data: { id: wordId },
        success: function (response) {
            if (response.success) {
                checkbox.prop('checked', response.isFavorite);
            } else {
                checkbox.prop('checked', !isChecked);
                alert(response.message || 'Bir hata oluştu.');
            }
        },
        error: function () {
            checkbox.prop('checked', !isChecked);
            alert('Bir hata oluştu. Lütfen tekrar deneyin.');
        }
    });
});

// Unknows checkbox toggle
$(document).on('change', '.unknows-checkbox', function () {
    var wordId = $(this).data('word-id');
    var isChecked = $(this).is(':checked');
    var checkbox = $(this);

    $.ajax({
        url: '/Unknows/ToggleUnknows',
        type: 'POST',
        data: { id: wordId },
        success: function (response) {
            if (response.success) {
                checkbox.prop('checked', response.isUnknows);
            } else {
                checkbox.prop('checked', !isChecked);
                alert(response.message || 'Bir hata oluştu.');
            }
        },
        error: function () {
            checkbox.prop('checked', !isChecked);
            alert('Bir hata oluştu. Lütfen tekrar deneyin.');
        }
    });
});

