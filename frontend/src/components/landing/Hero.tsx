
import { Link, useNavigate } from "react-router-dom";
import { ArrowRight, BookOpen, Sparkles, Github } from "lucide-react";
import { Button } from "@/components/ui/button";

export default function Hero() {
    const navigate = useNavigate();
    return (
        <section className="relative overflow-hidden bg-white dark:bg-primary-dark pt-32 pb-20 lg:pt-48 lg:pb-32">
            {/* Background Decor */}
            <div className="absolute top-0 left-0 w-full h-full overflow-hidden z-0 pointer-events-none">
                <div className="absolute -top-[10%] -right-[10%] w-[500px] h-[500px] rounded-full bg-primary-yellow/10 blur-[100px]" />
                <div className="absolute top-[20%] -left-[10%] w-[400px] h-[400px] rounded-full bg-blue-500/10 blur-[100px]" />
            </div>

            <div className="container mx-auto px-4 relative z-10 text-center">
                <div className="inline-flex items-center space-x-2 bg-yellow-50 dark:bg-yellow-900/20 px-4 py-2 rounded-full mb-8 animate-fade-in-up">
                    <span className="relative flex h-3 w-3">
                        <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary-yellow opacity-75"></span>
                        <span className="relative inline-flex rounded-full h-3 w-3 bg-primary-yellow"></span>
                    </span>
                    <span className="text-sm font-bold text-primary-dark dark:text-primary-yellow uppercase tracking-wide">v1.0 - Şimdi Açık Kaynak!</span>
                </div>

                <h1 className="text-5xl md:text-7xl font-black text-primary-dark dark:text-white mb-6 leading-tight tracking-tight">
                    Kelime Hazinenizi <br />
                    <span className="text-transparent bg-clip-text bg-gradient-to-r from-primary-yellow to-yellow-600 relative">
                        Özgürce Geliştirin
                        <svg className="absolute w-full h-3 -bottom-1 left-0 text-primary-yellow opacity-40" viewBox="0 0 100 10" preserveAspectRatio="none">
                            <path d="M0 5 Q 50 10 100 5" stroke="currentColor" strokeWidth="8" fill="none" />
                        </svg>
                    </span>
                </h1>

                <p className="text-xl text-gray-600 dark:text-gray-300 mb-10 leading-relaxed max-w-2xl mx-auto">
                    Binlerce kelimeyi modern tekniklerle öğrenin. Tamamen ücretsiz, açık kaynak ve topluluk destekli.
                </p>

                <div className="flex flex-col sm:flex-row items-center justify-center space-y-4 sm:space-y-0 sm:space-x-4">
                    <button
                        onClick={() => navigate("/register")}
                        className="w-full sm:w-auto px-8 py-4 bg-primary-dark dark:bg-white text-white dark:text-primary-dark rounded-xl font-bold text-lg hover:bg-gray-800 dark:hover:bg-gray-200 transition-all duration-300 transform hover:-translate-y-1 shadow-lg hover:shadow-xl flex items-center justify-center group"
                    >
                        Hemen Başla
                        <ArrowRight className="ml-2 group-hover:translate-x-1 transition-transform" />
                    </button>
                    <a
                        href="https://github.com/haylejj/WordMaster"
                        target="_blank"
                        rel="noopener noreferrer"
                        className="w-full sm:w-auto px-8 py-4 bg-white dark:bg-transparent border-2 border-gray-200 dark:border-gray-700 text-gray-700 dark:text-white rounded-xl font-bold text-lg hover:bg-gray-50 dark:hover:bg-gray-800 transition-all duration-300 flex items-center justify-center"
                    >
                        <Github className="mr-2" size={20} />
                        GitHub'da İncele
                    </a>
                </div>

                {/* Stats or Floating Cards */}
                <div className="mt-20 flex flex-wrap justify-center gap-8 animate-float">
                    <div className="bg-white dark:bg-gray-800 p-4 rounded-xl shadow-xl border border-gray-100 dark:border-gray-700 flex items-center space-x-3">
                        <div className="bg-green-100 p-2 rounded-lg text-green-600">
                            <BookOpen size={24} />
                        </div>
                        <div className="text-left">
                            <p className="text-xs text-gray-500">Öğrenilen Kelime</p>
                            <p className="font-bold text-lg">10,000+</p>
                        </div>
                    </div>
                    <div className="bg-white dark:bg-gray-800 p-4 rounded-xl shadow-xl border border-gray-100 dark:border-gray-700 flex items-center space-x-3">
                        <div className="bg-purple-100 p-2 rounded-lg text-purple-600">
                            <Sparkles size={24} />
                        </div>
                        <div className="text-left">
                            <p className="text-xs text-gray-500">Aktif Kullanıcı</p>
                            <p className="font-bold text-lg">500+</p>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    );
}
