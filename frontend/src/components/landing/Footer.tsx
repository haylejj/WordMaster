
import { useState } from "react";
import { Link } from "react-router-dom";
import { Github, Twitter, Linkedin, X } from "lucide-react";

export default function Footer() {
    const [showTerms, setShowTerms] = useState(false);

    return (
        <>
            <footer className="bg-[#121212] text-white pt-16 pb-8 border-t border-gray-800">
                <div className="container mx-auto px-4">
                    <div className="grid grid-cols-1 md:grid-cols-4 gap-12 mb-12">
                        <div className="col-span-1 md:col-span-1">
                            <div className="space-y-6">
                                <div className="flex items-center space-x-2">
                                    <div className="bg-primary-yellow p-1 rounded-lg">
                                        <img src="/logo2.png" alt="WordMaster Logo" className="w-8 h-8 object-contain" />
                                    </div>
                                    <span className="text-2xl font-bold text-primary-dark dark:text-white tracking-tight">wordmaster</span>
                                </div>
                                <p className="text-gray-500 dark:text-gray-400 leading-relaxed">
                                    Kelime hazinenizi geliştirmek için geliştirilmiş, topluluk destekli ve tamamen açık kaynaklı modern bir platform. Dil öğrenimini herkes için erişilebilir kılıyoruz.
                                </p>
                                <div className="flex items-center gap-2">
                                    <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                        🟢 Open Source
                                    </span>
                                    <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                                        MIT License
                                    </span>
                                </div>
                                <div className="flex space-x-4">
                                    <a href="https://github.com/your-repo" target="_blank" rel="noopener noreferrer" className="bg-gray-800 p-2 rounded-full hover:bg-primary-yellow hover:text-black transition-colors">
                                        <Github size={18} />
                                    </a>
                                    <a href="#" className="bg-gray-800 p-2 rounded-full hover:bg-primary-yellow hover:text-black transition-colors">
                                        <Twitter size={18} />
                                    </a>
                                    <a href="#" className="bg-gray-800 p-2 rounded-full hover:bg-primary-yellow hover:text-black transition-colors">
                                        <Linkedin size={18} />
                                    </a>
                                </div>
                            </div>
                        </div>

                        <div>
                            <h4 className="font-bold text-lg mb-6 text-gray-100">Hızlı Erişim</h4>
                            <ul className="space-y-3">
                                <li><Link to="/" className="text-gray-400 hover:text-primary-yellow transition-colors text-sm">Ana Sayfa</Link></li>
                                <li><Link to="/login" className="text-gray-400 hover:text-primary-yellow transition-colors text-sm">Giriş Yap</Link></li>
                                <li><Link to="/register" className="text-gray-400 hover:text-primary-yellow transition-colors text-sm">Kayıt Ol</Link></li>
                            </ul>
                        </div>

                        <div>
                            <h4 className="font-bold text-lg mb-6 text-gray-100">Destek</h4>
                            <ul className="space-y-3">
                                <li><a href="#faq" className="text-gray-400 hover:text-primary-yellow transition-colors text-sm">SSS</a></li>
                                <li><a href="mailto:support@wordmaster.com" className="text-gray-400 hover:text-primary-yellow transition-colors text-sm">İletişim</a></li>
                                <li><button onClick={() => setShowTerms(true)} className="text-gray-400 hover:text-primary-yellow transition-colors text-sm text-left">Kullanım Şartları</button></li>
                            </ul>
                        </div>

                        <div>
                            <h4 className="font-bold text-lg mb-6 text-gray-100">Bülten</h4>
                            <p className="text-gray-400 text-sm mb-4">Yeniliklerden haberdar olmak için abone olun.</p>
                            <div className="flex">
                                <input type="email" placeholder="Email adresiniz" className="bg-gray-800 text-white px-4 py-2 rounded-l-lg w-full focus:outline-none focus:ring-1 focus:ring-primary-yellow font-sans text-sm" />
                                <button className="bg-primary-yellow text-primary-dark font-bold px-4 py-2 rounded-r-lg hover:bg-yellow-400 transition-colors">
                                    <ArrowRightIcon />
                                </button>
                            </div>
                        </div>
                    </div>

                    <div className="pt-8 flex flex-col md:flex-row justify-between items-center text-sm text-gray-500 dark:text-gray-400">
                        <p>&copy; {new Date().getFullYear()} WordMaster. Tüm hakları saklıdır (MIT Lisansı altında açık kaynak).</p>
                        <div className="flex space-x-6 mt-4 md:mt-0">
                            <button onClick={() => setShowTerms(true)} className="hover:text-gray-300">Gizlilik Politikası</button>
                            <button onClick={() => setShowTerms(true)} className="hover:text-gray-300">Kullanım Şartları</button>
                        </div>
                    </div>
                </div>
            </footer>

            {/* Terms Modal */}
            {showTerms && (
                <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
                    <div className="bg-white dark:bg-[#1a1f24] w-full max-w-3xl rounded-2xl shadow-2xl overflow-hidden max-h-[80vh] flex flex-col">
                        <div className="p-6 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center bg-gray-50 dark:bg-[#1e242b]">
                            <h3 className="text-2xl font-bold text-primary-dark dark:text-white">Kullanım Şartları ve Gizlilik</h3>
                            <button
                                onClick={() => setShowTerms(false)}
                                className="p-2 hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full transition-colors"
                            >
                                <X size={24} className="text-gray-500" />
                            </button>
                        </div>
                        <div className="p-8 overflow-y-auto text-gray-600 dark:text-gray-300 space-y-6 leading-relaxed">
                            <div>
                                <h4 className="text-lg font-bold text-primary-dark dark:text-white mb-2">1. Genel Hükümler</h4>
                                <p>WordMaster servisine hoş geldiniz. Bu hizmeti kullanarak aşağıdaki şartları kabul etmiş sayılırsınız. Hizmetimiz, kişisel kelime öğreniminizi desteklemek amacıyla sunulmaktadır.</p>
                            </div>

                            <div>
                                <h4 className="text-lg font-bold text-primary-dark dark:text-white mb-2">2. Kullanıcı Sorumlulukları</h4>
                                <p>Kullanıcılar, hesap bilgilerinin güvenliğinden sorumludur. Oluşturulan içeriklerin yasalara uygun olması gerekmektedir. Sistemimize zarar verecek faaliyetlerde bulunmak yasaktır.</p>
                            </div>

                            <div>
                                <h4 className="text-lg font-bold text-primary-dark dark:text-white mb-2">3. Gizlilik</h4>
                                <p>Kişisel verileriniz KVKK kapsamında korunmaktadır. Verileriniz, hizmet kalitesini artırmak dışında üçüncü şahıslarla paylaşılmaz. Çerezler, site deneyimini iyileştirmek için kullanılabilir.</p>
                            </div>

                            <div>
                                <h4 className="text-lg font-bold text-primary-dark dark:text-white mb-2">4. Hizmet Değişiklikleri</h4>
                                <p>WordMaster, özellikleri haber vermeksizin değiştirme veya sonlandırma hakkını saklı tutar. Ancak, kullanıcı verilerinin güvenliği önceliğimizdir.</p>
                            </div>

                            <div>
                                <h4 className="text-lg font-bold text-primary-dark dark:text-white mb-2">5. Üyelik İptali</h4>
                                <p>Kullanıcılar diledikleri zaman üyeliklerini silebilirler. Silinen hesapların verileri belirli bir süre sonra kalıcı olarak temizlenir.</p>
                            </div>
                        </div>
                        <div className="p-6 border-t border-gray-100 dark:border-gray-700 bg-gray-50 dark:bg-[#1e242b] flex justify-end">
                            <button
                                onClick={() => setShowTerms(false)}
                                className="px-6 py-2 bg-primary-dark text-white rounded-lg hover:bg-primary-gray transition-colors font-medium"
                            >
                                Anladım, Kapat
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}

function ArrowRightIcon() {
    return (
        <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
            <path d="M5 12h14"></path>
            <path d="m12 5 7 7-7 7"></path>
        </svg>
    )
}
