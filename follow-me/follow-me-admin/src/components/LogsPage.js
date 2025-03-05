import { useEffect, useState } from "react";
import axios from "axios";

const API_BASE = window.location.hostname.includes("localhost")
  ? "http://localhost:8080/admin"
  : "https://follow-me.reaport.ru/admin";

export default function LogsPage() {
  const [logs, setLogs] = useState([]);

  useEffect(() => {
    axios.get(`${API_BASE}/logs`).then((res) => setLogs(res.data));
  }, []);

  return (
    <div>
      <h1>Логи</h1>
      <table>
        <thead>
          <tr>
            <th>Запись</th>
          </tr>
        </thead>
        <tbody>
          {logs.map((log, index) => (
            <tr key={index}>
              <td>{log}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
