namespace FollowMe.Data
{
    /// <summary>
    /// Класс, представляющий ответ с ошибкой.
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// Код ошибки.
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// Сообщение об ошибке.
        /// </summary>
        public string Message { get; set; }
    }
}