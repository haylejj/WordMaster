function ready(callback) {
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', callback);
    } else {
        callback();
    }
}

ready(() => {
    const root = document.querySelector('[data-practice-root]');
    if (!root) {
        return;
    }

    const getWordUrl = root.getAttribute('data-get-word-url');
    const checkUrl = root.getAttribute('data-check-url');

    const englishWordDisplay = document.getElementById('practiceEnglishWord');
    const englishWordValue = document.getElementById('practiceEnglishWordValue');
    const turkishInput = document.getElementById('practiceTurkishInput');
    const checkButton = document.getElementById('practiceCheckButton');
    const dontKnowButton = document.getElementById('practiceDontKnowButton');
    const statusEl = document.getElementById('practiceStatus');
    const statusTitleEl = document.getElementById('practiceStatusTitle');
    const statusMessageEl = document.getElementById('practiceStatusMessage');

    if (!getWordUrl || !checkUrl || !englishWordDisplay || !englishWordValue || !turkishInput || !checkButton || !dontKnowButton || !statusEl || !statusTitleEl || !statusMessageEl) {
        return;
    }

    const translationCache = new Map();
    let nextWordTimeoutId = null;
    let isProcessing = false;

    const safeTrim = (value) => (value || '').trim();

    function getCurrentEnglishWord() {
        return safeTrim(englishWordValue.value);
    }

    function setEnglishWord(word) {
        const value = word || '';
        englishWordDisplay.textContent = value || '...';
        englishWordValue.value = value;
        turkishInput.value = '';
    }

    function focusInput() {
        if (!turkishInput.disabled) {
            turkishInput.focus();
        }
    }

    function setControlsDisabled(disabled) {
        checkButton.disabled = disabled;
        dontKnowButton.disabled = disabled;
        turkishInput.disabled = disabled;
    }

    function hideStatus() {
        statusEl.classList.add('d-none');
        statusEl.classList.remove('alert-success', 'alert-danger', 'alert-warning', 'alert-info');
        statusTitleEl.textContent = '';
        statusMessageEl.textContent = '';
    }

    function showStatus(type, _title, message) {
        hideStatus();
        statusEl.classList.remove('d-none');
        statusEl.classList.add(`alert-${type}`);
        statusTitleEl.textContent = '';
        statusMessageEl.textContent = message || '';
    }

    function clearScheduledAdvance() {
        if (nextWordTimeoutId) {
            window.clearTimeout(nextWordTimeoutId);
            nextWordTimeoutId = null;
        }
    }

    async function fetchNewWord() {
        setControlsDisabled(true);
        try {
            const response = await fetch(getWordUrl, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (response.status === 401) {
                window.location.href = '/Login/LogIn';
                return false;
            }
            if (!response.ok) {
                throw new Error('Yeni kelime alınamadı.');
            }
            const text = safeTrim(await response.text());
            if (!text) {
                setEnglishWord('');
                showStatus('info', 'Kelime bulunamadı.', 'Lütfen sözlüğünüze kelime ekleyin.');
                return false;
            }
            setEnglishWord(text);
            hideStatus();
            preloadTranslation(text);
            return true;
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Bir hata oluştu.';
            showStatus('danger', 'Hata', message);
            return false;
        } finally {
            const hasWord = Boolean(getCurrentEnglishWord());
            setControlsDisabled(!hasWord);
            if (hasWord) {
                focusInput();
            }
        }
    }

    function scheduleNextWord(delay) {
        clearScheduledAdvance();
        nextWordTimeoutId = window.setTimeout(async () => {
            await fetchNewWord();
            isProcessing = false;
        }, delay);
    }

    async function checkTranslationRequest(turkishWord, englishWord) {
        const params = new URLSearchParams({
            turkishWord: turkishWord || '',
            englishWord: englishWord || ''
        });
        const response = await fetch(`${checkUrl}?${params.toString()}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (response.status === 401) {
            window.location.href = '/Login/LogIn';
            throw new Error('Oturum bulunamadı.');
        }
        if (!response.ok) {
            throw new Error('Cevap kontrol edilirken bir hata oluştu.');
        }
        const data = await response.json().catch(() => null);
        if (!data || typeof data.isCorrect === 'undefined') {
            throw new Error('Sunucudan geçersiz yanıt alındı.');
        }
        return data;
    }

    function preloadTranslation(word) {
        if (!word) {
            return;
        }
        void getTranslation(word);
    }

    function getTranslation(word) {
        if (!word) {
            return Promise.resolve('');
        }
        const key = word.toLowerCase();
        const cached = translationCache.get(key);
        if (typeof cached === 'string') {
            return Promise.resolve(cached);
        }
        if (cached && typeof cached.then === 'function') {
            return cached;
        }
        const promise = fetchMeaningFromWordPage(word)
            .then(result => {
                translationCache.set(key, result || '');
                return result || '';
            })
            .catch(() => {
                translationCache.set(key, '');
                return '';
            });
        translationCache.set(key, promise);
        return promise;
    }

    async function fetchMeaningFromWordPage(word) {
        const params = new URLSearchParams({
            search: word,
            page: '1',
            pageSize: '1'
        });
        const response = await fetch(`/Word?${params.toString()}`, {
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (response.status === 401) {
            window.location.href = '/Login/LogIn';
            return '';
        }
        if (!response.ok) {
            throw new Error('Türkçe anlam alınamadı.');
        }
        const html = await response.text();
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const rows = doc.querySelectorAll('table tbody tr');
        let fallback = '';

        for (const row of rows) {
            const cells = row.querySelectorAll('td');
            if (cells.length < 2) {
                continue;
            }
            const englishCell = safeTrim(cells[0].textContent || '');
            const turkishCell = safeTrim(cells[1].textContent || '');
            if (!fallback && turkishCell) {
                fallback = turkishCell;
            }
            if (englishCell.toLowerCase() === word.toLowerCase()) {
                return turkishCell;
            }
        }

        return fallback;
    }

    async function revealCorrectAnswer(alertType, title, englishWord) {
        const meaning = await getTranslation(englishWord);
        const message = meaning ? `Doğru cevap: ${meaning}` : 'Doğru cevap bulunamadı.';
        showStatus(alertType, title, message);
    }

    async function handleCheck() {
        if (isProcessing) {
            return;
        }
        clearScheduledAdvance();

        const englishWord = getCurrentEnglishWord();
        if (!englishWord) {
            showStatus('info', 'Kelime yok.', 'Yeni kelime alınamadı.');
            return;
        }

        const userAnswer = safeTrim(turkishInput.value);
        if (!userAnswer) {
            showStatus('info', 'Bilgi', 'Lütfen cevabınızı girin.');
            turkishInput.focus();
            return;
        }

        isProcessing = true;
        setControlsDisabled(true);
        hideStatus();

        try {
            const response = await checkTranslationRequest(userAnswer, englishWord);
            if (response.isCorrect) {
                showStatus('success', 'Tebrikler!', 'Cevabınız doğru.');
                scheduleNextWord(1200);
                return;
            }

            await revealCorrectAnswer('danger', 'Yanlış cevap', englishWord);
            scheduleNextWord(2600);
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Bir hata oluştu.';
            showStatus('danger', 'Hata', message);
            setControlsDisabled(false);
            isProcessing = false;
        }
    }

    async function handleDontKnow() {
        if (isProcessing) {
            return;
        }

        const englishWord = getCurrentEnglishWord();
        if (!englishWord) {
            return;
        }

        clearScheduledAdvance();
        isProcessing = true;
        setControlsDisabled(true);
        hideStatus();

        try {
            await checkTranslationRequest('', englishWord);
        } catch (error) {
            console.warn('Bilmiyorum isteği tamamlanamadı.', error);
        }

        await revealCorrectAnswer('warning', 'Bilmiyorum seçildi', englishWord);
        scheduleNextWord(3500);
    }

    checkButton.addEventListener('click', handleCheck);
    dontKnowButton.addEventListener('click', handleDontKnow);

    turkishInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            event.preventDefault();
            handleCheck();
        }
    });

    const initialWord = getCurrentEnglishWord();
    setEnglishWord(initialWord);

    if (initialWord) {
        setControlsDisabled(false);
        preloadTranslation(initialWord);
        focusInput();
    } else {
        setControlsDisabled(true);
        void fetchNewWord();
    }
});
