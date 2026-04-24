import { BrowserRouter, Routes, Route, Navigate, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import useHeartbeat from "./hooks/useHeartbeat";
import { isAuthenticated, getUserRole } from "./services/authService";
import Home from "./pages/Home";
import Login from "./pages/Login";
import AdminDashboard from "./pages/AdminDashboard";
import ManagerDashboard from "./pages/ManagerDashboard";
import AddLocation from "./pages/AddLocation";
import EditLocation from "./pages/EditLocation";
import Favorites from "./pages/Favorites";
import TourList from "./pages/TourList";
import TourDetail from "./pages/TourDetail";

/**
 * Route guard: requires login. Redirects unauthorized role to their dashboard.
 * Only admin and manager roles exist - no "user" role.
 */
function ProtectedRoute({ element, requiredRole = null }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  if (requiredRole) {
    const userRole = getUserRole();
    if (userRole !== requiredRole) {
      if (userRole === "admin") return <Navigate to="/admin/dashboard" replace />;
      return <Navigate to="/manager/dashboard" replace />;
    }
  }

  return element;
}

/**
 * Route guard: redirect already-logged-in users away from public auth pages.
 */
function PublicRoute({ element }) {
  if (isAuthenticated()) {
    const userRole = getUserRole();
    if (userRole === "admin") return <Navigate to="/admin/dashboard" replace />;
    return <Navigate to="/manager/dashboard" replace />;
  }
  return element;
}

function PublicAnalyticsTracker() {
  const location = useLocation();
  const isAdminRoute = location.pathname.toLowerCase().startsWith("/admin");
  useHeartbeat({
    enabled: !isAdminRoute,
    pageKey: `${location.pathname}${location.search}`,
  });
  return null;
}

function App() {
  const [authChecked, setAuthChecked] = useState(false);

  useEffect(() => {
    const authenticated = isAuthenticated();
    console.log("Auth check on app load:", authenticated);
    setAuthChecked(true);
  }, []);

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
      <PublicAnalyticsTracker />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<PublicRoute element={<Login />} />} />

        <Route path="/favorites" element={<Favorites />} />

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

        <Route
          path="/manager"
          element={<ProtectedRoute element={<ManagerDashboard />} requiredRole="manager" />}
        />
        <Route
          path="/manager/dashboard"
          element={<ProtectedRoute element={<ManagerDashboard />} requiredRole="manager" />}
        />

        <Route path="/tours" element={<TourList />} />
        <Route path="/tours/:id" element={<TourDetail />} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
