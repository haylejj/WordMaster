
import { Folder, Zap, PlayCircle, Star, Shield, Smartphone } from "lucide-react";

const features = [
    {
        icon: Folder,
        title: "Akıllı Klasörleme",
        description: "Kelimelerinizi konularına veya zorluk seviyelerine göre klasörlere ayırarak düzenli bir çalışma ortamı oluşturun.",
        color: "text-blue-500",
        bg: "bg-blue-50"
    },
    {
        icon: Zap,
        title: "Etkili Öğrenme",
        description: "Zorlandığınız kelimeleri 'Bilinmeyenler' listesine ekleyin ve ustalaşana kadar tekrar edin.",
        color: "text-yellow-500",
        bg: "bg-yellow-50"
    },
    {
        icon: PlayCircle,
        title: "Pratik Modları",
        description: "İster tüm kelimelerle, ister sadece favorilerinizle pratik yapın. Kendinizi sürekli test edin.",
        color: "text-green-500",
        bg: "bg-green-50"
    },
    {
        icon: Star,
        title: "Favori Listesi",
        description: "En sevdiğiniz veya sık kullandığınız kelimeleri favorilere ekleyerek her an elinizin altında tutun.",
        color: "text-purple-500",
        bg: "bg-purple-50"
    },
    {
        icon: Shield,
        title: "Güvenli Yedekleme",
        description: "Tüm verileriniz bulutta güvenle saklanır. WordMaster hesabınızla her yerden erişim sağlayın.",
        color: "text-red-500",
        bg: "bg-red-50"
    },
    {
        icon: Smartphone,
        title: "Mobil Uyumlu",
        description: "Bilgisayarda, tablette veya telefonda... WordMaster her cihazda kusursuz çalışır.",
        color: "text-indigo-500",
        bg: "bg-indigo-50"
    }
];

export default function Features() {
    return (
        <section id="features" className="py-24 bg-gray-50 dark:bg-[#1a1f24]">
            <div className="container mx-auto px-4">
                <div className="text-center max-w-3xl mx-auto mb-16">
                    <h2 className="text-base font-bold text-primary-yellow uppercase tracking-wider mb-2">Özellikler</h2>
                    <h3 className="text-3xl md:text-4xl font-bold text-primary-dark dark:text-white mb-4">
                        Dil Öğreniminizi Bir Üst Seviyeye Taşıyın
                    </h3>
                    <p className="text-gray-600 dark:text-gray-400">
                        WordMaster, kelime ezberleme sürecinizi optimize etmek için ihtiyacınız olan tüm araçları sunar.
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-8">
                    {features.map((feature, index) => (
                        <div
                            key={index}
                            className="group bg-white dark:bg-gray-800 rounded-2xl p-8 shadow-sm hover:shadow-xl transition-all duration-300 hover:-translate-y-1 border border-transparent hover:border-primary-yellow/20"
                        >
                            <div className={`w-14 h-14 rounded-xl ${feature.bg} ${feature.color} flex items-center justify-center mb-6 group-hover:scale-110 transition-transform duration-300`}>
                                <feature.icon size={28} />
                            </div>
                            <h4 className="text-xl font-bold text-primary-dark dark:text-white mb-3">
                                {feature.title}
                            </h4>
                            <p className="text-gray-500 dark:text-gray-400 leading-relaxed">
                                {feature.description}
                            </p>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}
