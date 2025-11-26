import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "@/pages/auth/LoginPage";
import RegisterPage from "@/pages/auth/RegisterPage";
import ForgotPasswordPage from "@/pages/auth/ForgotPasswordPage";
import ResetPasswordPage from "@/pages/auth/ResetPasswordPage";
import DashboardLayout from "@/layouts/DashboardLayout";
import WordsPage from "@/pages/dashboard/WordsPage";
import CreateWordPage from "@/pages/dashboard/CreateWordPage";
import FoldersPage from "@/pages/dashboard/FoldersPage";
import FolderDetailPage from "@/pages/dashboard/FolderDetailPage";
import ProfilePage from "@/pages/dashboard/ProfilePage";
import ChangePasswordPage from "@/pages/dashboard/ChangePasswordPage";
import GeneralPracticePage from "@/pages/dashboard/GeneralPracticePage";
import FolderPracticePage from "@/pages/dashboard/FolderPracticePage";

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/ResetPassword" element={<ResetPasswordPage />} />

        {/* Protected Dashboard Routes */}
        <Route path="/" element={<DashboardLayout />}>
          <Route index element={<WordsPage />} />
          <Route path="add-word" element={<CreateWordPage />} />
          <Route path="folders" element={<FoldersPage />} />
          <Route path="folders/:id" element={<FolderDetailPage />} />
          <Route path="favorites" element={<WordsPage variant="favorites" />} />
          <Route path="unknowns" element={<WordsPage variant="unknowns" />} />
          <Route path="profile" element={<ProfilePage />} />
          <Route path="change-password" element={<ChangePasswordPage />} />
          <Route path="practice/:type" element={<GeneralPracticePage />} />
          <Route path="folders/:id/practice" element={<FolderPracticePage />} />
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Router>
  );
}

export default App;
