import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import Login from './pages/Login';
import MainLayout from './layouts/MainLayout';
import InventoryList from './pages/InventoryList';
import AuditLogsList from './pages/AuditLogsList';


const DashboardPlaceholder = () => (
  <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
    <h1 className="text-2xl font-bold mb-2">System Overview</h1>
    <p className="text-gray-600">Select an module from the sidebar to begin.</p>
  </div>
);

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route element={<ProtectedRoute />}>
            <Route element={<MainLayout />}>
              
              <Route path="/dashboard" element={<DashboardPlaceholder />} />
              
              <Route path="/inventory" element={<InventoryList/>} />
              <Route path="/suppliers" element={<div>Suppliers Grid (Pending)</div>} />

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