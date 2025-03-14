namespace FollowMe.Data
{
    /// <summary>
    /// Класс, представляющий ответ на запрос получения машины сопровождения.
    /// </summary>
    public class WeNeedFollowMeResponseDto
    {
        /// <summary>
        /// Идентификатор машины сопровождения.
        /// </summary>
        public Guid CarId { get; set; }

        /// <summary>
        /// Флаг, указывающий, требуется ли ожидание.
        /// </summary>
        public bool Wait { get; set; }
    }
}