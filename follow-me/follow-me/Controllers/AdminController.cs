using FollowMe.Data;
using FollowMe.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

public class AdminController : Controller
{
    private readonly CarRepository _carRepository;

    public AdminController()
    {
        _carRepository = new CarRepository();
    }

    // Отображение списка машин
    public IActionResult Index()
    {
        var cars = _carRepository.GetAllCars();
        return View(cars);
    }

    // Добавление новой машины
    [HttpPost]
    [ValidateAntiForgeryToken] // Защита от CSRF
    public IActionResult AddCar()
    {
        Debug.WriteLine("AddCar method called."); // Логирование
        var cars = _carRepository.GetAllCars();
        var newInternalId = (cars.Count + 1).ToString();
        var newCar = new Car { InternalId = newInternalId, ExternalId = "", Status = CarStatusEnum.Available };
        cars.Add(newCar);

        _carRepository.SaveAllCars(cars);
        Debug.WriteLine($"New car added: {newInternalId}"); // Логирование
        return RedirectToAction("Index");
    }

    // Удаление машины
    [HttpPost]
    [ValidateAntiForgeryToken] // Защита от CSRF
    public IActionResult RemoveCar(string internalId)
    {
        Debug.WriteLine($"RemoveCar method called for internalId: {internalId}"); // Логирование
        var cars = _carRepository.GetAllCars();
        var car = cars.FirstOrDefault(c => c.InternalId == internalId);
        if (car != null)
        {
            cars.Remove(car);
            _carRepository.SaveAllCars(cars);
            Debug.WriteLine($"Car removed: {internalId}"); // Логирование
        }

        return RedirectToAction("Index");
    }
}