
import Navbar from "@/components/landing/Navbar";
import Hero from "@/components/landing/Hero";
import Features from "@/components/landing/Features";
import HowItWorks from "@/components/landing/HowItWorks";
import Testimonials from "@/components/landing/Testimonials";
import FAQ from "@/components/landing/FAQ";
import Footer from "@/components/landing/Footer";

export default function LandingPage() {
    return (
        <div className="min-h-screen bg-white dark:bg-[#121212] font-sans selection:bg-primary-yellow selection:text-primary-dark">
            <Navbar />
            <Hero />
            <Features />
            <HowItWorks />
            <Testimonials />
            <FAQ />
            <Footer />
        </div>
    );
}
