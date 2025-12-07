import { useState } from "react";
import { databaseService } from "@/services/database.service";
import { toast } from "sonner";
import { Shield, AlertTriangle, Loader2, Database, AlertCircle } from "lucide-react";

// Types
type TableType = "words" | "unknows" | "favorites" | "folders" | "folderwords";

const tables: { id: TableType; label: string; description: string }[] = [
    { id: "words", label: "Kelimeler", description: "Tüm kelimeleri sıfırlar. Bu işlem kelimelere bağlı olan favorileri, bilinmeyenleri ve klasör içeriklerini de silecektir." },
    { id: "unknows", label: "Bilinmeyenler", description: "Sadece bilinmeyen kelimeler listesini sıfırlar." },
    { id: "favorites", label: "Favoriler", description: "Sadece favori kelimeler listesini sıfırlar." },
    { id: "folders", label: "Klasörler", description: "Tüm klasörleri ve içeriklerini sıfırlar." },
    { id: "folderwords", label: "Klasör İçerikleri", description: "Klasörleri tutar ancak içlerindeki kelimeleri boşaltır." },
];

export default function DatabaseResetPage() {
    const [selectedTable, setSelectedTable] = useState<TableType | null>(null);
    const [password, setPassword] = useState("");
    const [targetUserId, setTargetUserId] = useState("");
    const [cardInputs, setCardInputs] = useState<Record<string, string>>({});
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleClose = () => {
        setSelectedTable(null);
        setPassword("");
        setTargetUserId("");
        setError(null);
    };

    const handleCardInputChange = (tableId: string, value: string) => {
        setCardInputs(prev => ({ ...prev, [tableId]: value }));
    };

    const handleTableSelect = (tableId: TableType) => {
        setSelectedTable(tableId);
        // If there was an input value for this card, set it as the targetUserId
        if (cardInputs[tableId]) {
            setTargetUserId(cardInputs[tableId]);
        }
    };

    const handleReset = async () => {
        if (!selectedTable || !password) return;

        setIsLoading(true);
        setError(null);
        try {
            await databaseService.resetTable(selectedTable, password, targetUserId || undefined);
            toast.success("Tablo başarıyla sıfırlandı.");
            handleClose();
            // Clear inputs if success? maybe
            setCardInputs({});
        } catch (err: any) {
            console.error(err);
            let msg = "İşlem başarısız.";
            if (err.response?.data?.errorList && Array.isArray(err.response.data.errorList) && err.response.data.errorList.length > 0) {
                msg = err.response.data.errorList[0];
            } else if (err.response?.data?.error) {
                msg = err.response.data.error;
            } else if (err.message) {
                msg = err.message;
            }
            setError(msg);
            toast.error(msg);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold text-white mb-2">Veritabanı Yönetimi</h1>
                    <p className="text-gray-400">
                        Bu sayfadan veritabanı tablolarını sıfırlayabilirsiniz. Bu işlemler geri alınamaz.
                    </p>
                </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                {tables.map((table) => (
                    <div
                        key={table.id}
                        className="bg-[#1a1a1a] border border-white/5 rounded-xl p-6 hover:border-red-500/30 transition-all group"
                    >
                        <div className="flex items-start justify-between mb-4">
                            <div className="p-3 bg-red-500/10 rounded-lg text-red-500 group-hover:bg-red-500/20 transition-colors">
                                <Database size={24} />
                            </div>
                            <Shield className="text-gray-600" size={20} />
                        </div>
                        <h3 className="text-lg font-bold text-white mb-2">{table.label}</h3>
                        <p className="text-sm text-gray-400 mb-4 min-h-[60px]">{table.description}</p>

                        <div className="mb-4">
                            <input
                                type="text"
                                placeholder="Kullanıcı ID (Opsiyonel)"
                                value={cardInputs[table.id] || ""}
                                onChange={(e) => handleCardInputChange(table.id, e.target.value)}
                                className="w-full bg-black/30 border border-white/10 rounded-lg px-3 py-2 text-sm text-white focus:outline-none focus:border-red-500 transition-colors"
                            />
                            <p className="text-xs text-gray-500 mt-1">Sadece bu kullanıcının verilerini silmek için ID girin. Boş bırakırsanız **TÜM** veriler silinir.</p>
                        </div>

                        <button
                            onClick={() => handleTableSelect(table.id)}
                            className="w-full py-2.5 px-4 bg-red-600/10 hover:bg-red-600 text-red-500 hover:text-white rounded-lg transition-all font-medium flex items-center justify-center gap-2"
                        >
                            <AlertTriangle size={16} />
                            Tabloyu Sıfırla
                        </button>
                    </div>
                ))}
            </div>

            {/* Confirmation Modal */}
            {selectedTable && (
                <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 backdrop-blur-sm p-4 animate-in fade-in duration-200">
                    <div className="bg-[#1a1a1a] border border-white/10 rounded-xl w-full max-w-md shadow-2xl p-6">
                        <div className="text-center">
                            <div className="w-16 h-16 bg-red-500/10 text-red-500 rounded-full flex items-center justify-center mx-auto mb-4">
                                <AlertTriangle size={32} />
                            </div>
                            <h3 className="text-xl font-bold text-white mb-2">Emin misiniz?</h3>
                            <p className="text-gray-400 mb-6">
                                <span className="font-bold text-white">{tables.find(t => t.id === selectedTable)?.label}</span> tablosunu
                                {targetUserId ? <span> (Kullanıcı: <span className="text-red-400 font-mono">{targetUserId}</span>) </span> : " "}
                                sıfırlamak üzeresiniz. Bu işlem geri alınamaz ve {targetUserId ? "bu kullanıcıya ait" : "tüm"} veriler kalıcı olarak silinecektir.
                            </p>

                            <div className="text-left bg-black/40 p-4 rounded-lg border border-white/5 mb-6">
                                <label className="block text-sm font-medium text-gray-400 mb-2">Güvenlik Doğrulaması</label>
                                <p className="text-xs text-gray-500 mb-3 block">İşlemi onaylamak için lütfen şifrenizi girin.</p>
                                <input
                                    type="password"
                                    value={password}
                                    onChange={(e) => setPassword(e.target.value)}
                                    placeholder="Şifreniz"
                                    className="w-full bg-black/50 border border-white/10 rounded-lg px-4 py-2 text-white focus:outline-none focus:border-red-500 transition-colors"
                                />
                                {error && (
                                    <div className="flex items-center gap-2 mt-3 text-red-400 text-sm">
                                        <AlertCircle size={14} />
                                        <span>{error}</span>
                                    </div>
                                )}
                            </div>

                            <div className="flex items-center gap-3">
                                <button
                                    onClick={handleClose}
                                    disabled={isLoading}
                                    className="flex-1 py-2.5 bg-white/5 hover:bg-white/10 text-white rounded-lg transition-colors font-medium"
                                >
                                    İptal
                                </button>
                                <button
                                    onClick={handleReset}
                                    disabled={isLoading || !password}
                                    className="flex-1 py-2.5 bg-red-600 hover:bg-red-700 text-white rounded-lg transition-colors font-medium flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    {isLoading ? <Loader2 className="animate-spin" size={18} /> : "Onayla ve Sıfırla"}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
}
