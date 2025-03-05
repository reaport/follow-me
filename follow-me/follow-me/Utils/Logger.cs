using System;
using System.IO;

namespace FollowMe.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath = "logs.txt";
        private static readonly string AuditFilePath = "audit.txt";

        // Метод для логирования
        public static void Log(string controller, string code, string message)
        {
            // Проверяем, существует ли файл logs.txt
            if (!File.Exists(LogFilePath))
            {
                // Создаем файл, если он не существует
                File.Create(LogFilePath).Close(); // Закрываем поток после создания
            }

            string logEntry = $"{DateTime.Now} | {controller} | {code} | {message}";

            // Записываем лог в файл logs.txt
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }

        // Метод для логирования аудита
        public static void LogAudit(string carId, string movement)
        {
            // Проверяем, существует ли файл audit.txt
            if (!File.Exists(AuditFilePath))
            {
                // Создаем файл, если он не существует
                File.Create(AuditFilePath).Close(); // Закрываем поток после создания
            }

            string auditEntry = $"{DateTime.Now} | {carId} | {movement}";

            // Записываем аудит в файл audit.txt
            using (StreamWriter writer = new StreamWriter(AuditFilePath, true))
            {
                writer.WriteLine(auditEntry);
            }
        }
    }
}