using ElevatorSystem.Core.Enums;
using ElevatorSystem.Core.Services;
using ElevatorSystem.Core.Utilities;

namespace ElevatorSystem.Core.Models
{
    /// <summary>
    /// Represents an individual elevator in the system.
    /// </summary>
    public class Elevator
    {
        /// <summary>
        /// Unique identifier for the elevator.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Current floor of the elevator.
        /// </summary>
        public int CurrentFloor { get; private set; }

        /// <summary>
        /// Current direction of the elevator.
        /// </summary>
        public Direction Direction { get; private set; } = Direction.Idle;

        /// <summary>
        /// Queue of target floors for the elevator to visit.
        /// </summary>
        private readonly Queue<int> _targets = new Queue<int>();

        /// <summary>
        /// Lock object for thread safety.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// Configuration settings for the elevator.
        /// </summary>
        private readonly ElevatorConfig _config;

        /// <summary>
        /// Logger for recording elevator events.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the Elevator class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        /// <param name="initialFloor"></param>
        public Elevator(int id, ElevatorConfig config, ILogger logger, int initialFloor = 1)
        {
            Id = id;
            CurrentFloor = initialFloor;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Indicates whether the elevator is currently idle.
        /// </summary>
        public bool IsIdle => Direction == Direction.Idle && !_targets.Any();

        /// <summary>
        /// Gets a read-only collection of target floors.
        /// </summary>
        public IReadOnlyCollection<int> Targets => _targets.ToList().AsReadOnly();

        /// <summary>
        /// Adds a new floor request to the elevator's target queue.
        /// </summary>
        /// <param name="floor"></param>
        public void AddRequest(int floor)
        {
            lock (_lock)
            {
                if (!_targets.Contains(floor))
                    _targets.Enqueue(floor);
            }
        }

        /// <summary>
        /// Runs the elevator, processing floor requests.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_targets.Count > 0)
                {
                    int targetFloor;
                    lock (_lock)
                        targetFloor = _targets.Peek();

                    if (CurrentFloor < targetFloor)
                    {
                        Direction = Direction.Up;
                        await MoveOneFloorAsync(1, token);
                    }
                    else if (CurrentFloor > targetFloor)
                    {
                        Direction = Direction.Down;
                        await MoveOneFloorAsync(-1, token);
                    }
                    else
                    {
                        await StopAtFloorAsync(token);
                        lock (_lock)
                            _targets.Dequeue();
                    }
                }
                else
                {
                    Direction = Direction.Idle;
                    await Task.Delay(1000, token);
                }
            }
        }

        /// <summary>
        /// Moves the elevator one floor in the specified direction.
        /// </summary>
        /// <param name="step"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task MoveOneFloorAsync(int step, CancellationToken token)
        {
            await Task.Delay(_config.TimePerFloorMs, token);
            CurrentFloor += step;
        }

        /// <summary>
        /// Stops the elevator at the current floor for loading/unloading.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task StopAtFloorAsync(CancellationToken token)
        {
            await Task.Delay(_config.LoadUnloadTimeMs, token);
        }

        /// <summary>
        /// Gets the status string of the elevator for dashboard display.
        /// </summary>
        /// <returns></returns>
        public string GetStatusString()
        {
            string queue = _targets.Count > 0 ? string.Join(",", _targets.ToArray()) : "-";
            return $"{Id,-9}| {CurrentFloor,-5}| {Direction,-10}| {queue}";
        }
    }
}