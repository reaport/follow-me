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
    private readonly IGroundControlService _groundControlService;

    public AdminController(CarRepository carRepository, IGroundControlService groundControlService)
    {
        _carRepository = carRepository;
        _groundControlService = groundControlService;
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
    public async Task<IActionResult> AddCar()
    {
        var cars = _carRepository.GetAllCars();

        if (cars.Count < 10)
        {
            // Генерация нового GUID для InternalId
            var newInternalId = Guid.NewGuid().ToString();

            // Регистрируем транспорт
            var registrationResponse = await _groundControlService.RegisterVehicle("follow-me");

            // Проверяем формат VehicleId
            if (string.IsNullOrEmpty(registrationResponse.VehicleId))
            {
                Logger.Log("AdminController", "ERROR", "Не удалось зарегистрировать машину.");
                return StatusCode(500, new { Message = "Не удалось зарегистрировать машину." });
            }

            // Создание новой машины с GUID в качестве InternalId
            var newCar = new Car
            {
                InternalId = newInternalId,
                ExternalId = registrationResponse.VehicleId, // Используем VehicleId из регистрации
                Status = CarStatusEnum.Available,
                CurrentNode = registrationResponse.GarrageNodeId
            };

            // Добавление новой машины в список
            cars.Add(newCar);

            // Сохранение обновленного списка машин
            _carRepository.SaveAllCars(cars);

            // Логирование в аудит
            Logger.LogAudit(newInternalId, "Машина добавлена и зарегистрирована");

            // Возвращаем JSON с сообщением
            return Ok(new { Message = $"Машина {newInternalId} добавлена и зарегистрирована с ExternalId: {registrationResponse.VehicleId}!" });
        }
        else
        {
            // Возвращаем ошибку, если достигнут лимит машин
            return BadRequest(new { Message = "Невозможно добавить машину. Достигнут лимит в 10 машин." });
        }
    }

    [HttpPost("cars/reload")]
    public async Task<IActionResult> ReloadCars()
    {
        Logger.Log("AdminController", "INFO", "Запрос на перезагрузку всех машин.");

        try
        {
            // Перезагружаем машины
            await _carRepository.ReloadCars();

            Logger.Log("AdminController", "INFO", "Все машины успешно перезагружены.");
            return Ok(new { Message = "Все машины успешно перезагружены." });
        }
        catch (Exception ex)
        {
            Logger.Log("AdminController", "ERROR", $"Ошибка при перезагрузке машин: {ex.Message}");
            return StatusCode(500, new { Message = "Произошла ошибка при перезагрузке машин." });
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

    [HttpPost("logs/clear")] // Доступно по /admin/admin/logs/clear
    public IActionResult ClearLogs()
    {
        try
        {
            if (System.IO.File.Exists("logs.txt"))
            {
                System.IO.File.WriteAllText("logs.txt", string.Empty); // Очищаем файл логов
            }
            return Ok(new { Message = "Логи успешно очищены." });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при очистке логов: {ex.Message}");
            return StatusCode(500, new { Message = "Произошла ошибка при очистке логов." });
        }
    }

    [HttpPost("audit/clear")] // Доступно по /admin/admin/audit/clear
    public IActionResult ClearAudit()
    {
        try
        {
            if (System.IO.File.Exists("audit.txt"))
            {
                System.IO.File.WriteAllText("audit.txt", string.Empty); // Очищаем файл аудита
            }
            return Ok(new { Message = "Аудит успешно очищен." });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при очистке аудита: {ex.Message}");
            return StatusCode(500, new { Message = "Произошла ошибка при очистке аудита." });
        }
    }
}