import { Star, MessageCircle, Quote } from "lucide-react";

const testimonials = [
    {
        name: "Ayşe Yılmaz",
        role: "Üniversite Öğrencisi",
        avatar: "AY",
        content: "YDS'ye hazırlanırken kelime ezberlemek en büyük kabusumdu. WordMaster'ın klasörleme sistemi sayesinde kelimeleri konulara ayırarak çalıştım ve sınavda hedeflediğim puanı aldım.",
        rating: 5
    },
    {
        name: "Mehmet Demir",
        role: "Yazılım Mühendisi",
        avatar: "MD",
        content: "Teknik terimleri akılda tutmak zor olabiliyor. Kendi kelime listelerimi oluşturup boş zamanlarımda pratik yapmak inanılmaz verimli. Arayüzü çok temiz ve hızlı.",
        rating: 5
    },
    {
        name: "Zeynep Kaya",
        role: "İngilizce Öğretmeni",
        avatar: "ZK",
        content: "Öğrencilerime sürekli tavsiye ettiğim bir uygulama. Özellikle 'Bilinmeyenler' listesi özelliği, öğrencilerin eksiklerini görmesi açısından harika.",
        rating: 4
    }
];

export default function Testimonials() {
    return (
        <section className="py-24 bg-white dark:bg-[#121212] overflow-hidden">
            <div className="container mx-auto px-4">
                <div className="text-center max-w-3xl mx-auto mb-16">
                    <h2 className="text-base font-bold text-primary-yellow uppercase tracking-wider mb-2">Başarı Hikayeleri</h2>
                    <h3 className="text-3xl md:text-4xl font-bold text-primary-dark dark:text-white mb-4">
                        Kullanıcılarımız Ne Diyor?
                    </h3>
                    <p className="text-gray-600 dark:text-gray-400">
                        Binlerce kullanıcı dil öğrenme yolculuğunda WordMaster'a güveniyor.
                    </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
                    {testimonials.map((testimonial, index) => (
                        <div
                            key={index}
                            className="bg-gray-50 dark:bg-gray-800 rounded-2xl p-8 relative flex flex-col h-full border border-transparent hover:border-primary-yellow/20 transition-all duration-300 hover:shadow-lg"
                        >
                            <Quote className="absolute top-8 right-8 text-primary-yellow/20" size={48} />

                            <div className="flex gap-1 mb-6">
                                {[...Array(5)].map((_, i) => (
                                    <Star
                                        key={i}
                                        size={16}
                                        className={i < testimonial.rating ? "fill-primary-yellow text-primary-yellow" : "text-gray-300 dark:text-gray-600"}
                                    />
                                ))}
                            </div>

                            <p className="text-gray-700 dark:text-gray-300 mb-8 leading-relaxed italic flex-grow">
                                "{testimonial.content}"
                            </p>

                            <div className="flex items-center gap-4 mt-auto">
                                <div className="w-12 h-12 bg-primary-dark text-white rounded-full flex items-center justify-center font-bold text-lg">
                                    {testimonial.avatar}
                                </div>
                                <div>
                                    <h4 className="font-bold text-primary-dark dark:text-white">{testimonial.name}</h4>
                                    <p className="text-sm text-gray-500 dark:text-gray-400">{testimonial.role}</p>
                                </div>
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </section>
    );
}
