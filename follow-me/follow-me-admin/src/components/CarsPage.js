import { useEffect, useState } from "react";
import axios from "axios";
import "./TableView.css";

const API_BASE = window.location.hostname.includes("localhost")
  ? "https://localhost:8081/admin"
  : "https://follow-me.reaport.ru/admin";

export default function CarsPage() {
  const [cars, setCars] = useState([]);

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
    axios.post(`${API_BASE}/cars/add`).then(() => window.location.reload());
  };

  const removeCar = (internalId) => {
    axios.post(`${API_BASE}/cars/remove`, null, { params: { internalId } }).then(() => window.location.reload());
  };

  return (
    <div>
      <h1>Машины</h1>
      <table>
        <thead>
          <tr>
            <th>Внутренний ID</th>
            <th>Внешний ID</th>
            <th>Статус</th>
            <th>Текущее местоположение</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {cars.map((car) => (
            <tr key={car.internalId}>
              <td>{car.internalId}</td>
              <td>{car.externalId}</td>
              <td>{car.status}</td>
              <td>{car.currentNode}</td>
              <td>
                <button onClick={() => removeCar(car.internalId)}>Удалить</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      <button onClick={addCar}>Добавить машину</button>
    </div>
  );
}