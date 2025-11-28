import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { ipService } from "@/services/ip.service";
import type { AllowedIpAddressResponse } from "@/types/ip";
import {
    Search,
    Plus,
    Edit2,
    Trash2,
    X,
    Globe,
    Loader2,
    CheckCircle2,
    XCircle,
    AlertCircle
} from "lucide-react";
import { cn } from "@/lib/utils";
import ConfirmationModal from "@/components/ui/ConfirmationModal";

const ipSchema = z.object({
    ipAddress: z.string().min(1, "IP adresi zorunludur").regex(/^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/, "Geçerli bir IPv4 adresi giriniz"),
    description: z.string().optional(),
    isActive: z.boolean()
});

type IpFormData = z.infer<typeof ipSchema>;

export default function AdminIpAddressesPage() {
    const [ips, setIps] = useState<AllowedIpAddressResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");

    // Edit/Create Modal State
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingIp, setEditingIp] = useState<AllowedIpAddressResponse | null>(null);
    const [actionLoading, setActionLoading] = useState(false);
    const [modalSuccess, setModalSuccess] = useState<string | null>(null);
    const [modalError, setModalError] = useState<string | null>(null);

    // Delete Modal State
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [ipToDelete, setIpToDelete] = useState<AllowedIpAddressResponse | null>(null);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [deleteSuccess, setDeleteSuccess] = useState<string | null>(null);
    const [deleteError, setDeleteError] = useState<string | null>(null);

    const { register, handleSubmit, reset, setValue, formState: { errors } } = useForm<IpFormData>({
        resolver: zodResolver(ipSchema),
        defaultValues: {
            isActive: true
        }
    });

    const fetchIps = async () => {
        try {
            const result = await ipService.getAll();
            if (result.isSuccess) {
                setIps(result.data);
            }
        } catch (error) {
            console.error("IP adresleri yüklenirken hata oluştu", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchIps();
    }, []);

    const filteredIps = ips.filter(ip =>
        ip.ipAddress.includes(searchQuery) ||
        (ip.description && ip.description.toLowerCase().includes(searchQuery.toLowerCase()))
    );

    const handleOpenModal = (ip?: AllowedIpAddressResponse) => {
        setModalSuccess(null);
        setModalError(null);
        if (ip) {
            setEditingIp(ip);
            setValue("ipAddress", ip.ipAddress);
            setValue("description", ip.description || "");
            setValue("isActive", ip.isActive);
        } else {
            setEditingIp(null);
            reset({ isActive: true });
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingIp(null);
        reset();
        setModalSuccess(null);
        setModalError(null);
    };

    const onSubmit = async (data: IpFormData) => {
        setActionLoading(true);
        setModalSuccess(null);
        setModalError(null);
        try {
            if (editingIp) {
                const result = await ipService.update({
                    id: editingIp.id,
                    ipAddress: data.ipAddress,
                    description: data.description,
                    isActive: data.isActive
                });
                if (result.isSuccess) {
                    setModalSuccess("IP adresi başarıyla güncellendi.");
                    fetchIps();
                } else {
                    setModalError(result.errorList?.[0] || "Güncelleme başarısız.");
                }
            } else {
                const result = await ipService.create(data);
                if (result.isSuccess) {
                    setModalSuccess("IP adresi başarıyla oluşturuldu.");
                    fetchIps();
                    reset({ isActive: true });
                } else {
                    setModalError(result.errorList?.[0] || "Oluşturma başarısız.");
                }
            }
        } catch (error) {
            setModalError("Bir hata oluştu.");
            console.error("İşlem başarısız", error);
        } finally {
            setActionLoading(false);
        }
    };

    const handleDeleteClick = (ip: AllowedIpAddressResponse) => {
        setIpToDelete(ip);
        setDeleteSuccess(null);
        setDeleteError(null);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!ipToDelete) return;

        setDeleteLoading(true);
        setDeleteSuccess(null);
        setDeleteError(null);

        try {
            const result = await ipService.delete(ipToDelete.id);
            if (result.isSuccess) {
                setDeleteSuccess("IP adresi başarıyla silindi.");
                fetchIps();
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
        setIpToDelete(null);
        setDeleteSuccess(null);
        setDeleteError(null);
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center h-full text-red-500">
                <Loader2 className="animate-spin w-12 h-12" />
            </div>
        );
    }

    return (
        <div className="space-y-6 animate-in fade-in duration-500">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-white mb-2 flex items-center gap-2">
                        <Globe className="text-red-500" />
                        IP Adres Yönetimi
                    </h1>
                    <p className="text-gray-400">Sisteme erişimine izin verilen IP adreslerini yönetin.</p>
                </div>
                <button
                    onClick={() => handleOpenModal()}
                    className="flex items-center gap-2 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
                >
                    <Plus size={20} />
                    Yeni IP Ekle
                </button>
            </div>

            {/* Search and Table Container */}
            <div className="bg-[#121212] border border-white/5 rounded-xl overflow-hidden">
                {/* Search Bar */}
                <div className="p-4 border-b border-white/5">
                    <div className="relative max-w-md">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
                        <input
                            type="text"
                            placeholder="IP veya açıklama ara..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            className="w-full bg-[#1a1a1a] text-white pl-10 pr-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors"
                        />
                    </div>
                </div>

                {/* Table */}
                <div className="overflow-x-auto">
                    <table className="w-full text-left">
                        <thead className="bg-white/5 text-gray-400 uppercase text-xs font-bold">
                            <tr>
                                <th className="px-6 py-4">IP Adresi</th>
                                <th className="px-6 py-4">Açıklama</th>
                                <th className="px-6 py-4">Durum</th>
                                <th className="px-6 py-4">Eklenme Tarihi</th>
                                <th className="px-6 py-4 text-right">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-white/5">
                            {filteredIps.length > 0 ? (
                                filteredIps.map((ip) => (
                                    <tr key={ip.id} className="hover:bg-white/5 transition-colors">
                                        <td className="px-6 py-4 text-white font-mono font-medium">
                                            {ip.ipAddress}
                                        </td>
                                        <td className="px-6 py-4 text-gray-400 text-sm">
                                            {ip.description || "-"}
                                        </td>
                                        <td className="px-6 py-4">
                                            {ip.isActive ? (
                                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-500/10 text-green-500 border border-green-500/20">
                                                    <CheckCircle2 size={12} />
                                                    Aktif
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-500/10 text-red-500 border border-red-500/20">
                                                    <XCircle size={12} />
                                                    Pasif
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 text-gray-500 text-sm">
                                            {new Date(ip.createdAt).toLocaleDateString("tr-TR")}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <div className="flex items-center justify-end gap-2">
                                                <button
                                                    onClick={() => handleOpenModal(ip)}
                                                    className="p-2 hover:bg-blue-500/20 text-blue-400 rounded-lg transition-colors"
                                                    title="Düzenle"
                                                >
                                                    <Edit2 size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleDeleteClick(ip)}
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
                                    <td colSpan={5} className="px-6 py-8 text-center text-gray-500">
                                        Kayıt bulunamadı.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Edit/Create Modal */}
            {isModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-md shadow-2xl">
                        <div className="flex items-center justify-between p-6 border-b border-white/10">
                            <h2 className="text-xl font-bold text-white">
                                {editingIp ? "IP Adresini Düzenle" : "Yeni IP Ekle"}
                            </h2>
                            <button onClick={handleCloseModal} className="text-gray-400 hover:text-white transition-colors">
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
                                        onClick={handleCloseModal}
                                        className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
                                    >
                                        Kapat
                                    </button>
                                </div>
                            ) : (
                                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                                    {modalError && (
                                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 flex items-center gap-3 text-red-500">
                                            <AlertCircle size={20} />
                                            <p className="text-sm">{modalError}</p>
                                        </div>
                                    )}

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">IP Adresi</label>
                                        <input
                                            {...register("ipAddress")}
                                            className={cn(
                                                "w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors font-mono",
                                                errors.ipAddress && "border-red-500"
                                            )}
                                            placeholder="Örn: 192.168.1.1"
                                        />
                                        {errors.ipAddress && (
                                            <p className="text-sm text-red-500">{errors.ipAddress.message}</p>
                                        )}
                                    </div>

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">Açıklama (İsteğe Bağlı)</label>
                                        <input
                                            {...register("description")}
                                            className="w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors"
                                            placeholder="Örn: Ofis VPN"
                                        />
                                    </div>

                                    <div className="flex items-center gap-2">
                                        <input
                                            type="checkbox"
                                            id="isActive"
                                            {...register("isActive")}
                                            className="w-4 h-4 rounded border-gray-600 bg-[#121212] text-red-600 focus:ring-red-500 focus:ring-offset-gray-900"
                                        />
                                        <label htmlFor="isActive" className="text-sm font-medium text-gray-300 select-none">
                                            Aktif
                                        </label>
                                    </div>

                                    <div className="flex justify-end gap-3 pt-2">
                                        <button
                                            type="button"
                                            onClick={handleCloseModal}
                                            className="px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-lg transition-colors"
                                        >
                                            İptal
                                        </button>
                                        <button
                                            type="submit"
                                            disabled={actionLoading}
                                            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
                                        >
                                            {actionLoading && <Loader2 className="animate-spin" size={16} />}
                                            {editingIp ? "Güncelle" : "Oluştur"}
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
                title="IP Adresini Sil"
                message={`"${ipToDelete?.ipAddress}" adresini silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
                confirmText="Evet, Sil"
                variant="danger"
                isLoading={deleteLoading}
                successMessage={deleteSuccess}
                errorMessage={deleteError}
            />
        </div>
    );
}
