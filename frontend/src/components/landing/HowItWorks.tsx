import { UserPlus, FileUp, FolderPlus, BrainCircuit } from "lucide-react";

const steps = [
    {
        icon: UserPlus,
        title: "1. Ücretsiz Kayıt Ol",
        description: "Saniyeler içinde hesabını oluştur ve kişisel kelime hazineni oluşturmaya başla."
    },
    {
        icon: FileUp,
        title: "2. Kelimeleri Ekle",
        description: "İster tek tek kelime ekle, istersen elindeki Excel/CSV listelerini tek tıkla içeri aktar.",
        highlight: true // Special styling for this step
    },
    {
        icon: FolderPlus,
        title: "3. Klasörlerini Düzenle",
        description: "Kelimelerini konulara, zorluk seviyelerine veya kaynaklara göre klasörlere ayır.",
    },
    {
        icon: BrainCircuit,
        title: "4. Pratik Yap",
        description: "Kartlar, çoktan seçmeli testler ve yazma alıştırmaları ile kelimeleri kalıcı hafızana at.",
    }
];

export default function HowItWorks() {
    return (
        <section className="py-24 bg-white dark:bg-[#121212]">
            <div className="container mx-auto px-4">
                <div className="text-center max-w-3xl mx-auto mb-16">
                    <h2 className="text-base font-bold text-primary-yellow uppercase tracking-wider mb-2">Nasıl Çalışır?</h2>
                    <h3 className="text-3xl md:text-4xl font-bold text-primary-dark dark:text-white mb-4">
                        4 Adımda Kelime Avcısı Ol
                    </h3>
                    <p className="text-gray-600 dark:text-gray-400">
                        WordMaster ile dil öğrenmek hiç bu kadar sistematik ve kolay olmamıştı.
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 relative">
                    {/* Connecting Line for Desktop */}
                    <div className="hidden lg:block absolute top-12 left-0 w-full h-0.5 bg-gray-100 dark:bg-gray-800 -z-10" />

                    {steps.map((step, index) => (
                        <div key={index} className="relative group">
                            <div className={`w-24 h-24 mx-auto bg-white dark:bg-[#1a1f24] rounded-full border-4 ${step.highlight ? 'border-primary-yellow shadow-[0_0_20px_rgba(255,215,0,0.3)]' : 'border-gray-100 dark:border-gray-800'} flex items-center justify-center mb-6 transition-all duration-300 group-hover:scale-110 group-hover:border-primary-yellow z-10`}>
                                <step.icon
                                    size={32}
                                    className={`${step.highlight ? 'text-primary-dark' : 'text-gray-400'} group-hover:text-primary-yellow transition-colors duration-300`}
                                />
                            </div>

                            <div className="text-center px-2">
                                <h4 className="text-xl font-bold text-primary-dark dark:text-white mb-3">
                                    {step.title}
                                </h4>
                                <p className="text-gray-500 dark:text-gray-400 text-sm leading-relaxed">
                                    {step.description}
                                </p>
                            </div>

                            {/* Mobile Connector */}
                            {index < steps.length - 1 && (
                                <div className="lg:hidden w-0.5 h-12 bg-gray-100 dark:bg-gray-800 mx-auto my-4" />
                            )}
                        </div>
                    ))}
                </div>

                {/* Import Feature Visual Highlight */}
                <div className="mt-20 bg-gray-50 dark:bg-[#1a1f24] rounded-3xl p-8 md:p-12 border border-gray-100 dark:border-gray-800 flex flex-col md:flex-row items-center gap-12">
                    <div className="flex-1 space-y-6">
                        <div className="inline-flex items-center gap-2 px-4 py-2 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-400 rounded-full text-sm font-bold">
                            <FileUp size={16} />
                            <span>Toplu Aktarım Özelliği</span>
                        </div>
                        <h4 className="text-3xl font-bold text-primary-dark dark:text-white">
                            Listeniz Hazır mı? <br />
                            <span className="text-primary-yellow">Saniyeler İçinde Aktarın</span>
                        </h4>
                        <p className="text-gray-600 dark:text-gray-400 text-lg">
                            Elinizde Excel veya CSV formatında binlerce kelime mi var? Tek tek girmekle uğraşmayın. WordMaster'ın akıllı içe aktarma aracı ile tüm listenizi anında çalışma setine dönüştürün.
                        </p>
                    </div>

                    {/* Abstract Visual Representation of Import */}
                    <div className="flex-1 flex justify-center w-full">
                        <div className="relative w-full max-w-sm aspect-video bg-white dark:bg-gray-900 rounded-xl shadow-2xl border border-gray-200 dark:border-gray-700 p-6 flex flex-col gap-4 rotate-3 hover:rotate-0 transition-transform duration-500">
                            <div className="flex items-center gap-3 border-b border-gray-100 dark:border-gray-800 pb-4">
                                <div className="w-3 h-3 rounded-full bg-red-500" />
                                <div className="w-3 h-3 rounded-full bg-yellow-500" />
                                <div className="w-3 h-3 rounded-full bg-green-500" />
                                <span className="ml-auto text-xs text-gray-400">kelimeler.csv</span>
                            </div>
                            <div className="space-y-3">
                                {[1, 2, 3].map((i) => (
                                    <div key={i} className="flex gap-4 opacity-50">
                                        <div className="h-4 w-1/3 bg-gray-100 dark:bg-gray-800 rounded animate-pulse" />
                                        <div className="h-4 w-1/3 bg-gray-100 dark:bg-gray-800 rounded animate-pulse delay-75" />
                                        <div className="h-4 w-1/4 bg-gray-100 dark:bg-gray-800 rounded animate-pulse delay-150" />
                                    </div>
                                ))}
                                <div className="flex justify-center mt-4">
                                    <div className="w-12 h-12 bg-primary-yellow rounded-full flex items-center justify-center shadow-lg animate-bounce">
                                        <FileUp className="text-primary-dark" size={24} />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}
