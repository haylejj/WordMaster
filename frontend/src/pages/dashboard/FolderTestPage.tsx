
import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import TestCard from "@/components/TestCard";
import { folderService } from "@/services/folder.service";
import { wordService } from "@/services/word.service";
import type { FolderWordResponse } from "@/types/folder";
import { X, Loader2 } from "lucide-react";

interface TestResult {
    word: FolderWordResponse;
    isCorrect: boolean;
    userAnswer: string;
}

export default function FolderTestPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    // Data
    const [words, setWords] = useState<FolderWordResponse[]>([]);
    const [folderName, setFolderName] = useState("");

    // Game State
    const [currentIndex, setCurrentIndex] = useState(0);
    const [options, setOptions] = useState<string[]>([]);
    const [results, setResults] = useState<TestResult[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [showResults, setShowResults] = useState(false);

    // Fetch Folder Data
    useEffect(() => {
        const loadCategory = async () => {
            if (!id) return;
            setIsLoading(true);
            try {
                const [fRes, wRes] = await Promise.all([
                    folderService.getFolder(Number(id)),
                    folderService.getWords(Number(id))
                ]);

                if (fRes.data) setFolderName(fRes.data.name);
                if (wRes.data) setWords(wRes.data);

            } catch (err) {
                console.error(err);
            } finally {
                setIsLoading(false);
            }
        };
        loadCategory();
    }, [id]);

    // Prepare Question when Index changes
    useEffect(() => {
        const prepareQuestion = async () => {
            if (words.length === 0) return;
            if (currentIndex >= words.length) return;

            const current = words[currentIndex];
            const targetAnswer = current.turkishWord;

            // Pick 3 distractors
            // 1. Try to pick from folder words first
            const otherWords = words.filter(w => w.id !== current.id && w.turkishWord !== targetAnswer);
            let pool = new Set<string>();
            pool.add(targetAnswer);

            // Add from folder
            // Shuffle otherWords
            const shuffledOthers = [...otherWords].sort(() => Math.random() - 0.5);
            for (const w of shuffledOthers) {
                if (pool.size >= 4) break;
                pool.add(w.turkishWord);
            }

            // 2. If not enough, fetch random global words
            if (pool.size < 4) {
                const needed = 4 - pool.size;
                // Fetch randoms in one go
                const res = await wordService.getDistractors(needed + 2, current.id); // +2 for buffer

                if (res.isSuccess && res.data) {
                    for (const w of res.data) {
                        if (w !== targetAnswer) {
                            pool.add(w);
                        }
                        if (pool.size >= 4) break;
                    }
                }
            }

            // Shuffle final options
            const finalOptions = Array.from(pool).sort(() => Math.random() - 0.5);
            setOptions(finalOptions);
        };

        if (!isLoading && words.length > 0) {
            prepareQuestion();
        }
    }, [currentIndex, words, isLoading]);


    const handleAnswer = (isCorrect: boolean, answer: string) => {
        const currentWord = words[currentIndex];
        setResults(prev => [...prev, { word: currentWord, isCorrect, userAnswer: answer }]);
    };

    const handleNext = () => {
        if (currentIndex < words.length - 1) {
            setCurrentIndex(prev => prev + 1);
        } else {
            handleFinish();
        }
    };

    const handleFinish = async () => {
        if (results.length > 0) {
            await submitResults();
        }
        setShowResults(true);
    };

    const submitResults = async () => {
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

    if (isLoading) {
        return (
            <div className="flex justify-center items-center py-32">
                <Loader2 className="animate-spin text-primary-yellow" size={48} />
            </div>
        );
    }

    if (words.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 text-center animate-in fade-in zoom-in duration-300">
                <div className="bg-yellow-50 p-6 rounded-full mb-6">
                    <span className="text-4xl">📂</span>
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-2">Klasör Boş</h2>
                <p className="text-gray-500 max-w-md mb-8">
                    Bu klasörde henüz kelime yok. Test yapmak için önce kelime eklemelisin.
                </p>
                <div className="flex gap-4">
                    <button
                        onClick={() => navigate("/dashboard/folders")}
                        className="px-6 py-3 bg-gray-100 text-gray-700 rounded-xl font-bold hover:bg-gray-200 transition-colors"
                    >
                        Klasörlere Dön
                    </button>
                    <button
                        onClick={() => navigate(`/dashboard/folders/${id}`)}
                        className="px-6 py-3 bg-primary-yellow text-primary-dark rounded-xl font-bold hover:bg-[#FFC107] transition-colors"
                    >
                        Kelime Ekle
                    </button>
                </div>
            </div>
        );
    }

    if (showResults) {
        const correctCount = results.filter(r => r.isCorrect).length;
        const wrongCount = results.length - correctCount;

        return (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4 animate-in fade-in duration-200">
                <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl overflow-hidden">
                    <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
                        <h2 className="text-lg font-bold uppercase tracking-wider">{folderName} - SONUÇLAR</h2>
                        <button onClick={() => navigate("/dashboard/folders")} className="text-gray-400 hover:text-white transition-colors">
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
                            onClick={() => navigate("/dashboard/folders")}
                            className="px-6 py-2.5 bg-white border border-gray-200 text-gray-700 rounded-xl font-semibold hover:bg-gray-50 transition-colors"
                        >
                            Klasör Listesi
                        </button>
                        <button
                            onClick={() => {
                                setResults([]);
                                setCurrentIndex(0);
                                setShowResults(false);
                            }}
                            className="px-6 py-2.5 bg-primary-yellow text-primary-dark rounded-xl font-bold hover:bg-[#FFC107] transition-colors shadow-lg shadow-yellow-500/10"
                        >
                            Tekrar Test Yap
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    if (currentIndex >= words.length) {
        // Fallback if renderer is faster than finish handler
        return null;
    }

    const currentWord = words[currentIndex];

    return (
        <div className="max-w-4xl mx-auto px-4">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-2xl font-bold text-gray-900 uppercase tracking-tight">{folderName} - TEST</h1>
                <button
                    onClick={() => setShowResults(true)}
                    className="px-5 py-2.5 bg-red-50 text-red-600 border border-red-100 rounded-xl font-bold hover:bg-red-100 hover:text-red-700 transition-all hover:shadow-sm"
                >
                    Testi Bitir
                </button>
            </div>

            <TestCard
                englishWord={currentWord.englishWord}
                correctAnswer={currentWord.turkishWord}
                options={options}
                onAnswer={handleAnswer}
                onNext={handleNext}
                currentIndex={currentIndex + 1}
                totalCount={words.length}
            />
        </div>
    );
}
