using ElevatorSystem.Core.Services;
using ElevatorSystem.Core.Utilities;
namespace ElevatorSystem.Core
{
    internal class Program
    {
        /// <summary>
        /// The main entry point of the Elevator Control System simulation.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            var config = new ElevatorConfig();
            var logger = new ConsoleLogger();
            var controller = new ElevatorController(config, logger);
            var dashboard = new ElevatorDashboard(controller, config, logger);

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;
            logger.Log("Elevator Control System Simulation Started...");

            using var cts = new CancellationTokenSource();

            var controllerTask = controller.StartAsync(cts.Token);
            var dashboardTask = dashboard.StartAsync(cts.Token);

            Console.WriteLine("Press ENTER to stop simulation...");
            Console.ReadLine();
            cts.Cancel();

            await Task.WhenAll(controllerTask, dashboardTask);
        }
    }
}
