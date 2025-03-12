using FollowMe.Data;
using FollowMe.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using FollowMe.Utils;

[Route("/admin")]
[ApiController] // Указываем, что это API-контроллер
public class AdminController : ControllerBase
{
    private readonly CarRepository _carRepository;

    public AdminController()
    {
        _carRepository = new CarRepository();
    }

    [HttpGet("")] // Доступно по /admin/admin
    public IActionResult Index()
    {
        var cars = _carRepository.GetAllCars();
        return Ok(cars); // Возвращаем JSON
    }

    [HttpGet("cars")] // Доступно по /admin/admin/cars
    public IActionResult GetCars()
    {
        var cars = _carRepository.GetAllCars();
        return Ok(cars); // Возвращаем JSON
    }

    [HttpPost("cars/add")] // Доступно по /admin/admin/cars/add
    public IActionResult AddCar()
    {
        var cars = _carRepository.GetAllCars();

        if (cars.Count < 10)
        {
            // Генерация нового GUID для InternalId
            var newInternalId = Guid.NewGuid().ToString();

            // Создание новой машины с GUID в качестве InternalId
            var newCar = new Car
            {
                InternalId = newInternalId,
                ExternalId = "",
                Status = CarStatusEnum.Available,
                CurrentNode = "garage-node"
            };

            // Добавление новой машины в список
            cars.Add(newCar);

            // Сохранение обновленного списка машин
            _carRepository.SaveAllCars(cars);

            // Логирование в аудит
            Logger.LogAudit(newInternalId, "Машина добавлена");

            // Возвращаем JSON с сообщением
            return Ok(new { Message = $"Машина {newInternalId} добавлена!" });
        }
        else
        {
            // Возвращаем ошибку, если достигнут лимит машин
            return BadRequest(new { Message = "Невозможно добавить машину. Достигнут лимит в 10 машин." });
        }
    }

    [HttpGet("logs")] // Доступно по /admin/admin/logs
    public IActionResult GetLogs()
    {
        try
        {
            if (!System.IO.File.Exists("logs.txt"))
            {
                return Ok(new List<string>());
            }

            var logs = System.IO.File.ReadAllLines("logs.txt").ToList();
            return Ok(logs);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при чтении логов: {ex.Message}");
            return StatusCode(500, new { Message = "Произошла ошибка при получении логов." });
        }
    }

    [HttpGet("audit")] // Доступно по /admin/admin/audit
    public IActionResult GetAudit()
    {
        try
        {
            if (!System.IO.File.Exists("audit.txt"))
            {
                return Ok(new List<AuditDto>());
            }

            var auditLines = System.IO.File.ReadAllLines("audit.txt");
            var auditEntries = auditLines.Select(line =>
            {
                var parts = line.Split('|');
                return new AuditDto
                {
                    Timestamp = parts[0].Trim(),
                    CarId = parts[1].Trim(),
                    Movement = parts[2].Trim()
                };
            }).ToList();

            return Ok(auditEntries);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при чтении аудита: {ex.Message}");
            return StatusCode(500, new { Message = "Произошла ошибка при получении аудита." });
        }
    }
}