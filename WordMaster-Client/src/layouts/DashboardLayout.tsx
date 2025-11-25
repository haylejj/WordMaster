import { useEffect, useState } from "react";
import { Outlet, Link, useLocation, useNavigate } from "react-router-dom";
import { Book, Plus, Star, HelpCircle, PlayCircle, User, LogOut, Settings, Key, ChevronDown, Folder } from "lucide-react";
import { authService } from "@/services/auth.service";
import { cn } from "@/lib/utils";

export default function DashboardLayout() {
  const location = useLocation();
  const navigate = useNavigate();
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);

  useEffect(() => {
    let isMounted = true;
    const checkSession = async () => {
      try {
        await authService.checkSession();
      } catch (error) {
        if (isMounted) {
          console.error("Session check failed", error);
        }
      }
    };
    checkSession();
    const intervalId = window.setInterval(checkSession, 5 * 60 * 1000);
    return () => {
      isMounted = false;
      window.clearInterval(intervalId);
    };
  }, []);

  const handleLogout = async () => {
    try {
      await authService.logout();
    } finally {
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      navigate("/login");
    }
  };

  const navItems = [
    { name: "Kelimelerim", icon: Book, path: "/" },
    { name: "Klasörlerim", icon: Folder, path: "/folders" },
    { name: "Favoriler", icon: Star, path: "/favorites" },
    { name: "Bilinmeyenler", icon: HelpCircle, path: "/unknowns" },
  ];

  return (
    <div className="min-h-screen bg-[#f4f6f8]">
      {/* Top Navigation Bar */}
      <header className="bg-[#1a1f24] text-white h-16 flex items-center justify-between px-6 shadow-md fixed w-full z-50">
        <div className="flex items-center space-x-8">
          <nav className="flex items-center space-x-2">
            {navItems.map((item) => {
              const isActive = location.pathname === item.path;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className={cn(
                    "flex items-center px-3 py-2 rounded-md text-sm font-medium transition-colors",
                    isActive
                      ? "text-white bg-[#2c333a]"
                      : "text-gray-400 hover:text-white hover:bg-[#2c333a]"
                  )}
                >
                  <item.icon size={18} className="mr-2" />
                  {item.name}
                </Link>
              );
            })}

            <Link
              to="/add-word"
              className={cn(
                "flex items-center px-3 py-2 rounded-md text-sm font-medium transition-colors",
                location.pathname === "/add-word"
                  ? "text-white bg-[#2c333a]"
                  : "text-gray-400 hover:text-white hover:bg-[#2c333a]"
              )}
            >
              <Plus size={18} className="mr-2" />
              Yeni Kelime Ekle
            </Link>
          </nav>

          <div className="relative group">
            <button className="flex items-center px-4 py-2 bg-primary-yellow text-primary-dark rounded-full text-sm font-bold hover:bg-[#FFC107] transition-colors">
              <PlayCircle size={18} className="mr-2" />
              Pratik Yap
              <ChevronDown size={16} className="ml-2" />
            </button>
          </div>
        </div>

        <div className="flex items-center space-x-4">
          <div className="relative">
            <button
              onClick={() => setIsUserMenuOpen(!isUserMenuOpen)}
              className="flex items-center space-x-2 text-sm font-medium text-gray-300 hover:text-white transition-colors focus:outline-none"
            >
              <User size={20} />
              <span>HAYLEJJ</span> {/* Mock user name */}
              <ChevronDown size={16} />
            </button>

            {isUserMenuOpen && (
              <div className="absolute right-0 mt-2 w-56 bg-white rounded-md shadow-lg py-1 z-50 border border-gray-200">
                <Link to="/profile" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center">
                  <Settings size={16} className="mr-2" /> Update Your Profile
                </Link>
                <Link to="/change-password" className="block px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 flex items-center">
                  <Key size={16} className="mr-2" /> Password Change
                </Link>
                <div className="border-t border-gray-100 my-1"></div>
                <button
                  onClick={handleLogout}
                  className="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-gray-50 flex items-center"
                >
                  <LogOut size={16} className="mr-2" /> Log Out
                </button>
              </div>
            )}
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="pt-20 px-6 pb-8 max-w-[1600px] mx-auto">
        <Outlet />
      </main>
    </div>
  );
}
