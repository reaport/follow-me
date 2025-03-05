using FollowMe.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FollowMe.Services
{
    public class CarRepository
    {
        private readonly string _filePath = "cars.txt";

        // Чтение всех машин из файла
        public List<Car> GetAllCars()
        {
            if (!File.Exists(_filePath))
            {
                // Если файл не существует, создаем его с начальными данными
                var defaultCars = new List<Car>
                {
                    new Car { InternalId = "1", ExternalId = "", Status = CarStatusEnum.Available },
                    new Car { InternalId = "2", ExternalId = "", Status = CarStatusEnum.Available },
                    new Car { InternalId = "3", ExternalId = "", Status = CarStatusEnum.Available },
                    new Car { InternalId = "4", ExternalId = "", Status = CarStatusEnum.Available }
                };
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
                    Status = (CarStatusEnum)Enum.Parse(typeof(CarStatusEnum), parts[2])
                };
            }).ToList();

            return cars;
        }

        // Сохранение всех машин в файл
        public void SaveAllCars(List<Car> cars)
        {
            try
            {
                var lines = cars.Select(car => $"{car.InternalId}|{car.ExternalId}|{car.Status}");
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