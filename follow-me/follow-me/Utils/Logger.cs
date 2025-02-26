using System;
using System.IO;

namespace FollowMe.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath = "logs.txt";

        public static void Log(string controller, string code, string message)
        {
            string logEntry = $"{DateTime.Now} | {controller} | {code} | {message}";

            // Записываем лог в файл
            using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            {
                writer.WriteLine(logEntry);
            }
        }
    }
}