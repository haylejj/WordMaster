(function () {
    const practiceData = window.practiceFolderData;
    if (!practiceData || !Array.isArray(practiceData.words) || practiceData.words.length === 0) {
        return;
    }

    const words = practiceData.words;
    let currentIndex = 0;
    let isProcessing = false;
    let pendingTimeoutId = null;
    const results = [];

    const englishWordEl = document.getElementById('practiceEnglishWord');
    const turkishInputEl = document.getElementById('practiceTurkishInput');
    const checkButtonEl = document.getElementById('practiceCheckButton');
    const dontKnowButtonEl = document.getElementById('practiceDontKnowButton');
    const progressEl = document.getElementById('practiceProgress');
    const statusEl = document.getElementById('practiceStatus');
    const statusTitleEl = document.getElementById('practiceStatusTitle');
    const statusMessageEl = document.getElementById('practiceStatusMessage');

    function setControlsDisabled(disabled) {
        checkButtonEl.disabled = disabled;
        dontKnowButtonEl.disabled = disabled;
        turkishInputEl.disabled = disabled;
    }

    function hideStatus() {
        if (!statusEl) {
            return;
        }
        statusEl.classList.add('d-none');
        statusEl.classList.remove('alert-success', 'alert-danger', 'alert-warning', 'alert-info');
        if (statusTitleEl) {
            statusTitleEl.textContent = '';
        }
        if (statusMessageEl) {
            statusMessageEl.textContent = '';
        }
    }

    function showStatus(type, title, message) {
        if (!statusEl) {
            return;
        }
        hideStatus();
        statusEl.classList.remove('d-none');
        statusEl.classList.add(`alert-${type}`);
        if (statusTitleEl) {
            statusTitleEl.textContent = title || '';
        }
        if (statusMessageEl) {
            statusMessageEl.textContent = message || '';
        }
    }

    function updateProgress() {
        if (progressEl) {
            progressEl.textContent = `${currentIndex + 1} / ${words.length}`;
        }
    }

    function renderCurrentWord() {
        const currentWord = words[currentIndex];
        englishWordEl.textContent = currentWord.englishWord || '';
        turkishInputEl.value = '';
        hideStatus();
        updateProgress();
        setControlsDisabled(false);
        isProcessing = false;
        turkishInputEl.focus();
    }

    async function sendCheckRequest(turkishWord, englishWord) {
        const response = await fetch('/PracticeFolder/CheckTranslation', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                turkishWord: turkishWord,
                englishWord: englishWord
            })
        });

        if (response.status === 401) {
            window.location.href = '/Login/LogIn';
            return { success: false, isCorrect: false };
        }

        let data = null;
        try {
            data = await response.json();
        } catch (error) {
            throw new Error('Sunucudan geçersiz yanıt alındı.');
        }

        if (!response.ok || !data || data.success !== true) {
            const message = (data && data.errorMessage) ? data.errorMessage : 'Kontrol işlemi başarısız.';
            throw new Error(message);
        }

        return data;
    }

    function scheduleNextWord(delay) {
        if (pendingTimeoutId) {
            clearTimeout(pendingTimeoutId);
        }

        pendingTimeoutId = window.setTimeout(() => {
            currentIndex += 1;
            if (currentIndex >= words.length) {
                finishPractice();
            } else {
                renderCurrentWord();
            }
        }, delay);
    }

    function finishPractice() {
        hideStatus();
        setControlsDisabled(true);

        const correctItems = results.filter(item => item.isCorrect);
        const incorrectItems = results.filter(item => !item.isCorrect);

        const correctCountEl = document.getElementById('summaryCorrectCount');
        const incorrectCountEl = document.getElementById('summaryIncorrectCount');
        if (correctCountEl) {
            correctCountEl.textContent = correctItems.length.toString();
        }
        if (incorrectCountEl) {
            incorrectCountEl.textContent = incorrectItems.length.toString();
        }

        populateResultList('summaryCorrectList', correctItems, true);
        populateResultList('summaryIncorrectList', incorrectItems, false);

        const modalElement = document.getElementById('practiceSummaryModal');
        if (modalElement && window.bootstrap && typeof window.bootstrap.Modal === 'function') {
            const modal = new window.bootstrap.Modal(modalElement);
            modal.show();
        }
    }

    function populateResultList(elementId, items, isCorrectList) {
        const listElement = document.getElementById(elementId);
        if (!listElement) {
            return;
        }

        listElement.innerHTML = '';

        if (items.length === 0) {
            const emptyItem = document.createElement('li');
            emptyItem.className = 'list-group-item text-muted';
            emptyItem.textContent = 'Kayıt yok.';
            listElement.appendChild(emptyItem);
            return;
        }

        items.forEach(item => {
            const li = document.createElement('li');
            li.className = 'list-group-item';

            const englishLine = document.createElement('div');
            englishLine.className = 'fw-semibold';
            englishLine.textContent = item.englishWord || '';
            li.appendChild(englishLine);

            const turkishLine = document.createElement('div');
            turkishLine.className = 'small text-muted';
            turkishLine.textContent = `Türkçe: ${item.turkishWord || '-'}`;
            li.appendChild(turkishLine);

            if (!isCorrectList) {
                const answerLine = document.createElement('div');
                answerLine.className = 'small';
                if (item.isSkipped) {
                    answerLine.textContent = 'Bilmiyorum seçildi.';
                } else {
                    const userAnswer = item.userAnswer && item.userAnswer.length > 0 ? item.userAnswer : '-';
                    answerLine.textContent = `Senin cevabın: ${userAnswer}`;
                }
                li.appendChild(answerLine);
            }

            listElement.appendChild(li);
        });
    }

    async function handleAnswer(markAsUnknown) {
        if (isProcessing) {
            return;
        }

        const currentWord = words[currentIndex];
        const userAnswer = markAsUnknown ? '' : (turkishInputEl.value || '').trim();

        if (!markAsUnknown && userAnswer.length === 0) {
            showStatus('info', 'Bilgi', 'Lütfen cevabınızı girin.');
            turkishInputEl.focus();
            return;
        }

        isProcessing = true;
        setControlsDisabled(true);
        hideStatus();

        try {
            const response = await sendCheckRequest(userAnswer, currentWord.englishWord);
            const isCorrect = markAsUnknown ? false : Boolean(response.isCorrect);

            if (isCorrect) {
                showStatus('success', 'Cevap doğru!', '');
            } else {
                if (markAsUnknown) {
                    showStatus('warning', 'Bilmiyorum seçildi.', `Doğru cevap: ${currentWord.turkishWord || '-'}`);
                } else {
                    showStatus('danger', 'Yanlış cevap!', `Doğru cevap: ${currentWord.turkishWord || '-'}`);
                }
            }

            results.push({
                wordId: currentWord.wordId,
                englishWord: currentWord.englishWord,
                turkishWord: currentWord.turkishWord,
                isCorrect: isCorrect,
                userAnswer: markAsUnknown ? null : userAnswer,
                isSkipped: markAsUnknown
            });

            const delay = isCorrect ? 1200 : 2600;
            scheduleNextWord(delay);
        } catch (error) {
            console.error('Practice check failed:', error);
            const message = error instanceof Error ? error.message : 'Bir hata oluştu.';
            showStatus('danger', 'Hata', message);
            setControlsDisabled(false);
            isProcessing = false;
        }
    }

    checkButtonEl.addEventListener('click', () => handleAnswer(false));
    dontKnowButtonEl.addEventListener('click', () => handleAnswer(true));

    turkishInputEl.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleAnswer(false);
        }
    });

    renderCurrentWord();
})();

