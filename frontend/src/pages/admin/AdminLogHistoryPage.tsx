import { useEffect, useState } from "react";
import { logHistoryService } from "@/services/logHistory.service";
import type { LogHistoryResponse } from "@/types/logHistory";
import {
    Search,
    Loader2,
    ChevronLeft,
    ChevronRight,
    Activity,
    CheckCircle,
    XCircle,
    Monitor,
    Shield
} from "lucide-react";
import { cn } from "@/lib/utils";

export default function AdminLogHistoryPage() {
    // List State
    const [logs, setLogs] = useState<LogHistoryResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [searchQuery, setSearchQuery] = useState("");
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalCount, setTotalCount] = useState(0);
    const [pageSize, setPageSize] = useState(20);

    const [searchInEmail, setSearchInEmail] = useState(true);
    const [searchInUserId, setSearchInUserId] = useState(true);
    const [searchInIp, setSearchInIp] = useState(true);

    const fetchLogs = async (pageNum: number = 1, search: string = "", size: number = 20) => {
        setLoading(true);
        try {
            const result = await logHistoryService.getLogs({
                page: pageNum,
                size: size,
                searchTerm: search,
                searchInEmail,
                searchInUserId,
                searchInIp
            });
            if (result.isSuccess) {
                setLogs(result.data.items);
                setTotalPages(result.data.totalPages);
                setTotalCount(result.data.totalCount);
                setPage(result.data.pageNumber);
            }
        } catch (error) {
            console.error("Loglar yüklenirken hata oluştu", error);
        } finally {
            setLoading(false);
        }
    };

    // İlk yükleme ve pageSize değiştiğinde tetiklenir
    useEffect(() => {
        fetchLogs(1, searchQuery, pageSize);
    }, [pageSize]);

    const handleSearchClick = () => {
        setPage(1);
        fetchLogs(1, searchQuery, pageSize);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
        if (e.key === "Enter") {
            handleSearchClick();
        }
    };

    const handlePageChange = (newPage: number) => {
        if (newPage >= 1 && newPage <= totalPages) {
            fetchLogs(newPage, searchQuery, pageSize);
        }
    };

    return (
        <div className="space-y-6 animate-in fade-in duration-500">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div>
                    <h1 className="text-3xl font-bold text-white mb-2 flex items-center gap-2">
                        <Activity className="text-purple-500" />
                        Log Geçmişi
                    </h1>
                    <p className="text-gray-400">Sistemdeki kullanıcı oturum ve işlem loglarını görüntüleyin.</p>
                </div>
            </div>

            {/* Search and Table Container */}
            <div className="bg-[#121212] border border-white/5 rounded-xl overflow-hidden">
                {/* Search Bar & Filters */}
                <div className="p-4 border-b border-white/5 flex flex-col gap-4">
                    <div className="flex flex-col lg:flex-row justify-between items-start lg:items-center gap-4">
                        <div className="flex flex-1 w-full gap-2">
                            <div className="relative flex-1">
                                <Search className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" size={20} />
                                <input
                                    type="text"
                                    placeholder="Arama yap..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    onKeyDown={handleKeyDown}
                                    className="w-full bg-[#1a1a1a] text-white pl-10 pr-4 py-2 rounded-lg border border-white/10 focus:border-purple-500 focus:outline-none transition-colors"
                                />
                            </div>
                            <button
                                onClick={handleSearchClick}
                                className="px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white rounded-lg transition-colors font-medium flex items-center gap-2"
                            >
                                <Search size={18} />
                                Ara
                            </button>
                        </div>

                        <div className="flex items-center gap-2 min-w-fit">
                            <span className="text-sm text-gray-400">Sayfa Başına:</span>
                            <select
                                value={pageSize}
                                onChange={(e) => {
                                    const newSize = Number(e.target.value);
                                    setPageSize(newSize);
                                    setPage(1);
                                }}
                                className="bg-[#1a1a1a] text-white px-3 py-2 rounded-lg border border-white/10 focus:border-purple-500 focus:outline-none transition-colors text-sm"
                            >
                                <option value={10}>10</option>
                                <option value={20}>20</option>
                                <option value={50}>50</option>
                                <option value={100}>100</option>
                            </select>
                        </div>
                    </div>

                    {/* Checkbox Filters */}
                    <div className="flex flex-wrap items-center gap-4 text-sm text-gray-300">
                        <span className="text-gray-500">Arama Kriterleri:</span>
                        <label className="flex items-center gap-2 cursor-pointer hover:text-white transition-colors">
                            <input
                                type="checkbox"
                                checked={searchInEmail}
                                onChange={(e) => setSearchInEmail(e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-[#1a1a1a] text-purple-600 focus:ring-purple-500"
                            />
                            Email
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer hover:text-white transition-colors">
                            <input
                                type="checkbox"
                                checked={searchInUserId}
                                onChange={(e) => setSearchInUserId(e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-[#1a1a1a] text-purple-600 focus:ring-purple-500"
                            />
                            User ID
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer hover:text-white transition-colors">
                            <input
                                type="checkbox"
                                checked={searchInIp}
                                onChange={(e) => setSearchInIp(e.target.checked)}
                                className="w-4 h-4 rounded border-gray-600 bg-[#1a1a1a] text-purple-600 focus:ring-purple-500"
                            />
                            IP Adresi
                        </label>
                    </div>
                </div>

                {/* Table */}
                <div className="overflow-x-auto">
                    <table className="w-full text-left">
                        <thead className="bg-white/5 text-gray-400 uppercase text-xs font-bold">
                            <tr>
                                <th className="px-6 py-4">Zaman</th>
                                <th className="px-6 py-4">Durum</th>
                                <th className="px-6 py-4">Kullanıcı (Email / ID)</th>
                                <th className="px-6 py-4">IP Adresi</th>
                                <th className="px-6 py-4">Kaynak</th>
                            </tr>
                        </thead>
                        <tbody className="divide-y divide-white/5">
                            {loading ? (
                                <tr>
                                    <td colSpan={5} className="px-6 py-8 text-center text-gray-500">
                                        <div className="flex justify-center items-center gap-2">
                                            <Loader2 className="animate-spin" size={20} />
                                            Yükleniyor...
                                        </div>
                                    </td>
                                </tr>
                            ) : logs.length > 0 ? (
                                logs.map((log) => (
                                    <tr key={log.id} className="hover:bg-white/5 transition-colors">
                                        <td className="px-6 py-4 text-gray-400 text-sm font-mono">
                                            {new Date(log.attemptedAt).toLocaleString("tr-TR")}
                                        </td>
                                        <td className="px-6 py-4">
                                            {log.isSuccessful ? (
                                                <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium bg-green-500/10 text-green-500 border border-green-500/20">
                                                    <CheckCircle size={12} />
                                                    Başarılı
                                                </span>
                                            ) : (
                                                <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium bg-red-500/10 text-red-500 border border-red-500/20">
                                                    <XCircle size={12} />
                                                    Başarısız
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 text-white">
                                            <div className="flex flex-col">
                                                {log.email && <span className="font-medium">{log.email}</span>}
                                                {log.appUserId ? (
                                                    <span className="text-xs text-gray-500 font-mono" title={log.appUserId}>{log.appUserId}</span>
                                                ) : (
                                                    <span className="text-xs text-gray-600 italic">Anonim</span>
                                                )}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2 text-gray-300">
                                                <Monitor size={14} className="text-gray-500" />
                                                <span className="font-mono text-sm">{log.ipAddress || "-"}</span>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4">
                                            <div className="flex items-center gap-2 text-gray-300">
                                                <Shield size={14} className="text-gray-500" />
                                                <span>{log.source || "Sistem"}</span>
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

                {/* Pagination */}
                <div className="p-4 border-t border-white/5 flex items-center justify-between">
                    <span className="text-sm text-gray-500">
                        Toplam {totalCount} kayıt, Sayfa {page} / {totalPages}
                    </span>
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => handlePageChange(page - 1)}
                            disabled={page === 1}
                            className="p-2 bg-white/5 hover:bg-white/10 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                            <ChevronLeft size={16} />
                        </button>

                        {(() => {
                            const pages = [];
                            const showEllipsisStart = page > 3;
                            const showEllipsisEnd = page < totalPages - 2;

                            if (totalPages <= 7) {
                                for (let i = 1; i <= totalPages; i++) {
                                    pages.push(i);
                                }
                            } else {
                                pages.push(1);
                                if (showEllipsisStart) pages.push("...");

                                let start = Math.max(2, page - 1);
                                let end = Math.min(totalPages - 1, page + 1);

                                if (page <= 3) end = 4;
                                if (page >= totalPages - 2) start = totalPages - 3;

                                for (let i = start; i <= end; i++) {
                                    pages.push(i);
                                }

                                if (showEllipsisEnd) pages.push("...");
                                pages.push(totalPages);
                            }

                            return pages.map((p, index) => (
                                <button
                                    key={index}
                                    onClick={() => typeof p === 'number' ? handlePageChange(p) : null}
                                    disabled={p === "..."}
                                    className={cn(
                                        "w-8 h-8 flex items-center justify-center rounded-lg text-sm font-medium transition-colors",
                                        p === page
                                            ? "bg-purple-600 text-white"
                                            : p === "..."
                                                ? "text-gray-500 cursor-default"
                                                : "bg-white/5 hover:bg-white/10 text-gray-300 hover:text-white"
                                    )}
                                >
                                    {p}
                                </button>
                            ));
                        })()}

                        <button
                            onClick={() => handlePageChange(page + 1)}
                            disabled={page === totalPages}
                            className="p-2 bg-white/5 hover:bg-white/10 text-white rounded-lg disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                        >
                            <ChevronRight size={16} />
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}
