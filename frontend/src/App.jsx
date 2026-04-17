import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { isAuthenticated, getUserRole } from "./services/authService";
import Login from "./pages/Login";
import Register from "./pages/Register";
import AdminDashboard from "./pages/AdminDashboard";
import ManagerDashboard from "./pages/ManagerDashboard";

function ProtectedRoute({ element, requiredRole }) {
  if (!isAuthenticated()) return <Navigate to="/login" replace />;
  const role = getUserRole();
  if (role !== requiredRole) {
    if (role === "admin") return <Navigate to="/admin/dashboard" replace />;
    if (role === "manager") return <Navigate to="/manager/dashboard" replace />;
    return <Navigate to="/login" replace />;
  }
  return element;
}

function PublicRoute({ element }) {
  if (isAuthenticated()) {
    const role = getUserRole();
    if (role === "admin") return <Navigate to="/admin/dashboard" replace />;
    if (role === "manager") return <Navigate to="/manager/dashboard" replace />;
  }
  return element;
}

function App() {
  return (
    <BrowserRouter
      future={{
        v7_startTransition: true,
        v7_relativeSplatPath: true,
      }}
    >
      <Routes>
        <Route path="/login" element={<PublicRoute element={<Login />} />} />
        <Route path="/register" element={<PublicRoute element={<Register />} />} />

        <Route path="/admin" element={<ProtectedRoute element={<AdminDashboard />} requiredRole="admin" />} />
        <Route path="/admin/dashboard" element={<ProtectedRoute element={<AdminDashboard />} requiredRole="admin" />} />

        <Route path="/manager" element={<ProtectedRoute element={<ManagerDashboard />} requiredRole="manager" />} />
        <Route path="/manager/dashboard" element={<ProtectedRoute element={<ManagerDashboard />} requiredRole="manager" />} />

        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
