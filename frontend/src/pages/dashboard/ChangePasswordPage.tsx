import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { Key, KeyRound, Lock, ShieldCheck, Save, Loader2, AlertCircle, CheckCircle, Eye, EyeOff, Check, X } from "lucide-react";
import { changePasswordSchema, type ChangePasswordRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn, getErrorMessage } from "@/lib/utils";

const passwordRequirements = [
  { id: "length", label: "En az 6 karakter", test: (pwd: string) => pwd.length >= 6 },
  { id: "uppercase", label: "En az bir büyük harf (A-Z)", test: (pwd: string) => /[A-Z]/.test(pwd) },
  { id: "lowercase", label: "En az bir küçük harf (a-z)", test: (pwd: string) => /[a-z]/.test(pwd) },
  { id: "number", label: "En az bir rakam (0-9)", test: (pwd: string) => /\d/.test(pwd) },
  { id: "special", label: "En az bir özel karakter (!@#$%^&*)", test: (pwd: string) => /[^a-zA-Z0-9]/.test(pwd) },
];

export default function ChangePasswordPage() {
  const navigate = useNavigate();
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [showOldPassword, setShowOldPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [showSuccessModal, setShowSuccessModal] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    watch,
    formState: { errors },
  } = useForm<ChangePasswordRequest>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      oldPassword: "",
      newPassword: "",
      confirmNewPassword: "",
    },
  });

  const newPassword = watch("newPassword") || "";

  const onSubmit = async (data: ChangePasswordRequest) => {
    setSaving(true);
    setSuccessMessage(null);
    setErrorMessage(null);

    try {
      const response = await authService.changePassword(data);
      if (response.isSuccess) {
        setShowSuccessModal(true);
        reset();
      } else {
        setErrorMessage(response.errorList?.join(", ") || "Şifre değiştirme başarısız oldu.");
      }
    } catch (error) {
      console.error("Password change failed", error);
      setErrorMessage(getErrorMessage(error));
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    window.history.back();
  };

  const handleSuccessModalClose = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    navigate("/login");
  };

  useEffect(() => {
    if (showSuccessModal) {
      const timer = setTimeout(() => {
        handleSuccessModalClose();
      }, 3500);
      return () => clearTimeout(timer);
    }
  }, [showSuccessModal, navigate]);

  return (
    <div className="max-w-2xl mx-auto">
      <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
        {/* Header */}
        <div className="bg-[#1a1f24] px-8 py-6">
          <h1 className="text-xl font-bold text-white flex items-center justify-center gap-3">
            <Key className="text-primary-yellow" size={24} />
            Şifre Değiştir
          </h1>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="p-8 space-y-5">
          {/* Success Message */}
          {successMessage && (
            <div className="flex items-center gap-3 p-4 bg-green-50 border border-green-200 rounded-xl text-green-700">
              <CheckCircle size={20} />
              <span className="text-sm font-medium">{successMessage}</span>
            </div>
          )}

          {/* Error Message */}
          {errorMessage && (
            <div className="flex items-center gap-3 p-4 bg-red-50 border border-red-200 rounded-xl text-red-700">
              <AlertCircle size={20} />
              <span className="text-sm font-medium">{errorMessage}</span>
            </div>
          )}

          {/* Old Password */}
          <div className="space-y-1">
            <div
              className={cn(
                "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                errors.oldPassword ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
              )}
            >
              <KeyRound className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Mevcut Şifre</label>
                <input
                  {...register("oldPassword")}
                  type={showOldPassword ? "text" : "password"}
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                  placeholder="Mevcut şifrenizi girin"
                />
              </div>
              <button
                type="button"
                onClick={() => setShowOldPassword(!showOldPassword)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                {showOldPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.oldPassword && <p className="text-xs text-red-500 ml-4">{errors.oldPassword.message}</p>}
          </div>

          {/* New Password */}
          <div className="space-y-1">
            <div
              className={cn(
                "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                errors.newPassword ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
              )}
            >
              <Lock className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Yeni Şifre</label>
                <input
                  {...register("newPassword")}
                  type={showNewPassword ? "text" : "password"}
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                  placeholder="Yeni şifrenizi girin"
                />
              </div>
              <button
                type="button"
                onClick={() => setShowNewPassword(!showNewPassword)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.newPassword && <p className="text-xs text-red-500 ml-4">{errors.newPassword.message}</p>}
          </div>

          {/* Confirm New Password */}
          <div className="space-y-1">
            <div
              className={cn(
                "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                errors.confirmNewPassword ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
              )}
            >
              <ShieldCheck className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Yeni Şifre (Tekrar)</label>
                <input
                  {...register("confirmNewPassword")}
                  type={showConfirmPassword ? "text" : "password"}
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                  placeholder="Yeni şifrenizi tekrar girin"
                />
              </div>
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
            {errors.confirmNewPassword && <p className="text-xs text-red-500 ml-4">{errors.confirmNewPassword.message}</p>}
          </div>

          {/* Password Requirements - Live Check */}
          <div className="bg-gray-50 rounded-xl p-4">
            <p className="text-sm font-medium text-gray-700 mb-3">Şifre gereksinimleri:</p>
            <ul className="space-y-2">
              {passwordRequirements.map((req) => {
                const passed = req.test(newPassword);
                return (
                  <li
                    key={req.id}
                    className={cn(
                      "flex items-center gap-2 text-xs transition-colors",
                      passed ? "text-green-600" : "text-gray-400"
                    )}
                  >
                    {passed ? (
                      <Check size={14} className="text-green-500" />
                    ) : (
                      <X size={14} className="text-gray-300" />
                    )}
                    <span className={passed ? "font-medium" : ""}>{req.label}</span>
                  </li>
                );
              })}
            </ul>
          </div>

          {/* Buttons */}
          <div className="flex flex-col gap-3 pt-4">
            <button
              type="submit"
              disabled={saving}
              className="w-full flex items-center justify-center gap-2 bg-primary-yellow text-primary-dark font-bold py-3 rounded-xl hover:bg-[#FFC107] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? (
                <>
                  <Loader2 className="animate-spin" size={18} />
                  Güncelleniyor...
                </>
              ) : (
                <>
                  <Save size={18} />
                  Şifreyi Güncelle
                </>
              )}
            </button>

            <button
              type="button"
              onClick={handleCancel}
              className="w-full flex items-center justify-center gap-2 bg-white border-2 border-gray-200 text-gray-600 font-medium py-3 rounded-xl hover:bg-gray-50 transition-colors"
            >
              İptal
            </button>
          </div>
        </form>
      </div>

      {/* Success Modal */}
      {showSuccessModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm animate-in fade-in duration-200">
          <div className="bg-white rounded-2xl shadow-xl w-full max-w-sm mx-4 overflow-hidden animate-in zoom-in-95 duration-200 p-6 text-center">
            <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <CheckCircle className="text-green-600" size={32} />
            </div>
            <h3 className="text-xl font-bold text-gray-900 mb-2">Başarılı!</h3>
            <p className="text-gray-600 mb-6">
              Şifreniz başarıyla değiştirildi. Güvenliğiniz için yeniden giriş yapmanız gerekmektedir.
            </p>
            <button
              onClick={handleSuccessModalClose}
              className="w-full bg-primary-yellow text-primary-dark font-bold py-3 rounded-xl hover:bg-[#FFC107] transition-colors"
            >
              Giriş Yap
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
