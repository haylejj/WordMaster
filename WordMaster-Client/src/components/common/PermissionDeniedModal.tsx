import { ShieldAlert, X } from "lucide-react";
import { useEffect, useState } from "react";

export default function PermissionDeniedModal() {
    const [open, setOpen] = useState(false);

    useEffect(() => {
        const handlePermissionDenied = () => {
            setOpen(true);
        };

        window.addEventListener("permission-denied", handlePermissionDenied);

        return () => {
            window.removeEventListener("permission-denied", handlePermissionDenied);
        };
    }, []);

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-[100] overflow-y-auto">
            {/* Backdrop */}
            <div
                className="fixed inset-0 bg-black/60 backdrop-blur-sm transition-opacity"
                onClick={() => setOpen(false)}
            />

            {/* Modal */}
            <div className="flex min-h-full items-center justify-center p-4">
                <div className="relative w-full max-w-md transform overflow-hidden rounded-2xl bg-[#0a0a0a] border border-red-900/30 shadow-2xl transition-all animate-in fade-in zoom-in-95 duration-200">
                    {/* Close button */}
                    <button
                        onClick={() => setOpen(false)}
                        className="absolute right-4 top-4 p-1 rounded-full text-gray-400 hover:text-white hover:bg-white/10 transition-colors"
                    >
                        <X size={18} />
                    </button>

                    <div className="p-6 pb-4">
                        {/* Icon */}
                        <div className="mx-auto w-16 h-16 rounded-full flex items-center justify-center mb-6 bg-red-900/20 border border-red-900/50 shadow-[0_0_15px_rgba(220,38,38,0.2)]">
                            <ShieldAlert className="text-red-500" size={32} />
                        </div>

                        {/* Content */}
                        <div className="text-center">
                            <h3 className="text-xl font-bold text-white mb-3">Erişim Engellendi</h3>
                            <p className="text-sm text-gray-400 leading-relaxed">
                                Bu işlemi gerçekleştirmek için gerekli yetkiye sahip değilsiniz.
                                <br />
                                Lütfen yöneticinizle iletişime geçin.
                            </p>
                        </div>
                    </div>

                    {/* Actions */}
                    <div className="px-6 pb-6">
                        <button
                            onClick={() => setOpen(false)}
                            className="w-full px-4 py-3 text-sm font-semibold text-white bg-red-600 rounded-xl hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-red-500 focus:ring-offset-2 focus:ring-offset-[#0a0a0a] transition-all shadow-lg shadow-red-900/20"
                        >
                            Tamam
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
