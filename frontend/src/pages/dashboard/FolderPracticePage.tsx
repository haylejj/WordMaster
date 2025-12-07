import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { PracticeCard } from "@/components/PracticeCard";
import { folderService } from "@/services/folder.service";
import { wordService } from "@/services/word.service";
import type { FolderWordResponse } from "@/types/folder";
import { X } from "lucide-react";

interface PracticeResult {
    word: FolderWordResponse;
    isCorrect: boolean;
    userAnswer?: string;
}

export default function FolderPracticePage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [words, setWords] = useState<FolderWordResponse[]>([]);
    const [currentIndex, setCurrentIndex] = useState(0);
    const [results, setResults] = useState<PracticeResult[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [showResults, setShowResults] = useState(false);
    const [folderName, setFolderName] = useState("");

    useEffect(() => {
        const fetchFolderData = async () => {
            if (!id) return;
            setIsLoading(true);
            try {
                const folderResult = await folderService.getFolder(Number(id));
                if (folderResult.data) {
                    setFolderName(folderResult.data.name);
                }

                const wordsResult = await folderService.getWords(Number(id));
                if (wordsResult.data) {
                    setWords(wordsResult.data);
                }
            } catch (error) {
                console.error("Failed to fetch folder data", error);
            } finally {
                setIsLoading(false);
            }
        };
        fetchFolderData();
    }, [id]);

    const handleCheck = async (answer: string) => {
        const currentWord = words[currentIndex];

        // Local check
        const normalizedAnswer = answer.trim().toLocaleLowerCase("tr-TR");
        const normalizedCorrect = (currentWord.turkishWord || "").trim().toLocaleLowerCase("tr-TR");
        const isCorrect = normalizedAnswer === normalizedCorrect;

        // If 'answer' is empty string, it means "I don't know" button was clicked
        // because "Cevabı Kontrol Et" button is disabled when input is empty.
        setResults(prev => [...prev, { word: currentWord, isCorrect, userAnswer: answer }]);
        return isCorrect;
    };

    const handleNext = async () => {
        if (currentIndex < words.length - 1) {
            setCurrentIndex(prev => prev + 1);
        } else {
            await submitResults();
            setShowResults(true);
        }
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
        return <div className="text-center py-12">Yükleniyor...</div>;
    }

    if (words.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-20 text-center">
                <div className="bg-yellow-50 p-6 rounded-full mb-6">
                    <span className="text-4xl">📂</span>
                </div>
                <h2 className="text-2xl font-bold text-gray-900 mb-2">Klasör Boş</h2>
                <p className="text-gray-500 max-w-md mb-8">
                    Bu klasörde henüz pratik yapılacak kelime bulunmuyor. Klasöre yeni kelimeler ekleyerek başlayabilirsiniz.
                </p>
                <div className="flex gap-4">
                    <button
                        onClick={() => navigate("/folders")}
                        className="px-6 py-3 bg-gray-100 text-gray-700 rounded-xl font-bold hover:bg-gray-200 transition-colors"
                    >
                        Klasörlere Dön
                    </button>
                    <button
                        onClick={() => navigate(`/folders/${id}`)}
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
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
                <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl overflow-hidden">
                    <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
                        <h2 className="text-lg font-bold uppercase">{folderName} - PRATİK SONUCU</h2>
                        <button onClick={() => navigate("/folders")} className="text-gray-400 hover:text-white">
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
                                                <div className="text-sm text-gray-500">Türkçe: {r.word.turkishWord || "Çeviri yok"}</div>
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
                                                <div className="text-sm text-gray-500">Türkçe: {r.word.turkishWord || "Çeviri yok"}</div>
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
                            onClick={() => navigate("/folders")}
                            className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700 transition-colors"
                        >
                            Kapat
                        </button>
                        <button
                            onClick={() => navigate(`/folders/${id}`)}
                            className="px-4 py-2 border border-blue-500 text-blue-500 rounded hover:bg-blue-50 transition-colors"
                        >
                            Klasöre Dön
                        </button>
                    </div>
                </div>
            </div>
        );
    }

    const currentWord = words[currentIndex];

    return (
        <div className="max-w-4xl mx-auto">
            <h1 className="text-2xl font-bold text-gray-900 mb-8 text-center uppercase">{folderName}</h1>
            <PracticeCard
                key={`${currentWord.id}-${currentIndex}`}
                englishWord={currentWord.englishWord}
                turkishWord={currentWord.turkishWord || ""}
                wordId={currentWord.id}
                onCheck={handleCheck}
                onNext={handleNext}
                isFolderPractice={true}
                currentIndex={currentIndex + 1}
                totalCount={words.length}
            />
        </div>
    );
}
