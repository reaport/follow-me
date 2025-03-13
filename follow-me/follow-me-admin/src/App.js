import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import CarsPage from "./components/CarsPage";
import AuditPage from "./components/AuditPage";
import LogsPage from "./components/LogsPage";
import "./App.css"; // Импортируем стили

export default function App() {
  return (
    <Router>
      <div className="app-container">
        <nav>
          <Link to="/">Машины</Link>
          <Link to="/audit">Аудит</Link>
          <Link to="/logs">Логи</Link>
        </nav>
        <Routes>
          <Route path="/" element={<CarsPage />} />
          <Route path="/audit" element={<AuditPage />} />
          <Route path="/logs" element={<LogsPage />} />
        </Routes>
      </div>
    </Router>
  );
}