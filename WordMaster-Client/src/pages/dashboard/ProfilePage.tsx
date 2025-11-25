import { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { User, Mail, Phone, Calendar, Users, Save, Loader2, AlertCircle, CheckCircle } from "lucide-react";
import { userService, type UpdateProfileRequest } from "@/services/user.service";
import { cn, getErrorMessage } from "@/lib/utils";
import DatePicker from "@/components/ui/DatePicker";

const profileSchema = z.object({
  userName: z.string().min(1, "Kullanıcı adı boş bırakılamaz"),
  email: z.string().min(1, "E-posta boş bırakılamaz").email("Geçerli bir e-posta giriniz"),
  phone: z.string().min(1, "Telefon numarası boş bırakılamaz"),
  birthDate: z.string().nullable(),
  gender: z.string().nullable(),
});

type ProfileFormData = z.infer<typeof profileSchema>;

export default function ProfilePage() {
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    control,
    formState: { errors, isDirty },
  } = useForm<ProfileFormData>({
    resolver: zodResolver(profileSchema),
    defaultValues: {
      userName: "",
      email: "",
      phone: "",
      birthDate: null,
      gender: null,
    },
  });

  useEffect(() => {
    const fetchProfile = async () => {
      try {
        const response = await userService.getProfile();
        if (response.isSuccess && response.data) {
          reset({
            userName: response.data.userName || "",
            email: response.data.email || "",
            phone: response.data.phone || "",
            birthDate: response.data.birthDate ? response.data.birthDate.split("T")[0] : null,
            gender: response.data.gender?.toString() || null,
          });
        }
      } catch (error) {
        console.error("Failed to fetch profile", error);
        setErrorMessage(getErrorMessage(error));
      } finally {
        setLoading(false);
      }
    };
    fetchProfile();
  }, [reset]);

  const onSubmit = async (data: ProfileFormData) => {
    setSaving(true);
    setSuccessMessage(null);
    setErrorMessage(null);

    try {
      const payload: UpdateProfileRequest = {
        userName: data.userName,
        email: data.email,
        phone: data.phone,
        birthDate: data.birthDate || null,
        gender: data.gender ? parseInt(data.gender) : null,
      };

      const response = await userService.updateProfile(payload);
      if (response.isSuccess) {
        setSuccessMessage("Profil bilgileri başarıyla güncellendi!");
        reset(data);
      } else {
        setErrorMessage(response.errorList?.join(", ") || "Güncelleme başarısız oldu.");
      }
    } catch (error) {
      console.error("Update failed", error);
      setErrorMessage(getErrorMessage(error));
    } finally {
      setSaving(false);
    }
  };

  const handleCancel = () => {
    window.history.back();
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center min-h-[400px]">
        <Loader2 className="animate-spin text-primary-yellow" size={48} />
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
      <div className="bg-white rounded-2xl shadow-lg overflow-hidden">
        {/* Header */}
        <div className="bg-[#1a1f24] px-8 py-6">
          <h1 className="text-xl font-bold text-white flex items-center justify-center gap-3">
            <User className="text-primary-yellow" size={24} />
            Profil Bilgilerini Güncelle
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

          {/* Username */}
          <div className="space-y-1">
            <div
              className={cn(
                "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                errors.userName ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
              )}
            >
              <User className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Kullanıcı Adı</label>
                <input
                  {...register("userName")}
                  type="text"
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                  placeholder="Kullanıcı adınızı girin"
                />
              </div>
            </div>
            {errors.userName && <p className="text-xs text-red-500 ml-4">{errors.userName.message}</p>}
          </div>

          {/* Email */}
          <div className="space-y-1">
            <div
              className={cn(
                "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                errors.email ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
              )}
            >
              <Mail className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">E-posta</label>
                <input
                  {...register("email")}
                  type="email"
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                  placeholder="E-posta adresinizi girin"
                />
              </div>
            </div>
            {errors.email && <p className="text-xs text-red-500 ml-4">{errors.email.message}</p>}
          </div>

          {/* Phone */}
          <div className="space-y-1">
            <div
              className={cn(
                "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                errors.phone ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
              )}
            >
              <Phone className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Telefon</label>
                <input
                  {...register("phone")}
                  type="tel"
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                  placeholder="Telefon numaranızı girin"
                />
              </div>
            </div>
            {errors.phone && <p className="text-xs text-red-500 ml-4">{errors.phone.message}</p>}
          </div>

          {/* Birth Date */}
          <div className="space-y-1">
            <div className="flex items-center border-2 border-gray-200 rounded-xl px-4 py-3 focus-within:border-primary-yellow transition-colors">
              <Calendar className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Doğum Tarihi</label>
                <Controller
                  name="birthDate"
                  control={control}
                  render={({ field }) => (
                    <DatePicker
                      value={field.value}
                      onChange={field.onChange}
                      placeholder="Doğum tarihinizi seçin"
                    />
                  )}
                />
              </div>
            </div>
          </div>

          {/* Gender */}
          <div className="space-y-1">
            <div className="flex items-center border-2 border-gray-200 rounded-xl px-4 py-3 focus-within:border-primary-yellow transition-colors">
              <Users className="text-primary-yellow mr-3" size={18} />
              <div className="flex-1">
                <label className="block text-xs font-medium text-gray-500">Cinsiyet</label>
                <select
                  {...register("gender")}
                  className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5 cursor-pointer"
                >
                  <option value="">Seçiniz...</option>
                  <option value="1">Kadın</option>
                  <option value="2">Erkek</option>
                </select>
              </div>
            </div>
          </div>

          {/* Buttons */}
          <div className="flex flex-col gap-3 pt-4">
            <button
              type="submit"
              disabled={saving || !isDirty}
              className="w-full flex items-center justify-center gap-2 bg-primary-yellow text-primary-dark font-bold py-3 rounded-xl hover:bg-[#FFC107] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {saving ? (
                <>
                  <Loader2 className="animate-spin" size={18} />
                  Kaydediliyor...
                </>
              ) : (
                <>
                  <Save size={18} />
                  Değişiklikleri Kaydet
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
    </div>
  );
}
