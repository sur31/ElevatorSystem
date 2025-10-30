namespace ElevatorSystem.Core.Utilities
{
    /// <summary>
    /// Logger interface for logging messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a message to the logger.
        /// </summary>
        /// <param name="message"></param>
        void Log(string message);
        /// <summary>
        /// Retrieves recent log messages.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        IEnumerable<string> GetRecentLogs(int count);
    }
}