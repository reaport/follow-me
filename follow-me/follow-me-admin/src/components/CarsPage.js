import { useEffect, useState } from "react";
import axios from "axios";
import "./TableView.css";

const API_BASE = window.location.hostname.includes("localhost")
  ? "https://localhost:8081/admin"
  : "https://follow-me.reaport.ru/admin";

export default function CarsPage() {
  const [cars, setCars] = useState([]);
  const [error, setError] = useState(null); // Состояние для хранения ошибки

  useEffect(() => {
    axios.get(`${API_BASE}/cars`).then((res) => {
      // Преобразуем статус в строку
      const formattedCars = res.data.map(car => ({
        ...car,
        status: car.status === 0 ? "available" : "busy",
        currentNode: car.currentNode || "Unknown" // Если местоположение не задано, отображаем "Unknown"
      }));
      setCars(formattedCars);
    });
  }, []);

  const addCar = () => {
    axios
      .post(`${API_BASE}/cars/add`)
      .then(() => {
        window.location.reload(); // Перезагружаем страницу после успешного добавления
      })
      .catch((error) => {
        if (error.response && error.response.status === 400) {
          // Обрабатываем ошибку BadRequest
          setError("Невозможно добавить машину. Достигнут лимит в 10 машин.");
        } else {
          setError("Произошла ошибка при добавлении машины.");
        }
      });
  };

  return (
    <div>
      <h1>Машины</h1>
      {error && <div className="error-message">{error}</div>} {/* Отображаем сообщение об ошибке */}
      <table>
        <thead>
          <tr>
            <th>Внутренний ID</th>
            <th>Внешний ID</th>
            <th>Статус</th>
            <th>Текущее местоположение</th>
          </tr>
        </thead>
        <tbody>
          {cars.map((car) => (
            <tr key={car.internalId}>
              <td>{car.internalId}</td>
              <td>{car.externalId}</td>
              <td>{car.status}</td>
              <td>{car.currentNode}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <button onClick={addCar}>Добавить машину</button>
    </div>
  );
}