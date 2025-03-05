import { useEffect, useState } from "react";
import axios from "axios";
import "./TableView.css"

const API_BASE = window.location.hostname.includes("localhost")
  ? "http://localhost:8080/admin"
  : "https://follow-me.reaport.ru/admin";

export default function AuditPage() {
  const [audit, setAudit] = useState([]);

  useEffect(() => {
    axios.get(`${API_BASE}/audit`).then((res) => setAudit(res.data));
  }, []);

  return (
    <div>
      <h1>Аудит</h1>
      <table>
        <thead>
          <tr>
            <th>Дата</th>
            <th>ID машины</th>
            <th>Перемещение</th>
          </tr>
        </thead>
        <tbody>
          {audit.map((entry, index) => (
            <tr key={index}>
              <td>{entry.timestamp}</td>
              <td>{entry.carId}</td>
              <td>{entry.movement}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
