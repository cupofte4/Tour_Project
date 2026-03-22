import { useState } from "react";
import { login } from "../services/authService";

function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleLogin = async () => {
    const user = await login(username, password);
    if (user) {
      localStorage.setItem("user", JSON.stringify(user));
      window.location.href = "/admin";
    } else {
      alert("Sai tài khoản");
    }
  };

  return (
    <div>
      <h2>Login Admin</h2>

      <input
        placeholder="Username"
        onChange={(e) => setUsername(e.target.value)}
      />
      <br />

      <input
        placeholder="Password"
        type="password"
        onChange={(e) => setPassword(e.target.value)}
      />
      <br />

      <button onClick={handleLogin}>Login</button>
    </div>
  );
}

export default Login;