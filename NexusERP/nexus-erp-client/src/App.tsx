import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import AccountProtectedRoute from './components/AccountProtectedRoute';
import Login from './pages/Login';
import Register from './pages/Register';
import VerifyEmail from './pages/VerifyEmail';
import Companies from './pages/Companies';
import CompanyAdministration from './pages/CompanyAdministration';
import MainLayout from './layouts/MainLayout';
import InventoryList from './pages/InventoryList';
import AuditLogsList from './pages/AuditLogsList';
import SupplierList from './pages/SupplierList';
import Dashboard from './pages/Dashboard';
import CategoryList from './pages/CategoryList';
import { Toaster } from 'react-hot-toast';
import { Permissions } from './constants/permissions';
import Profile from './pages/Profile';
import EmployeeList from './pages/EmployeeList';
import PendingLeaves from './pages/PendingLeaves';
import StoreList from './pages/StoreList';
import ShiftAuditList from './pages/ShiftAuditList';
import PrivacyPolicy from './pages/PrivacyPolicy';
import TermsOfService from './pages/TermsOfService';

function App() {
    return (
        <AuthProvider>
            <Toaster position="top-right" toastOptions={{ duration: 4000 }} />
            <BrowserRouter>
                <Routes>
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/verify-email" element={<VerifyEmail />} />
                    <Route path="/privacy-policy" element={<PrivacyPolicy />} />
                    <Route path="/terms-of-service" element={<TermsOfService />} />

                    <Route element={<AccountProtectedRoute />}>
                        <Route path="/companies" element={<Companies />} />
                    </Route>

                    <Route element={<ProtectedRoute />}>
                        <Route element={<MainLayout />}>
                            <Route path="/profile" element={<Profile />} />
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewDashboard} />}>
                                <Route path="/dashboard" element={<Dashboard />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredAnyPermissions={[Permissions.ViewUsers, Permissions.ManageRoles]} />}>
                                <Route path="/employees" element={<EmployeeList />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredAnyPermissions={[Permissions.ManageJoinRequests, Permissions.ManageJoinCodes]} />}>
                                <Route path="/company-admin" element={<CompanyAdministration />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ManageAbsences} />}>
                                <Route path="/pending-leaves" element={<PendingLeaves />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewProducts} />}>
                                <Route path="/inventory" element={<InventoryList />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewSuppliers} />}>
                                <Route path="/suppliers" element={<SupplierList />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewCategories} />}>
                                <Route path="/categories" element={<CategoryList />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewAuditLogs} />}>
                                <Route path="/logs" element={<AuditLogsList />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewStores} />}>
                                <Route path="/stores" element={<StoreList />} />
                            </Route>
                            <Route element={<ProtectedRoute requiredPermission={Permissions.ViewShifts} />}>
                                <Route path="/shiftAuditList" element={<ShiftAuditList />} />
                            </Route>
                        </Route>
                    </Route>

                    <Route path="/" element={<Navigate to="/companies" replace />} />
                    <Route path="*" element={<Navigate to="/companies" replace />} />
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;
