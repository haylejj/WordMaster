import { AlertTriangle, Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";

interface ConfirmationModalProps {
    isOpen: boolean;
    onClose: () => void;
    onConfirm: () => void;
    title: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    isLoading?: boolean;
    variant?: "danger" | "warning" | "info";
    successMessage?: string | null;
    errorMessage?: string | null;
}

export default function ConfirmationModal({
    isOpen,
    onClose,
    onConfirm,
    title,
    message,
    confirmText = "Onayla",
    cancelText = "İptal",
    isLoading = false,
    variant = "danger",
    successMessage,
    errorMessage
}: ConfirmationModalProps) {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
            <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-md shadow-2xl transform transition-all scale-100">
                <div className="p-6 text-center">
                    {!successMessage && (
                        <div className={cn(
                            "w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4",
                            variant === "danger" ? "bg-red-500/10 text-red-500" :
                                variant === "warning" ? "bg-yellow-500/10 text-yellow-500" :
                                    "bg-blue-500/10 text-blue-500"
                        )}>
                            <AlertTriangle size={32} />
                        </div>
                    )}

                    <h3 className="text-xl font-bold text-white mb-2">{title}</h3>

                    {successMessage ? (
                        <div className="bg-green-500/10 border border-green-500/20 rounded-lg p-4 mb-4">
                            <p className="text-green-500 font-medium">{successMessage}</p>
                        </div>
                    ) : errorMessage ? (
                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 mb-4">
                            <p className="text-red-500 font-medium">{errorMessage}</p>
                        </div>
                    ) : (
                        <p className="text-gray-400 mb-6">{message}</p>
                    )}

                    {!successMessage && (
                        <div className="flex items-center justify-center gap-3">
                            <button
                                onClick={onClose}
                                disabled={isLoading}
                                className="px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-lg transition-colors disabled:opacity-50"
                            >
                                {cancelText}
                            </button>
                            <button
                                onClick={onConfirm}
                                disabled={isLoading}
                                className={cn(
                                    "px-4 py-2 text-white rounded-lg transition-colors flex items-center gap-2 disabled:opacity-50",
                                    variant === "danger" ? "bg-red-600 hover:bg-red-700" :
                                        variant === "warning" ? "bg-yellow-600 hover:bg-yellow-700" :
                                            "bg-blue-600 hover:bg-blue-700"
                                )}
                            >
                                {isLoading && <Loader2 className="animate-spin" size={16} />}
                                {confirmText}
                            </button>
                        </div>
                    )}

                    {successMessage && (
                        <button
                            onClick={onClose}
                            className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors mt-2"
                        >
                            Kapat
                        </button>
                    )}
                </div>
            </div>
        </div>
    );
}
