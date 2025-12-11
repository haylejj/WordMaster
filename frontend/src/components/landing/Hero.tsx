
import { Link } from "react-router-dom";
import { ArrowRight, BookOpen, Sparkles } from "lucide-react";
import { Button } from "@/components/ui/button"; 

export default function Hero() {
  return (
    <section className="relative overflow-hidden bg-white dark:bg-primary-dark pt-32 pb-20 lg:pt-48 lg:pb-32">
      {/* Background Decor */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden z-0 pointer-events-none">
        <div className="absolute -top-[10%] -right-[10%] w-[500px] h-[500px] rounded-full bg-primary-yellow/10 blur-[100px]" />
        <div className="absolute top-[20%] -left-[10%] w-[400px] h-[400px] rounded-full bg-blue-500/10 blur-[100px]" />
      </div>

      <div className="container mx-auto px-4 relative z-10 text-center">
        <div className="inline-flex items-center rounded-full border px-3 py-1 text-sm text-muted-foreground mb-8 backdrop-blur-sm bg-white/30 dark:bg-black/30">
          <span className="flex h-2 w-2 rounded-full bg-primary-yellow mr-2 animate-pulse"></span>
          WordMaster ile Dil Öğrenimini Hızlandır
        </div>
        
        <h1 className="text-5xl md:text-7xl font-bold tracking-tight text-primary-dark dark:text-white mb-6">
          Kelimeleri <span className="text-primary-yellow relative inline-block">
            Ustalıkla
            <svg className="absolute w-full h-3 -bottom-1 left-0 text-primary-yellow opacity-50" viewBox="0 0 100 10" preserveAspectRatio="none">
              <path d="M0 5 Q 50 10 100 5" stroke="currentColor" strokeWidth="3" fill="none" />
            </svg>
          </span> Öğrenin
        </h1>
        
        <p className="text-xl text-gray-600 dark:text-gray-300 max-w-2xl mx-auto mb-10 leading-relaxed">
          Kelimeleri ezberlemek hiç bu kadar kolay olmamıştı. Akıllı tekrar sistemi, klasörleme ve pratik modları ile yabancı dil serüveninizde yanınızdayız.
        </p>
        
        <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
          <Link to="/register">
            <button className="px-8 py-4 bg-primary-yellow text-primary-dark font-bold rounded-xl text-lg hover:bg-yellow-400 hover:scale-105 transition-all shadow-[0_4px_14px_0_rgba(255,215,0,0.39)] flex items-center">
              Hemen Başla <ArrowRight className="ml-2 h-5 w-5" />
            </button>
          </Link>
          <Link to="/login">
            <button className="px-8 py-4 bg-white dark:bg-gray-800 text-primary-dark dark:text-white font-semibold rounded-xl text-lg border-2 border-transparent hover:border-primary-yellow transition-all flex items-center shadow-lg">
              Giriş Yap
            </button>
          </Link>
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
