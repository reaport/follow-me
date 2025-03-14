using FollowMe.Data;
using FollowMe.Services;
using Microsoft.AspNetCore.Mvc;
using FollowMe.Utils;

[Route("/admin")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly CarRepository _carRepository;
    private readonly IGroundControlService _groundControlService;

    /// <summary>
    /// Конструктор контроллера. Внедряет зависимости через DI.
    /// </summary>
    /// <param name="carRepository">Репозиторий для работы с машинами.</param>
    /// <param name="groundControlService">Сервис для взаимодействия с системой управления.</param>
    public AdminController(CarRepository carRepository, IGroundControlService groundControlService)
    {
        _carRepository = carRepository;
        _groundControlService = groundControlService;
    }

    /// <summary>
    /// Получает список всех машин. Доступно по /admin/admin.
    /// </summary>
    /// <returns>Список машин в формате JSON.</returns>
    [HttpGet("")]
    public IActionResult Index()
    {
        var cars = _carRepository.GetAllCars();
        return Ok(cars);
    }

    /// <summary>
    /// Получает список всех машин. Доступно по /admin/admin/cars.
    /// </summary>
    /// <returns>Список машин в формате JSON.</returns>
    [HttpGet("cars")]
    public IActionResult GetCars()
    {
        var cars = _carRepository.GetAllCars();
        return Ok(cars);
    }

    /// <summary>
    /// Добавляет новую машину в систему. Доступно по /admin/admin/cars/add.
    /// </summary>
    /// <returns>Сообщение об успешном добавлении или ошибку, если лимит машин достигнут.</returns>
    [HttpPost("cars/add")]
    public async Task<IActionResult> AddCar()
    {
        var cars = _carRepository.GetAllCars();

        // Проверяем, не достигнут ли лимит машин (10 штук)
        if (cars.Count < 10)
        {
            var newInternalId = Guid.NewGuid().ToString();

            var registrationResponse = await _groundControlService.RegisterVehicle("follow-me");

            if (string.IsNullOrEmpty(registrationResponse.VehicleId))
            {
                Logger.Log("AdminController", "ERROR", "Не удалось зарегистрировать машину.");
                return StatusCode(500, new { Message = "Не удалось зарегистрировать машину." });
            }

            var newCar = new Car
            {
                InternalId = newInternalId,
                ExternalId = registrationResponse.VehicleId,
                Status = CarStatusEnum.Available,
                CurrentNode = registrationResponse.GarrageNodeId
            };

            cars.Add(newCar);

            _carRepository.SaveAllCars(cars);

            Logger.LogAudit(newInternalId, "Машина добавлена и зарегистрирована");

            return Ok(new { Message = $"Машина {newInternalId} добавлена и зарегистрирована с ExternalId: {registrationResponse.VehicleId}!" });
        }
        else
        {
            return BadRequest(new { Message = "Невозможно добавить машину. Достигнут лимит в 10 машин." });
        }
    }

    /// <summary>
    /// Перезагружает все машины в системе. Доступно по /admin/admin/cars/reload.
    /// </summary>
    /// <returns>Сообщение об успешной перезагрузке или ошибку.</returns>
    [HttpPost("cars/reload")]
    public async Task<IActionResult> ReloadCars()
    {
        Logger.Log("AdminController", "INFO", "Запрос на перезагрузку всех машин.");

        try
        {
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

    /// <summary>
    /// Получает логи системы. Доступно по /admin/admin/logs.
    /// </summary>
    /// <returns>Список логов в формате JSON или ошибку.</returns>
    [HttpGet("logs")]
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
            return StatusCode(500, new { Message = "Произошла ошибка при получении логов." });
        }
    }

    /// <summary>
    /// Получает записи аудита. Доступно по /admin/admin/audit.
    /// </summary>
    /// <returns>Список записей аудита в формате JSON или ошибку.</returns>
    [HttpGet("audit")]
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
            return StatusCode(500, new { Message = "Произошла ошибка при получении аудита." });
        }
    }

    /// <summary>
    /// Очищает логи системы. Доступно по /admin/admin/logs/clear.
    /// </summary>
    /// <returns>Сообщение об успешной очистке или ошибку.</returns>
    [HttpPost("logs/clear")]
    public IActionResult ClearLogs()
    {
        try
        {
            if (System.IO.File.Exists("logs.txt"))
            {
                System.IO.File.WriteAllText("logs.txt", string.Empty);
            }
            return Ok(new { Message = "Логи успешно очищены." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Произошла ошибка при очистке логов." });
        }
    }

    /// <summary>
    /// Очищает записи аудита. Доступно по /admin/admin/audit/clear.
    /// </summary>
    /// <returns>Сообщение об успешной очистке или ошибку.</returns>
    [HttpPost("audit/clear")]
    public IActionResult ClearAudit()
    {
        try
        {
            if (System.IO.File.Exists("audit.txt"))
            {
                System.IO.File.WriteAllText("audit.txt", string.Empty);
            }
            return Ok(new { Message = "Аудит успешно очищен." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "Произошла ошибка при очистке аудита." });
        }
    }
}