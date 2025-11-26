import React, { useState, useEffect } from "react";
import { Check, HelpCircle, ArrowRight } from "lucide-react";
import { cn } from "@/lib/utils";

interface PracticeCardProps {
    englishWord: string;
    turkishWord: string; // Correct answer
    wordId: number;
    onCheck: (answer: string) => Promise<boolean>;
    onNext: () => void;
    isFolderPractice?: boolean;
    currentIndex?: number;
    totalCount?: number;
    hideCounter?: boolean;
}

export function PracticeCard({
    englishWord,
    turkishWord,
    wordId,
    onCheck,
    onNext,
    isFolderPractice = false,
    currentIndex,
    totalCount,
    hideCounter = false,
}: PracticeCardProps) {
    const [answer, setAnswer] = useState("");
    const [status, setStatus] = useState<"idle" | "correct" | "wrong" | "unknown">("idle");
    const [isLoading, setIsLoading] = useState(false);

    // Reset state when word changes
    useEffect(() => {
        setAnswer("");
        setStatus("idle");
    }, [englishWord]);

    const handleCheck = async () => {
        if (!answer.trim()) return;
        setIsLoading(true);
        try {
            const isCorrect = await onCheck(answer);
            if (isCorrect) {
                setStatus("correct");
            } else {
                setStatus("wrong");
            }
        } catch (error) {
            console.error("Check failed", error);
        } finally {
            setIsLoading(false);
        }
    };

    const handleUnknown = async () => {
        setIsLoading(true);
        try {
            await onCheck(""); // Send empty to count as wrong
            setStatus("unknown");
        } catch (error) {
            console.error("Check failed", error);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-8 max-w-2xl mx-auto text-center relative">
            {!hideCounter && isFolderPractice && currentIndex !== undefined && totalCount !== undefined && (
                <div className="absolute top-4 right-4 bg-gray-800 text-white text-xs font-bold px-3 py-1 rounded-full">
                    {currentIndex} / {totalCount}
                </div>
            )}

            <div className="mb-8">
                <h2 className="text-sm font-bold text-gray-500 uppercase tracking-wider mb-2">İngilizce Kelime</h2>
                <h1 className="text-4xl font-extrabold text-gray-900">{englishWord}</h1>
            </div>

            <div className="mb-6">
                <label className="block text-sm font-medium text-gray-700 mb-2 text-left">Türkçe Karşılık</label>
                <input
                    type="text"
                    value={answer}
                    onChange={(e) => setAnswer(e.target.value)}
                    placeholder="Cevabınızı yazın"
                    className={cn(
                        "w-full px-4 py-3 rounded-lg border focus:ring-2 focus:ring-primary-yellow focus:border-transparent outline-none transition-all text-lg",
                        status === "idle" ? "border-gray-300" : "",
                        status === "correct" ? "border-green-500 bg-green-50" : "",
                        status === "wrong" ? "border-red-500 bg-red-50" : "",
                        status === "unknown" ? "border-gray-300 bg-gray-100" : ""
                    )}
                    disabled={status !== "idle" || isLoading}
                    onKeyDown={(e) => {
                        if (e.key === "Enter" && status === "idle") {
                            handleCheck();
                        }
                    }}
                />
            </div>

            {status === "unknown" && (
                <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded-lg text-yellow-800 font-medium">
                    Bilmiyorum seçildi. Doğru cevap: {turkishWord}
                </div>
            )}

            {status === "wrong" && (
                <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-lg text-red-800 font-medium">
                    Yanlış cevap. Doğru cevap: {turkishWord}
                </div>
            )}

            {status === "correct" && (
                <div className="mb-6 p-4 bg-green-50 border border-green-200 rounded-lg text-green-800 font-medium">
                    Doğru cevap!
                </div>
            )}

            <div className="flex space-x-4 justify-center">
                {status === "idle" ? (
                    <>
                        <button
                            onClick={handleCheck}
                            disabled={isLoading || !answer.trim()}
                            className="flex items-center px-6 py-3 bg-primary-yellow text-primary-dark rounded-lg font-bold hover:bg-[#FFC107] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <Check size={20} className="mr-2" />
                            Cevabı Kontrol Et
                        </button>
                        <button
                            onClick={handleUnknown}
                            disabled={isLoading}
                            className="flex items-center px-6 py-3 bg-white border border-gray-300 text-gray-700 rounded-lg font-bold hover:bg-gray-50 transition-colors"
                        >
                            <HelpCircle size={20} className="mr-2" />
                            Bilmiyorum
                        </button>
                    </>
                ) : (
                    <button
                        onClick={onNext}
                        className="flex items-center px-8 py-3 bg-gray-900 text-white rounded-lg font-bold hover:bg-gray-800 transition-colors"
                    >
                        <ArrowRight size={20} className="mr-2" />
                        Sıradaki Kelime
                    </button>
                )}
            </div>
        </div>
    );
}
