import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import Login from './pages/Login';

const DashboardPlaceholder = () => (
  <div className="p-8">
    <h1 className="text-3xl font-bold">ERP Dashboard</h1>
    <p>Welcome to the secure zone.</p>
  </div>
);
function App() {
  return(
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<Login />} />

          <Route element={<ProtectedRoute />}>
            <Route path="/dashboard" element={<DashboardPlaceholder />} />
            
          </Route>

          <Route element={<ProtectedRoute allowedRoles={['Admin']} />}>
            <Route path="/admin" element={<div>Admin Settings (Placeholder)</div>} />
          </Route>

          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App
