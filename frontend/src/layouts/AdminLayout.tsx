import { useEffect, useState } from "react";
import { Outlet, Link, useLocation, useNavigate } from "react-router-dom";
import { cn } from "@/lib/utils";
import {
    LayoutDashboard,
    Users,
    Shield,
    Network,
    LogOut,
    ExternalLink,
    Menu,
    BookOpen,
    Lock,
    Database,
    Activity,
    Loader2
} from "lucide-react";
import { authService } from "@/services/auth.service";
import { useAuth } from "@/hooks/useAuth";

export default function AdminLayout() {
    const location = useLocation();
    const navigate = useNavigate();
    const { isAuthenticated, isLoading, clearAuth } = useAuth();
    const [isSidebarOpen, setIsSidebarOpen] = useState(true);

    // Auth durumu yüklendiğinde ve kullanıcı login değilse admin login'e yönlendir
    useEffect(() => {
        if (!isLoading && !isAuthenticated) {
            navigate("/admin/login");
        }
    }, [isLoading, isAuthenticated, navigate]);

    const handleLogout = async () => {
        try {
            await authService.logout();
        } finally {
            clearAuth();
            navigate("/admin/login");
        }
    };

    // Auth durumu yüklenirken loading göster
    if (isLoading) {
        return (
            <div className="min-h-screen bg-black flex items-center justify-center">
                <Loader2 className="h-8 w-8 animate-spin text-red-600" />
            </div>
        );
    }

    // Authenticated değilse hiçbir şey gösterme
    if (!isAuthenticated) {
        return null;
    }

    const navItems = [
        { icon: LayoutDashboard, label: "Kontrol Paneli", path: "/admin/dashboard" },
        { icon: Users, label: "Kullanıcılar", path: "/admin/users" },
        { icon: BookOpen, label: "Kelimeler", path: "/admin/words" },
        { icon: Shield, label: "Roller", path: "/admin/roles" },
        { icon: Lock, label: "Yetkilendirme", path: "/admin/permissions" },
        { icon: Network, label: "IP Adresler", path: "/admin/ip-addresses" },
        { icon: Activity, label: "Log Geçmişi", path: "/admin/logs" },
        { icon: Database, label: "Veritabanı", path: "/admin/database" },
    ];

    return (
        <div className="min-h-screen bg-black text-white flex font-[Segoe_UI]">
            {/* Sidebar */}
            <aside
                className={cn(
                    "fixed inset-y-0 left-0 z-50 w-64 bg-[#0a0a0a] border-r border-red-900/20 transition-transform duration-300 ease-in-out lg:translate-x-0 lg:sticky lg:top-0 lg:h-screen",
                    !isSidebarOpen && "-translate-x-full lg:hidden"
                )}
            >
                <div className="h-full flex flex-col">
                    {/* Logo */}
                    <div className="p-6 border-b border-red-900/10 flex items-center gap-3">
                        <div className="w-8 h-8 bg-red-600 rounded-lg flex items-center justify-center shadow-[0_0_15px_rgba(220,38,38,0.5)]">
                            <span className="font-bold text-white text-lg">W</span>
                        </div>
                        <div>
                            <h1 className="font-bold text-lg leading-none">WordMaster</h1>
                            <span className="text-xs text-gray-500">Admin Panel</span>
                        </div>
                    </div>

                    {/* Navigation */}
                    <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
                        {navItems.map((item) => {
                            const isActive = location.pathname === item.path;
                            return (
                                <Link
                                    key={item.path}
                                    to={item.path}
                                    className={cn(
                                        "flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium transition-all duration-200",
                                        isActive
                                            ? "bg-red-900/20 text-red-500 border border-red-900/30 shadow-[0_0_10px_rgba(220,38,38,0.1)]"
                                            : "text-gray-400 hover:text-white hover:bg-white/5"
                                    )}
                                >
                                    <item.icon size={20} />
                                    {item.label}
                                </Link>
                            );
                        })}
                    </nav>

                    {/* Bottom Actions */}
                    <div className="p-4 border-t border-red-900/10 space-y-2">
                        <Link
                            to="/dashboard"
                            className="flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-gray-400 hover:text-white hover:bg-white/5 transition-colors"
                        >
                            <ExternalLink size={20} />
                            Siteye Dön
                        </Link>
                        <button
                            onClick={handleLogout}
                            className="w-full flex items-center gap-3 px-4 py-3 rounded-lg text-sm font-medium text-red-400 hover:text-red-300 hover:bg-red-900/20 transition-colors"
                        >
                            <LogOut size={20} />
                            Çıkış Yap
                        </button>
                    </div>
                </div>
            </aside>

            {/* Main Content */}
            <div className="flex-1 flex flex-col min-w-0">
                {/* Mobile Header */}
                <header className="lg:hidden h-16 bg-[#0a0a0a] border-b border-red-900/20 flex items-center px-4 justify-between">
                    <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-red-600 rounded-lg flex items-center justify-center">
                            <span className="font-bold text-white text-lg">W</span>
                        </div>
                        <span className="font-bold">WordMaster Admin</span>
                    </div>
                    <button onClick={() => setIsSidebarOpen(!isSidebarOpen)} className="text-gray-400">
                        <Menu size={24} />
                    </button>
                </header>

                {/* Page Content */}
                <main className="flex-1 p-6 lg:p-8 overflow-y-auto">
                    <Outlet />
                </main>
            </div>

            {/* Overlay for mobile sidebar */}
            {!isSidebarOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-40 lg:hidden"
                    onClick={() => setIsSidebarOpen(true)}
                />
            )}
        </div>
    );
}
