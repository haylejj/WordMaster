import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import FolderFlashCard from "@/components/folders/FolderFlashCard";
import { folderService } from "@/services/folder.service";
import { wordService } from "@/services/word.service";
import type { WordResponse } from "@/types/word";
import type { FolderResponse, FolderWordResponse } from "@/types/folder";
import { Folder, Loader2, ChevronLeft, Plus, Search, ChevronDown } from "lucide-react";
import ConfirmDialog from "@/components/common/ConfirmDialog";
import UpdateWordModal from "@/components/dashboard/UpdateWordModal";

export default function FolderDetailPage() {
    const { id } = useParams<{ id: string }>();
    const folderId = Number(id);
    const navigate = useNavigate();

    const [folder, setFolder] = useState<FolderResponse | null>(null);
    const [words, setWords] = useState<FolderWordResponse[]>([]);
    const [allWords, setAllWords] = useState<WordResponse[]>([]);
    const [selectedWordId, setSelectedWordId] = useState<number | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [removeTarget, setRemoveTarget] = useState<FolderWordResponse | null>(null);
    const [updateTarget, setUpdateTarget] = useState<FolderWordResponse | null>(null);
    const [busy, setBusy] = useState(false);
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const [wordQuery, setWordQuery] = useState("");
    const dropdownRef = useRef<HTMLDivElement>(null);

    const fetchData = async () => {
        if (!folderId) return;
        setLoading(true);
        try {
            const [folderRes, wordsRes, allWordsRes] = await Promise.all([
                folderService.getFolder(folderId),
                folderService.getWords(folderId),
                wordService.getUserWords(),
            ]);

            if (folderRes.isSuccess && folderRes.data) {
                setFolder(folderRes.data);
            } else {
                setError(folderRes.errorList?.join(" ") || "Klasör yüklenemedi.");
            }

            if (wordsRes.isSuccess && wordsRes.data) {
                setWords(wordsRes.data);
            } else {
                setError(wordsRes.errorList?.join(" ") || "Kelime listesi alınamadı.");
            }

            if (allWordsRes.isSuccess && allWordsRes.data) {
                setAllWords(allWordsRes.data);
            }
        } catch (err) {
            console.error(err);
            setError("Klasör bilgileri alınamadı.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchData();
    }, [folderId]);

    const availableWords = useMemo(() => {
        const existingIds = new Set(words.map((w) => w.id));
        return allWords.filter((w) => !existingIds.has(w.id));
    }, [words, allWords]);

    const filteredWords = useMemo(() => {
        const term = wordQuery.trim().toLowerCase();
        if (!term) return availableWords;
        return availableWords.filter((word) => {
            const english = (word.englishWord ?? "").toLowerCase();
            const turkish = (word.turkishWord ?? "").toLowerCase();
            return english.includes(term) || turkish.includes(term);
        });
    }, [availableWords, wordQuery]);

    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
                setDropdownOpen(false);
            }
        };
        if (dropdownOpen) {
            window.addEventListener("mousedown", handleClickOutside);
        }
        return () => window.removeEventListener("mousedown", handleClickOutside);
    }, [dropdownOpen]);

    const handleAddWord = async () => {
        if (!selectedWordId) return;
        setBusy(true);
        setError(null);
        try {
            const response = await folderService.addWordToFolder({ folderId, wordId: selectedWordId });
            if (response.isSuccess) {
                // jQuery tarzı optimizasyon: Sadece eklenen kelimeyi state'e ekle
                const addedWord = allWords.find(w => w.id === selectedWordId);
                if (addedWord) {
                    setWords(prev => [...prev, addedWord]);
                }
                setSelectedWordId(null);
                setDropdownOpen(false);
            } else {
                setError(response.errorList?.join(" ") || "Kelime klasöre eklenemedi.");
            }
        } catch (err) {
            console.error(err);
            setError("Kelime klasöre eklenemedi.");
        } finally {
            setBusy(false);
        }
    };

    const handleRemoveWord = async () => {
        if (!removeTarget) return;
        setBusy(true);
        try {
            const response = await folderService.removeWordFromFolder({ folderId, wordId: removeTarget.id });
            if (response.isSuccess) {
                // jQuery tarzı optimizasyon: Sadece çıkarılan kelimeyi state'ten kaldır
                setWords(prev => prev.filter(w => w.id !== removeTarget.id));
                setRemoveTarget(null);
            } else {
                setError(response.errorList?.join(" ") || "Kelime çıkarılamadı.");
            }
        } catch (err) {
            console.error(err);
            setError("Kelime çıkarılamadı.");
        } finally {
            setBusy(false);
        }
    };

    if (!folderId) {
        return <div>Klasör bulunamadı.</div>;
    }

    return (
        <div className="space-y-8">
            <button
                onClick={() => navigate(-1)}
                className="inline-flex items-center text-sm font-medium text-gray-500 hover:text-gray-800"
            >
                <ChevronLeft size={18} className="mr-1" />
                Geri dön
            </button>

            {loading ? (
                <div className="flex justify-center py-20">
                    <Loader2 className="animate-spin text-primary-yellow" size={40} />
                </div>
            ) : (
                <>
                    <div className="rounded-3xl bg-white p-6 shadow-sm border border-gray-100">
                        <div className="flex flex-col gap-6 md:flex-row md:items-center md:justify-between">
                            <div className="flex items-center gap-4">
                                <span className="inline-flex h-14 w-14 items-center justify-center rounded-2xl bg-yellow-50 text-yellow-500">
                                    <Folder size={28} />
                                </span>
                                <div>
                                    <h1 className="text-3xl font-bold text-gray-900">{folder?.name}</h1>
                                    <p className="text-sm text-gray-500 mt-1">
                                        {words.length} kelime | Oluşturulma:{" "}
                                        {folder ? new Date(folder.createdTime).toLocaleDateString("tr-TR") : "-"}
                                    </p>
                                </div>
                            </div>

                            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
                                <button
                                    onClick={() => navigate(`/dashboard/folders/${folderId}/practice`)}
                                    className="rounded-full bg-green-500 px-5 py-2 text-sm font-semibold text-white shadow hover:bg-green-600"
                                >
                                    Pratik Yap
                                </button>
                                <div className="relative w-full max-w-xs z-20">
                                    <div
                                        ref={dropdownRef}
                                        className="flex items-center gap-2 rounded-full bg-white px-2 py-1 border border-gray-200 shadow-inner"
                                    >
                                        <button
                                            type="button"
                                            onClick={() => setDropdownOpen((prev) => !prev)}
                                            className="flex items-center justify-between gap-2 rounded-full bg-gray-50 px-4 py-2 text-sm text-gray-700 min-w-[220px]"
                                        >
                                            <span className="truncate">
                                                {selectedWordId
                                                    ? availableWords.find((w) => w.id === selectedWordId)?.englishWord || "Kelime seçin..."
                                                    : "Kelime seçin..."}
                                            </span>
                                            <ChevronDown size={16} className="text-gray-400" />
                                        </button>
                                        <button
                                            onClick={handleAddWord}
                                            disabled={!selectedWordId || busy}
                                            className="inline-flex items-center gap-1 rounded-full bg-blue-500 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600 disabled:opacity-60"
                                        >
                                            <Plus size={16} />
                                            Ekle
                                        </button>

                                        {dropdownOpen && (
                                            <div className="absolute top-full left-0 mt-2 w-[280px] rounded-2xl border border-gray-100 bg-white p-3 shadow-2xl z-30">
                                                <div className="flex items-center rounded-xl border border-gray-200 bg-gray-50 px-3">
                                                    <Search size={16} className="text-gray-400" />
                                                    <input
                                                        value={wordQuery}
                                                        onChange={(e) => setWordQuery(e.target.value)}
                                                        placeholder="Kelime ara..."
                                                        className="w-full bg-transparent px-2 py-2 text-sm focus:outline-none"
                                                    />
                                                </div>
                                                <div className="mt-3 max-h-56 space-y-1 overflow-y-auto">
                                                    {filteredWords.length === 0 ? (
                                                        <p className="py-4 text-center text-sm text-gray-400">Eşleşen kelime bulunamadı.</p>
                                                    ) : (
                                                        filteredWords.map((word) => (
                                                            <button
                                                                key={word.id}
                                                                onClick={() => {
                                                                    setSelectedWordId(word.id);
                                                                    setDropdownOpen(false);
                                                                }}
                                                                className="flex w-full flex-col rounded-xl px-3 py-2 text-left text-sm hover:bg-gray-50"
                                                            >
                                                                <span className="font-semibold text-gray-800">{word.englishWord}</span>
                                                                <span className="text-xs text-gray-500">{word.turkishWord}</span>
                                                            </button>
                                                        ))
                                                    )}
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    {error && <div className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

                    {words.length === 0 ? (
                        <div className="rounded-3xl border border-dashed border-gray-200 bg-white py-16 text-center text-gray-500 shadow-sm">
                            Bu klasörde henüz kelime yok. Yukarıdan kelime ekleyebilirsin.
                        </div>
                    ) : (
                        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                            {words.map((word) => (
                                <FolderFlashCard
                                    key={word.id}
                                    word={word}
                                    onEdit={setUpdateTarget}
                                    onRemove={setRemoveTarget}
                                />
                            ))}
                        </div>
                    )}
                </>
            )}

            <UpdateWordModal
                isOpen={!!updateTarget}
                onClose={() => setUpdateTarget(null)}
                onSuccess={(updatedWord) => {
                    // jQuery tarzı optimizasyon: Sadece güncellenen kelimeyi state'te değiştir
                    if (updatedWord) {
                        setWords(prev => prev.map(w => w.id === updatedWord.id ? updatedWord : w));
                    }
                    setUpdateTarget(null);
                }}
                word={updateTarget}
            />

            <ConfirmDialog
                open={!!removeTarget}
                title="Kelimeyi klasörden çıkar"
                description={`"${removeTarget?.englishWord ?? ""}" klasörden çıkarılacak.`}
                confirmText="Çıkar"
                loading={busy}
                onConfirm={handleRemoveWord}
                onCancel={() => setRemoveTarget(null)}
            />
        </div>
    );
}

