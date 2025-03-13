using FollowMe.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowMe.Services
{
    public class CarRepository
    {
        private readonly string _filePath = "cars.txt";
        private readonly IGroundControlService _groundControlService;

        public CarRepository(IGroundControlService groundControlService)
        {
            _groundControlService = groundControlService;
        }

        // Чтение всех машин из файла
        public List<Car> GetAllCars()
        {
            if (!File.Exists(_filePath))
            {
                // Если файл не существует, создаем его с начальными данными
                var defaultCars = CreateDefaultCars().Result; // Регистрируем дефолтные машины
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
                    CurrentNode = parts.Length > 3 ? parts[3] : "garage-node" // Если поле отсутствует, используем "Garage"
                };
            }).ToList();

            return cars;
        }

        // Создание дефолтных машин с регистрацией
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