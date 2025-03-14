using System;
using System.Diagnostics;
using System.IO;

namespace FollowMe.Utils
{
    /// <summary>
    /// Статический класс для логирования сообщений и аудита.
    /// </summary>
    public static class Logger
    {
        private static readonly string LogFilePath = "logs.txt";
        private static readonly string AuditFilePath = "audit.txt";

        /// <summary>
        /// Логирует сообщение с указанным контроллером, кодом и текстом сообщения.
        /// </summary>
        /// <param name="controller">Название контроллера или сервиса, откуда происходит логирование.</param>
        /// <param name="code">Код или уровень логирования (например, INFO, ERROR).</param>
        /// <param name="message">Текст сообщения для логирования.</param>
        public static void Log(string controller, string code, string message)
        {
            string logEntry = $"{DateTime.Now} | {controller} | {code} | {message}";
            WriteToFile(LogFilePath, logEntry);
            Console.WriteLine(logEntry);
            Debug.WriteLine(logEntry);
        }

        /// <summary>
        /// Логирует аудиторскую запись с указанным идентификатором машины и действием.
        /// </summary>
        /// <param name="carId">Идентификатор машины.</param>
        /// <param name="movement">Действие или движение, которое нужно залогировать.</param>
        public static void LogAudit(string carId, string movement)
        {
            string auditEntry = $"{DateTime.Now} | {carId} | {movement}";
            WriteToFile(AuditFilePath, auditEntry);
            Console.WriteLine(auditEntry);
            Debug.WriteLine(auditEntry);
        }

        /// <summary>
        /// Вспомогательный метод для записи данных в файл.
        /// </summary>
        /// <param name="filePath">Путь к файлу, в который нужно записать данные.</param>
        /// <param name="content">Содержимое, которое нужно записать.</param>
        private static void WriteToFile(string filePath, string content)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(content);
            }
        }
    }
}