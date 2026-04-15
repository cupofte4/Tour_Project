import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useEffect, useState } from "react";
import { isAuthenticated, getUserRole } from "./services/authService";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Register from "./pages/Register";
import AdminDashboard from "./pages/AdminDashboard";
import ManagerDashboard from "./pages/ManagerDashboard";
import AddLocation from "./pages/AddLocation";
import EditLocation from "./pages/EditLocation";
import MyProfile from "./pages/MyProfile";
import Settings from "./pages/Settings";
import Favorites from "./pages/Favorites";

/**
 * Route guard: Check if user is authenticated
 * If not, redirect to login; if yes, check role
 */
function ProtectedRoute({ element, requiredRole = null }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole) {
    const userRole = getUserRole();
    if (userRole !== requiredRole) {
      // Redirect based on role
      if (userRole === "admin") return <Navigate to="/admin/dashboard" replace />;
      if (userRole === "manager") return <Navigate to="/manager/dashboard" replace />;
      return <Navigate to="/" replace />;
    }
  }

  return element;
}

/**
 * Route guard: Prevent access if already logged in
 */
function PublicRoute({ element }) {
  if (isAuthenticated()) {
    const userRole = getUserRole();
    if (userRole === "admin") return <Navigate to="/admin/dashboard" replace />;
    if (userRole === "manager") return <Navigate to="/manager/dashboard" replace />;
    return <Navigate to="/" replace />;
  }
  return element;
}

function App() {
  const [authChecked, setAuthChecked] = useState(false);

  useEffect(() => {
    // Check authentication on app load
    // This ensures persisted auth state is recognized
    const authenticated = isAuthenticated();
    console.log("Auth check on app load:", authenticated);
    setAuthChecked(true);
  }, []);

  // Prevent flickering by not rendering routes until auth is checked
  if (!authChecked) {
    return <div>Loading...</div>;
  }

  return (
    <BrowserRouter
      future={{
        v7_startTransition: true,
        v7_relativeSplatPath: true,
      }}
    >
      <Routes>
        {/* Public routes */}
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<PublicRoute element={<Login />} />} />
        <Route path="/register" element={<PublicRoute element={<Register />} />} />

        {/* Protected user routes */}
        <Route path="/profile" element={<ProtectedRoute element={<MyProfile />} />} />
        <Route path="/favorites" element={<ProtectedRoute element={<Favorites />} />} />
        <Route path="/settings" element={<ProtectedRoute element={<Settings />} />} />

        {/* Protected admin routes */}
        <Route
          path="/admin"
          element={<ProtectedRoute element={<AdminDashboard />} requiredRole="admin" />}
        />
        <Route
          path="/admin/dashboard"
          element={<ProtectedRoute element={<AdminDashboard />} requiredRole="admin" />}
        />
        <Route
          path="/admin/add"
          element={<ProtectedRoute element={<AddLocation />} requiredRole="admin" />}
        />
        <Route
          path="/admin/tts"
          element={<ProtectedRoute element={<EditLocation />} requiredRole="admin" />}
        />

        {/* Protected manager routes */}
        <Route
          path="/manager"
          element={<ProtectedRoute element={<ManagerDashboard />} requiredRole="manager" />}
        />
        <Route
          path="/manager/dashboard"
          element={<ProtectedRoute element={<ManagerDashboard />} requiredRole="manager" />}
        />

        {/* Catch-all */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
