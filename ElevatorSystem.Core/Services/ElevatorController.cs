using ElevatorSystem.Core.Models;
using ElevatorSystem.Core.Enums;
using ElevatorSystem.Core.Utilities;

namespace ElevatorSystem.Core.Services
{
    /// <summary>
    /// Controls and manages multiple elevators in the system.
    /// </summary>
    public class ElevatorController
    {
        private readonly List<Elevator> _elevators;
        private readonly ElevatorConfig _config;
        private readonly ILogger _logger;
        private readonly Random _random = new Random();

        /// <summary>
        /// Initializes a new instance of the ElevatorController class.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="logger"></param>
        public ElevatorController(ElevatorConfig config, ILogger logger)
        {
            _config = config;
            _logger = logger;
            _elevators = Enumerable.Range(1, config.ElevatorCount)
                                   .Select(i => new Elevator(i, config, logger))
                                   .ToList();
        }

        public IEnumerable<Elevator> Elevators => _elevators;

        /// <summary>
        /// Starts the elevator controller to process requests.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken token)
        {
            foreach (var elevator in _elevators)
                _ = Task.Run(() => elevator.RunAsync(token), token);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(_random.Next(_config.RandomCallIntervalMs.min, _config.RandomCallIntervalMs.max), token);
                GenerateRandomRequest();
            }
        }

        /// <summary>
        /// Generates a random elevator request from a random floor.
        /// </summary>
        private void GenerateRandomRequest()
        {
            int floor = _random.Next(1, _config.Floors + 1);
            Direction direction = floor == _config.Floors ? Direction.Down :
                                  floor == 1 ? Direction.Up :
                                  (_random.Next(2) == 0 ? Direction.Up : Direction.Down);

            _logger.Log($"[CALL] Floor {floor} requests {direction} elevator.");
            AssignElevatorOptimized(new ElevatorRequest(floor, direction));
        }

        /// <summary>
        /// Assigns the best elevator to the given request using an optimized strategy.
        /// </summary>
        /// <param name="request"></param>
        private void AssignElevatorOptimized(ElevatorRequest request)
        {
            Elevator selectedElevator = FindBestElevator(request);
            selectedElevator.AddRequest(request.Floor);
            _logger.Log($"[Controller] Assigned Elevator {selectedElevator.Id} for floor {request.Floor}.");
        }

        /// <summary>
        /// Finds the best elevator for the given request based on current states.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Elevator FindBestElevator(ElevatorRequest request)
        {
            // Priority 1: Moving in same direction and can pick up on the way
            var sameDir = _elevators
                .Where(e => e.Direction == request.Direction)
                .Where(e => request.Direction == Direction.Up
                            ? e.CurrentFloor <= request.Floor
                            : e.CurrentFloor >= request.Floor)
                .OrderBy(e => Math.Abs(e.CurrentFloor - request.Floor))
                .FirstOrDefault();

            if (sameDir != null) return sameDir;

            // Priority 2: Idle elevator nearest to request floor
            var idle = _elevators
                .Where(e => e.IsIdle)
                .OrderBy(e => Math.Abs(e.CurrentFloor - request.Floor))
                .FirstOrDefault();

            if (idle != null) return idle;

            // Priority 3: Least busy elevator
            return _elevators
                .OrderBy(e => e.Targets.Count)
                .ThenBy(e => Math.Abs(e.CurrentFloor - request.Floor))
                .First();
        }
    }
}