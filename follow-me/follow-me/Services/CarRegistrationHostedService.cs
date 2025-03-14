namespace FollowMe.Services
{
    /// <summary>
    /// Фоновая служба для регистрации машин при старте приложения.
    /// </summary>
    public class CarRegistrationHostedService : IHostedService
    {
        private readonly CarRepository _carRepository;
        private readonly ILogger<CarRegistrationHostedService> _logger;

        public CarRegistrationHostedService(CarRepository carRepository, ILogger<CarRegistrationHostedService> logger)
        {
            _carRepository = carRepository;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Запуск регистрации машин...");

            try
            {
                // Перезагружаем машины при старте приложения
                await _carRepository.ReloadCars();
                _logger.LogInformation("Машины успешно перезагружены.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при перезагрузке машин.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Остановка службы регистрации машин.");
            return Task.CompletedTask;
        }
    }
}