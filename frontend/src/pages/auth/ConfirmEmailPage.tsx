import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { authService } from "@/services/auth.service";
import { CheckCircle2, XCircle, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

export default function ConfirmEmailPage() {
    const [searchParams] = useSearchParams();
    const [status, setStatus] = useState<"loading" | "success" | "error">("loading");
    const [message, setMessage] = useState("Email adresiniz doğrulanıyor...");

    useEffect(() => {
        const confirm = async () => {
            const userId = searchParams.get("userId");
            const token = searchParams.get("token");

            if (!userId || !token) {
                setStatus("error");
                setMessage("Geçersiz doğrulama bağlantısı.");
                return;
            }

            try {
                const result = await authService.verifyEmail(userId, token);
                if (result.isSuccess) {
                    setStatus("success");
                    setMessage("Email adresiniz başarıyla doğrulandı! Artık giriş yapabilirsiniz.");
                } else {
                    setStatus("error");
                    const errorMessage = result.errorList && result.errorList.length > 0
                        ? result.errorList.join(", ")
                        : "Email doğrulama işlemi başarısız oldu.";
                    setMessage(errorMessage);
                }
            } catch (error) {
                setStatus("error");
                setMessage("Bir hata oluştu. Lütfen tekrar deneyiniz.");
            }
        };

        confirm();
    }, [searchParams]);

    return (
        <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4 py-12 dark:bg-gray-900 sm:px-6 lg:px-8">
            <Card className="w-full max-w-md">
                <CardHeader className="text-center">
                    <div className="flex justify-center mb-4">
                        {status === "loading" && <Loader2 className="h-12 w-12 animate-spin text-blue-500" />}
                        {status === "success" && <CheckCircle2 className="h-12 w-12 text-green-500" />}
                        {status === "error" && <XCircle className="h-12 w-12 text-red-500" />}
                    </div>
                    <CardTitle className="text-2xl font-bold">
                        {status === "loading" && "Doğrulanıyor"}
                        {status === "success" && "Başarılı!"}
                        {status === "error" && "Hata!"}
                    </CardTitle>
                </CardHeader>
                <CardContent className="text-center space-y-6">
                    <p className="text-muted-foreground">{message}</p>

                    {status !== "loading" && (
                        <div className="pt-4">
                            <Link to="/login">
                                <Button className="w-full">
                                    Giriş Yap
                                </Button>
                            </Link>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
