import { z } from "zod";

// Zod schemas for validation matching WordMaster.Application validators

// Common password validation rule based on Identity options
// RequireDigit = true
// RequireLowercase = true
// RequireUppercase = true
// RequiredLength = 6
// RequireNonAlphanumeric = true
const passwordValidation = z.string()
  .min(1, "Şifre alanı boş bırakılamaz")
  .min(6, "Şifre en az 6 karakter olmalıdır")
  .regex(/\d/, "Şifre en az bir rakam içermelidir")
  .regex(/[a-z]/, "Şifre en az bir küçük harf içermelidir")
  .regex(/[A-Z]/, "Şifre en az bir büyük harf içermelidir")
  .regex(/[^a-zA-Z0-9]/, "Şifre en az bir özel karakter içermelidir (örn. !@#$%^&*)");

export const loginSchema = z.object({
  email: z.string()
    .min(1, "Email alanı boş bırakılamaz")
    .email("Lütfen geçerli bir email giriniz."),
  password: z.string()
    .min(1, "Şifre alanı boş bırakılamaz")
    .min(6, "Şifre en az 6 karakter olmalıdır"), // Login validation can be less strict or same, keeping basic length
  rememberMe: z.boolean(),
});

export type LoginRequest = z.infer<typeof loginSchema>;

export const registerSchema = z.object({
  userName: z.string()
    .min(1, "Kullanıcı Adı alanı boş bırakılamaz"),
  email: z.string()
    .min(1, "Email alanı boş bırakılamaz")
    .email("Lütfen geçerli bir email giriniz."),
  phone: z.string()
    .min(1, "Telefon alanı boş bırakılamaz"),
  password: passwordValidation,
  passwordConfirm: z.string()
    .min(1, "Şifre tekrar alanı boş bırakılamaz"),
  gender: z.enum(["1", "2"], {
    message: "Cinsiyet alanı boş olamaz.",
  }), // 1: Kadın, 2: Erkek
}).refine((data) => data.password === data.passwordConfirm, {
  message: "Şifreler aynı değildir.",
  path: ["passwordConfirm"],
});

export type RegisterRequest = z.infer<typeof registerSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string()
    .min(1, "Email alanı boş bırakılamaz")
    .email("Lütfen geçerli bir email giriniz."),
});

export type ForgotPasswordRequest = z.infer<typeof forgotPasswordSchema>;

export const resetPasswordSchema = z.object({
  password: passwordValidation,
  passwordConfirm: z.string()
    .min(1, "Şifre tekrar alanı boş bırakılamaz"),
  userId: z.string().optional(),
  token: z.string().optional(),
}).refine((data) => data.password === data.passwordConfirm, {
  message: "Şifreler aynı değildir.",
  path: ["passwordConfirm"],
});

export type ResetPasswordRequest = z.infer<typeof resetPasswordSchema>;

// Adding ChangePassword schema for authenticated users as well
export const changePasswordSchema = z.object({
  oldPassword: z.string()
    .min(1, "Eski şifre alanı boş bırakılamaz"),
  newPassword: passwordValidation,
  confirmNewPassword: z.string()
    .min(1, "Yeni şifre tekrar alanı boş bırakılamaz"),
}).refine((data) => data.newPassword === data.confirmNewPassword, {
  message: "Yeni şifreler eşleşmiyor.",
  path: ["confirmNewPassword"],
});

export type ChangePasswordRequest = z.infer<typeof changePasswordSchema>;
