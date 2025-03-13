import { useEffect, useState } from "react";
import axios from "axios";
import "./CarsPage.css"; // Импортируем стили

const API_BASE = window.location.hostname.includes("localhost")
  ? "http://localhost:8080/admin"
  : "https://follow-me.reaport.ru/admin";

export default function AuditPage() {
  const [audit, setAudit] = useState([]);

  // Загрузка аудита
  useEffect(() => {
    fetchAudit();
  }, []);

  // Функция для загрузки аудита
  const fetchAudit = () => {
    axios.get(`${API_BASE}/audit`)
      .then((res) => setAudit(res.data))
      .catch((error) => {
        console.error("Ошибка при загрузке аудита:", error);
      });
  };

  // Функция для очистки аудита
  const clearAudit = () => {
    axios.post(`${API_BASE}/audit/clear`)
      .then(() => {
        fetchAudit(); // Перезагружаем аудит после очистки
      })
      .catch((error) => {
        console.error("Ошибка при очистке аудита:", error);
      });
  };

  return (
    <div>
      <h1>Аудит</h1>
      <button onClick={clearAudit}>Очистить аудит</button>
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