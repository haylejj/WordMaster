import { useState, useEffect } from "react";
import { roleService } from "@/services/role.service";
import { permissionService } from "@/services/permission.service";
import type { RoleResponse } from "@/types/role";
import type { PermissionResponse } from "@/types/permission";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Loader2, Shield, RefreshCw, Save, Check } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";

export default function AdminPermissionsPage() {
    const [roles, setRoles] = useState<RoleResponse[]>([]);
    const [permissions, setPermissions] = useState<PermissionResponse[]>([]);
    const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null);
    const [selectedPermissionIds, setSelectedPermissionIds] = useState<Set<string>>(new Set());
    const [isLoadingRoles, setIsLoadingRoles] = useState(false);
    const [isLoadingPermissions, setIsLoadingPermissions] = useState(false);
    const [isScanning, setIsScanning] = useState(false);
    const [isSaving, setIsSaving] = useState(false);

    useEffect(() => {
        loadRoles();
        loadAllPermissions();
    }, []);

    useEffect(() => {
        if (selectedRoleId) {
            loadRolePermissions(selectedRoleId);
        } else {
            setSelectedPermissionIds(new Set());
        }
    }, [selectedRoleId]);

    const loadRoles = async () => {
        setIsLoadingRoles(true);
        try {
            const result = await roleService.getRoles();
            if (result.isSuccess) {
                setRoles(result.data);
            } else {
                toast.error(result.errorList?.[0] || "Roller yüklenirken bir hata oluştu.");
            }
        } catch {
            toast.error("Roller yüklenirken bir hata oluştu.");
        } finally {
            setIsLoadingRoles(false);
        }
    };

    const loadAllPermissions = async () => {
        setIsLoadingPermissions(true);
        try {
            const result = await permissionService.getAll();
            if (result.isSuccess) {
                setPermissions(result.data);
            } else {
                toast.error(result.errorList?.[0] || "İzinler yüklenirken bir hata oluştu.");
            }
        } catch {
            toast.error("İzinler yüklenirken bir hata oluştu.");
        } finally {
            setIsLoadingPermissions(false);
        }
    };

    const loadRolePermissions = async (roleId: string) => {
        try {
            const result = await permissionService.getByRole(roleId);
            if (result.isSuccess) {
                setSelectedPermissionIds(new Set(result.data.map(p => p.id)));
            } else {
                toast.error(result.errorList?.[0] || "Rol izinleri yüklenirken bir hata oluştu.");
            }
        } catch {
            toast.error("Rol izinleri yüklenirken bir hata oluştu.");
        }
    };

    const handleScan = async () => {
        setIsScanning(true);
        try {
            const result = await permissionService.scan();
            if (result.isSuccess) {
                const { addedCount, deletedCount } = result.data;
                toast.success(`Tarama tamamlandı. Eklendi: ${addedCount}, Silindi: ${deletedCount}`);
                await loadAllPermissions();
                // If a role is selected, reload its permissions to remove any deleted ones from selection
                if (selectedRoleId) {
                    await loadRolePermissions(selectedRoleId);
                }
            } else {
                toast.error(result.errorList?.[0] || "İzin taraması başarısız oldu.");
            }
        } catch {
            toast.error("İzin taraması sırasında bir hata oluştu.");
        } finally {
            setIsScanning(false);
        }
    };

    const handleSave = async () => {
        if (!selectedRoleId) return;

        setIsSaving(true);
        try {
            const result = await permissionService.updateRolePermissions({
                roleId: selectedRoleId,
                permissionIds: Array.from(selectedPermissionIds)
            });
            if (result.isSuccess) {
                toast.success("İzinler başarıyla kaydedildi.");
            } else {
                toast.error(result.errorList?.[0] || "İzinler güncellenirken bir hata oluştu.");
            }
        } catch {
            toast.error("İzinler güncellenirken bir hata oluştu.");
        } finally {
            setIsSaving(false);
        }
    };

    const togglePermission = (permissionId: string) => {
        const newSet = new Set(selectedPermissionIds);
        if (newSet.has(permissionId)) {
            newSet.delete(permissionId);
        } else {
            newSet.add(permissionId);
        }
        setSelectedPermissionIds(newSet);
    };

    // Group permissions by Controller
    const groupedPermissions = permissions.reduce((acc, permission) => {
        const group = permission.controllerName;
        if (!acc[group]) {
            acc[group] = [];
        }
        acc[group].push(permission);
        return acc;
    }, {} as Record<string, PermissionResponse[]>);

    return (
        <div className="container mx-auto p-6 space-y-6 font-[Segoe_UI]">
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold flex items-center gap-3 text-white">
                    <Shield className="h-8 w-8 text-red-600" />
                    Yetkilendirme Yönetimi
                </h1>
                <Button
                    onClick={handleScan}
                    disabled={isScanning}
                    className="bg-red-600 hover:bg-red-700 text-white border-none shadow-[0_0_15px_rgba(220,38,38,0.5)] transition-all duration-300"
                >
                    {isScanning ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <RefreshCw className="mr-2 h-4 w-4" />}
                    İzinleri Tara
                </Button>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
                {/* Roles List */}
                <Card className="md:col-span-1 bg-[#0a0a0a] border-red-900/20 shadow-lg">
                    <CardHeader className="border-b border-red-900/10">
                        <CardTitle className="text-gray-200">Roller</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-2 pt-6">
                        {isLoadingRoles ? (
                            <div className="flex justify-center p-4">
                                <Loader2 className="h-6 w-6 animate-spin text-red-500" />
                            </div>
                        ) : (
                            roles.map(role => (
                                <div
                                    key={role.id}
                                    onClick={() => setSelectedRoleId(role.id)}
                                    className={cn(
                                        "p-3 rounded-lg cursor-pointer transition-all duration-200 border",
                                        selectedRoleId === role.id
                                            ? "bg-red-900/20 text-red-500 border-red-900/50 shadow-[0_0_10px_rgba(220,38,38,0.1)]"
                                            : "border-transparent text-gray-400 hover:bg-white/5 hover:text-white"
                                    )}
                                >
                                    <div className="font-medium flex items-center justify-between">
                                        {role.name}
                                        {selectedRoleId === role.id && <Check className="h-4 w-4" />}
                                    </div>
                                </div>
                            ))
                        )}
                    </CardContent>
                </Card>

                {/* Permissions List */}
                <Card className="md:col-span-3 bg-[#0a0a0a] border-red-900/20 shadow-lg">
                    <CardHeader className="flex flex-row justify-between items-center border-b border-red-900/10">
                        <CardTitle className="text-gray-200">
                            {selectedRoleId
                                ? `${roles.find(r => r.id === selectedRoleId)?.name} Rolü İzinleri`
                                : "İzinleri yönetmek için bir rol seçin"
                            }
                        </CardTitle>
                        {selectedRoleId && (
                            <Button
                                onClick={handleSave}
                                disabled={isSaving}
                                className="bg-white text-black hover:bg-gray-200 border-none"
                            >
                                {isSaving ? <Loader2 className="mr-2 h-4 w-4 animate-spin" /> : <Save className="mr-2 h-4 w-4" />}
                                Değişiklikleri Kaydet
                            </Button>
                        )}
                    </CardHeader>
                    <CardContent className="pt-6">
                        {isLoadingPermissions ? (
                            <div className="flex justify-center p-8">
                                <Loader2 className="h-8 w-8 animate-spin text-red-500" />
                            </div>
                        ) : !selectedRoleId ? (
                            <div className="text-center text-gray-500 p-12 flex flex-col items-center gap-4">
                                <Shield className="h-16 w-16 opacity-20" />
                                <p>Sol menüden bir rol seçerek izinleri görüntüleyebilir ve düzenleyebilirsiniz.</p>
                            </div>
                        ) : (
                            <div className="space-y-6">
                                {Object.entries(groupedPermissions).map(([controller, perms]) => (
                                    <div key={controller} className="border border-red-900/10 rounded-xl p-5 bg-white/5 hover:border-red-900/30 transition-colors">
                                        <h3 className="font-semibold text-lg mb-4 text-red-400 flex items-center gap-2">
                                            <div className="w-1.5 h-6 bg-red-600 rounded-full"></div>
                                            {controller}
                                        </h3>
                                        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                            {perms.map(permission => (
                                                <div
                                                    key={permission.id}
                                                    className={cn(
                                                        "flex items-start space-x-3 p-3 rounded-lg transition-all duration-200 cursor-pointer border",
                                                        selectedPermissionIds.has(permission.id)
                                                            ? "bg-red-900/10 border-red-900/30"
                                                            : "bg-black/20 border-transparent hover:bg-white/5"
                                                    )}
                                                    onClick={() => togglePermission(permission.id)}
                                                >
                                                    <div className={cn(
                                                        "h-5 w-5 rounded border flex items-center justify-center mt-0.5 transition-colors",
                                                        selectedPermissionIds.has(permission.id)
                                                            ? "bg-red-600 border-red-600 text-white"
                                                            : "border-gray-600 bg-transparent"
                                                    )}>
                                                        {selectedPermissionIds.has(permission.id) && <Check className="h-3.5 w-3.5" />}
                                                    </div>
                                                    <div className="grid gap-1 leading-none flex-1">
                                                        <label className="text-sm font-medium text-gray-200 cursor-pointer">
                                                            {permission.actionName}
                                                            <span className={cn(
                                                                "ml-2 text-[10px] px-1.5 py-0.5 rounded uppercase tracking-wider font-bold",
                                                                permission.httpMethod === "GET" ? "bg-blue-500/20 text-blue-400" :
                                                                    permission.httpMethod === "POST" ? "bg-green-500/20 text-green-400" :
                                                                        permission.httpMethod === "PUT" ? "bg-yellow-500/20 text-yellow-400" :
                                                                            permission.httpMethod === "DELETE" ? "bg-red-500/20 text-red-400" :
                                                                                "bg-gray-500/20 text-gray-400"
                                                            )}>
                                                                {permission.httpMethod}
                                                            </span>
                                                        </label>
                                                        <p className="text-xs text-gray-500 line-clamp-2">
                                                            {permission.description}
                                                        </p>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </CardContent>
                </Card>
            </div>
        </div>
    );
}
