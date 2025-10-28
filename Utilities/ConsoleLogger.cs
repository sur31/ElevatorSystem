namespace ElevatorSystem.Utilities
{
    /// <summary>
    /// A simple console logger implementation of ILogger.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly List<string> _logs = new List<string>();
        private readonly object _lock = new object();
        
        /// <summary>
        /// Logs a message to the logger.
        /// </summary>
        /// <param name="message"></param>
        public void Log(string message)
        {
            lock (_lock)
            {
                _logs.Add($"{DateTime.Now:HH:mm:ss} - {message}");
                if (_logs.Count > 100)
                    _logs.RemoveRange(0, _logs.Count - 100);
            }
        }

        /// <summary>
        /// Retrieves recent log messages.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public IEnumerable<string> GetRecentLogs(int count)
        {
            lock (_lock)
            {
                return _logs.TakeLast(count).ToList();
            }
        }
    }
}