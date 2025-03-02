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
            string auditEntry = $"{DateTime.Now} | {carId} | {movement}";

            // Записываем аудит в файл audit.txt
            using (StreamWriter writer = new StreamWriter("audit.txt", true))
            {
                writer.WriteLine(auditEntry);
            }
        }
    }
}