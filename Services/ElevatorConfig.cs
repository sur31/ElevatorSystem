namespace ElevatorSystem.Services
{
    /// <summary>
    /// Configuration settings for the Elevator System.
    /// </summary>
    public class ElevatorConfig
    {
        /// <summary>
        /// Total number of floors in the building.
        /// </summary>
        public int Floors { get; set; } = 10;
        /// <summary>
        /// Total number of elevators in the system.
        /// </summary>
        public int ElevatorCount { get; set; } = 4;
        /// <summary>
        /// Time taken to move between floors in milliseconds.
        /// </summary>
        public int TimePerFloorMs { get; set; } = 10000;
        /// <summary>
        /// Time taken to load/unload passengers at a floor in milliseconds.
        /// </summary>
        public int LoadUnloadTimeMs { get; set; } = 10000;
        /// <summary>
        /// Dashboard refresh interval in milliseconds.
        /// </summary>
        public int DashboardRefreshMs { get; set; } = 2000;
        /// <summary>
        /// Interval range for random elevator calls in milliseconds.
        /// </summary>
        public (int min, int max) RandomCallIntervalMs { get; set; } = (5000, 15000);
    }
}