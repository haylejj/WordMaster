
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import TestCard from "@/components/TestCard";
import { wordService } from "@/services/word.service";
import type { PracticeWordResponse } from "@/types/word";
import { X, Loader2 } from "lucide-react";

interface TestResult {
    word: PracticeWordResponse;
    isCorrect: boolean;
    userAnswer: string;
}

export default function GeneralTestPage() {
    const { type } = useParams<{ type: "all" | "favorites" | "unknowns" }>();
    const navigate = useNavigate();

    // Game State
    const [currentWord, setCurrentWord] = useState<PracticeWordResponse | null>(null);
    const [options, setOptions] = useState<string[]>([]);
    const [results, setResults] = useState<TestResult[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [showResults, setShowResults] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Fetch a new question
    const fetchQuestion = async () => {
        setIsLoading(true);
        setError(null);
        try {
            const excludeId = currentWord?.id;
            let quizRes;

            if (type === "favorites") {
                quizRes = await wordService.getFavoriteQuiz(excludeId);
            } else if (type === "unknowns") {
                quizRes = await wordService.getUnknownQuiz(excludeId);
            } else {
                quizRes = await wordService.getQuiz(excludeId);
            }

            if (!quizRes.isSuccess || !quizRes.data) {
                if (type === "favorites" || type === "unknowns") {
                    setError("Listenizde yeterli kelime yok veya bir hata oluştu.");
                } else {
                    setError("Test sorusu alınamadı.");
                }
                setIsLoading(false);
                return;
            }

            const quizData = quizRes.data;
            setCurrentWord(quizData.question);
            setOptions(quizData.options);

        } catch (err) {
            console.error("Test prep failed", err);
            setError("Bir hata oluştu.");
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        setResults([]);
        setShowResults(false);
        fetchQuestion();
    }, [type]);

    const handleAnswer = (isCorrect: boolean, answer: string) => {
        if (!currentWord) return;
        setResults(prev => [...prev, { word: currentWord, isCorrect, userAnswer: answer }]);
    };

    const handleNext = () => {
        fetchQuestion();
    };

    const handleFinish = async () => {
        await submitResults();
        setShowResults(true);
    };

    const submitResults = async () => {
        if (results.length === 0) return;
        const payload = results.map(r => ({
            wordId: r.word.id,
            isCorrect: r.isCorrect
        }));
        try {
            await wordService.bulkUpdateStats(payload);
        } catch (err) {
            console.error("Failed to submit results", err);
        }
    };

    const getTitle = () => {
        switch (type) {
            case "favorites": return "FAVORİLER - TEST";
            case "unknowns": return "BİLİNMEYENLER - TEST";
            default: return "GENEL TEST";
        }
    };

    if (showResults) {
        const correctCount = results.filter(r => r.isCorrect).length;
        const wrongCount = results.length - correctCount;

        return (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4 animate-in fade-in duration-200">
                <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl overflow-hidden">
                    <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
                        <h2 className="text-lg font-bold uppercase tracking-wider">TEST SONUCU</h2>
                        <button onClick={() => navigate("/dashboard")} className="text-gray-400 hover:text-white transition-colors">
                            <X size={24} />
                        </button>
                    </div>

                    <div className="p-8">
                        {/* Score Summary */}
                        <div className="flex justify-center gap-6 mb-8">
                            <div className="text-center p-4 bg-green-50 rounded-2xl min-w-[120px]">
                                <div className="text-3xl font-black text-green-600">{correctCount}</div>
                                <div className="text-xs font-bold text-green-800 uppercase mt-1">DOĞRU</div>
                            </div>
                            <div className="text-center p-4 bg-red-50 rounded-2xl min-w-[120px]">
                                <div className="text-3xl font-black text-red-600">{wrongCount}</div>
                                <div className="text-xs font-bold text-red-800 uppercase mt-1">YANLIŞ</div>
                            </div>
                        </div>

                        {/* Details List */}
                        <div className="space-y-6">
                            <div>
                                <h3 className="text-xs font-bold text-gray-400 uppercase tracking-wider mb-3">Detaylar</h3>
                                <div className="bg-gray-50 rounded-xl border border-gray-100 max-h-[300px] overflow-y-auto divide-y divide-gray-100">
                                    {results.length === 0 ? (
                                        <div className="p-4 text-center text-gray-500 text-sm">Henüz soru cevaplanmadı.</div>
                                    ) : (
                                        results.map((r, idx) => (
                                            <div key={idx} className="p-4 flex items-center justify-between hover:bg-white transition-colors">
                                                <div>
                                                    <div className="font-bold text-gray-800">{r.word.englishWord}</div>
                                                    <div className="text-sm text-gray-500">{r.word.turkishWord}</div>
                                                </div>
                                                <div className="text-right">
                                                    {r.isCorrect ? (
                                                        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                                            Doğru
                                                        </span>
                                                    ) : (
                                                        <div className="flex flex-col items-end">
                                                            <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800">
                                                                Yanlış
                                                            </span>
                                                            <span className="text-xs text-red-500 mt-1">Seçilen: {r.userAnswer}</span>
                                                        </div>
                                                    )}
                                                </div>
                                            </div>
                                        ))
                                    )}
                                </div>
                            </div>
                        </div>
                    </div>

                    <div className="bg-gray-50 px-6 py-5 flex justify-between border-t border-gray-100">
                        <button
                            onClick={() => navigate("/dashboard")}
                            className="px-6 py-2.5 bg-white border border-gray-200 text-gray-700 rounded-xl font-semibold hover:bg-gray-50 transition-colors"
                        >
                            Ana Sayfa
                        </button>
                        <button
                            onClick={() => {
                                setResults([]);
                                setShowResults(false);
                                fetchQuestion();
                            }}
                            className="px-6 py-2.5 bg-primary-yellow text-primary-dark rounded-xl font-bold hover:bg-[#FFC107] transition-colors shadow-lg shadow-yellow-500/10"
                        >
                            Yeni Test Başlat
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="flex flex-col items-center justify-center py-20 text-center animate-in fade-in zoom-in duration-300">
                <div className="bg-red-50 p-6 rounded-full mb-6">
                    <span className="text-4xl">⚠️</span>
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-2">Ops! Bir Sorun Var</h2>
                <p className="text-gray-500 max-w-md mb-8">{error}</p>
                <button
                    onClick={() => navigate("/dashboard")}
                    className="px-8 py-3 bg-gray-900 text-white rounded-xl font-bold hover:bg-gray-800 transition-colors"
                >
                    Geri Dön
                </button>
            </div>
        );
    }

    if (isLoading && !currentWord) {
        return (
            <div className="flex justify-center items-center py-32">
                <Loader2 className="animate-spin text-primary-yellow" size={48} />
            </div>
        );
    }

    return (
        <div className="max-w-4xl mx-auto px-4">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-2xl font-bold text-gray-900 uppercase tracking-tight">{getTitle()}</h1>
                <button
                    onClick={handleFinish}
                    className="px-5 py-2.5 bg-red-50 text-red-600 border border-red-100 rounded-xl font-bold hover:bg-red-100 hover:text-red-700 transition-all hover:shadow-sm"
                >
                    Testi Bitir
                </button>
            </div>

            {currentWord && (
                <TestCard
                    englishWord={currentWord.englishWord}
                    correctAnswer={currentWord.turkishWord}
                    options={options}
                    onAnswer={handleAnswer}
                    onNext={handleNext}
                />
            )}
        </div>
    );
}
