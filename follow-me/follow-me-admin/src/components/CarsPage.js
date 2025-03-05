import { useEffect, useState } from "react";
import axios from "axios";

const API_BASE = window.location.hostname.includes("localhost")
  ? "https://localhost:8081/admin"
  : "https://follow-me.reaport.ru/admin";

export default function CarsPage() {
  const [cars, setCars] = useState([]);

  useEffect(() => {
    axios.get(`${API_BASE}/cars`).then((res) => setCars(res.data));
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
      <button onClick={addCar}>Добавить машину</button>
      <table>
        <thead>
          <tr>
            <th>Внутренний ID</th>
            <th>Внешний ID</th>
            <th>Статус</th>
            <th>Действия</th>
          </tr>
        </thead>
        <tbody>
          {cars.map((car) => (
            <tr key={car.internalId}>
              <td>{car.internalId}</td>
              <td>{car.externalId}</td>
              <td>{car.status}</td>
              <td>
                <button onClick={() => removeCar(car.internalId)}>Удалить</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
