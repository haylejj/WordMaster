import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { roleService } from "@/services/role.service";
import type { RoleResponse } from "@/types/role";
import {
    Search,
    Plus,
    Edit2,
    Trash2,
    X,
    Shield,
    Loader2,
    CheckCircle2,
    AlertCircle
} from "lucide-react";
import { cn } from "@/lib/utils";
import ConfirmationModal from "@/components/ui/ConfirmationModal";

const roleSchema = z.object({
    name: z.string().min(1, "Rol adı zorunludur"),
});

type RoleFormData = z.infer<typeof roleSchema>;

export default function AdminRolesPage() {
    const [roles, setRoles] = useState<RoleResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");

    // Edit/Create Modal State
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingRole, setEditingRole] = useState<RoleResponse | null>(null);
    const [actionLoading, setActionLoading] = useState(false);
    const [modalSuccess, setModalSuccess] = useState<string | null>(null);
    const [modalError, setModalError] = useState<string | null>(null);

    // Delete Modal State
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [roleToDelete, setRoleToDelete] = useState<RoleResponse | null>(null);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [deleteSuccess, setDeleteSuccess] = useState<string | null>(null);
    const [deleteError, setDeleteError] = useState<string | null>(null);

    const { register, handleSubmit, reset, setValue, formState: { errors } } = useForm<RoleFormData>({
        resolver: zodResolver(roleSchema)
    });

    const fetchRoles = async () => {
        try {
            const result = await roleService.getRoles();
            if (result.isSuccess) {
                setRoles(result.data);
            }
        } catch (error) {
            console.error("Roller yüklenirken hata oluştu", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRoles();
    }, []);

    const filteredRoles = roles.filter(role =>
        role.name.toLowerCase().includes(searchQuery.toLowerCase())
    );

    const handleOpenModal = (role?: RoleResponse) => {
        setModalSuccess(null);
        setModalError(null);
        if (role) {
            setEditingRole(role);
            setValue("name", role.name);
        } else {
            setEditingRole(null);
            reset();
        }
        setIsModalOpen(true);
    };

    const handleCloseModal = () => {
        setIsModalOpen(false);
        setEditingRole(null);
        reset();
        setModalSuccess(null);
        setModalError(null);
    };

    const onSubmit = async (data: RoleFormData) => {
        setActionLoading(true);
        setModalSuccess(null);
        setModalError(null);
        try {
            if (editingRole) {
                const result = await roleService.updateRole({ id: editingRole.id, name: data.name });
                if (result.isSuccess) {
                    setModalSuccess("Rol başarıyla güncellendi.");
                    fetchRoles();
                } else {
                    setModalError(result.errorList?.[0] || "Güncelleme başarısız.");
                }
            } else {
                const result = await roleService.createRole({ name: data.name });
                if (result.isSuccess) {
                    setModalSuccess("Rol başarıyla oluşturuldu.");
                    fetchRoles();
                    reset();
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

    const handleDeleteClick = (role: RoleResponse) => {
        setRoleToDelete(role);
        setDeleteSuccess(null);
        setDeleteError(null);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!roleToDelete) return;

        setDeleteLoading(true);
        setDeleteSuccess(null);
        setDeleteError(null);

        try {
            const result = await roleService.deleteRole(roleToDelete.id);
            if (result.isSuccess) {
                setDeleteSuccess("Rol başarıyla silindi.");
                fetchRoles();
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
        setRoleToDelete(null);
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
                        <Shield className="text-red-500" />
                        Rol Yönetimi
                    </h1>
                    <p className="text-gray-400">Sistemdeki kullanıcı rollerini yönetin.</p>
                </div>
                <button
                    onClick={() => handleOpenModal()}
                    className="flex items-center gap-2 px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors"
                >
                    <Plus size={20} />
                    Yeni Rol Ekle
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
                            placeholder="Rol ara..."
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
                                <th className="px-6 py-4">Rol Adı</th>
                                <th className="px-6 py-4">ID</th>
                                <th className="px-6 py-4 text-right">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-white/5">
                            {filteredRoles.length > 0 ? (
                                filteredRoles.map((role) => (
                                    <tr key={role.id} className="hover:bg-white/5 transition-colors">
                                        <td className="px-6 py-4 text-white font-medium">
                                            <div className="flex items-center gap-2">
                                                <div className="w-2 h-2 rounded-full bg-red-500"></div>
                                                {role.name}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-gray-500 font-mono text-sm">{role.id}</td>
                                        <td className="px-6 py-4 text-right">
                                            <div className="flex items-center justify-end gap-2">
                                                <button
                                                    onClick={() => handleOpenModal(role)}
                                                    className="p-2 hover:bg-blue-500/20 text-blue-400 rounded-lg transition-colors"
                                                    title="Düzenle"
                                                >
                                                    <Edit2 size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleDeleteClick(role)}
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
                                    <td colSpan={3} className="px-6 py-8 text-center text-gray-500">
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
                                {editingRole ? "Rolü Düzenle" : "Yeni Rol Ekle"}
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
                                        <label className="text-sm font-medium text-gray-300">Rol Adı</label>
                                        <input
                                            {...register("name")}
                                            className={cn(
                                                "w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors",
                                                errors.name && "border-red-500"
                                            )}
                                            placeholder="Örn: Editor"
                                        />
                                        {errors.name && (
                                            <p className="text-sm text-red-500">{errors.name.message}</p>
                                        )}
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
                                            {editingRole ? "Güncelle" : "Oluştur"}
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
                title="Rolü Sil"
                message={`"${roleToDelete?.name}" rolünü silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
                confirmText="Evet, Sil"
                variant="danger"
                isLoading={deleteLoading}
                successMessage={deleteSuccess}
                errorMessage={deleteError}
            />
        </div>
    );
}
