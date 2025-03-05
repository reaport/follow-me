// api.js
import axios from 'axios';

const API_URL = process.env.NODE_ENV === 'development'
  ? process.env.REACT_APP_API_URL_LOCAL
  : process.env.REACT_APP_API_URL_PROD;

const api = axios.create({
  baseURL: API_URL,
});

export const fetchCars = async () => {
  try {
    const response = await api.get('/cars');
    return response.data;
  } catch (error) {
    console.error("Ошибка при загрузке машин:", error);
    throw error;
  }
};

export const addCar = async () => {
  try {
    await api.post('/cars/add');
  } catch (error) {
    console.error("Ошибка при добавлении машины:", error);
    throw error;
  }
};

export const removeCar = async (internalId) => {
  try {
    await api.post('/cars/remove', { internalId });
  } catch (error) {
    console.error("Ошибка при удалении машины:", error);
    throw error;
  }
};

export const fetchAuditEntries = async () => {
  try {
    const response = await api.get('/audit');
    return response.data;
  } catch (error) {
    console.error("Ошибка при загрузке аудита:", error);
    throw error;
  }
};

export const fetchLogs = async () => {
  try {
    const response = await api.get('/logs');
    return response.data;
  } catch (error) {
    console.error("Ошибка при загрузке логов:", error);
    throw error;
  }
};