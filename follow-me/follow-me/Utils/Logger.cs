using System;
using System.Diagnostics;
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
            // Формируем запись лога
            string logEntry = $"{DateTime.Now} | {controller} | {code} | {message}";

            // Логирование в файл
            WriteToFile(LogFilePath, logEntry);

            // Логирование в консоль
            Console.WriteLine(logEntry);

            // Логирование в Debug Console
            Debug.WriteLine(logEntry);
        }

        // Метод для логирования аудита
        public static void LogAudit(string carId, string movement)
        {
            // Формируем запись аудита
            string auditEntry = $"{DateTime.Now} | {carId} | {movement}";

            // Логирование в файл
            WriteToFile(AuditFilePath, auditEntry);

            // Логирование в консоль
            Console.WriteLine(auditEntry);

            // Логирование в Debug Console
            Debug.WriteLine(auditEntry);
        }

        // Вспомогательный метод для записи в файл
        private static void WriteToFile(string filePath, string content)
        {
            // Проверяем, существует ли файл
            if (!File.Exists(filePath))
            {
                // Создаем файл, если он не существует
                File.Create(filePath).Close(); // Закрываем поток после создания
            }

            // Записываем данные в файл
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(content);
            }
        }
    }
}