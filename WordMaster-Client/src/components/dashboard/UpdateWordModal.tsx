import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { updateWordSchema } from "@/types/word";
import type { UpdateWordRequest, WordResponse } from "@/types/word";
import { wordService } from "@/services/word.service";
import { X } from "lucide-react";
import { cn } from "@/lib/utils";

interface UpdateWordModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  word: WordResponse | null;
}

export default function UpdateWordModal({ isOpen, onClose, onSuccess, word }: UpdateWordModalProps) {
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<UpdateWordRequest>({
    resolver: zodResolver(updateWordSchema),
    defaultValues: {
      id: 0,
      englishWord: "",
      turkishWord: "",
    },
  });

  useEffect(() => {
    if (word && isOpen) {
      setValue("id", word.id);
      setValue("englishWord", word.englishWord);
      setValue("turkishWord", word.turkishWord);
    }
  }, [word, isOpen, setValue]);

  const onSubmit = async (data: UpdateWordRequest) => {
    setLoading(true);
    setError(null);
    try {
      const response = await wordService.updateWord(data);
      if (response.isSuccess) {
        onSuccess();
        onClose();
      } else {
        setError(response.errorList?.join(" ") || "Kelime güncellenemedi.");
      }
    } catch (err: any) {
      setError(err.response?.data?.message || "Bir hata oluştu.");
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm animate-in fade-in duration-200">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md mx-4 overflow-hidden animate-in zoom-in-95 duration-200">
        <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
          <h2 className="text-lg font-bold uppercase tracking-wide flex items-center">
            <span className="mr-2 text-primary-yellow">✏️</span> KELİME GÜNCELLE
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-white transition-colors">
            <X size={20} />
          </button>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
          {error && (
            <div className="bg-red-100 text-red-700 p-3 rounded text-sm">
              {error}
            </div>
          )}

          <input type="hidden" {...register("id", { valueAsNumber: true })} />

          <div className="space-y-1">
            <label className="block text-xs font-bold text-gray-500 uppercase tracking-wider">İNGİLİZCE</label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <span className="text-gray-400 text-xs font-bold border border-gray-300 rounded px-1 bg-gray-50">A</span>
              </div>
              <input
                {...register("englishWord")}
                className={cn(
                  "w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-yellow focus:border-transparent",
                  errors.englishWord && "border-red-500"
                )}
                placeholder="word"
              />
            </div>
            {errors.englishWord && <p className="text-xs text-red-500">{errors.englishWord.message}</p>}
          </div>

          <div className="space-y-1">
            <label className="block text-xs font-bold text-gray-500 uppercase tracking-wider">TÜRKÇE</label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <span className="text-gray-400 text-xs font-bold border border-gray-300 rounded px-1 bg-gray-50">tr</span>
              </div>
              <input
                {...register("turkishWord")}
                className={cn(
                  "w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-yellow focus:border-transparent",
                  errors.turkishWord && "border-red-500"
                )}
                placeholder="kelime"
              />
            </div>
            {errors.turkishWord && <p className="text-xs text-red-500">{errors.turkishWord.message}</p>}
          </div>

          <div className="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 transition-colors font-medium"
            >
              İptal
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-primary-yellow text-primary-dark rounded-md font-bold hover:bg-[#FFC107] transition-colors disabled:opacity-70 flex items-center"
            >
              {loading ? "Güncelleniyor..." : "💾 Güncelle"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

