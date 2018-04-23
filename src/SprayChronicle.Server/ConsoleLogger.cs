using System;

namespace SprayChronicle.Server
{
    public sealed class ConsoleLogger<T> : ILogger<T>
    {
        private void Log(string level, string message)
        {
            Console.WriteLine($"{typeof(T).FullName}\n  [{level}] {message}");
        }

        public void LogDebug(Exception error)
        {
            Log("DEBUG", error.ToString());
        }

        public void LogDebug(string message)
        {
            Log("DEBUG", message);
        }

        public void LogDebug(Exception error, string message)
        {
            Log("DEBUG", $"{message}\n{error}");
        }

        public void LogInformation(string message)
        {
            Log("INFO", message);
        }

        public void LogWarning(Exception error)
        {
            Log("WARNING", error.ToString());
        }

        public void LogWarning(Exception error, string message)
        {
            Log("WARNING", $"{message}\n{error}");
        }

        public void LogError(Exception error)
        {
            Log("ERROR", error.ToString());
        }

        public void LogError(Exception error, string message)
        {
            Log("ERROR", $"{message}\n{error}");
        }

        public void LogCritical(Exception error)
        {
            Log("CRITICAL", error.ToString());
        }

        public void LogCritical(Exception error, string message)
        {
            Log("CRITICAL", $"{message}\n{error}");
        }
    }
}