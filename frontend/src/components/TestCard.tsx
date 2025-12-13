
import { useState, useEffect } from "react";
import { Check, X, ArrowRight } from "lucide-react";
import { cn } from "@/lib/utils";

interface TestCardProps {
    englishWord: string;
    correctAnswer: string;
    options: string[];
    onAnswer: (isCorrect: boolean, answer: string) => void;
    onNext: () => void;
    currentIndex?: number;
    totalCount?: number;
    isSubmitting?: boolean;
}

export default function TestCard({
    englishWord,
    correctAnswer,
    options,
    onAnswer,
    onNext,
    currentIndex,
    totalCount,
    isSubmitting = false
}: TestCardProps) {
    const [selectedOption, setSelectedOption] = useState<string | null>(null);
    const [isAnswered, setIsAnswered] = useState(false);

    // Reset state when question changes
    useEffect(() => {
        setSelectedOption(null);
        setIsAnswered(false);
    }, [englishWord]);

    const handleOptionClick = (option: string) => {
        if (isAnswered || isSubmitting) return;

        setSelectedOption(option);
        setIsAnswered(true);

        const isCorrect = option === correctAnswer;
        onAnswer(isCorrect, option);
    };

    const getOptionStyle = (option: string) => {
        const baseStyle = "w-full p-4 rounded-xl border-2 text-left font-medium transition-all duration-200 flex items-center justify-between";

        if (!isAnswered) {
            return cn(baseStyle, "bg-white border-gray-100 hover:border-primary-yellow hover:bg-yellow-50 text-gray-700");
        }

        if (option === correctAnswer) {
            return cn(baseStyle, "bg-green-50 border-green-500 text-green-700");
        }

        if (selectedOption === option && option !== correctAnswer) {
            return cn(baseStyle, "bg-red-50 border-red-500 text-red-700");
        }

        return cn(baseStyle, "bg-gray-50 border-transparent opacity-50 text-gray-400");
    };

    return (
        <div className="w-full max-w-2xl mx-auto">
            <div className="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
                {/* Header / Progress */}
                {(currentIndex !== undefined && totalCount !== undefined) && (
                    <div className="bg-gray-50 px-6 py-4 border-b border-gray-100 flex justify-between items-center text-sm text-gray-500 font-medium">
                        <span>Kelime Testi</span>
                        <span>{currentIndex} / {totalCount}</span>
                    </div>
                )}

                <div className="p-8 md:p-10">
                    {/* Question Section */}
                    <div className="text-center mb-10">
                        <h2 className="text-gray-400 text-sm font-bold uppercase tracking-wider mb-3">İNGİLİZCESİ</h2>
                        <div className="text-4xl md:text-5xl font-extrabold text-gray-800">
                            {englishWord}
                        </div>
                    </div>

                    {/* Options Grid */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {options.map((option, index) => (
                            <button
                                key={index}
                                onClick={() => handleOptionClick(option)}
                                disabled={isAnswered || isSubmitting}
                                className={cn(getOptionStyle(option), "h-full min-h-[4rem]")}
                            >
                                <span className="text-left leading-tight py-1 break-words">{option}</span>
                                {isAnswered && option === correctAnswer && (
                                    <Check size={20} className="text-green-600 flex-shrink-0 ml-2" />
                                )}
                                {isAnswered && selectedOption === option && option !== correctAnswer && (
                                    <X size={20} className="text-red-500 flex-shrink-0 ml-2" />
                                )}
                            </button>
                        ))}
                    </div>

                    {/* Next Button */}
                    {isAnswered && (
                        <div className="mt-8 flex justify-end animate-in fade-in slide-in-from-bottom-4 duration-300">
                            <button
                                onClick={onNext}
                                className="inline-flex items-center gap-2 px-8 py-3 bg-primary-yellow text-primary-dark rounded-xl font-bold hover:bg-[#FFC107] transition-colors shadow-lg shadow-yellow-500/20"
                            >
                                Sonraki Soru
                                <ArrowRight size={20} />
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
