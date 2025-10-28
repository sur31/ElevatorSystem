using ElevatorSystem.Utilities;
namespace ElevatorSystem.Services
{
    /// <summary>
    /// Represents the dashboard for monitoring elevator statuses.
    /// </summary>
    public class ElevatorDashboard
    {
        private readonly ElevatorController _controller;
        private readonly ElevatorConfig _config;
        private readonly ILogger _logger;
        private readonly DateTime _start = DateTime.Now;

        /// <summary>
        /// Initializes a new instance of the ElevatorDashboard class.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public ElevatorDashboard(ElevatorController controller, ElevatorConfig config, ILogger logger)
        {
            _controller = controller;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Starts the dashboard to display elevator statuses periodically.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Console.Clear();
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine($"Time: {(DateTime.Now - _start):hh\\:mm\\:ss}");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine($"Elevator | Floor | Direction | Pending Stops");
                Console.WriteLine("---------------------------------------------");

                foreach (var elevator in _controller.Elevators)
                    Console.WriteLine(elevator.GetStatusString());

                Console.WriteLine("---------------------------------------------");

                foreach (var msg in _logger.GetRecentLogs(6))
                    Console.WriteLine(msg);

                await Task.Delay(_config.DashboardRefreshMs, token);
            }
        }
    }
}