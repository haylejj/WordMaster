interface ConfirmDialogProps {
  open: boolean;
  title: string;
  description?: string;
  confirmText?: string;
  cancelText?: string;
  loading?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmDialog({
  open,
  title,
  description,
  confirmText = "Tamam",
  cancelText = "İptal",
  loading = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm px-4">
      <div className="w-full max-w-sm rounded-2xl bg-white shadow-xl border border-gray-100">
        <div className="px-6 py-5">
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
          {description && <p className="mt-2 text-sm text-gray-600">{description}</p>}
        </div>
        <div className="flex border-t border-gray-100">
          <button
            onClick={onCancel}
            className="flex-1 py-3 text-sm font-medium text-gray-600 hover:bg-gray-50 transition-colors"
            disabled={loading}
          >
            {cancelText}
          </button>
          <button
            onClick={onConfirm}
            disabled={loading}
            className="flex-1 py-3 text-sm font-semibold text-white bg-primary-dark hover:bg-primary-yellow hover:text-primary-dark transition-colors disabled:opacity-60"
          >
            {loading ? "Siliniyor..." : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}

