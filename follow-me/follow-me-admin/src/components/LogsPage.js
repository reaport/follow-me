import { useEffect, useState } from "react";
import axios from "axios";
import "./CarsPage.css"; // Импортируем стили

const API_BASE = window.location.hostname.includes("localhost")
  ? "http://localhost:8080/admin"
  : "https://follow-me.reaport.ru/admin";

export default function LogsPage() {
  const [logs, setLogs] = useState([]);
  const [sortConfig, setSortConfig] = useState({
    key: null, // Колонка для сортировки
    direction: "asc", // Направление сортировки (asc или desc)
  });

  // Загрузка логов
  useEffect(() => {
    fetchLogs();
  }, []);

  // Функция для загрузки логов
  const fetchLogs = () => {
    axios.get(`${API_BASE}/logs`)
      .then((res) => {
        const parsedLogs = res.data.map((log) => {
          const [timestamp, controller, code, message] = log.split(" | ");
          return { timestamp, controller, code, message };
        });
        setLogs(parsedLogs);
      })
      .catch((error) => {
        console.error("Ошибка при загрузке логов:", error);
      });
  };

  // Функция для очистки логов
  const clearLogs = () => {
    axios.post(`${API_BASE}/logs/clear`)
      .then(() => {
        fetchLogs(); // Перезагружаем логи после очистки
      })
      .catch((error) => {
        console.error("Ошибка при очистке логов:", error);
      });
  };

  // Функция для сортировки логов
  const sortedLogs = [...logs].sort((a, b) => {
    if (sortConfig.key) {
      if (a[sortConfig.key] < b[sortConfig.key]) {
        return sortConfig.direction === "asc" ? -1 : 1;
      }
      if (a[sortConfig.key] > b[sortConfig.key]) {
        return sortConfig.direction === "asc" ? 1 : -1;
      }
    }
    return 0;
  });

  // Функция для изменения порядка сортировки
  const requestSort = (key) => {
    let direction = "asc";
    if (sortConfig.key === key && sortConfig.direction === "asc") {
      direction = "desc";
    }
    setSortConfig({ key, direction });
  };

  return (
    <div>
      <h1>Логи</h1>
      <button onClick={clearLogs}>Очистить логи</button>
      <table>
        <thead>
          <tr>
            <th onClick={() => requestSort("timestamp")}>
              Дата и время{" "}
              {sortConfig.key === "timestamp" && (
                <span>{sortConfig.direction === "asc" ? "▲" : "▼"}</span>
              )}
            </th>
            <th onClick={() => requestSort("controller")}>
              Контроллер{" "}
              {sortConfig.key === "controller" && (
                <span>{sortConfig.direction === "asc" ? "▲" : "▼"}</span>
              )}
            </th>
            <th onClick={() => requestSort("code")}>
              Код{" "}
              {sortConfig.key === "code" && (
                <span>{sortConfig.direction === "asc" ? "▲" : "▼"}</span>
              )}
            </th>
            <th onClick={() => requestSort("message")}>
              Сообщение{" "}
              {sortConfig.key === "message" && (
                <span>{sortConfig.direction === "asc" ? "▲" : "▼"}</span>
              )}
            </th>
          </tr>
        </thead>
        <tbody>
          {sortedLogs.map((log, index) => (
            <tr key={index}>
              <td>{log.timestamp}</td>
              <td>{log.controller}</td>
              <td>{log.code}</td>
              <td>{log.message}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}