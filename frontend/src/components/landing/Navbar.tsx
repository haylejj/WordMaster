
import { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { BookOpen, Menu, X } from "lucide-react";
import { cn } from "@/lib/utils";

export default function Navbar() {
    const [isScrolled, setIsScrolled] = useState(false);
    const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        const handleScroll = () => {
            setIsScrolled(window.scrollY > 20);
        };
        window.addEventListener("scroll", handleScroll);
        return () => window.removeEventListener("scroll", handleScroll);
    }, []);

    const scrollToSection = (id: string) => {
        setMobileMenuOpen(false);
        const element = document.getElementById(id);
        if (element) {
            const headerOffset = 80;
            const elementPosition = element.getBoundingClientRect().top;
            const offsetPosition = elementPosition + window.pageYOffset - headerOffset;
            window.scrollTo({
                top: offsetPosition,
                behavior: "smooth"
            });
        }
    };

    return (
        <nav
            className={cn(
                "fixed top-0 left-0 w-full z-50 transition-all duration-300",
                isScrolled
                    ? "bg-white/80 dark:bg-[#1a1f24]/90 backdrop-blur-md shadow-md py-4"
                    : "bg-transparent py-6"
            )}
        >
            <div className="container mx-auto px-4 flex items-center justify-between">
                <div className="flex items-center space-x-2">
                    <Link to="/" className="flex items-center space-x-2">
                        <div className="bg-primary-yellow p-1 rounded-lg">
                            <img src="/logo2.png" alt="WordMaster Logo" className="w-8 h-8 object-contain" />
                        </div>
                        <span className="text-2xl font-bold text-primary-dark dark:text-white tracking-tight">
                            WordMaster
                        </span>
                    </Link>
                </div>

                {/* Desktop Menu */}
                <div className="hidden md:flex items-center space-x-8">
                    <button
                        onClick={() => scrollToSection("features")}
                        className="text-sm font-medium text-gray-600 dark:text-gray-300 hover:text-primary-yellow transition-colors"
                    >
                        Özellikler
                    </button>
                    <button
                        onClick={() => scrollToSection("faq")}
                        className="text-sm font-medium text-gray-600 dark:text-gray-300 hover:text-primary-yellow transition-colors"
                    >
                        SSS
                    </button>
                    <div className="h-5 w-px bg-gray-300 dark:bg-gray-700 mx-2"></div>
                    <Link to="/login" className="text-sm font-bold text-gray-700 dark:text-gray-200 hover:text-primary-yellow transition-colors">
                        Giriş Yap
                    </Link>
                    <Link to="/register">
                        <button className="bg-primary-dark dark:bg-white dark:text-primary-dark text-white px-5 py-2.5 rounded-full text-sm font-bold hover:bg-gray-800 dark:hover:bg-gray-200 transition-colors shadow-lg hover:shadow-xl hover:-translate-y-0.5 transform duration-200">
                            Ücretsiz Başla
                        </button>
                    </Link>
                </div>

                {/* Mobile Menu Button */}
                <button
                    className="md:hidden text-primary-dark dark:text-white"
                    onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
                >
                    {mobileMenuOpen ? <X size={28} /> : <Menu size={28} />}
                </button>

                {/* Mobile Menu */}
                {mobileMenuOpen && (
                    <div className="absolute top-full left-0 w-full bg-white dark:bg-[#1a1f24] shadow-xl border-t border-gray-100 dark:border-gray-800 p-6 flex flex-col space-y-6 md:hidden animate-in slide-in-from-top-5 duration-200">
                        <button
                            onClick={() => scrollToSection("features")}
                            className="text-lg font-medium text-left text-gray-800 dark:text-white"
                        >
                            Özellikler
                        </button>
                        <button
                            onClick={() => scrollToSection("faq")}
                            className="text-lg font-medium text-left text-gray-800 dark:text-white"
                        >
                            SSS
                        </button>
                        <hr className="border-gray-100 dark:border-gray-800" />
                        <Link
                            to="/login"
                            className="text-lg font-bold text-left text-gray-800 dark:text-white"
                            onClick={() => setMobileMenuOpen(false)}
                        >
                            Giriş Yap
                        </Link>
                        <Link
                            to="/register"
                            onClick={() => setMobileMenuOpen(false)}
                        >
                            <button className="w-full bg-primary-yellow text-primary-dark py-3 rounded-lg font-bold text-lg">
                                Ücretsiz Başla
                            </button>
                        </Link>
                    </div>
                )}
            </div>
        </nav>
    );
}
