import { useEffect, useState } from "react";
import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer,
    Cell
} from "recharts";
import {
    BookOpen,
    GraduationCap,
    Target,
    CheckCircle2,
    XCircle,
    Activity,
    Flame
} from "lucide-react";
import { statisticsService } from "../../services/statisticsService";
import type { DashboardStatisticsResponse } from "../../types/statistics";
import { toast } from "sonner";
import { format, parseISO } from "date-fns";
import { tr } from "date-fns/locale";

export default function StatisticsPage() {
    const [stats, setStats] = useState<DashboardStatisticsResponse | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchStats();
    }, []);

    const fetchStats = async () => {
        try {
            const result = await statisticsService.getDashboardStatistics();
            if (result.isSuccess && result.data) {
                setStats(result.data);
            } else {
                toast.error(result.errorList?.join(", ") || "İstatistikler alınamadı.");
            }
        } catch (error) {
            toast.error("Bir hata oluştu.");
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex h-full items-center justify-center p-8">
                <div className="h-8 w-8 animate-spin rounded-full border-4 border-indigo-600 border-t-transparent"></div>
            </div>
        );
    }

    if (!stats) return null;

    const StatCard = ({ title, value, icon: Icon, color, subText }: any) => (
        <div className="relative overflow-hidden rounded-2xl bg-white p-6 shadow-sm transition-all hover:shadow-md border border-slate-100">
            <div className={`absolute -right-4 -top-4 h-24 w-24 rounded-full opacity-10 ${color}`}></div>
            <div className="relative flex items-center justify-between">
                <div>
                    <p className="text-sm font-medium text-slate-500">{title}</p>
                    <h3 className="mt-2 text-3xl font-bold text-slate-900">{value}</h3>
                    {subText && <p className="mt-1 text-xs text-slate-400">{subText}</p>}
                </div>
                <div className={`rounded-xl p-3 ${color.replace('bg-', 'bg-opacity-10 text-')}`}>
                    <Icon className={`h-6 w-6 ${color.replace('bg-', 'text-')}`} />
                </div>
            </div>
        </div>
    );

    return (
        <div className="space-y-8 p-8 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <div>
                <h1 className="text-2xl font-bold text-slate-900">Gelişmiş İstatistikler</h1>
                <p className="text-slate-500">Öğrenme sürecini ve performansını analiz et.</p>
            </div>

            {/* Hero Stats */}
            <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Günlük Seri"
                    value={`${stats.currentStreak} Gün`}
                    icon={Flame}
                    color="bg-orange-500"
                    subText="Seriyi bozma!"
                />
                <StatCard
                    title="Toplam Kelime"
                    value={stats.totalWords}
                    icon={BookOpen}
                    color="bg-blue-600"
                    subText="Kütüphanendeki kelimeler"
                />
                <StatCard
                    title="Öğrenilen"
                    value={stats.totalLearnedWords}
                    icon={GraduationCap}
                    color="bg-green-600"
                    subText="5+ kez üst üste doğru"
                />
                <StatCard
                    title="Başarı Oranı"
                    value={`%${stats.accuracyRate}`}
                    icon={Target}
                    color="bg-indigo-600"
                    subText={`${stats.totalCorrectCount} Doğru / ${stats.totalWrongCount} Yanlış`}
                />
            </div>

            {/* Activity Graph */}
            <div className="rounded-2xl bg-white p-6 shadow-sm border border-slate-100">
                <div className="mb-6 flex items-center justify-between">
                    <div>
                        <h2 className="text-lg font-semibold text-slate-900">Son Aktivite</h2>
                        <p className="text-sm text-slate-500">Son 14 gündeki pratik yoğunluğun</p>
                    </div>
                    <Activity className="h-5 w-5 text-slate-400" />
                </div>
                <div className="h-[300px] w-full">
                    <ResponsiveContainer width="100%" height="100%">
                        <BarChart data={stats.lastActivities}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#e2e8f0" />
                            <XAxis
                                dataKey="date"
                                tickFormatter={(date: string) => {
                                    try {
                                        return format(parseISO(date), "d MMM", { locale: tr })
                                    } catch (e) {
                                        return date;
                                    }
                                }}
                                stroke="#94a3b8"
                                fontSize={12}
                                tickLine={false}
                                axisLine={false}
                            />
                            <YAxis
                                stroke="#94a3b8"
                                fontSize={12}
                                tickLine={false}
                                axisLine={false}
                            />
                            <Tooltip
                                cursor={{ fill: 'transparent' }}
                                content={({ active, payload, label }) => {
                                    if (active && payload && payload.length) {
                                        return (
                                            <div className="rounded-lg bg-slate-900 p-3 text-white shadow-xl">
                                                <p className="mb-1 text-xs text-slate-400">
                                                    {format(parseISO(label as string), "d MMMM yyyy", { locale: tr })}
                                                </p>
                                                <p className="font-medium">
                                                    {payload[0].value} Kelime Çalışıldı
                                                </p>
                                            </div>
                                        );
                                    }
                                    return null;
                                }}
                            />
                            <Bar
                                dataKey="count"
                                radius={[4, 4, 0, 0]}
                                maxBarSize={50}
                            >
                                {stats.lastActivities.map((entry, index) => (
                                    <Cell
                                        key={`cell-${index}`}
                                        fill={entry.count > 0 ? "#4f46e5" : "#e2e8f0"}
                                    />
                                ))}
                            </Bar>
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>

            {/* Top Lists */}
            <div className="grid gap-8 lg:grid-cols-2">
                {/* Best Words */}
                <div className="rounded-2xl bg-white p-6 shadow-sm border border-slate-100">
                    <div className="mb-6 flex items-center gap-2">
                        <div className="rounded-lg bg-green-100 p-2 text-green-600">
                            <CheckCircle2 className="h-5 w-5" />
                        </div>
                        <div>
                            <h2 className="text-lg font-semibold text-slate-900">En İyi Bildiklerin</h2>
                            <p className="text-sm text-slate-500">En çok doğru cevaplananlar</p>
                        </div>
                    </div>
                    <div className="space-y-4">
                        {stats.topBestWords.length === 0 ? (
                            <p className="text-sm text-slate-500">Henüz yeterli veri yok.</p>
                        ) : (
                            stats.topBestWords.map((word) => (
                                <div key={word.id} className="flex items-center justify-between rounded-xl bg-slate-50 p-4 transition-colors hover:bg-slate-100">
                                    <div>
                                        <p className="font-semibold text-slate-900">{word.englishWord}</p>
                                        <p className="text-sm text-slate-500">{word.turkishWord}</p>
                                    </div>
                                    <div className="text-right">
                                        <p className="font-bold text-green-600">{word.correctCount} Doğru</p>
                                        <p className="text-xs text-slate-400">{word.wrongCount} Yanlış</p>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>

                {/* Worst Words */}
                <div className="rounded-2xl bg-white p-6 shadow-sm border border-slate-100">
                    <div className="mb-6 flex items-center gap-2">
                        <div className="rounded-lg bg-red-100 p-2 text-red-600">
                            <XCircle className="h-5 w-5" />
                        </div>
                        <div>
                            <h2 className="text-lg font-semibold text-slate-900">Zorlandıkların</h2>
                            <p className="text-sm text-slate-500">En sık hata yaptıkların</p>
                        </div>
                    </div>
                    <div className="space-y-4">
                        {stats.topWorstWords.length === 0 ? (
                            <p className="text-sm text-slate-500">Henüz yeterli veri yok.</p>
                        ) : (
                            stats.topWorstWords.map((word) => (
                                <div key={word.id} className="flex items-center justify-between rounded-xl bg-slate-50 p-4 transition-colors hover:bg-slate-100">
                                    <div>
                                        <p className="font-semibold text-slate-900">{word.englishWord}</p>
                                        <p className="text-sm text-slate-500">{word.turkishWord}</p>
                                    </div>
                                    <div className="text-right">
                                        <p className="font-bold text-red-600">{word.wrongCount} Yanlış</p>
                                        <p className="text-xs text-slate-400">{word.correctCount} Doğru</p>
                                    </div>
                                </div>
                            ))
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
}
