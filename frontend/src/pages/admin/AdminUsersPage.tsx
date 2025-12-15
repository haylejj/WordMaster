import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { userService } from "@/services/user.service";
import { roleService } from "@/services/role.service";
import type { UserWithRolesResponse, UserDetailResponse } from "@/types/user";
import type { RoleResponse } from "@/types/role";
import {
    Search,
    Edit2,
    Trash2,
    X,
    Users,
    Loader2,
    CheckCircle2,
    AlertCircle,
    Eye,
    KeyRound,
    Mail,
    Phone,
    User,
    Activity,
    BookOpen,
    Star,
    HelpCircle,
    Lock,
    Unlock
} from "lucide-react";
import { cn } from "@/lib/utils";
import ConfirmationModal from "@/components/ui/ConfirmationModal";

// Schema for User Update
const userSchema = z.object({
    userName: z.string().min(3, "Kullanıcı adı en az 3 karakter olmalıdır"),
    email: z.string().email("Geçerli bir e-posta adresi giriniz"),
    phone: z.string().optional(),
    firstName: z.string().max(100, "Ad en fazla 100 karakter olabilir").optional(),
    lastName: z.string().max(50, "Soyad en fazla 50 karakter olabilir").optional()
});

type UserFormData = z.infer<typeof userSchema>;

