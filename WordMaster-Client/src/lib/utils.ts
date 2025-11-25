import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"


import { type ServiceResult } from "@/types/api";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function getErrorMessage(error: any): string {
  if (error?.response?.data) {
    const data = error.response.data as ServiceResult;
    if (data.errorList && Array.isArray(data.errorList) && data.errorList.length > 0) {
      return data.errorList.join(", ");
    }
  }
  return "Bir hata oluştu. Lütfen tekrar deneyin.";
}
