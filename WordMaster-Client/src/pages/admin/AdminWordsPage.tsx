import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { adminWordService, type AdminWordResponse } from "@/services/admin.word.service";
import {
    Search,
    Edit2,
    Trash2,
    X,
    BookOpen,
    Loader2,
    CheckCircle2,
    AlertCircle,
    User,
    ChevronLeft,
    ChevronRight
} from "lucide-react";
import { cn } from "@/lib/utils";
import ConfirmationModal from "@/components/ui/ConfirmationModal";

// Schema for Word Update
const wordSchema = z.object({
    englishWord: z.string().min(1, "İngilizce kelime zorunludur"),
    turkishWord: z.string().min(1, "Türkçe karşılık zorunludur")
});

type WordFormData = z.infer<typeof wordSchema>;

export default function AdminWordsPage() {
    // List State
    const [words, setWords] = useState<AdminWordResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [pageSize, setPageSize] = useState(10);

    // Edit Modal State
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [editingWord, setEditingWord] = useState<AdminWordResponse | null>(null);
    const [actionLoading, setActionLoading] = useState(false);
    const [modalSuccess, setModalSuccess] = useState<string | null>(null);
    const [modalError, setModalError] = useState<string | null>(null);

    // Delete Modal State
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [wordToDelete, setWordToDelete] = useState<AdminWordResponse | null>(null);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [deleteSuccess, setDeleteSuccess] = useState<string | null>(null);
    const [deleteError, setDeleteError] = useState<string | null>(null);

    const { register, handleSubmit, reset, setValue, formState: { errors } } = useForm<WordFormData>({
        resolver: zodResolver(wordSchema)
    });

    const fetchWords = async (pageNum: number = 1, search: string = "", size: number = 10) => {
        setLoading(true);
        try {
            const result = await adminWordService.getPagedWords(pageNum, size, search);
            if (result.isSuccess) {
                setWords(result.data.items);
                setTotalPages(result.data.totalPages);
                setTotalCount(result.data.totalCount);
                setPage(result.data.pageNumber);
            }
        } catch (error) {
            console.error("Kelimeler yüklenirken hata oluştu", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const timeoutId = setTimeout(() => {
            fetchWords(1, searchQuery, pageSize);
        }, 500);
        return () => clearTimeout(timeoutId);
    }, [searchQuery, pageSize]);

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            fetchWords(newPage, searchQuery, pageSize);
        }
    };

    // Edit Modal Handlers
    const handleOpenEditModal = (word: AdminWordResponse) => {
        setModalSuccess(null);
        setModalError(null);
        setEditingWord(word);
        setValue("englishWord", word.englishWord);
        setValue("turkishWord", word.turkishWord);
        setEditModalOpen(true);
    };

    const handleCloseEditModal = () => {
        setEditModalOpen(false);
        setEditingWord(null);
        reset();
        setModalSuccess(null);
        setModalError(null);
    };

    const onSubmitEdit = async (data: WordFormData) => {
        if (!editingWord) return;

        setActionLoading(true);
        setModalSuccess(null);
        setModalError(null);

        try {
            const result = await adminWordService.updateWord(editingWord.id, data);

            if (result.isSuccess) {
                setModalSuccess("Kelime başarıyla güncellendi.");
                fetchWords(page, searchQuery, pageSize);
            } else {
                setModalError(result.errorList?.[0] || "Güncelleme başarısız.");
            }
        } catch (error) {
            setModalError("Bir hata oluştu.");
            console.error("İşlem başarısız", error);
        } finally {
            setActionLoading(false);
        }
    };

    // Delete Modal Handlers
    const handleDeleteClick = (word: AdminWordResponse) => {
        setWordToDelete(word);
        setDeleteSuccess(null);
        setDeleteError(null);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!wordToDelete) return;

        setDeleteLoading(true);
        setDeleteSuccess(null);
        setDeleteError(null);

        try {
            const result = await adminWordService.deleteWord(wordToDelete.id);
            if (result.isSuccess) {
                setDeleteSuccess("Kelime başarıyla silindi.");
                fetchWords(page, searchQuery, pageSize);
            } else {
                setDeleteError(result.errorList?.[0] || "Silme işlemi başarısız.");
            }
        } catch (error) {
            setDeleteError("Silme işlemi sırasında bir hata oluştu.");
            console.error("Silme işlemi başarısız", error);
        } finally {
            setDeleteLoading(false);
        }
    };

    const handleCloseDeleteModal = () => {
        setDeleteModalOpen(false);
        setWordToDelete(null);
        setDeleteSuccess(null);
        setDeleteError(null);
    };

    return (
        <div className="space-y-6 animate-in fade-in duration-500">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-white mb-2 flex items-center gap-2">
                        <BookOpen className="text-blue-500" />
                        Kelime Yönetimi
                    </h1>
                    <p className="text-gray-400">Sistemdeki tüm kelimeleri yönetin (Global Sözlük).</p>
                </div>
            </div>

            {/* Search and Table Container */}
            <div className="bg-[#121212] border border-white/5 rounded-xl overflow-hidden">
                {/* Search Bar */}
                <div className="p-4 border-b border-white/5 flex flex-col sm:flex-row justify-between items-center gap-4">
                    <div className="relative max-w-md">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
                        <input
                            type="text"
                            placeholder="Kelime, anlam, ID veya kullanıcı ara..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            className="w-full bg-[#1a1a1a] text-white pl-10 pr-4 py-2 rounded-lg border border-white/10 focus:border-blue-500 focus:outline-none transition-colors"
                        />
                    </div>

                    <div className="flex items-center gap-2">
                        <span className="text-sm text-gray-400">Sayfa Başına:</span>
                        <select
                            value={pageSize}
                            onChange={(e) => {
                                const newSize = Number(e.target.value);
                                setPageSize(newSize);
                                setPage(1); // Reset to first page when size changes
                            }}
                            className="bg-[#1a1a1a] text-white px-3 py-2 rounded-lg border border-white/10 focus:border-blue-500 focus:outline-none transition-colors text-sm"
                        >
                            <option value={10}>10</option>
                            <option value={20}>20</option>
                            <option value={50}>50</option>
                            <option value={100}>100</option>
                        </select>
                    </div>
                </div>

                {/* Table */}
                <div className="overflow-x-auto">
                    <table className="w-full text-left">
                        <thead className="bg-white/5 text-gray-400 uppercase text-xs font-bold">
                            <tr>
                                <th className="px-6 py-4">ID</th>
                                <th className="px-6 py-4">İngilizce</th>
                                <th className="px-6 py-4">Türkçe</th>
                                <th className="px-6 py-4">Sahibi</th>
                                <th className="px-6 py-4">Oluşturulma</th>
                                <th className="px-6 py-4 text-right">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-white/5">
                            {loading ? (
                                <tr>
                                    <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                                        <div className="flex justify-center items-center gap-2">
                                            <Loader2 className="animate-spin" size={20} />
                                            Yükleniyor...
                                        </div>
                                    </td>
                                </tr>
                            ) : words.length > 0 ? (
                                words.map((word) => (
                                    <tr key={word.id} className="hover:bg-white/5 transition-colors">
                                        <td className="px-6 py-4 text-gray-500 font-mono text-xs max-w-[100px] truncate" title={word.id.toString()}>
                                            {word.id}
                                        </td>
                                        <td className="px-6 py-4 text-white font-medium">
                                            {word.englishWord}
                                        </td>
                                        <td className="px-6 py-4 text-gray-300">
                                            {word.turkishWord}
                                        </td>
                                        <td className="px-6 py-4">
                                            {word.userName ? (
                                                <div className="flex items-center gap-2 text-sm text-blue-400">
                                                    <User size={14} />
                                                    {word.userName}
                                                </div>
                                            ) : (
                                                <span className="text-gray-600 text-xs italic">Sistem / Bilinmiyor</span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 text-gray-500 text-xs">
                                            {new Date(word.createdTime).toLocaleDateString("tr-TR")}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <div className="flex items-center justify-end gap-2">
                                                <button
                                                    onClick={() => handleOpenEditModal(word)}
                                                    className="p-2 hover:bg-blue-500/20 text-blue-400 rounded-lg transition-colors"
                                                    title="Düzenle"
                                                >
                                                    <Edit2 size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleDeleteClick(word)}
                                                    className="p-2 hover:bg-red-500/20 text-red-400 rounded-lg transition-colors"
                                                    title="Sil"
                                                >
                                                    <Trash2 size={18} />
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                ))
                            ) : (
                                <tr>
                                    <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                                        Kayıt bulunamadı.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>

                {/* Pagination */}
                <div className="p-4 border-t border-white/5 flex items-center justify-between">
                    <span className="text-sm text-gray-500">
                        Toplam {totalCount} kayıt, Sayfa {page} / {totalPages}
                    </span>
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => handlePageChange(page - 1)}
                            disabled={page === 1}
                            className="p-2 bg-white/5 hover:bg-white/10 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                            <ChevronLeft size={16} />
                        </button>

                        {(() => {
                            const pages = [];
                            const showEllipsisStart = page > 3;
                            const showEllipsisEnd = page < totalPages - 2;

                            if (totalPages <= 7) {
                                for (let i = 1; i <= totalPages; i++) {
                                    pages.push(i);
                                }
                            } else {
                                pages.push(1);
                                if (showEllipsisStart) pages.push("...");

                                let start = Math.max(2, page - 1);
                                let end = Math.min(totalPages - 1, page + 1);

                                if (page <= 3) end = 4;
                                if (page >= totalPages - 2) start = totalPages - 3;

                                for (let i = start; i <= end; i++) {
                                    pages.push(i);
                                }

                                if (showEllipsisEnd) pages.push("...");
                                pages.push(totalPages);
                            }

                            return pages.map((p, index) => (
                                <button
                                    key={index}
                                    onClick={() => typeof p === 'number' ? handlePageChange(p) : null}
                                    disabled={p === "..."}
                                    className={cn(
                                        "w-8 h-8 flex items-center justify-center rounded-lg text-sm font-medium transition-colors",
                                        p === page
                                            ? "bg-blue-600 text-white"
                                            : p === "..."
                                                ? "text-gray-500 cursor-default"
                                                : "bg-white/5 hover:bg-white/10 text-gray-300 hover:text-white"
                                    )}
                                >
                                    {p}
                                </button>
                            ));
                        })()}

                        <button
                            onClick={() => handlePageChange(page + 1)}
                            disabled={page === totalPages}
                            className="p-2 bg-white/5 hover:bg-white/10 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                            <ChevronRight size={16} />
                        </button>
                    </div>
                </div>
            </div>

            {/* Edit Modal */}
            {editModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-md shadow-2xl">
                        <div className="flex items-center justify-between p-6 border-b border-white/10">
                            <h2 className="text-xl font-bold text-white">Kelime Düzenle</h2>
                            <button onClick={handleCloseEditModal} className="text-gray-400 hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6">
                            {modalSuccess ? (
                                <div className="flex flex-col items-center justify-center py-4 space-y-4">
                                    <div className="w-16 h-16 bg-green-500/10 rounded-full flex items-center justify-center text-green-500">
                                        <CheckCircle2 size={32} />
                                    </div>
                                    <p className="text-green-500 font-medium text-center">{modalSuccess}</p>
                                    <button
                                        onClick={handleCloseEditModal}
                                        className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
                                    >
                                        Kapat
                                    </button>
                                </div>
                            ) : (
                                <form onSubmit={handleSubmit(onSubmitEdit)} className="space-y-4">
                                    {modalError && (
                                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 flex items-center gap-3 text-red-500">
                                            <AlertCircle size={20} />
                                            <p className="text-sm">{modalError}</p>
                                        </div>
                                    )}

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">İngilizce</label>
                                        <input
                                            {...register("englishWord")}
                                            className={cn(
                                                "w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-blue-500 focus:outline-none transition-colors",
                                                errors.englishWord && "border-red-500"
                                            )}
                                        />
                                        {errors.englishWord && <p className="text-sm text-red-500">{errors.englishWord.message}</p>}
                                    </div>

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">Türkçe</label>
                                        <input
                                            {...register("turkishWord")}
                                            className={cn(
                                                "w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-blue-500 focus:outline-none transition-colors",
                                                errors.turkishWord && "border-red-500"
                                            )}
                                        />
                                        {errors.turkishWord && <p className="text-sm text-red-500">{errors.turkishWord.message}</p>}
                                    </div>

                                    <div className="flex justify-end gap-3 pt-2">
                                        <button
                                            type="button"
                                            onClick={handleCloseEditModal}
                                            className="px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-lg transition-colors"
                                        >
                                            İptal
                                        </button>
                                        <button
                                            type="submit"
                                            disabled={actionLoading}
                                            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
                                        >
                                            {actionLoading && <Loader2 className="animate-spin" size={16} />}
                                            Güncelle
                                        </button>
                                    </div>
                                </form>
                            )}
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            <ConfirmationModal
                isOpen={deleteModalOpen}
                onClose={handleCloseDeleteModal}
                onConfirm={handleConfirmDelete}
                title="Kelimeyi Sil"
                message={`"${wordToDelete?.englishWord}" kelimesini silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
                confirmText="Evet, Sil"
                variant="danger"
                isLoading={deleteLoading}
                successMessage={deleteSuccess}
                errorMessage={deleteError}
            />
        </div>
    );
}