export default function AdminUsersPage() {
    // List State
    const [users, setUsers] = useState<UserWithRolesResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);

    // Detail Modal State
    const [detailModalOpen, setDetailModalOpen] = useState(false);
    const [selectedUserDetail, setSelectedUserDetail] = useState<UserDetailResponse | null>(null);
    const [detailLoading, setDetailLoading] = useState(false);

    // Edit Modal State
    const [editModalOpen, setEditModalOpen] = useState(false);
    const [editingUser, setEditingUser] = useState<UserWithRolesResponse | null>(null);
    const [actionLoading, setActionLoading] = useState(false);
    const [modalSuccess, setModalSuccess] = useState<string | null>(null);
    const [modalError, setModalError] = useState<string | null>(null);

    // Delete Modal State
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [userToDelete, setUserToDelete] = useState<UserWithRolesResponse | null>(null);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [deleteSuccess, setDeleteSuccess] = useState<string | null>(null);
    const [deleteError, setDeleteError] = useState<string | null>(null);

    // Reset Password Modal State
    const [resetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
    const [userToReset, setUserToReset] = useState<UserWithRolesResponse | null>(null);
    const [resetLoading, setResetLoading] = useState(false);
    const [resetSuccess, setResetSuccess] = useState<string | null>(null);
    const [resetError, setResetError] = useState<string | null>(null);

    // Role Modal State
    const [roleModalOpen, setRoleModalOpen] = useState(false);
    const [userToChangeRole, setUserToChangeRole] = useState<UserWithRolesResponse | null>(null);
    const [availableRoles, setAvailableRoles] = useState<RoleResponse[]>([]);
    const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
    const [roleLoading, setRoleLoading] = useState(false);
    const [roleSuccess, setRoleSuccess] = useState<string | null>(null);
    const [roleError, setRoleError] = useState<string | null>(null);

    const { register, handleSubmit, reset, setValue, formState: { errors } } = useForm<UserFormData>({
        resolver: zodResolver(userSchema)
    });

    const fetchUsers = async (pageNum: number = 1, search: string = "") => {
        setLoading(true);
        try {
            const result = await userService.getPagedUsers(pageNum, 10, search);
            if (result.isSuccess) {
                setUsers(result.data.items);
                setTotalPages(result.data.totalPages);
                setTotalCount(result.data.totalCount);
                setPage(result.data.pageNumber);
            }
        } catch (error) {
            console.error("Kullanıcılar yüklenirken hata oluştu", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const timeoutId = setTimeout(() => {
            fetchUsers(1, searchQuery);
        }, 500);
        return () => clearTimeout(timeoutId);
    }, [searchQuery]);

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            fetchUsers(newPage, searchQuery);
        }
    };

    // Detail Modal Handlers
    const handleOpenDetailModal = async (userId: string) => {
        setDetailLoading(true);
        setDetailModalOpen(true);
        try {
            const result = await userService.getUserDetail(userId);
            if (result.isSuccess) {
                setSelectedUserDetail(result.data);
            }
        } catch (error) {
            console.error("Kullanıcı detayları alınamadı", error);
        } finally {
            setDetailLoading(false);
        }
    };

    const handleCloseDetailModal = () => {
        setDetailModalOpen(false);
        setSelectedUserDetail(null);
    };

    // Edit Modal Handlers
    const handleOpenEditModal = async (user: UserWithRolesResponse) => {
        setModalSuccess(null);
        setModalError(null);
        setEditingUser(user);
        setEditModalOpen(true);

        // Fetch detailed info to populate form properly
        try {
            const result = await userService.getUserDetail(user.id);
            if (result.isSuccess) {
                const detail = result.data;
                setValue("userName", detail.userName);
                setValue("email", detail.email);
                setValue("phone", detail.phone || "");
                setValue("firstName", detail.firstName || "");
                setValue("lastName", detail.lastName || "");
            }
        } catch (error) {
            console.error("Kullanıcı detayları alınamadı", error);
        }
    };

    const handleCloseEditModal = () => {
        setEditModalOpen(false);
        setEditingUser(null);
        reset();
        setModalSuccess(null);
        setModalError(null);
    };

    const onSubmitEdit = async (data: UserFormData) => {
        if (!editingUser) return;

        setActionLoading(true);
        setModalSuccess(null);
        setModalError(null);

        try {
            const result = await userService.updateUser({
                id: editingUser.id,
                userName: data.userName,
                email: data.email,
                phone: data.phone,
                firstName: data.firstName || undefined,
                lastName: data.lastName || undefined
            });

            if (result.isSuccess) {
                setModalSuccess("Kullanıcı bilgileri başarıyla güncellendi.");
                fetchUsers(page, searchQuery);
            } else {
                setModalError(result.errorList?.[0] || "Güncelleme başarısız.");
            }
        } catch (error) {
            setModalError("Bir hata oluştu.");
            console.error("İşlem başarısız", error);
        } finally {
            setActionLoading(false);
        }
    };

    // Delete Modal Handlers
    const handleDeleteClick = (user: UserWithRolesResponse) => {
        setUserToDelete(user);
        setDeleteSuccess(null);
        setDeleteError(null);
        setDeleteModalOpen(true);
    };

    const handleConfirmDelete = async () => {
        if (!userToDelete) return;

        setDeleteLoading(true);
        setDeleteSuccess(null);
        setDeleteError(null);

        try {
            const result = await userService.deleteUser(userToDelete.id);
            if (result.isSuccess) {
                setDeleteSuccess("Kullanıcı başarıyla silindi.");
                fetchUsers(page, searchQuery);
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
        setUserToDelete(null);
        setDeleteSuccess(null);
        setDeleteError(null);
    };

    // Reset Password Modal Handlers
    const handleResetPasswordClick = (user: UserWithRolesResponse) => {
        setUserToReset(user);
        setResetSuccess(null);
        setResetError(null);
        setResetPasswordModalOpen(true);
    };

    const handleConfirmResetPassword = async () => {
        if (!userToReset) return;

        setResetLoading(true);
        setResetSuccess(null);
        setResetError(null);

        try {
            const result = await userService.resetPassword(userToReset.id);
            if (result.isSuccess) {
                setResetSuccess(`Şifre başarıyla sıfırlandı. Yeni şifre: ${result.data}`);
            } else {
                setResetError(result.errorList?.[0] || "Şifre sıfırlama başarısız.");
            }
        } catch (error) {
            setResetError("İşlem sırasında bir hata oluştu.");
            console.error("Şifre sıfırlama başarısız", error);
        } finally {
            setResetLoading(false);
        }
    };

    const handleCloseResetModal = () => {
        setResetPasswordModalOpen(false);
        setUserToReset(null);
        setResetSuccess(null);
        setResetError(null);
    };

    // Role Modal Handlers
    const handleOpenRoleModal = async (user: UserWithRolesResponse) => {
        setUserToChangeRole(user);
        setSelectedRoles(user.roles);
        setRoleSuccess(null);
        setRoleError(null);
        setRoleModalOpen(true);

        // Fetch roles if not already fetched
        if (availableRoles.length === 0) {
            try {
                const result = await roleService.getRoles();
                if (result.isSuccess) {
                    setAvailableRoles(result.data);
                }
            } catch (error) {
                console.error("Roller yüklenemedi", error);
            }
        }
    };

    const handleCloseRoleModal = () => {
        setRoleModalOpen(false);
        setUserToChangeRole(null);
        setSelectedRoles([]);
        setRoleSuccess(null);
        setRoleError(null);
    };

    const handleRoleToggle = (roleName: string) => {
        if (selectedRoles.includes(roleName)) {
            setSelectedRoles(selectedRoles.filter(r => r !== roleName));
        } else {
            setSelectedRoles([...selectedRoles, roleName]);
        }
    };

    const handleConfirmRoleChange = async () => {
        if (!userToChangeRole) return;

        setRoleLoading(true);
        setRoleSuccess(null);
        setRoleError(null);

        try {
            const result = await userService.changeUserRole(userToChangeRole.id, selectedRoles);
            if (result.isSuccess) {
                setRoleSuccess("Kullanıcı rolleri başarıyla güncellendi.");
                fetchUsers(page, searchQuery);
            } else {
                setRoleError(result.errorList?.[0] || "Rol güncelleme başarısız.");
            }
        } catch (error) {
            setRoleError("İşlem sırasında bir hata oluştu.");
            console.error("Rol güncelleme başarısız", error);
        } finally {
            setRoleLoading(false);
        }
    };



    return (
        <div className="space-y-6 animate-in fade-in duration-500">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-white mb-2 flex items-center gap-2">
                        <Users className="text-red-500" />
                        Kullanıcı Yönetimi
                    </h1>
                    <p className="text-gray-400">Sistemdeki kullanıcıları yönetin ve detaylarını görüntüleyin.</p>
                </div>
            </div>

            {/* Search and Table Container */}
            <div className="bg-[#121212] border border-white/5 rounded-xl overflow-hidden">
                {/* Search Bar */}
                <div className="p-4 border-b border-white/5">
                    <div className="relative max-w-md">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
                        <input
                            type="text"
                            placeholder="Kullanıcı adı veya e-posta ara..."
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
                                <th className="px-6 py-4">ID</th>
                                <th className="px-6 py-4">Kullanıcı Adı</th>
                                <th className="px-6 py-4">E-Posta</th>
                                <th className="px-6 py-4">Roller</th>
                                <th className="px-6 py-4">Durum</th>
                                <th className="px-6 py-4 text-right">İşlemler</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-white/5">
                            {loading ? (
                                <tr>
                                    <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                                        <div className="flex justify-center items-center gap-2">
                                            <Loader2 className="animate-spin" size={20} />
                                            Yükleniyor...
                                        </div>
                                    </td>
                                </tr>
                            ) : users.length > 0 ? (
                                users.map((user) => (
                                    <tr key={user.id} className="hover:bg-white/5 transition-colors">
                                        <td className="px-6 py-4 text-gray-500 font-mono text-xs max-w-[100px] truncate" title={user.id}>
                                            {user.id}
                                        </td>
                                        <td className="px-6 py-4 text-white font-medium">
                                            <div className="flex items-center gap-2">
                                                <div className="w-8 h-8 rounded-full bg-red-600 flex items-center justify-center text-xs font-bold">
                                                    {user.userName.substring(0, 2).toUpperCase()}
                                                </div>
                                                {user.userName}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 text-gray-400 text-sm">{user.email}</td>
                                        <td className="px-6 py-4">
                                            <div className="flex flex-wrap gap-1">
                                                {user.roles.map((role, idx) => (
                                                    <span key={idx} className="px-2 py-0.5 rounded-full text-xs font-medium bg-blue-500/10 text-blue-400 border border-blue-500/20">
                                                        {role}
                                                    </span>
                                                ))}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            {user.isLockedOut ? (
                                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-500/10 text-red-500 border border-red-500/20">
                                                    <Lock size={12} />
                                                    Kilitli
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-500/10 text-green-500 border border-green-500/20">
                                                    <Unlock size={12} />
                                                    Aktif
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 text-right">
                                            <div className="flex items-center justify-end gap-2">
                                                <button
                                                    onClick={() => handleOpenDetailModal(user.id)}
                                                    className="p-2 hover:bg-emerald-500/20 text-emerald-400 rounded-lg transition-colors"
                                                    title="Detaylar"
                                                >
                                                    <Eye size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleOpenEditModal(user)}
                                                    className="p-2 hover:bg-blue-500/20 text-blue-400 rounded-lg transition-colors"
                                                    title="Düzenle"
                                                >
                                                    <Edit2 size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleOpenRoleModal(user)}
                                                    className="p-2 hover:bg-red-500/20 text-red-400 rounded-lg transition-colors"
                                                    title="Rolleri Düzenle"
                                                >
                                                    <Users size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleResetPasswordClick(user)}
                                                    className="p-2 hover:bg-yellow-500/20 text-yellow-400 rounded-lg transition-colors"
                                                    title="Şifre Sıfırla"
                                                >
                                                    <KeyRound size={18} />
                                                </button>
                                                <button
                                                    onClick={() => handleDeleteClick(user)}
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
                                    <td colSpan={6} className="px-6 py-8 text-center text-gray-500">
                                        Kayıt bulunamadı.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>

                {/* Pagination */}
                <div className="p-4 border-t border-white/5 flex items-center justify-between">
                    <span className="text-sm text-gray-500">
                        Toplam {totalCount} kayıt, Sayfa {page} / {totalPages}
                    </span>
                    <div className="flex gap-2">
                        <button
                            onClick={() => handlePageChange(page - 1)}
                            disabled={page === 1}
                            className="px-3 py-1 bg-white/5 hover:bg-white/10 text-white rounded-lg disabled:opacity-50 transition-colors"
                        >
                            Önceki
                        </button>
                        <button
                            onClick={() => handlePageChange(page + 1)}
                            disabled={page === totalPages}
                            className="px-3 py-1 bg-white/5 hover:bg-white/10 text-white rounded-lg disabled:opacity-50 transition-colors"
                        >
                            Sonraki
                        </button>
                    </div>
                </div>
            </div>

            {/* Detail Modal */}
            {detailModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-2xl shadow-2xl max-h-[90vh] overflow-y-auto">
                        <div className="flex items-center justify-between p-6 border-b border-white/10 sticky top-0 bg-[#1a1a1a] z-10">
                            <h2 className="text-xl font-bold text-white flex items-center gap-2">
                                <User className="text-emerald-500" />
                                Kullanıcı Detayları
                            </h2>
                            <button onClick={handleCloseDetailModal} className="text-gray-400 hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6">
                            {detailLoading ? (
                                <div className="flex justify-center py-12">
                                    <Loader2 className="animate-spin text-emerald-500" size={48} />
                                </div>
                            ) : selectedUserDetail ? (
                                <div className="space-y-8">
                                    {/* Personal Info */}
                                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                                        <div className="space-y-4">
                                            <h3 className="text-sm font-bold text-gray-500 uppercase tracking-wider border-b border-white/5 pb-2">Kişisel Bilgiler</h3>
                                            <div className="space-y-3">
                                                <div className="flex items-center gap-3 text-gray-300">
                                                    <User size={18} className="text-gray-500" />
                                                    <span className="font-medium text-white">{selectedUserDetail.userName}</span>
                                                </div>
                                                <div className="flex items-center gap-3 text-gray-300">
                                                    <Mail size={18} className="text-gray-500" />
                                                    <span>{selectedUserDetail.email}</span>
                                                </div>
                                                <div className="flex items-center gap-3 text-gray-300">
                                                    <Phone size={18} className="text-gray-500" />
                                                    <span>{selectedUserDetail.phone || "-"}</span>
                                                </div>
                                                <div className="flex items-center gap-3 text-gray-300">
                                                    <User size={18} className="text-gray-500" />
                                                    <span>
                                                        {selectedUserDetail.firstName && selectedUserDetail.lastName
                                                            ? `${selectedUserDetail.firstName} ${selectedUserDetail.lastName}`
                                                            : selectedUserDetail.firstName || selectedUserDetail.lastName || "-"}
                                                    </span>
                                                </div>
                                            </div>
                                        </div>

                                        {/* Activity Stats */}
                                        <div className="space-y-4">
                                            <h3 className="text-sm font-bold text-gray-500 uppercase tracking-wider border-b border-white/5 pb-2">Aktivite</h3>
                                            <div className="space-y-3">
                                                <div className="flex justify-between items-center">
                                                    <span className="text-gray-400 text-sm">Son Giriş</span>
                                                    <span className="text-white font-medium">
                                                        {selectedUserDetail.lastLoginDate
                                                            ? new Date(selectedUserDetail.lastLoginDate).toLocaleString("tr-TR")
                                                            : "-"}
                                                    </span>
                                                </div>
                                                <div className="flex justify-between items-center">
                                                    <span className="text-gray-400 text-sm">Son IP</span>
                                                    <span className="text-white font-mono text-sm">
                                                        {selectedUserDetail.lastLoginIpAddress || "-"}
                                                    </span>
                                                </div>
                                                <div className="flex justify-between items-center">
                                                    <span className="text-gray-400 text-sm">Son Pratik</span>
                                                    <span className="text-white font-medium">
                                                        {selectedUserDetail.lastPracticeDate
                                                            ? new Date(selectedUserDetail.lastPracticeDate).toLocaleString("tr-TR")
                                                            : "-"}
                                                    </span>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                    {/* Stats Cards */}
                                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                        <div className="bg-blue-500/10 border border-blue-500/20 rounded-xl p-4 text-center">
                                            <BookOpen className="w-6 h-6 text-blue-500 mx-auto mb-2" />
                                            <div className="text-2xl font-bold text-white">{selectedUserDetail.wordCount}</div>
                                            <div className="text-xs text-blue-400 font-medium uppercase">Kelime</div>
                                        </div>
                                        <div className="bg-yellow-500/10 border border-yellow-500/20 rounded-xl p-4 text-center">
                                            <Star className="w-6 h-6 text-yellow-500 mx-auto mb-2" />
                                            <div className="text-2xl font-bold text-white">{selectedUserDetail.favoriteCount}</div>
                                            <div className="text-xs text-yellow-400 font-medium uppercase">Favori</div>
                                        </div>
                                        <div className="bg-orange-500/10 border border-orange-500/20 rounded-xl p-4 text-center">
                                            <HelpCircle className="w-6 h-6 text-orange-500 mx-auto mb-2" />
                                            <div className="text-2xl font-bold text-white">{selectedUserDetail.unknowsCount}</div>
                                            <div className="text-xs text-orange-400 font-medium uppercase">Bilinmeyen</div>
                                        </div>
                                        <div className="bg-purple-500/10 border border-purple-500/20 rounded-xl p-4 text-center">
                                            <Activity className="w-6 h-6 text-purple-500 mx-auto mb-2" />
                                            <div className="text-2xl font-bold text-white">{selectedUserDetail.totalLoginAttempts}</div>
                                            <div className="text-xs text-purple-400 font-medium uppercase">Giriş</div>
                                        </div>
                                    </div>
                                </div>
                            ) : (
                                <div className="text-center text-gray-500">Detay bulunamadı.</div>
                            )}
                        </div>
                    </div>
                </div>
            )}

            {/* Edit Modal */}
            {editModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-md shadow-2xl">
                        <div className="flex items-center justify-between p-6 border-b border-white/10">
                            <h2 className="text-xl font-bold text-white">Kullanıcı Düzenle</h2>
                            <button onClick={handleCloseEditModal} className="text-gray-400 hover:text-white transition-colors">
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
                                        onClick={handleCloseEditModal}
                                        className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
                                    >
                                        Kapat
                                    </button>
                                </div>
                            ) : (
                                <form onSubmit={handleSubmit(onSubmitEdit)} className="space-y-4">
                                    {modalError && (
                                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 flex items-center gap-3 text-red-500">
                                            <AlertCircle size={20} />
                                            <p className="text-sm">{modalError}</p>
                                        </div>
                                    )}

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">Kullanıcı Adı</label>
                                        <input
                                            {...register("userName")}
                                            className={cn(
                                                "w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors",
                                                errors.userName && "border-red-500"
                                            )}
                                        />
                                        {errors.userName && <p className="text-sm text-red-500">{errors.userName.message}</p>}
                                    </div>

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">E-Posta</label>
                                        <input
                                            {...register("email")}
                                            className={cn(
                                                "w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors",
                                                errors.email && "border-red-500"
                                            )}
                                        />
                                        {errors.email && <p className="text-sm text-red-500">{errors.email.message}</p>}
                                    </div>

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">Telefon</label>
                                        <input
                                            {...register("phone")}
                                            className="w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors"
                                        />
                                    </div>

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">Ad</label>
                                        <input
                                            {...register("firstName")}
                                            className="w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors"
                                            placeholder="Ad"
                                        />
                                    </div>

                                    <div className="space-y-2">
                                        <label className="text-sm font-medium text-gray-300">Soyad</label>
                                        <input
                                            {...register("lastName")}
                                            className="w-full bg-[#121212] text-white px-4 py-2 rounded-lg border border-white/10 focus:border-red-500 focus:outline-none transition-colors"
                                            placeholder="Soyad"
                                        />
                                    </div>

                                    <div className="flex justify-end gap-3 pt-2">
                                        <button
                                            type="button"
                                            onClick={handleCloseEditModal}
                                            className="px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-lg transition-colors"
                                        >
                                            İptal
                                        </button>
                                        <button
                                            type="submit"
                                            disabled={actionLoading}
                                            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
                                        >
                                            {actionLoading && <Loader2 className="animate-spin" size={16} />}
                                            Güncelle
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
                title="Kullanıcıyı Sil"
                message={`"${userToDelete?.userName}" kullanıcısını silmek istediğinize emin misiniz? Bu işlem geri alınamaz.`}
                confirmText="Evet, Sil"
                variant="danger"
                isLoading={deleteLoading}
                successMessage={deleteSuccess}
                errorMessage={deleteError}
            />

            {/* Reset Password Confirmation Modal */}
            <ConfirmationModal
                isOpen={resetPasswordModalOpen}
                onClose={handleCloseResetModal}
                onConfirm={handleConfirmResetPassword}
                title="Şifre Sıfırla"
                message={`"${userToReset?.userName}" kullanıcısının şifresini sıfırlamak istediğinize emin misiniz? Yeni şifre e-posta olarak gönderilecektir.`}
                confirmText="Sıfırla"
                variant="warning"
                isLoading={resetLoading}
                successMessage={resetSuccess}
                errorMessage={resetError}
            />

            {/* Role Change Modal */}
            {roleModalOpen && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-md shadow-2xl">
                        <div className="flex items-center justify-between p-6 border-b border-white/10">
                            <h2 className="text-xl font-bold text-white flex items-center gap-2">
                                <Users className="text-red-500" />
                                Rolleri Düzenle
                            </h2>
                            <button onClick={handleCloseRoleModal} className="text-gray-400 hover:text-white transition-colors">
                                <X size={24} />
                            </button>
                        </div>

                        <div className="p-6">
                            {roleSuccess ? (
                                <div className="flex flex-col items-center justify-center py-4 space-y-4">
                                    <div className="w-16 h-16 bg-green-500/10 rounded-full flex items-center justify-center text-green-500">
                                        <CheckCircle2 size={32} />
                                    </div>
                                    <p className="text-green-500 font-medium text-center">{roleSuccess}</p>
                                    <button
                                        onClick={handleCloseRoleModal}
                                        className="px-6 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg transition-colors"
                                    >
                                        Kapat
                                    </button>
                                </div>
                            ) : (
                                <div className="space-y-6">
                                    {roleError && (
                                        <div className="bg-red-500/10 border border-red-500/20 rounded-lg p-4 flex items-center gap-3 text-red-500">
                                            <AlertCircle size={20} />
                                            <p className="text-sm">{roleError}</p>
                                        </div>
                                    )}

                                    <div className="space-y-3">
                                        <p className="text-sm text-gray-400">
                                            <span className="text-white font-medium">{userToChangeRole?.userName}</span> kullanıcısı için rolleri seçin:
                                        </p>

                                        <div className="space-y-2 max-h-60 overflow-y-auto pr-2">
                                            {availableRoles.map((role) => (
                                                <label
                                                    key={role.id}
                                                    className={`flex items-center justify-between p-3 rounded-lg border cursor-pointer transition-all ${selectedRoles.includes(role.name)
                                                        ? "bg-red-500/10 border-red-500/50"
                                                        : "bg-[#121212] border-white/10 hover:border-white/20"
                                                        }`}
                                                    onClick={() => handleRoleToggle(role.name)}
                                                >
                                                    <div className="flex items-center gap-3">
                                                        <div className={`w-5 h-5 rounded border flex items-center justify-center transition-colors ${selectedRoles.includes(role.name)
                                                            ? "bg-red-500 border-red-500"
                                                            : "border-gray-500"
                                                            }`}>
                                                            {selectedRoles.includes(role.name) && <CheckCircle2 size={14} className="text-white" />}
                                                        </div>
                                                        <span className={selectedRoles.includes(role.name) ? "text-white" : "text-gray-400"}>
                                                            {role.name}
                                                        </span>
                                                    </div>
                                                </label>
                                            ))}

                                            {availableRoles.length === 0 && (
                                                <div className="text-center py-4 text-gray-500">
                                                    Yüklü rol bulunamadı.
                                                </div>
                                            )}
                                        </div>
                                    </div>

                                    <div className="flex justify-end gap-3 pt-2">
                                        <button
                                            onClick={handleCloseRoleModal}
                                            className="px-4 py-2 bg-white/5 hover:bg-white/10 text-white rounded-lg transition-colors"
                                        >
                                            İptal
                                        </button>
                                        <button
                                            onClick={handleConfirmRoleChange}
                                            disabled={roleLoading}
                                            className="px-4 py-2 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors disabled:opacity-50 flex items-center gap-2"
                                        >
                                            {roleLoading && <Loader2 className="animate-spin" size={16} />}
                                            Kaydet
                                        </button>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
