import { useEffect, useState } from "react";
import { adminService } from "@/services/admin.service";
import type { AdminDashboardResponse } from "@/types/admin";
import {
    Book,
    Star,
    HelpCircle,
    Users,
    LogIn,
    AlertCircle,
    Folder,
    ShieldAlert
} from "lucide-react";
import {
    LineChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    Legend
} from "recharts";
import { cn } from "@/lib/utils";

export default function AdminDashboardPage() {
    const [data, setData] = useState<AdminDashboardResponse | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchData = async () => {
            try {
                const result = await adminService.getDashboard();
                if (result.isSuccess) {
                    setData(result.data);
                } else {
                    setError(result.errorList?.[0] || "Veri yüklenemedi.");
                }
            } catch (err) {
                setError("Veriler yüklenirken bir hata oluştu.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []);

    if (loading) {
        return (
            <div className="flex items-center justify-center h-full text-red-500">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-current"></div>
            </div>
        );
    }

    if (error || !data) {
        return (
            <div className="flex flex-col items-center justify-center h-full text-red-400 gap-4">
                <AlertCircle size={48} />
                <p>{error || "Veri bulunamadı."}</p>
                <button
                    onClick={() => window.location.reload()}
                    className="px-4 py-2 bg-red-900/20 rounded-lg hover:bg-red-900/40 transition-colors"
                >
                    Tekrar Dene
                </button>
            </div>
        );
    }

    const stats = [
        {
            label: "TOPLAM KLASÖR",
            value: data.totalFolders,
            icon: Folder,
            color: "text-emerald-500",
            bg: "bg-emerald-500/10",
            border: "border-emerald-500/20"
        },
        {
            label: "TOPLAM KELİME",
            value: data.totalWords,
            icon: Book,
            color: "text-blue-500",
            bg: "bg-blue-500/10",
            border: "border-blue-500/20"
        },
        {
            label: "FAVORİLER",
            value: data.totalFavorites,
            icon: Star,
            color: "text-yellow-500",
            bg: "bg-yellow-500/10",
            border: "border-yellow-500/20"
        },
        {
            label: "BİLİNMEYENLER",
            value: data.totalUnknows,
            icon: HelpCircle,
            color: "text-orange-500",
            bg: "bg-orange-500/10",
            border: "border-orange-500/20"
        },
        {
            label: "KULLANICILAR",
            value: data.totalUsers,
            icon: Users,
            color: "text-purple-500",
            bg: "bg-purple-500/10",
            border: "border-purple-500/20"
        },
        {
            label: "KİLİTLİ HESAPLAR",
            value: data.lockedUserCount,
            icon: ShieldAlert,
            color: "text-red-500",
            bg: "bg-red-500/10",
            border: "border-red-500/20"
        },
        {
            label: "AKTİF KULLANICILAR (24S)",
            value: data.activeUsers24h,
            icon: Users,
            color: "text-green-500",
            bg: "bg-green-500/10",
            border: "border-green-500/20"
        },
        {
            label: "YENİ KELİMELER (24S)",
            value: data.newWords24h,
            icon: Book,
            color: "text-blue-400",
            bg: "bg-blue-400/10",
            border: "border-blue-400/20"
        },
        {
            label: "YENİ KLASÖRLER (24S)",
            value: data.newFolders24h,
            icon: Folder,
            color: "text-emerald-400",
            bg: "bg-emerald-400/10",
            border: "border-emerald-400/20"
        },
    ];

    return (
        <div className="space-y-8 animate-in fade-in duration-500">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-3xl font-bold text-white mb-2">Kontrol Paneli</h1>
                    <p className="text-gray-400">Sistem durumunu ve istatistikleri buradan takip edebilirsiniz.</p>
                </div>
                <div className="flex items-center gap-4 bg-[#121212] px-4 py-2 rounded-lg border border-white/5">
                    <div className="text-right">
                        <p className="text-xs text-gray-500 uppercase font-bold">Hoş Geldin</p>
                        <p className="text-sm font-bold text-white">ADMIN</p>
                    </div>
                    <div className="w-10 h-10 rounded-full bg-red-600 flex items-center justify-center text-white font-bold">
                        A
                    </div>
                </div>
            </div>

            {/* Stats Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-3 2xl:grid-cols-5 gap-6">
                {stats.map((stat, index) => (
                    <div
                        key={index}
                        className={cn(
                            "p-6 rounded-xl border bg-[#121212] transition-all duration-300 hover:scale-[1.02]",
                            stat.border
                        )}
                    >
                        <div className="flex items-start justify-between">
                            <div>
                                <p className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-1">{stat.label}</p>
                                <h3 className="text-3xl font-bold text-white">{stat.value}</h3>
                            </div>
                            <div className={cn("p-3 rounded-lg", stat.bg)}>
                                <stat.icon className={cn("w-6 h-6", stat.color)} />
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {/* Entity Stats Section */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                <div className="bg-[#121212] border border-blue-900/20 rounded-xl p-6">
                    <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">VERİ DURUMU</h3>
                    <div className="space-y-4">
                        <div className="flex justify-between items-center">
                            <span className="text-gray-500 text-sm">Son Word ID</span>
                            <span className="text-white font-bold">#{data.lastWordId}</span>
                        </div>
                        <div className="flex justify-between items-center">
                            <span className="text-gray-500 text-sm">Son Unknown ID</span>
                            <span className="text-white font-bold">#{data.lastUnknowsId}</span>
                        </div>
                        <div className="flex justify-between items-center">
                            <span className="text-gray-500 text-sm">Son Favorite ID</span>
                            <span className="text-white font-bold">#{data.lastFavoriteId}</span>
                        </div>
                        <div className="w-full bg-gray-800 rounded-full h-1.5 mt-2">
                            <div className="bg-blue-600 h-1.5 rounded-full" style={{ width: '75%' }}></div>
                        </div>
                    </div>
                </div>

                <div className="bg-[#121212] border border-emerald-900/20 rounded-xl p-6">
                    <h3 className="text-sm font-bold text-gray-400 uppercase tracking-wider mb-4">İÇERİK ANALİZİ</h3>
                    <div className="space-y-4">
                        <div className="flex justify-between items-center">
                            <span className="text-gray-500 text-sm">Klasörlerdeki Kelimeler</span>
                            <span className="text-white font-bold">{data.totalWordsInFolders.toLocaleString()}</span>
                        </div>
                        <div className="flex justify-between items-center">
                            <span className="text-gray-500 text-sm">Ortalama Klasör İçeriği</span>
                            <span className="text-white font-bold">
                                {data.totalFolders > 0 ? (data.totalWordsInFolders / data.totalFolders).toFixed(1) : 0}
                            </span>
                        </div>
                        <div className="w-full bg-gray-800 rounded-full h-1.5 mt-2">
                            <div className="bg-emerald-600 h-1.5 rounded-full" style={{ width: '60%' }}></div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Login Stats Section */}
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Total Login Card */}
                <div className="lg:col-span-1 bg-[#121212] border border-red-900/20 rounded-xl p-6 relative overflow-hidden group">
                    <div className="absolute top-0 right-0 w-32 h-32 bg-red-600/10 rounded-full blur-3xl -mr-16 -mt-16 transition-all duration-500 group-hover:bg-red-600/20"></div>

                    <div className="relative z-10">
                        <h3 className="text-lg font-bold text-white mb-6 flex items-center gap-2">
                            <LogIn className="text-red-500" size={20} />
                            Giriş İstatistikleri
                        </h3>

                        <div className="space-y-6">
                            <div>
                                <p className="text-xs text-gray-500 uppercase font-bold mb-1">TOPLAM GİRİŞ DENEMESİ</p>
                                <p className="text-4xl font-bold text-white">{data.totalLogins}</p>
                            </div>

                            <div className="grid grid-cols-2 gap-4">
                                <div className="bg-green-500/5 rounded-lg p-3 border border-green-500/10">
                                    <div className="flex items-center gap-2 mb-1">
                                        <div className="w-2 h-2 rounded-full bg-green-500"></div>
                                        <span className="text-xs text-gray-400">Başarılı</span>
                                    </div>
                                    <p className="text-xl font-bold text-green-500">{data.successfulLogins}</p>
                                </div>
                                <div className="bg-red-500/5 rounded-lg p-3 border border-red-500/10">
                                    <div className="flex items-center gap-2 mb-1">
                                        <div className="w-2 h-2 rounded-full bg-red-500"></div>
                                        <span className="text-xs text-gray-400">Başarısız</span>
                                    </div>
                                    <p className="text-xl font-bold text-red-500">{data.failedLogins}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* Chart */}
                <div className="lg:col-span-2 bg-[#121212] border border-white/5 rounded-xl p-6">
                    <h3 className="text-lg font-bold text-white mb-2">Son 7 Gün Giriş Analizi</h3>
                    <p className="text-sm text-gray-500 mb-6">Günlük başarılı ve başarısız giriş denemeleri</p>

                    <div className="h-[300px] w-full">
                        <ResponsiveContainer width="100%" height="100%">
                            <LineChart data={data.dailyLoginStats}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#333" vertical={false} />
                                <XAxis
                                    dataKey="date"
                                    stroke="#666"
                                    fontSize={12}
                                    tickLine={false}
                                    axisLine={false}
                                    dy={10}
                                />
                                <YAxis
                                    stroke="#666"
                                    fontSize={12}
                                    tickLine={false}
                                    axisLine={false}
                                    dx={-10}
                                />
                                <Tooltip
                                    contentStyle={{
                                        backgroundColor: '#1a1a1a',
                                        border: '1px solid #333',
                                        borderRadius: '8px',
                                        color: '#fff'
                                    }}
                                    itemStyle={{ fontSize: '12px' }}
                                    labelStyle={{ color: '#999', marginBottom: '5px' }}
                                />
                                <Legend iconType="circle" wrapperStyle={{ paddingTop: '20px' }} />
                                <Line
                                    type="monotone"
                                    dataKey="successCount"
                                    name="Başarılı"
                                    stroke="#22c55e"
                                    strokeWidth={3}
                                    dot={{ r: 4, fill: '#121212', strokeWidth: 2 }}
                                    activeDot={{ r: 6, fill: '#22c55e' }}
                                />
                                <Line
                                    type="monotone"
                                    dataKey="failCount"
                                    name="Başarısız"
                                    stroke="#ef4444"
                                    strokeWidth={3}
                                    dot={{ r: 4, fill: '#121212', strokeWidth: 2 }}
                                    activeDot={{ r: 6, fill: '#ef4444' }}
                                />
                            </LineChart>
                        </ResponsiveContainer>
                    </div>
                </div>
            </div>
        </div>
    );
}
