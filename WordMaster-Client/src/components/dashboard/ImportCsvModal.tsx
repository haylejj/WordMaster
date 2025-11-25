import { useState, useRef } from "react";
import { X, UploadCloud, HelpCircle, Download, FileSpreadsheet } from "lucide-react";
import { wordService } from "@/services/word.service";

interface ImportCsvModalProps {
    isOpen: boolean;
    onClose: () => void;
    onImported?: () => void;
}

export default function ImportCsvModal({ isOpen, onClose, onImported }: ImportCsvModalProps) {
    const [file, setFile] = useState<File | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [showInfo, setShowInfo] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const resetState = () => {
        setFile(null);
        setError(null);
        setSuccess(null);
        setShowInfo(false);
    };

    const handleClose = () => {
        resetState();
        onClose();
    };

    const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files && e.target.files[0]) {
            setFile(e.target.files[0]);
            setError(null);
        }
    };

    const handleDownloadSample = () => {
        const sample = "WORD,MEANING\nApple,Elma\nBook,Kitap Defter 3.Anlam";
        const blob = new Blob([sample], { type: "text/csv;charset=utf-8;" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = "wordmaster-sample.csv";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        URL.revokeObjectURL(url);
    };

    const handleUpload = async () => {
        if (!file) return;
        setLoading(true);
        setError(null);
        setSuccess(null);
        try {
            const response = await wordService.importCsv(file);
            if (response.isSuccess) {
                setSuccess("Kelimeler başarıyla içe aktarıldı!");
                setTimeout(() => {
                    handleClose();
                    onImported?.();
                }, 1500);
            } else {
                setError(response.errorList?.join(" ") || "İçe aktarma başarısız.");
            }
        } catch (err: any) {
            setError(err.response?.data?.message || "Bir hata oluştu.");
        } finally {
            setLoading(false);
        }
    };

    if (!isOpen) return null;

    if (showInfo) {
        return (
            <div className="fixed inset-0 z-[70] flex items-center justify-center bg-black/50 backdrop-blur-sm animate-in fade-in">
                <div className="bg-white rounded-lg shadow-xl w-full max-w-lg mx-4 overflow-hidden">
                    <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
                        <div className="flex items-center space-x-2">
                            <HelpCircle className="text-cyan-400" size={20} />
                            <h2 className="text-lg font-bold uppercase">CSV FORMATI HAKKINDA</h2>
                        </div>
                        <button onClick={() => setShowInfo(false)} className="text-gray-400 hover:text-white"><X size={20} /></button>
                    </div>
                    <div className="p-6 text-sm text-gray-600 space-y-4">
                        <p>CSV dosyanızın aşağıdaki kurallara uyması gerekmektedir:</p>
                        <ul className="list-none space-y-2">
                            <li className="flex items-center"><span className="text-green-500 mr-2">✓</span> Dosya uzantısı .csv olmalıdır.</li>
                            <li className="flex items-center"><span className="text-green-500 mr-2">✓</span> Başlık satırı (header) bulunmalıdır: <strong>WORD,MEANING</strong></li>
                            <li className="flex items-start"><span className="text-green-500 mr-2 mt-0.5">✓</span> <span>Sütunlar virgül (,) ile ayrılmalıdır. Eğer birden fazla anlam eklemek isterseniz, anlamları boşluk ile ayırabilirsiniz.</span></li>
                            <li className="flex items-center"><span className="text-green-500 mr-2">✓</span> Türkçe karakter desteği için dosya kodlaması <strong>UTF-8</strong> olmalıdır.</li>
                        </ul>
                        <div className="bg-gray-50 p-4 rounded border border-gray-200">
                            <p className="font-bold mb-2 text-gray-700">Örnek İçerik:</p>
                            <pre className="font-mono text-xs text-pink-600">
                                WORD,MEANING<br />
                                Apple,Elma<br />
                                Book,Kitap Defter 3.Anlam
                            </pre>
                        </div>
                        <div className="flex justify-end">
                            <button onClick={() => setShowInfo(false)} className="bg-primary-yellow text-primary-dark font-bold py-2 px-4 rounded hover:bg-[#FFC107]">← Geri Dön</button>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    return (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm animate-in fade-in">
            <div className="bg-white rounded-lg shadow-xl w-full max-w-lg mx-4 overflow-hidden">
                <div className="bg-[#1a1f24] text-white px-6 py-4 flex justify-between items-center">
                    <div className="flex items-center space-x-2">
                        <FileSpreadsheet className="text-green-500" size={20} />
                        <h2 className="text-lg font-bold uppercase">CSV İLE İÇE AKTAR</h2>
                    </div>
                    <button onClick={handleClose} className="text-gray-400 hover:text-white"><X size={20} /></button>
                </div>

                <div className="p-6">
                    <div className="flex justify-between items-start mb-4">
                        <p className="text-sm text-gray-500">Toplu kelime eklemek için CSV dosyası yükleyin.</p>
                        <div className="flex space-x-2">
                            <button onClick={() => setShowInfo(true)} className="text-gray-400 hover:text-gray-600"><HelpCircle size={20} /></button>
                            <button
                                onClick={handleDownloadSample}
                                className="flex items-center text-xs text-blue-600 hover:underline border border-blue-200 px-2 py-1 rounded bg-blue-50"
                            >
                                <Download size={12} className="mr-1" /> Örnek CSV
                            </button>
                        </div>
                    </div>

                    <div
                        className="border-2 border-dashed border-gray-300 rounded-lg p-8 flex flex-col items-center justify-center bg-gray-50 hover:bg-gray-100 transition-colors cursor-pointer"
                        onClick={() => fileInputRef.current?.click()}
                    >
                        <UploadCloud size={48} className="text-gray-400 mb-3" />
                        {file ? (
                            <div className="text-center">
                                <p className="font-bold text-gray-700">{file.name}</p>
                                <p className="text-xs text-gray-500">{(file.size / 1024).toFixed(2)} KB</p>
                            </div>
                        ) : (
                            <div className="text-center">
                                <p className="font-bold text-gray-700 mb-1">Dosya Seçin</p>
                                <p className="text-xs text-gray-500 mb-3">Dosyayı buraya sürükleyin veya seçin</p>
                                <button className="bg-white border border-gray-300 px-3 py-1.5 rounded text-sm text-gray-700 shadow-sm hover:bg-gray-50">Dosya Seç</button>
                            </div>
                        )}
                        <input
                            type="file"
                            accept=".csv"
                            ref={fileInputRef}
                            onChange={handleFileChange}
                            className="hidden"
                        />
                    </div>

                    <div className="mt-4 flex items-center text-xs text-gray-500">
                        <span className="bg-gray-200 rounded-full w-4 h-4 flex items-center justify-center mr-2 font-bold text-gray-600">i</span>
                        Sadece .csv dosyaları. Maksimum 10MB.
                    </div>

                    {error && <div className="mt-4 bg-red-100 text-red-700 p-3 rounded text-sm">{error}</div>}
                    {success && <div className="mt-4 bg-green-100 text-green-700 p-3 rounded text-sm">{success}</div>}

                    <button
                        onClick={handleUpload}
                        disabled={!file || loading}
                        className="w-full mt-6 bg-[#529E72] text-white font-bold py-3 rounded hover:bg-[#428a5f] disabled:opacity-50 flex items-center justify-center"
                    >
                        {loading ? "Yükleniyor..." : (
                            <>
                                <FileSpreadsheet size={18} className="mr-2" /> İçe Aktar
                            </>
                        )}
                    </button>
                </div>
            </div>
        </div>
    );
}

