import { useEffect, useState } from "react";
import { folderNameSchema, type FolderNameForm } from "@/types/folder";
import { z } from "zod";

interface FolderNameModalProps {
    open: boolean;
    title: string;
    initialName?: string;
    confirmLabel?: string;
    loading?: boolean;
    onClose: () => void;
    onSubmit: (values: FolderNameForm) => Promise<void> | void;
}

export default function FolderNameModal({
    open,
    title,
    initialName = "",
    confirmLabel = "Kaydet",
    loading = false,
    onClose,
    onSubmit,
}: FolderNameModalProps) {
    const [name, setName] = useState(initialName);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (open) {
            setName(initialName);
            setError(null);
        }
    }, [open, initialName]);

    if (!open) return null;

    const handleSubmit = async () => {
        try {
            const values = folderNameSchema.parse({ name });
            await onSubmit(values);
        } catch (err) {
            if (err instanceof z.ZodError) {
                setError(err.issues[0]?.message ?? "Geçersiz değer");
            }
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm px-4">
            <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-xl">
                <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
                <div className="mt-4 space-y-2">
                    <label className="text-sm font-medium text-gray-600">Klasör adı</label>
                    <input
                        value={name}
                        onChange={(e) => {
                            setName(e.target.value);
                            setError(null);
                        }}
                        className="w-full rounded-xl border border-gray-200 px-4 py-2.5 text-sm focus:border-primary-yellow focus:outline-none focus:ring-2 focus:ring-primary-yellow/40"
                        placeholder="Örn. Günlük çalışma"
                        disabled={loading}
                    />
                    {error && <p className="text-xs text-red-500">{error}</p>}
                </div>
                <div className="mt-6 flex justify-end gap-3">
                    <button
                        onClick={onClose}
                        disabled={loading}
                        className="rounded-xl border border-gray-200 px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-50 disabled:opacity-60"
                    >
                        Vazgeç
                    </button>
                    <button
                        onClick={handleSubmit}
                        disabled={loading}
                        className="rounded-xl bg-primary-yellow px-4 py-2 text-sm font-semibold text-primary-dark shadow hover:bg-[#ffd84d] disabled:opacity-60"
                    >
                        {loading ? "Kaydediliyor..." : confirmLabel}
                    </button>
                </div>
            </div>
        </div>
    );
}

