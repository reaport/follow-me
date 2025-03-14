using FollowMe.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowMe.Services
{
    /// <summary>
    /// Репозиторий для управления данными о машинах сопровождения.
    /// </summary>
    public class CarRepository
    {
        private readonly string _filePath = "cars.txt";
        private readonly IGroundControlService _groundControlService;

        /// <summary>
        /// Конструктор репозитория. Внедряет зависимости через DI.
        /// </summary>
        /// <param name="groundControlService">Сервис для взаимодействия с системой управления.</param>
        public CarRepository(IGroundControlService groundControlService)
        {
            _groundControlService = groundControlService;
        }

        /// <summary>
        /// Получает список всех машин из файла.
        /// </summary>
        /// <returns>Список машин.</returns>
        public List<Car> GetAllCars()
        {
            if (!File.Exists(_filePath))
            {
                var defaultCars = CreateDefaultCars().Result;
                SaveAllCars(defaultCars);
                return defaultCars;
            }

            var lines = File.ReadAllLines(_filePath);
            var cars = lines.Select(line =>
            {
                var parts = line.Split('|');
                return new Car
                {
                    InternalId = parts[0],
                    ExternalId = parts[1], 
                    Status = (CarStatusEnum)Enum.Parse(typeof(CarStatusEnum), parts[2]),
                    CurrentNode = parts.Length > 3 ? parts[3] : "garrage"
                };
            }).ToList();

            return cars;
        }

        /// <summary>
        /// Создает дефолтные машины и регистрирует их в системе.
        /// </summary>
        /// <returns>Список дефолтных машин.</returns>
        private async Task<List<Car>> CreateDefaultCars()
        {
            var defaultCars = new List<Car>();

            // Создаем первую машину
            var registrationResponse1 = await _groundControlService.RegisterVehicle("follow-me");
            if (!string.IsNullOrEmpty(registrationResponse1.VehicleId))
            {
                defaultCars.Add(new Car
                {
                    InternalId = "0000000-60bc-464d-8759-c04b79c25b25",
                    ExternalId = registrationResponse1.VehicleId,
                    Status = CarStatusEnum.Available,
                    CurrentNode = registrationResponse1.GarrageNodeId
                });
            }
            else
            {
                throw new InvalidOperationException("Не удалось зарегистрировать первую дефолтную машину.");
            }

            // Создаем вторую машину
            var registrationResponse2 = await _groundControlService.RegisterVehicle("follow-me");
            if (!string.IsNullOrEmpty(registrationResponse2.VehicleId))
            {
                defaultCars.Add(new Car
                {
                    InternalId = "1111111-60bc-464d-8759-c04b79c25b25",
                    ExternalId = registrationResponse2.VehicleId,
                    Status = CarStatusEnum.Available,
                    CurrentNode = registrationResponse2.GarrageNodeId
                });
            }
            else
            {
                throw new InvalidOperationException("Не удалось зарегистрировать вторую дефолтную машину.");
            }

            return defaultCars;
        }

        /// <summary>
        /// Перезагружает все машины, сбрасывая их состояние и регистрируя заново.
        /// </summary>
        public async Task ReloadCars()
        {
            var cars = GetAllCars();

            // Сбрасываем состояние каждой машины
            foreach (var car in cars)
            {
                car.ExternalId = "";
                car.Status = CarStatusEnum.Available;
                car.CurrentNode = "garrage";

                // Регистрируем машину заново
                var registrationResponse = await _groundControlService.RegisterVehicle("follow-me");
                if (!string.IsNullOrEmpty(registrationResponse.VehicleId))
                {
                    car.ExternalId = registrationResponse.VehicleId;
                    car.CurrentNode = registrationResponse.GarrageNodeId;
                }
                else
                {
                    throw new InvalidOperationException($"Не удалось зарегистрировать машину {car.InternalId}.");
                }
            }

            SaveAllCars(cars);
        }

        /// <summary>
        /// Сохраняет список машин в файл.
        /// </summary>
        /// <param name="cars">Список машин для сохранения.</param>
        public void SaveAllCars(List<Car> cars)
        {
            try
            {
                var lines = cars.Select(car => $"{car.InternalId}|{car.ExternalId}|{car.Status}|{car.CurrentNode}");
                File.WriteAllLines(_filePath, lines, Encoding.UTF8);
                Console.WriteLine("Файл успешно записан.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при записи в файл: {ex.Message}");
            }
        }
    }
}