import { useState } from "react";
import type { FolderWordResponse } from "@/types/folder";
import { Pencil, Trash2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface FolderFlashCardProps {
    word: FolderWordResponse;
    onEdit: (word: FolderWordResponse) => void;
    onRemove: (word: FolderWordResponse) => void;
}

export default function FolderFlashCard({ word, onEdit, onRemove }: FolderFlashCardProps) {
    const [flipped, setFlipped] = useState(false);

    return (
        <div
            className="relative h-48 w-full max-w-[320px] cursor-pointer [perspective:1000px]"
            onClick={() => setFlipped((prev) => !prev)}
        >
            <div
                className={cn(
                    "relative h-full w-full rounded-3xl bg-transparent text-center text-lg font-semibold text-gray-800 shadow-lg transition-transform duration-500 [transform-style:preserve-3d]",
                    flipped && "[transform:rotateY(180deg)]"
                )}
            >
                <div className="absolute inset-0 rounded-3xl bg-white px-6 py-6 text-xl text-gray-800 [backface-visibility:hidden]">
                    <div className="absolute inset-x-0 top-0 h-2 rounded-t-3xl bg-gradient-to-r from-[#FFC93C] to-[#FF9A3C]" />
                    <div className="flex justify-end gap-2 text-gray-600">
                        <button
                            className="rounded-full bg-yellow-100 p-2 shadow text-yellow-700"
                            onClick={(e) => {
                                e.stopPropagation();
                                onEdit(word);
                            }}
                        >
                            <Pencil size={16} />
                        </button>
                        <button
                            className="rounded-full bg-red-100 p-2 shadow text-red-500"
                            onClick={(e) => {
                                e.stopPropagation();
                                onRemove(word);
                            }}
                        >
                            <Trash2 size={16} />
                        </button>
                    </div>
                    <div className="mt-8 text-2xl font-bold capitalize text-gray-800">{word.englishWord}</div>

                </div>

                <div className="absolute inset-0 flex items-center justify-center rounded-3xl bg-[#1f2a37] px-6 text-2xl text-white [backface-visibility:hidden] [transform:rotateY(180deg)]">
                    <span className="tracking-wide">{word.turkishWord}</span>
                </div>
            </div>
        </div>
    );
}

