using FollowMe.Data;
using FollowMe.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

[Route("/admin")]
public class AdminController : Controller
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
        return View(cars);
    }

    [HttpGet("cars")] // Доступно по /admin/admin/cars
    public IActionResult GetCars()
    {
        var cars = _carRepository.GetAllCars();
        return Ok(cars);
    }

    [HttpPost("cars/add")] // Доступно по /admin/admin/cars/add
    public IActionResult AddCar()
    {
        Debug.WriteLine("AddCar method called.");
        var cars = _carRepository.GetAllCars();
        var newInternalId = (cars.Count + 1).ToString();
        var newCar = new Car { InternalId = newInternalId, ExternalId = "", Status = CarStatusEnum.Available };
        cars.Add(newCar);

        _carRepository.SaveAllCars(cars);
        TempData["Message"] = $"Машина {newInternalId} добавлена!";
        return RedirectToAction("Index");
    }

    [HttpPost("cars/remove")] // Доступно по /admin/admin/cars/remove
    public IActionResult RemoveCar(string internalId)
    {
        Debug.WriteLine($"RemoveCar method called for internalId: {internalId}");

        var cars = _carRepository.GetAllCars();
        Debug.WriteLine($"Current cars count before removal: {cars.Count}");

        var car = cars.FirstOrDefault(c => c.InternalId == internalId);
        if (car != null)
        {
            cars.Remove(car);
            Debug.WriteLine($"Car found and removed: {car.InternalId}");
            _carRepository.SaveAllCars(cars);
        }
        else
        {
            Debug.WriteLine($"Car with ID {internalId} not found!");
        }

        return RedirectToAction("Index");
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
            return StatusCode(500, "Произошла ошибка при получении логов.");
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
            return StatusCode(500, "Произошла ошибка при получении аудита.");
        }
    }
}