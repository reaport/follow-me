namespace FollowMe.Services
{
    /// <summary>
    /// Интерфейс для сервиса оркестрации, который управляет началом и окончанием движения машины.
    /// </summary>
    public interface IOrchestratorService
    {
        /// <summary>
        /// Инициирует начало движения машины с указанным идентификатором машины и самолета.
        /// </summary>
        /// <param name="carId">Идентификатор машины.</param>
        /// <param name="aircraftId">Идентификатор самолета.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task StartMovementAsync(string carId, string aircraftId);

        /// <summary>
        /// Завершает движение машины с указанным идентификатором машины, самолета и флагом взлета.
        /// </summary>
        /// <param name="carId">Идентификатор машины.</param>
        /// <param name="aircraftId">Идентификатор самолета.</param>
        /// <param name="isTakeoff">Флаг, указывающий, связан ли запрос с взлетом.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        Task EndMovementAsync(string carId, string aircraftId, bool isTakeoff);
    }
}