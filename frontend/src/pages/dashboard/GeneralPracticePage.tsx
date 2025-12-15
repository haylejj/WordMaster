import { useState, useEffect, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { PracticeCard } from "@/components/PracticeCard";
import { wordService } from "@/services/word.service";
import type { PracticeWordResponse } from "@/types/word";
import { X } from "lucide-react";

interface PracticeResult {
    word: PracticeWordResponse;
    isCorrect: boolean;
    userAnswer?: string;
}

export default function GeneralPracticePage() {
    const { type } = useParams<{ type: "all" | "favorites" | "unknowns" }>();
    const navigate = useNavigate();
    const [currentWord, setCurrentWord] = useState<PracticeWordResponse | null>(null);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [results, setResults] = useState<PracticeResult[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [showResults, setShowResults] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const resultsRef = useRef(results);

    useEffect(() => {
        resultsRef.current = results;
    }, [results]);

    const fetchNextWord = async () => {
        setIsLoading(true);
        setError(null);
        try {
            let result;
            const excludeId = currentWord?.id;
            if (type === "favorites") {
                result = await wordService.getPracticeRandomFavorite(excludeId);
            } else if (type === "unknowns") {
                result = await wordService.getPracticeRandomUnknown(excludeId);
            } else {
                result = await wordService.getPracticeRandomWord(excludeId);
            }

            if (result.isSuccess && result.data) {
                setCurrentWord(result.data);
            } else {
                setError(result.errorList?.[0] || "Pratik yapılacak kelime bulunamadı.");
            }
        } catch (err) {
            console.error("Failed to fetch word", err);
            setError("Pratik yapılacak kelime bulunamadı.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        // Reset state when type changes
        setResults([]);
        setCurrentIndex(0);
        setShowResults(false);
        fetchNextWord();
    }, [type]);

    const handleCheck = async (answer: string) => {
        if (!currentWord) return false;

        // Local check
        const normalizedAnswer = answer.replace(/\s+/g, ' ').trim().toLocaleLowerCase("tr-TR");
        const normalizedCorrect = currentWord.turkishWord.replace(/\s+/g, ' ').trim().toLocaleLowerCase("tr-TR");
        const isCorrect = normalizedAnswer === normalizedCorrect;

        setResults(prev => [...prev, { word: currentWord, isCorrect, userAnswer: answer }]);
        return isCorrect;
    };

    const handleNext = async () => {
        setCurrentIndex(prev => prev + 1);
        fetchNextWord();
    };

    const handleFinish = async () => {
        await submitResults();
        setShowResults(true);
    };

    const submitResults = async () => {
        if (resultsRef.current.length === 0) return;

        const payload = resultsRef.current.map(r => ({
            wordId: r.word.id,
            isCorrect: r.isCorrect
        }));

        try {
            await wordService.bulkUpdateStats(payload);
        } catch (err) {
            console.error("Failed to submit results", err);
        }
    };

    if (isLoading && !currentWord) {
        return <div className="text-center py-12">Yükleniyor...</div>;
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center py-20 text-center">
                <div className="bg-yellow-50 p-6 rounded-full mb-6">
                    <span className="text-4xl">🎉</span>
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-2">Harika Gidiyorsun!</h2>
                <p className="text-gray-500 max-w-md mb-8">
                    {type === "favorites"
                        ? "Favori listenizde pratik yapılacak kelime bulunmuyor."
                        : type === "unknowns"
                            ? "Bilinmeyen kelimeler listeniz tertemiz! Tüm kelimeleri öğrenmişsiniz."
                            : "Pratik yapılacak kelime bulunamadı. Yeni kelimeler ekleyerek başlayabilirsiniz."}
                </p>
                <div className="flex gap-4">
                    <button
                        onClick={() => navigate("/dashboard")}
                        className="px-6 py-3 bg-gray-100 text-gray-700 rounded-xl font-bold hover:bg-gray-200 transition-colors"
                    >
                        Ana Sayfaya Dön
                    </button>
                    {type !== "all" && (
                        <button
                            onClick={() => navigate("/dashboard/practice/all")}
                            className="px-6 py-3 bg-primary-yellow text-primary-dark rounded-xl font-bold hover:bg-[#FFC107] transition-colors"
                        >
                            Genel Pratik Yap
                        </button>
                    )}
                </div>
            </div>
        );
    }

    if (showResults) {
        const correctCount = results.filter(r => r.isCorrect).length;
        const wrongCount = results.length - correctCount;

        return (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl overflow-hidden">
                    <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
                        <h2 className="text-lg font-bold uppercase">PRATİK SONUCU</h2>
                        <button onClick={() => navigate("/dashboard")} className="text-gray-400 hover:text-white">
                            <X size={24} />
                        </button>
                    </div>

                    <div className="p-6">
                        <div className="flex space-x-4 mb-6">
                            <div className="bg-green-600 text-white px-4 py-2 rounded font-bold">
                                Doğru: {correctCount}
                            </div>
                            <div className="bg-red-600 text-white px-4 py-2 rounded font-bold">
                                Yanlış: {wrongCount}
                            </div>
                        </div>

                        <div className="grid grid-cols-2 gap-6">
                            <div>
                                <h3 className="font-bold text-gray-700 mb-2">Doğru Yanıtlananlar</h3>
                                <div className="border rounded-lg p-2 h-64 overflow-y-auto bg-gray-50">
                                    {results.filter(r => r.isCorrect).length === 0 ? (
                                        <p className="text-gray-400 text-sm p-2">Kayıt yok.</p>
                                    ) : (
                                        results.filter(r => r.isCorrect).map((r, idx) => (
                                            <div key={idx} className="p-2 border-b last:border-0">
                                                <div className="font-bold">{r.word.englishWord}</div>
                                                <div className="text-sm text-gray-500">Türkçe: {r.word.turkishWord}</div>
                                            </div>
                                        ))
                                    )}
                                </div>
                            </div>

                            <div>
                                <h3 className="font-bold text-gray-700 mb-2">Yanlış Yanıtlananlar</h3>
                                <div className="border rounded-lg p-2 h-64 overflow-y-auto bg-gray-50">
                                    {results.filter(r => !r.isCorrect).length === 0 ? (
                                        <p className="text-gray-400 text-sm p-2">Kayıt yok.</p>
                                    ) : (
                                        results.filter(r => !r.isCorrect).map((r, idx) => (
                                            <div key={idx} className="p-2 border-b last:border-0">
                                                <div className="font-bold">{r.word.englishWord}</div>
                                                <div className="text-sm text-gray-500">Türkçe: {r.word.turkishWord}</div>
                                                <div className="text-xs text-red-500 mt-1">
                                                    {r.userAnswer
                                                        ? `Yanıtınız: ${r.userAnswer}`
                                                        : "Bilmiyorum seçildi."}
                                                </div>
                                            </div>
                                        ))
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="bg-gray-50 px-6 py-4 flex justify-end space-x-3 border-t">
                        <button
                            onClick={() => navigate("/dashboard")}
                            className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700 transition-colors"
                        >
                            Çıkış
                        </button>
                        <button
                            onClick={() => {
                                setResults([]);
                                setCurrentIndex(0);
                                setShowResults(false);
                                fetchNextWord();
                            }}
                            className="px-4 py-2 border border-blue-500 text-blue-500 rounded hover:bg-blue-50 transition-colors"
                        >
                            Yeni Pratik Başlat
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    const getTitle = () => {
        switch (type) {
            case "favorites": return "FAVORİLERLE PRATİK";
            case "unknowns": return "BİLİNMEYENLERLE PRATİK";
            default: return "GENEL PRATİK";
        }
    };

    return (
        <div className="max-w-4xl mx-auto">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-2xl font-bold text-gray-900 uppercase">{getTitle()}</h1>
                <button
                    onClick={handleFinish}
                    className="px-4 py-2 bg-red-600 text-white rounded-md font-bold hover:bg-red-700 transition-colors"
                >
                    Pratiği Bitir
                </button>
            </div>
            {currentWord && (
                <PracticeCard
                    key={`${currentWord.id}-${currentIndex}`}
                    englishWord={currentWord.englishWord}
                    turkishWord={currentWord.turkishWord}
                    wordId={currentWord.id}
                    onCheck={handleCheck}
                    onNext={handleNext}
                    hideCounter={true}
                    isFolderPractice={false}
                />
            )}
        </div>
    );
}
