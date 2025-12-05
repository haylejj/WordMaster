import { useEffect, useState } from 'react';
import { cn } from "@/lib/utils";

const WORDS = [
  "Word", "Master", "English", "Learn", "Speak", "Hello", "Book", "Pen",
  "Study", "Read", "Write", "Listen", "Vocabulary", "Grammar", "Success",
  "Future", "Dream", "Global", "Fluency", "Practice", "Smart", "Think"
];
const ITEM_COUNT = 40; // Daha yoğun ama kontrollü

interface FallingItem {
  id: number;
  text: string;
  left: number;
  duration: number;
  delay: number;
  fontSize: number;
  opacity: number;
  isYellow: boolean;
  blur: number;
}

export default function AuthLayout({ children, title }: { children: React.ReactNode; title: string }) {
  const [items, setItems] = useState<FallingItem[]>([]);

  useEffect(() => {
    const newItems: FallingItem[] = [];
    for (let i = 0; i < ITEM_COUNT; i++) {
      const depth = Math.random(); // 0 to 1, 1 is closest
      const duration = Math.floor(Math.random() * 15) + 15; // 15-30s (daha yavaş ve zarif)

      newItems.push({
        id: i,
        text: WORDS[Math.floor(Math.random() * WORDS.length)],
        left: Math.floor(Math.random() * 100),
        duration: duration,
        delay: Math.floor(Math.random() * 30) * -1,
        fontSize: Math.floor(depth * 20) + 12, // Derinliğe göre boyut (12px - 32px)
        opacity: depth * 0.4 + 0.1, // Derinliğe göre opaklık (daha silik arkadakiler)
        isYellow: Math.random() > 0.85, // Sadece %15'i sarı olsun (vurgu için)
        blur: (1 - depth) * 4 // Arkadakiler flu olsun (0px - 4px blur)
      });
    }
    setItems(newItems);
  }, []);

  return (
    <div className="min-h-screen w-full bg-[#121212] flex items-center justify-center font-[Segoe_UI] text-[#121212] overflow-hidden relative selection:bg-primary-yellow selection:text-primary-dark">
      {/* Animated Radial Background Gradient */}
      <div className="fixed inset-0 bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-[#1a1a1a] via-[#121212] to-[#000000] z-0" />

      {/* Falling Words Layer */}
      <div className="fixed inset-0 z-1 overflow-hidden pointer-events-none perspective-1000">
        {items.map((item) => (
          <div
            key={item.id}
            className={cn(
              "absolute top-0 font-bold whitespace-nowrap select-none animate-fall will-change-transform",
              item.isYellow
                ? "text-primary-yellow animate-glow font-extrabold z-10"
                : "text-white z-0"
            )}
            style={{
              left: `${item.left}%`,
              fontSize: `${item.fontSize}px`,
              opacity: item.opacity,
              filter: `blur(${item.blur}px)`,
              animationDuration: `${item.duration}s`,
              animationDelay: `${item.delay}s`,
              // Custom property for keyframe usage if needed
              ['--target-opacity' as any]: item.opacity,
            }}
          >
            {/* Sway animation container */}
            <div
              className="animate-float"
              style={{
                animationDuration: `${item.duration / 3}s`,
                animationDelay: `${item.delay}s`
              }}
            >
              {item.text}
            </div>
          </div>
        ))}
      </div>

      {/* Glassmorphism Auth Card */}
      <div className="relative z-20 w-full max-w-[450px] m-5">
        {/* Glow Effect behind Card */}
        <div className="absolute -inset-1 bg-gradient-to-r from-primary-yellow to-yellow-600 rounded-[16px] blur opacity-20 animate-pulse transition-opacity duration-500 group-hover:opacity-40"></div>

        <div className="relative bg-white/95 backdrop-blur-sm rounded-[15px] shadow-[0_20px_50px_rgba(0,0,0,0.3)] border border-white/10 overflow-hidden">
          {/* Top Decorative Bar */}
          <div className="h-1.5 w-full bg-gradient-to-r from-primary-yellow via-yellow-400 to-yellow-600" />

          {/* Header */}
          <div className="bg-transparent pt-10 px-8 pb-6 text-center">
            <div className="text-[2.5rem] text-primary-dark font-[900] mb-2 inline-block uppercase tracking-[-1px] leading-tight drop-shadow-sm">
              Word<span className="text-transparent bg-clip-text bg-gradient-to-br from-primary-yellow to-yellow-500 filter drop-shadow-sm">Master</span>
            </div>
            <h1 className="text-[1.1rem] text-gray-500 m-0 font-semibold uppercase tracking-[2px] text-xs">
              {title}
            </h1>
          </div>

          {/* Body */}
          <div className="px-8 pb-10">
            {children}
          </div>
        </div>
      </div>
    </div>
  );
}
