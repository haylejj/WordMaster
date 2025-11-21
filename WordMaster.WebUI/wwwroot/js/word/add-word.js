$(document).ready(function() {
    const fileInput = document.getElementById('csvFile');
    const fileNameDisplay = document.getElementById('fileName');
    const fileErrorDisplay = document.getElementById('fileError');
    const importBtn = document.getElementById('btnImport');
    const dropZone = document.getElementById('dropZone');
    const maxFileSize = 10 * 1024 * 1024; // 10MB

    function validateFile(file) {
        fileErrorDisplay.textContent = '';
        fileNameDisplay.textContent = '';
        importBtn.disabled = true;

        if (!file) return;

        if (!file.name.toLowerCase().endsWith('.csv')) {
            fileErrorDisplay.textContent = 'Lütfen geçerli bir .csv dosyası seçin.';
            return;
        }

        if (file.size > maxFileSize) {
            fileErrorDisplay.textContent = 'Dosya boyutu 10MB\'dan büyük olamaz.';
            return;
        }

        fileNameDisplay.textContent = file.name + ' (' + (file.size / 1024).toFixed(2) + ' KB)';
        importBtn.disabled = false;
    }

    if (fileInput) {
        fileInput.addEventListener('change', function(e) {
            if (this.files.length > 0) {
                validateFile(this.files[0]);
            }
        });
    }

    if (dropZone) {
        // Drag and drop functionality
        dropZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            dropZone.classList.add('border-primary');
            dropZone.classList.add('bg-white');
        });

        dropZone.addEventListener('dragleave', (e) => {
            e.preventDefault();
            dropZone.classList.remove('border-primary');
            dropZone.classList.remove('bg-white');
        });

        dropZone.addEventListener('drop', (e) => {
            e.preventDefault();
            dropZone.classList.remove('border-primary');
            dropZone.classList.remove('bg-white');
            
            if (e.dataTransfer.files.length > 0) {
                fileInput.files = e.dataTransfer.files;
                validateFile(e.dataTransfer.files[0]);
            }
        });
    }

    if (importBtn) {
        importBtn.addEventListener('click', function() {
            const file = fileInput.files[0];
            if (!file) return;

            const formData = new FormData();
            formData.append('file', file);

            // Disable button and show loading state
            const originalBtnText = importBtn.innerHTML;
            importBtn.disabled = true;
            importBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Yükleniyor...';

            $.ajax({
                url: '/Word/ImportFromExcel',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function(response) {
                    if (response.success) {
                        swal({
                            title: "Başarılı!",
                            text: response.message,
                            icon: "success",
                            button: "Tamam",
                        }).then(() => {
                            // Close modal and refresh page or clear form
                            $('#importModal').modal('hide');
                            location.reload();
                        });
                    } else {
                        swal({
                            title: "Hata!",
                            text: response.message,
                            icon: "error",
                            button: "Tamam",
                        });
                        importBtn.disabled = false;
                        importBtn.innerHTML = originalBtnText;
                    }
                },
                error: function() {
                    swal({
                        title: "Hata!",
                        text: "Sunucu ile iletişim hatası oluştu.",
                        icon: "error",
                        button: "Tamam",
                    });
                    importBtn.disabled = false;
                    importBtn.innerHTML = originalBtnText;
                }
            });
        });
    }
});
