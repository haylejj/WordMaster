import { z } from "zod";

// Zod schemas for validation matching WordMaster.Application validators

export const loginSchema = z.object({
  email: z.string()
    .min(1, "Email alanı boş bırakılamaz")
    .email("Lütfen geçerli bir email giriniz."),
  password: z.string()
    .min(1, "Şifre alanı boş bırakılamaz")
    .min(6, "Şifre en az 6 karakter olmalıdır"),
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
  password: z.string()
    .min(1, "Şifre alanı boş bırakılamaz")
    .min(6, "Şifre en az 6 karakter olmalıdır"),
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
