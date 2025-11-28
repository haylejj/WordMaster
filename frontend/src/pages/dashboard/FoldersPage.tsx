import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { folderService } from "@/services/folder.service";
import type { FolderNameForm, FolderResponse } from "@/types/folder";
import FolderCard from "@/components/folders/FolderCard";
import FolderNameModal from "@/components/folders/FolderNameModal";
import ConfirmDialog from "@/components/common/ConfirmDialog";
import { Plus, Loader2 } from "lucide-react";

export default function FoldersPage() {
    const [folders, setFolders] = useState<FolderResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [feedback, setFeedback] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [editTarget, setEditTarget] = useState<FolderResponse | null>(null);
    const [deleteTarget, setDeleteTarget] = useState<FolderResponse | null>(null);
    const [createModalOpen, setCreateModalOpen] = useState(false);

    const navigate = useNavigate();

    const fetchFolders = async () => {
        setLoading(true);
        try {
            const response = await folderService.getFolders();
            if (response.isSuccess && response.data) {
                setFolders(response.data);
            } else {
                setError(response.errorList?.join(" ") || "Klasörler yüklenemedi.");
            }
        } catch (err) {
            console.error(err);
            setError("Klasörler yüklenemedi.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchFolders();
    }, []);

    const handleCreate = async ({ name }: FolderNameForm) => {
        setBusy(true);
        setError(null);
        try {
            const response = await folderService.createFolder(name.trim());
            if (response.isSuccess && response.data) {
                // Backend artık created folder dönüyor!
                setFolders(prev => [...prev, response.data!]);
                setFeedback("Klasör oluşturuldu.");
                setCreateModalOpen(false);
            } else {
                setError(response.errorList?.join(" ") || "Klasör oluşturulamadı.");
            }
        } catch (err) {
            console.error(err);
            setError("Klasör oluşturulamadı.");
        } finally {
            setBusy(false);
            setTimeout(() => setFeedback(null), 2000);
        }
    };

    const handleRename = async ({ name }: { name: string }) => {
        if (!editTarget) return;
        setBusy(true);
        try {
            const response = await folderService.updateFolder(editTarget.id, name);
            if (response.isSuccess && response.data) {
                // Backend updated folder dönüyor!
                setFolders(prev => prev.map(folder =>
                    folder.id === editTarget.id ? response.data! : folder
                ));
                setFeedback("Klasör güncellendi.");
                setEditTarget(null);
            } else {
                throw new Error(response.errorList?.join(" ") || "Klasör güncellenemedi.");
            }
        } catch (err: any) {
            setError(err.message || "Klasör güncellenemedi.");
        } finally {
            setBusy(false);
            setTimeout(() => setFeedback(null), 2000);
        }
    };

    const handleDelete = async () => {
        if (!deleteTarget) return;
        setBusy(true);
        try {
            const response = await folderService.deleteFolder(deleteTarget.id);
            if (response.isSuccess) {
                // jQuery tarzı optimizasyon: Silinen folder'ı state'ten kaldır
                setFolders(prev => prev.filter(folder => folder.id !== deleteTarget.id));
                setFeedback("Klasör silindi.");
                setDeleteTarget(null);
            } else {
                throw new Error(response.errorList?.join(" ") || "Klasör silinemedi.");
            }
        } catch (err: any) {
            setError(err.message || "Klasör silinemedi.");
        } finally {
            setBusy(false);
            setTimeout(() => setFeedback(null), 2000);
        }
    };

    return (
        <div className="space-y-8">
            <header className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900 tracking-tight">Klasörlerim</h1>
                    <p className="text-sm text-gray-500 mt-1">Kelime setlerini klasörler halinde düzenle.</p>
                </div>

                <button
                    onClick={() => setCreateModalOpen(true)}
                    className="flex items-center rounded-full bg-white px-4 py-2 text-sm font-semibold text-gray-600 shadow-inner border border-gray-100 hover:border-primary-yellow hover:text-primary-dark transition"
                >
                    <span className="mr-3 text-gray-400">Yeni klasör adı</span>
                    <span className="flex items-center gap-1 rounded-full bg-primary-yellow px-4 py-2 text-primary-dark shadow">
                        <Plus size={14} />
                        Ekle
                    </span>
                </button>
            </header>

            {feedback && <div className="rounded-xl bg-green-50 px-4 py-3 text-sm text-green-700">{feedback}</div>}
            {error && <div className="rounded-xl bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

            {loading ? (
                <div className="flex justify-center py-20">
                    <Loader2 className="animate-spin text-primary-yellow" size={36} />
                </div>
            ) : folders.length === 0 ? (
                <div className="rounded-3xl border border-dashed border-gray-200 bg-white py-16 text-center text-gray-500 shadow-sm">
                    Henüz klasör yok. Hemen yeni bir klasör ekleyerek başlayabilirsin.
                </div>
            ) : (
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
                    {folders.map((folder) => (
                        <FolderCard
                            key={folder.id}
                            folder={folder}
                            onOpen={() => navigate(`/folders/${folder.id}`)}
                            onEdit={setEditTarget}
                            onDelete={setDeleteTarget}
                        />
                    ))}
                </div>
            )}

            <FolderNameModal
                open={createModalOpen}
                title="Yeni klasör oluştur"
                confirmLabel="Oluştur"
                loading={busy}
                onClose={() => setCreateModalOpen(false)}
                onSubmit={handleCreate}
            />

            <FolderNameModal
                open={!!editTarget}
                title="Klasör adını düzenle"
                initialName={editTarget?.name ?? ""}
                loading={busy}
                onClose={() => setEditTarget(null)}
                onSubmit={handleRename}
            />

            <ConfirmDialog
                open={!!deleteTarget}
                title="Klasörü silmek istediğine emin misin?"
                description={`"${deleteTarget?.name ?? ""}" klasöründeki tüm bağlantılar silinecektir.`}
                confirmText="Sil"
                loading={busy}
                onConfirm={handleDelete}
                onCancel={() => setDeleteTarget(null)}
            />
        </div>
    );
}

