using System;

namespace SprayChronicle.Server
{
    public sealed class VoidLogger<T> : ILogger<T>
    {
        public void LogDebug(Exception error)
        {
        }

        public void LogDebug(string message)
        {
        }

        public void LogDebug(Exception error, string message)
        {
        }

        public void LogInformation(string message)
        {
        }

        public void LogInformation(Exception error, string message)
        {
            
        }

        public void LogWarning(Exception error)
        {
        }

        public void LogWarning(Exception error, string message)
        {
        }

        public void LogError(Exception error)
        {
        }

        public void LogError(string message)
        {
            
        }

        public void LogError(Exception error, string message)
        {
        }

        public void LogCritical(Exception error)
        {
        }

        public void LogCritical(Exception error, string message)
        {
        }
    }
}
