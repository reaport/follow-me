using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FollowMe.Data;

namespace FollowMe.Services
{
    /// <summary>
    /// Интерфейс для взаимодействия с системой управления наземным движением.
    /// </summary>
    public interface IGroundControlService
    {
        /// <summary>
        /// Регистрирует транспортное средство в системе.
        /// </summary>
        /// <param name="vehicleType">Тип транспортного средства.</param>
        /// <returns>Ответ с данными о регистрации.</returns>
        Task<VehicleRegistrationResponse> RegisterVehicle(string vehicleType);

        /// <summary>
        /// Получает маршрут между двумя узлами.
        /// </summary>
        /// <param name="from">Начальный узел.</param>
        /// <param name="to">Конечный узел.</param>
        /// <returns>Массив узлов маршрута.</returns>
        Task<string[]> GetRoute(string from, string to);

        /// <summary>
        /// Запрашивает разрешение на перемещение транспортного средства.
        /// </summary>
        /// <param name="vehicleId">Идентификатор транспортного средства.</param>
        /// <param name="vehicleType">Тип транспортного средства.</param>
        /// <param name="from">Начальный узел.</param>
        /// <param name="to">Конечный узел.</param>
        /// <param name="withAirplane">Идентификатор самолета (если требуется).</param>
        /// <returns>Расстояние для перемещения.</returns>
        Task<double> RequestMove(string vehicleId, string vehicleType, string from, string to, string withAirplane);

        /// <summary>
        /// Уведомляет систему о прибытии транспортного средства в узел.
        /// </summary>
        /// <param name="vehicleId">Идентификатор транспортного средства.</param>
        /// <param name="vehicleType">Тип транспортного средства.</param>
        /// <param name="nodeId">Идентификатор узла.</param>
        Task NotifyArrival(string vehicleId, string vehicleType, string nodeId);
    }
}