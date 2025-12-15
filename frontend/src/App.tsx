import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Toaster } from "sonner";
import { AuthProvider } from "@/hooks/useAuth";
import AuthSync from "@/components/common/AuthSync";

import LoginPage from "@/pages/auth/LoginPage";
import RegisterPage from "@/pages/auth/RegisterPage";
import ForgotPasswordPage from "@/pages/auth/ForgotPasswordPage";
import ConfirmEmailPage from "@/pages/auth/ConfirmEmailPage";
import ResetPasswordPage from "@/pages/auth/ResetPasswordPage";
import GoogleCallbackPage from "@/pages/auth/GoogleCallbackPage";
import AdminLoginPage from "@/pages/admin/AdminLoginPage";
import DashboardLayout from "@/layouts/DashboardLayout";
import WordsPage from "@/pages/dashboard/WordsPage";
import CreateWordPage from "@/pages/dashboard/CreateWordPage";
import FoldersPage from "@/pages/dashboard/FoldersPage";
import FolderDetailPage from "@/pages/dashboard/FolderDetailPage";
import ProfilePage from "@/pages/dashboard/ProfilePage";
import ChangePasswordPage from "@/pages/dashboard/ChangePasswordPage";
import GeneralPracticePage from "@/pages/dashboard/GeneralPracticePage";
import FolderPracticePage from "@/pages/dashboard/FolderPracticePage";
import StatisticsPage from "@/pages/dashboard/StatisticsPage";
import GeneralTestPage from "@/pages/dashboard/GeneralTestPage";
import FolderTestPage from "@/pages/dashboard/FolderTestPage";

import AdminLayout from "@/layouts/AdminLayout";
import AdminDashboardPage from "@/pages/admin/AdminDashboardPage";
import AdminUsersPage from "@/pages/admin/AdminUsersPage";
import AdminWordsPage from "@/pages/admin/AdminWordsPage";
import AdminRolesPage from "@/pages/admin/AdminRolesPage";
import AdminPermissionsPage from "@/pages/admin/AdminPermissionsPage";
import AdminIpAddressesPage from "@/pages/admin/AdminIpAddressesPage";
import DatabaseResetPage from "@/pages/admin/DatabaseResetPage";
import AdminLogHistoryPage from "@/pages/admin/AdminLogHistoryPage";

import LandingPage from "@/pages/LandingPage";

import PermissionDeniedModal from "@/components/common/PermissionDeniedModal";

function App() {
  return (
    <AuthProvider>
      <Router>
        <AuthSync />
        <Toaster position="top-right" theme="dark" />
        <PermissionDeniedModal />
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
          <Route path="/confirm-email" element={<ConfirmEmailPage />} />
          <Route path="/forgot-password" element={<ForgotPasswordPage />} />
          <Route path="/ResetPassword" element={<ResetPasswordPage />} />
          <Route path="/auth/google-callback" element={<GoogleCallbackPage />} />

          {/* Admin Routes */}
          <Route path="/admin/login" element={<AdminLoginPage />} />
          <Route path="/admin" element={<AdminLayout />}>
            <Route path="dashboard" element={<AdminDashboardPage />} />
            <Route path="users" element={<AdminUsersPage />} />
            <Route path="words" element={<AdminWordsPage />} />
            <Route path="roles" element={<AdminRolesPage />} />
            <Route path="permissions" element={<AdminPermissionsPage />} />
            <Route path="ip-addresses" element={<AdminIpAddressesPage />} />
            <Route path="logs" element={<AdminLogHistoryPage />} />
            <Route path="database" element={<DatabaseResetPage />} />
          </Route>

          {/* Public Landing Page */}
          <Route path="/" element={<LandingPage />} />

          {/* Protected Dashboard Routes */}
          <Route path="/dashboard" element={<DashboardLayout />}>
            <Route index element={<WordsPage key="words" />} />
            <Route path="add-word" element={<CreateWordPage />} />
            <Route path="folders" element={<FoldersPage />} />
            <Route path="folders/:id" element={<FolderDetailPage />} />
            <Route path="favorites" element={<WordsPage key="favorites" variant="favorites" />} />
            <Route path="unknowns" element={<WordsPage key="unknowns" variant="unknowns" />} />
            <Route path="profile" element={<ProfilePage />} />
            <Route path="change-password" element={<ChangePasswordPage />} />
            <Route path="practice/:type" element={<GeneralPracticePage />} />
            <Route path="test/:type" element={<GeneralTestPage />} />
            <Route path="folders/:id/practice" element={<FolderPracticePage />} />
            <Route path="folders/:id/test" element={<FolderTestPage />} />
            <Route path="statistics" element={<StatisticsPage />} />
          </Route>

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;
