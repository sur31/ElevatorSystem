using ElevatorSystem.Core.Enums;
namespace ElevatorSystem.Core.Models
{
    /// <summary>
    /// Represents a request made for an elevator.
    /// </summary>
    public class ElevatorRequest
    {
        /// <summary>
        /// The floor from which the request is made.
        /// </summary>
        public int Floor { get; }

        /// <summary>
        /// The direction of the requested elevator.
        /// </summary>
        public Direction Direction { get; }
        
        /// <summary>
        /// Initializes a new elevator request.
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="direction"></param>
        public ElevatorRequest(int floor, Direction direction)
        {
            Floor = floor;
            Direction = direction;
        }
    }
}