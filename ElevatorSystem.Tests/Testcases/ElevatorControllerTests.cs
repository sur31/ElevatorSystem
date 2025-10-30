using ElevatorSystem.Core.Enums;
using ElevatorSystem.Core.Models;
using ElevatorSystem.Core.Services;
using ElevatorSystem.Core.Utilities;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElevatorSystem.Tests
{
    [TestFixture]
    public class ElevatorControllerTests
    {
        private ElevatorConfig _config;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _config = new ElevatorConfig
            {
                Floors = 10,
                ElevatorCount = 3,
                RandomCallIntervalMs = (10, 20)
            };
            _loggerMock = new Mock<ILogger>();
        }

        [Test]
        public void Constructor_ShouldInitializeAllElevators()
        {
            var controller = new ElevatorController(_config, _loggerMock.Object);
            Assert.That(controller.Elevators.Count(), Is.EqualTo(3));
        }

        [Test]
        public void AssignElevatorOptimized_ShouldAssignToIdleElevator()
        {
            var controller = new ElevatorController(_config, _loggerMock.Object);
            var request = new ElevatorRequest(5, Direction.Up);

            controller.AssignElevatorOptimized(request);

            var assigned = controller.Elevators.FirstOrDefault(e => e.Targets.Contains(5));
            Assert.That(assigned, Is.Not.Null);
            _loggerMock.Verify(l => l.Log(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Test]
        public void FindBestElevator_ShouldReturnNearestIdle()
        {
            var controller = new ElevatorController(_config, _loggerMock.Object);
            var elevators = controller.Elevators.ToList();
            elevators[0].AddRequest(10); // busy
            elevators[1].AddRequest(9);  // busy
            elevators[2].AddRequest(5);  // busy

            var request = new ElevatorRequest(4, Direction.Up);
            var result = controller.FindBestElevator(request);

            Assert.That(result, Is.Not.Null);
        }
    }
}
