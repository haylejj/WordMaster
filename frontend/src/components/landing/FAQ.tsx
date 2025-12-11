
import { useState } from "react";
import { Plus, Minus } from "lucide-react";
import { cn } from "@/lib/utils";

const faqs = [
    {
        question: "WordMaster tamamen ücretsiz mi?",
        answer: "Evet, WordMaster açık kaynaklı bir projedir ve tamamen ücretsizdir. Topluluk odaklı geliştirilen bu projede tüm özelliklere sınırsız erişebilirsiniz."
    },
    {
        question: "Hesabıma farklı cihazlardan erişebilir miyim?",
        answer: "Evet! WordMaster bulut tabanlı çalışır. Hesabınıza giriş yaparak bilgisayar, tablet veya telefonunuzdan kelimelerinize ulaşabilirsiniz."
    },
    {
        question: "Kaç tane kelime ekleyebilirim?",
        answer: "Sınırsız! İstediğiniz kadar kelime ve klasör oluşturarak kütüphanenizi dilediğiniz gibi genişletebilirsiniz. Öğrenme sınırınızı kendiniz belirlersiniz."
    },
    {
        question: "WordMaster'ın diğer uygulamalardan farkı nedir?",
        answer: "WordMaster, sadece kelime ezberletmeye değil, kelimeleri bağlam içinde öğrenmenize ve kalıcı hafızaya atmanıza odaklanır. Kişiselleştirilebilir klasör yapısı ve akıllı tekrar sistemleri ile öğrenme sürecinizi sizin için optimize eder."
    },
    {
        question: "Bilinmeyenler listesi nasıl çalışır?",
        answer: "Pratik yaparken hatırlayamadığınız veya yanlış bildiğiniz kelimeler otomatik olarak veya sizin seçiminizle 'Bilinmeyenler' listesine eklenir. Bu liste, zayıf olduğunuz noktaları güçlendirmeniz için özel olarak tasarlanmıştır."
    },
    {
        question: "İnternet bağlantısı gerekli mi?",
        answer: "Evet, WordMaster web tabanlı bir uygulamadır ve verilerinizin senkronize olması için internet bağlantısına ihtiyaç duyar. Ancak mobil uyumluluğu sayesinde her yerden erişebilirsiniz."
    },
    {
        question: "Excel veya CSV dosyasından kelime aktarabilir miyim?",
        answer: "Kesinlikle! Hazır kelime listelerinizi CSV formatında toplu olarak yükleyebilir, tek tek ekleme zahmetinden kurtulabilirsiniz."
    }
];

export default function FAQ() {
    const [openIndex, setOpenIndex] = useState<number | null>(0);

    return (
        <section id="faq" className="py-24 bg-white dark:bg-[#121212]">
            <div className="container mx-auto px-4 max-w-4xl">
                <div className="text-center mb-16">
                    <h2 className="text-base font-bold text-primary-yellow uppercase tracking-wider mb-2">SSS</h2>
                    <h3 className="text-3xl md:text-4xl font-bold text-primary-dark dark:text-white mb-4">
                        Sıkça Sorulan Sorular
                    </h3>
                    <p className="text-gray-600 dark:text-gray-400">
                        Aklınıza takılan soruların cevaplarını burada bulabilirsiniz.
                    </p>
                </div>

                <div className="space-y-4">
                    {faqs.map((faq, index) => (
                        <div
                            key={index}
                            className={cn(
                                "border rounded-xl transition-all duration-300 overflow-hidden",
                                openIndex === index
                                    ? "border-primary-yellow bg-yellow-50/10 dark:bg-yellow-900/10 shadow-md"
                                    : "border-gray-200 dark:border-gray-800 hover:border-gray-300 dark:hover:border-gray-700"
                            )}
                        >
                            <button
                                onClick={() => setOpenIndex(openIndex === index ? null : index)}
                                className="w-full flex items-center justify-between p-6 text-left focus:outline-none"
                            >
                                <span className={cn(
                                    "text-lg font-semibold",
                                    openIndex === index ? "text-primary-dark dark:text-primary-yellow" : "text-gray-700 dark:text-gray-300"
                                )}>
                                    {faq.question}
                                </span>
                                <span className={cn(
                                    "p-2 rounded-full transition-colors",
                                    openIndex === index ? "bg-primary-yellow text-primary-dark" : "bg-gray-100 dark:bg-gray-800 text-gray-500"
                                )}>
                                    {openIndex === index ? <Minus size={20} /> : <Plus size={20} />}
                                </span>
                            </button>

                            <div
                                className={cn(
                                    "transition-all duration-300 ease-in-out overflow-hidden",
                                    openIndex === index ? "max-h-48 opacity-100" : "max-h-0 opacity-0"
                                )}
                            >
                                <div className="p-6 pt-0 text-gray-600 dark:text-gray-400 leading-relaxed border-t border-transparent">
                                    {faq.answer}
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}
