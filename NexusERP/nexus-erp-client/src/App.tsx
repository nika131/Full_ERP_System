import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import Login from './pages/Login';
import MainLayout from './layouts/MainLayout';
import InventoryList from './pages/InventoryList';
import AuditLogsList from './pages/AuditLogsList';
import SupplierList from './pages/SupplierList';
import Dashboard from './pages/Dashboard';
import CategoryList from './pages/CategoryList';
import { Toaster } from 'react-hot-toast';
import { Permissions } from './constants/permissions';


function App() {
return (
    <AuthProvider>
      <Toaster position="top-right" toastOptions={{ duration: 4000 }} /> 
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route element={<ProtectedRoute />}>

            <Route element={<MainLayout />}>
              
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<Dashboard />} />

              <Route element={<ProtectedRoute requiredPermission={Permissions.ViewProducts} />}>
                <Route path="/inventory" element={<InventoryList />} />
              </Route>

              <Route element={<ProtectedRoute requiredPermission={Permissions.ManageSuppliers} />}>
                <Route path="/suppliers" element={<SupplierList />} />
              </Route>

              <Route element={<ProtectedRoute requiredPermission={Permissions.ManageCategories} />}>
                <Route path="/categories" element={<CategoryList />} />
              </Route>

              <Route element={<ProtectedRoute requiredPermission={Permissions.ViewAuditLogs} />}>
                <Route path="/logs" element={<AuditLogsList />} />
              </Route>

            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;