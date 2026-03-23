import { BrowserRouter, Routes, Route } from "react-router-dom";
import Home from "./pages/Home";
import Login from "./pages/Login";
import Admin from "./pages/Admin";
import AddLocation from "./pages/AddLocation";
import EditLocation from "./pages/EditLocation";

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/admin" element={<Admin />} />
        <Route path="/admin/add" element={<AddLocation />} />
        <Route path="/admin/tts" element={<EditLocation />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;