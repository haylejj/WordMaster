import { useEffect, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { wordService } from "@/services/word.service";
import type { WordResponse } from "@/types/word";
import { Search, X, Edit, Trash2, Star, HelpCircle, AlertCircle, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
import UpdateWordModal from "@/components/dashboard/UpdateWordModal";
import ConfirmDialog from "@/components/common/ConfirmDialog";

const PAGE_SIZE_OPTIONS = [5, 10, 20, 50, 100];

const fetchHandlers = {
  all: wordService.getWords,
  favorites: wordService.getFavorites,
  unknowns: wordService.getUnknowns,
} as const;

type WordListVariant = keyof typeof fetchHandlers;

const variantTitles: Record<WordListVariant, string> = {
  all: "Kelimelerim",
  favorites: "Favorilerim",
  unknowns: "Bilinmeyenlerim",
};

const variantDescriptions: Record<WordListVariant, string> = {
  all: "Tüm kelimelerini yönet, düzenle veya sil.",
  favorites: "Favoriye aldığın kelimeleri burada hızlıca yönetebilirsin.",
  unknowns: "Zorlandığın kelimeleri tekrar çalışmak için burada tut.",
};

const emptyMessages: Record<WordListVariant, string> = {
  all: "Kayıtlı kelime bulunamadı.",
  favorites: "Henüz favorilere eklediğin bir kelime yok.",
  unknowns: "Bilinmeyenler listende kelime bulunamadı.",
};

interface WordsPageProps {
  variant?: WordListVariant;
}

export default function WordsPage({ variant = "all" }: WordsPageProps) {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialSearch = searchParams.get("search") || "";
  const initialPage = parseInt(searchParams.get("page") || "1", 10);
  const initialPageSize = parseInt(searchParams.get("pageSize") || "10", 10);

  const [searchInput, setSearchInput] = useState(initialSearch);
  const [searchTerm, setSearchTerm] = useState(initialSearch);
  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [words, setWords] = useState<WordResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);

  const [selectedWord, setSelectedWord] = useState<WordResponse | null>(null);
  const [isUpdateModalOpen, setIsUpdateModalOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<WordResponse | null>(null);
  const [deleteLoading, setDeleteLoading] = useState(false);

  const fetchWords = async (currentSearch = searchTerm, currentPage = page, currentPageSize = pageSize) => {
    setLoading(true);
    try {
      const response = await fetchHandlers[variant](currentSearch, currentPage, currentPageSize);
      if (response.isSuccess && response.data) {
        setWords(response.data.items);
        setTotalCount(response.data.totalCount);
        setTotalPages(response.data.totalPages);
      } else {
        setWords([]);
        setTotalCount(0);
        setTotalPages(0);
        console.error("API Error:", response.errorList);
      }
    } catch (error) {
      console.error("Failed to fetch words", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchWords(searchTerm, page, pageSize);

    const params = new URLSearchParams();
    if (searchTerm) params.set("search", searchTerm);
    if (page > 1) params.set("page", page.toString());
    if (pageSize !== 10) params.set("pageSize", pageSize.toString());
    setSearchParams(params);
  }, [searchTerm, page, pageSize, variant]);

  const handleSearch = () => {
    setPage(1);
    setSearchTerm(searchInput.trim());
  };

  const handleClear = () => {
    setSearchInput("");
    setSearchTerm("");
    setPage(1);
    setPageSize(10);
  };

  const handleDeleteClick = (word: WordResponse) => {
    setDeleteTarget(word);
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleteLoading(true);
    try {
      const response = await wordService.deleteWord(deleteTarget.id);
      if (response.isSuccess) {
        fetchWords();
      } else {
        alert(response.errorList?.join(" ") || "Silme başarısız oldu.");
      }
    } catch (error) {
      console.error("Delete failed", error);
    } finally {
      setDeleteLoading(false);
      setDeleteTarget(null);
    }
  };

  const handleEdit = (word: WordResponse) => {
    setSelectedWord(word);
    setIsUpdateModalOpen(true);
  };

  const handleToggleFavorite = async (word: WordResponse) => {
    try {
      await wordService.toggleFavorite(word.id);
      fetchWords();
    } catch (error) {
      console.error("Toggle favorite failed", error);
    }
  };

  const handleToggleUnknown = async (word: WordResponse) => {
    try {
      await wordService.toggleUnknown(word.id);
      fetchWords();
    } catch (error) {
      console.error("Toggle unknown failed", error);
    }
  };

  const ToggleSwitch = ({ checked, onChange }: { checked: boolean; onChange: () => void }) => (
    <button
      onClick={onChange}
      type="button"
      className={cn(
        "relative inline-flex h-5 w-9 flex-shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none",
        checked ? "bg-primary-yellow" : "bg-gray-200"
      )}
    >
      <span
        aria-hidden="true"
        className={cn(
          "pointer-events-none inline-block h-4 w-4 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out",
          checked ? "translate-x-4" : "translate-x-0"
        )}
      />
    </button>
  );

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-800">{variantTitles[variant]}</h1>
        <p className="text-sm text-gray-500 mt-1">{variantDescriptions[variant]}</p>
      </div>

      <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-100">
        <div className="grid grid-cols-1 md:grid-cols-12 gap-4 items-end">
          <div className="md:col-span-7 space-y-2">
            <label className="text-xs font-bold text-gray-500 uppercase tracking-wider">Ara</label>
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" size={18} />
              <input
                type="text"
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                placeholder="İngilizce veya Türkçe ara..."
                className="w-full pl-10 pr-4 py-3 bg-gray-50 border border-gray-200 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-yellow focus:bg-white transition-all text-sm"
                onKeyDown={(e) => e.key === "Enter" && handleSearch()}
              />
            </div>
          </div>
          <div className="md:col-span-2 space-y-2">
            <label className="text-xs font-bold text-gray-500 uppercase tracking-wider">Sayfa Başına</label>
            <select
              value={pageSize}
              onChange={(e) => {
                setPageSize(Number(e.target.value));
                setPage(1);
              }}
              className="w-full px-3 py-3 bg-gray-50 border border-gray-200 rounded-md focus:outline-none focus:ring-2 focus:ring-primary-yellow cursor-pointer text-sm"
            >
              {PAGE_SIZE_OPTIONS.map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </div>
          <div className="md:col-span-3 flex space-x-2">
            <button
              onClick={handleSearch}
              className="flex-1 bg-primary-yellow text-primary-dark font-bold py-3 px-4 rounded-md hover:bg-[#FFC107] transition-colors flex items-center justify-center text-sm"
            >
              <Search size={16} className="mr-2" /> Uygula
            </button>
            <button
              onClick={handleClear}
              className="flex-1 bg-white border border-gray-300 text-gray-700 font-semibold py-3 px-4 rounded-md hover:bg-gray-50 transition-colors flex items-center justify-center text-sm"
            >
              <X size={16} className="mr-2" /> Temizle
            </button>
          </div>
        </div>
      </div>

      <div className="bg-white rounded-lg shadow-sm border border-gray-100 overflow-hidden min-h-[400px] flex flex-col">
        {loading ? (
          <div className="flex-grow flex justify-center items-center">
            <Loader2 className="animate-spin text-primary-yellow" size={40} />
          </div>
        ) : (
          <>
            <div className="overflow-x-auto flex-grow">
              <table className="w-full">
                <thead className="bg-[#1a1f24] text-white">
                  <tr>
                    <th className="px-6 py-4 text-left text-xs font-bold uppercase tracking-wider w-1/3">İNGİLİZCE</th>
                    <th className="px-6 py-4 text-left text-xs font-bold uppercase tracking-wider w-1/3">TÜRKÇE</th>
                    <th className="px-6 py-4 text-center text-xs font-bold uppercase tracking-wider w-1/6">DURUM</th>
                    <th className="px-6 py-4 text-center text-xs font-bold uppercase tracking-wider w-1/12">DÜZENLE</th>
                    <th className="px-6 py-4 text-center text-xs font-bold uppercase tracking-wider w-1/12">SİL</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {words.length > 0 ? (
                    words.map((word) => (
                      <tr key={word.id} className="hover:bg-yellow-50/30 transition-colors">
                        <td className="px-6 py-4 font-medium text-primary-dark">{word.englishWord}</td>
                        <td className="px-6 py-4 text-gray-600">{word.turkishWord}</td>
                        <td className="px-6 py-4">
                          <div className="flex items-center justify-center space-x-4">
                            <div className="flex items-center space-x-2" title="Favorilere Ekle/Çıkar">
                              <ToggleSwitch checked={!!word.favoriteId} onChange={() => handleToggleFavorite(word)} />
                              <Star size={18} className={cn(word.favoriteId ? "fill-yellow-400 text-yellow-400" : "text-gray-300")} />
                            </div>
                            <div className="w-px h-4 bg-gray-200" />
                            <div className="flex items-center space-x-2" title="Bilinmeyenlere Ekle/Çıkar">
                              <ToggleSwitch checked={!!word.unknowsId} onChange={() => handleToggleUnknown(word)} />
                              <HelpCircle size={18} className={cn(word.unknowsId ? "fill-red-500 text-red-500" : "text-gray-300")} />
                            </div>
                          </div>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <button onClick={() => handleEdit(word)} className="p-2 bg-blue-50 text-blue-600 rounded-full hover:bg-blue-100 transition-colors">
                            <Edit size={16} />
                          </button>
                        </td>
                        <td className="px-6 py-4 text-center">
                          <button onClick={() => handleDeleteClick(word)} className="p-2 bg-red-50 text-red-600 rounded-full hover:bg-red-100 transition-colors">
                            <Trash2 size={16} />
                          </button>
                        </td>
                      </tr>
                    ))
                  ) : (
                    <tr>
                      <td colSpan={5} className="px-6 py-20">
                        <div className="flex flex-col items-center text-center text-gray-500 space-y-2">
                          <div className="bg-gray-100 p-4 rounded-full">
                            <AlertCircle className="text-gray-400" size={48} />
                          </div>
                          <p className="text-base font-semibold text-gray-600">{emptyMessages[variant]}</p>
                          <p className="text-sm text-gray-400">Yeni kelime ekleyerek başlayabilirsiniz.</p>
                        </div>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>

            {totalPages > 1 && (
              <div className="px-6 py-4 bg-gray-50 border-t border-gray-100 flex items-center justify-between">
                <span className="text-sm text-gray-500">
                  Toplam {totalCount} kayıttan {(page - 1) * pageSize + 1} - {Math.min(page * pageSize, totalCount)} arası gösteriliyor
                </span>
                <div className="flex space-x-1">
                  <button
                    onClick={() => setPage((prev) => Math.max(1, prev - 1))}
                    disabled={page === 1}
                    className="px-3 py-1 border rounded-md disabled:opacity-50 hover:bg-white"
                  >
                    Önceki
                  </button>
                  {Array.from({ length: totalPages }, (_, i) => i + 1)
                    .filter((p) => p === 1 || p === totalPages || (p >= page - 2 && p <= page + 2))
                    .map((p, index, array) => (
                      <span key={p} className="flex items-center">
                        {index > 0 && array[index - 1] !== p - 1 && <span className="px-2 text-gray-400">...</span>}
                        <button
                          onClick={() => setPage(p)}
                          className={cn(
                            "px-3 py-1 border rounded-md min-w-[32px]",
                            page === p ? "bg-primary-yellow border-primary-yellow font-bold" : "bg-white hover:bg-gray-50"
                          )}
                        >
                          {p}
                        </button>
                      </span>
                    ))}
                  <button
                    onClick={() => setPage((prev) => Math.min(totalPages, prev + 1))}
                    disabled={page === totalPages}
                    className="px-3 py-1 border rounded-md disabled:opacity-50 hover:bg-white"
                  >
                    Sonraki
                  </button>
                </div>
              </div>
            )}
          </>
        )}
      </div>

      <UpdateWordModal
        isOpen={isUpdateModalOpen}
        onClose={() => setIsUpdateModalOpen(false)}
        onSuccess={() => fetchWords()}
        word={selectedWord}
      />

      <ConfirmDialog
        open={!!deleteTarget}
        title="Kelimeyi silmek istediğine emin misin?"
        description={`"${deleteTarget?.englishWord ?? ""}" kelimesi tamamen silinecektir.`}
        confirmText="Sil"
        cancelText="Vazgeç"
        loading={deleteLoading}
        onConfirm={confirmDelete}
        onCancel={() => {
          if (!deleteLoading) setDeleteTarget(null);
        }}
      />
    </div>
  );
}
