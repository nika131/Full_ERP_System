import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import Login from './pages/Login';
import MainLayout from './layouts/MainLayout';
import InventoryList from './pages/InventoryList';
import AuditLogsList from './pages/AuditLogsList';
import SupplierList from './pages/SupplierList';
import Dashboard from './pages/Dashboard';


function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route element={<ProtectedRoute />}>
            <Route element={<MainLayout />}>
              
              <Route path="/dashboard" element={<Dashboard/>} />
              
              <Route path="/inventory" element={<InventoryList/>} />
              <Route path="/suppliers" element={<SupplierList/>} />

              <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
                <Route path="/Logs" element={<AuditLogsList/>} />
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