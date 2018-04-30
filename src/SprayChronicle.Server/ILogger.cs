using System;

namespace SprayChronicle.Server
{
    public interface ILogger<T>
    {
        void LogDebug(Exception error);
        
        void LogDebug(string message);
        
        void LogDebug(Exception error, string message);
        
        void LogInformation(string message);
        
        void LogInformation(Exception error, string message);

        void LogWarning(Exception error);
        
        void LogWarning(Exception error, string message);

        void LogError(Exception error);

        void LogError(string message);

        void LogError(Exception error, string message);
        
        void LogCritical(Exception error);
        
        void LogCritical(Exception error, string message);
    }
}
