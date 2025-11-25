import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { createWordSchema } from "@/types/word";
import type { CreateWordRequest } from "@/types/word";
import { wordService } from "@/services/word.service";
import { Plus, FileDown } from "lucide-react";
import { cn, getErrorMessage } from "@/lib/utils";

import ImportCsvModal from "@/components/dashboard/ImportCsvModal";

export default function CreateWordPage() {
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [isImportModalOpen, setIsImportModalOpen] = useState(false);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<CreateWordRequest>({
        resolver: zodResolver(createWordSchema),
        defaultValues: {
            englishWord: "",
            turkishWord: "",
        },
    });

    const onSubmit = async (data: CreateWordRequest) => {
        setLoading(true);
        setError(null);
        try {
            const response = await wordService.addWord(data);
            if (response.isSuccess) {
                setError(null);
                setSuccess("Kelime başarıyla kaydedildi!");
                reset();
            } else {
                setSuccess(null);
                setError(response.errorList?.join(" ") || "Kelime eklenemedi.");
            }
        } catch (err: any) {
            setSuccess(null);
            setError(getErrorMessage(err));
        } finally {
            setLoading(false);
        }
    };

    const handleCsvImported = () => {
        setError(null);
        setSuccess("CSV dosyası başarıyla içe aktarıldı!");
    };

    useEffect(() => {
        if (!success) return;
        const timer = setTimeout(() => setSuccess(null), 3000);
        return () => clearTimeout(timer);
    }, [success]);

    useEffect(() => {
        if (!error) return;
        const timer = setTimeout(() => setError(null), 3000);
        return () => clearTimeout(timer);
    }, [error]);

    return (
        <div className="max-w-2xl mx-auto mt-8">
            <div className="bg-white rounded-lg shadow-sm border border-gray-100 p-8">
                <div className="flex justify-between items-center mb-6">
                    <div className="flex items-center space-x-3">
                        <div className="w-10 h-10 bg-yellow-50 rounded-full flex items-center justify-center">
                            <Plus className="text-primary-yellow" size={24} />
                        </div>
                        <h1 className="text-xl font-bold text-gray-800">Yeni Kelime Ekle</h1>
                    </div>
                    <button
                        onClick={() => setIsImportModalOpen(true)}
                        className="flex items-center px-4 py-2 border border-green-600 text-green-700 rounded-md hover:bg-green-50 font-medium text-sm transition-colors"
                    >
                        <FileDown size={16} className="mr-2" />
                        Dışarıdan Aktar
                    </button>
                </div>

                <p className="text-gray-500 text-sm mb-8">
                    Tek bir kelime ve anlamını manuel olarak ekleyin veya CSV ile toplu yükleme yapın.
                </p>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                    {error && (
                        <div className="bg-red-100 text-red-700 p-3 rounded text-sm">
                            {error}
                        </div>
                    )}
                    {success && (
                        <div className="bg-green-100 text-green-700 p-3 rounded text-sm">
                            {success}
                        </div>
                    )}

                    <div className="space-y-1">
                        <label className="block text-sm font-bold text-gray-600">English Word</label>
                        <div className="relative">
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <span className="text-gray-500 text-xs font-bold bg-gray-100 px-1.5 py-0.5 rounded border">A</span>
                            </div>
                            <input
                                {...register("englishWord")}
                                className={cn(
                                    "w-full pl-10 pr-3 py-2.5 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-yellow focus:border-transparent transition-shadow",
                                    errors.englishWord && "border-red-500"
                                )}
                                placeholder="Apple"
                            />
                        </div>
                        {errors.englishWord && <p className="text-xs text-red-500">{errors.englishWord.message}</p>}
                    </div>

                    <div className="space-y-1">
                        <label className="block text-sm font-bold text-gray-600">Turkish Meaning</label>
                        <div className="relative">
                            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                <span className="text-gray-500 text-xs font-bold bg-gray-100 px-1.5 py-0.5 rounded border">tr</span>
                            </div>
                            <input
                                {...register("turkishWord")}
                                className={cn(
                                    "w-full pl-10 pr-3 py-2.5 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-yellow focus:border-transparent transition-shadow",
                                    errors.turkishWord && "border-red-500"
                                )}
                                placeholder="Elma"
                            />
                        </div>
                        {errors.turkishWord && <p className="text-xs text-red-500">{errors.turkishWord.message}</p>}
                    </div>

                    <button
                        type="submit"
                        disabled={loading}
                        className="w-full py-3 bg-primary-yellow text-primary-dark rounded-md font-bold hover:bg-[#FFC107] transition-colors disabled:opacity-70 flex items-center justify-center shadow-sm"
                    >
                        {loading ? "Kaydediliyor..." : "💾 Kaydet"}
                    </button>
                </form>
            </div>

            <ImportCsvModal
                isOpen={isImportModalOpen}
                onClose={() => setIsImportModalOpen(false)}
                onImported={handleCsvImported}
            />
        </div>
    );
}

