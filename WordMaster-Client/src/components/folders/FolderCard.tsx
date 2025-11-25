import type { FolderResponse } from "@/types/folder";
import { Folder, Clock3, Edit3, Trash2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface FolderCardProps {
    folder: FolderResponse;
    onOpen: (folder: FolderResponse) => void;
    onEdit: (folder: FolderResponse) => void;
    onDelete: (folder: FolderResponse) => void;
}

export default function FolderCard({ folder, onOpen, onEdit, onDelete }: FolderCardProps) {
    return (
        <div
            onClick={() => onOpen(folder)}
            className="group relative rounded-2xl border border-gray-100 bg-white p-6 shadow-sm transition hover:-translate-y-1 hover:shadow-xl cursor-pointer"
        >
            <div className="flex items-start justify-between mb-8">
                <div className="flex flex-col space-y-3">
                    <span className="inline-flex h-12 w-12 items-center justify-center rounded-xl bg-blue-50 text-blue-600">
                        <Folder size={28} />
                    </span>
                    <span className="text-lg font-semibold text-gray-800">{folder.name}</span>
                </div>

                <div className="flex gap-2 opacity-0 transition group-hover:opacity-100">
                    <button
                        onClick={(e) => {
                            e.stopPropagation();
                            onEdit(folder);
                        }}
                        className="flex h-9 w-9 items-center justify-center rounded-full bg-yellow-100 text-yellow-600 hover:bg-yellow-200"
                        title="Klasörü düzenle"
                    >
                        <Edit3 size={16} />
                    </button>
                    <button
                        onClick={(e) => {
                            e.stopPropagation();
                            onDelete(folder);
                        }}
                        className="flex h-9 w-9 items-center justify-center rounded-full bg-red-100 text-red-600 hover:bg-red-200"
                        title="Klasörü sil"
                    >
                        <Trash2 size={16} />
                    </button>
                </div>
            </div>

            <div className="flex items-center justify-between text-sm text-gray-500">
                <div className="flex items-center space-x-1">
                    <Clock3 size={16} />
                    <span>{new Date(folder.createdTime).toLocaleDateString("tr-TR")}</span>
                </div>
                <span
                    className={cn(
                        "rounded-full px-3 py-1 text-xs font-semibold",
                        folder.wordCount > 0 ? "bg-primary-yellow/20 text-primary-dark" : "bg-gray-100 text-gray-500"
                    )}
                >
                    {folder.wordCount} kelime
                </span>
            </div>
        </div>
    );
}

