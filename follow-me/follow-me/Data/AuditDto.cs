namespace FollowMe.Data
{
    /// <summary>
    /// Класс, представляющий запись аудита.
    /// </summary>
    public class AuditDto
    {
        /// <summary>
        /// Временная метка записи аудита.
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Идентификатор машины.
        /// </summary>
        public string CarId { get; set; }

        /// <summary>
        /// Действие машины.
        /// </summary>
        public string Movement { get; set; }
    }
}