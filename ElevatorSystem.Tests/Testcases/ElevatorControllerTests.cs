using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using ElevatorSystem.Core.Utilities;
using ElevatorSystem.Core.Models;
using ElevatorSystem.Core.Enums;
using ElevatorSystem.Core.Services;


namespace ElevatorSystem.Tests
{
    [TestFixture]
    public class ElevatorControllerTests
    {
        private Mock<ILogger> _mockLogger;
        private ElevatorConfig _config;
        private List<Mock<Elevator>> _mockElevators;
        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _config = new ElevatorConfig
            {
                Floors = 10,
                ElevatorCount = 4,
                TimePerFloorMs = 100,
                LoadUnloadTimeMs = 100,
                DashboardRefreshMs = 200,
                RandomCallIntervalMs = (500, 1500)
            };
        }
        
        private ElevatorController CreateControllerWithMockedElevators()
        {
            // Create mock elevators
            _mockElevators = Enumerable.Range(1, _config.ElevatorCount)
                .Select(i =>
                {
                    var mockElevator = new Mock<Elevator>(i, _config, _mockLogger.Object) { CallBase = true };
                    mockElevator.SetupGet(e => e.Id).Returns(i);
                    mockElevator.SetupGet(e => e.Targets).Returns(new List<int>());
                    return mockElevator;
                })
                .ToList();

            // Replace the real elevators list using reflection (for test isolation)
            var controller = new ElevatorController(_config, _mockLogger.Object);
            var field = typeof(ElevatorController).GetField("_elevators",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field!.SetValue(controller, _mockElevators.Select(m => m.Object).ToList());
            return controller;
        }

        [Test]
        public void Constructor_ShouldInitializeCorrectNumberOfElevators()
        {
            var controller = new ElevatorController(_config, _mockLogger.Object);
            Assert.That(controller.Elevators.Count(), Is.EqualTo(_config.ElevatorCount));
        }

        [Test]
        public void AssignElevatorOptimized_ShouldAssignToNearestIdleElevator()
        {
            var controller = CreateControllerWithMockedElevators();

            // Setup: Elevator 1 is idle and closest
            _mockElevators[0].SetupGet(e => e.CurrentFloor).Returns(3);
            _mockElevators[0].SetupGet(e => e.IsIdle).Returns(true);
            _mockElevators[1].SetupGet(e => e.CurrentFloor).Returns(7);
            _mockElevators[1].SetupGet(e => e.IsIdle).Returns(false);
            _mockElevators[2].SetupGet(e => e.CurrentFloor).Returns(9);
            _mockElevators[2].SetupGet(e => e.IsIdle).Returns(false);

            var request = new ElevatorRequest(4, Direction.Up);
            var method = typeof(ElevatorController).GetMethod("AssignElevatorOptimized",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            method!.Invoke(controller, new object[] { request });

            _mockElevators[0].Verify(e => e.AddRequest(4), Times.Once);
            _mockLogger.Verify(l => l.Log(It.Is<string>(s => s.Contains("Assigned Elevator"))), Times.Once);
        }

        [Test]
        public void FindBestElevator_ShouldPreferSameDirectionElevator()
        {
            var controller = CreateControllerWithMockedElevators();

            _mockElevators[0].SetupGet(e => e.Direction).Returns(Direction.Up);
            _mockElevators[0].SetupGet(e => e.CurrentFloor).Returns(2);
            _mockElevators[1].SetupGet(e => e.Direction).Returns(Direction.Down);
            _mockElevators[1].SetupGet(e => e.CurrentFloor).Returns(5);
            _mockElevators[2].SetupGet(e => e.IsIdle).Returns(true);
            _mockElevators[2].SetupGet(e => e.CurrentFloor).Returns(9);

            var request = new ElevatorRequest(5, Direction.Up);

            var method = typeof(ElevatorController).GetMethod("FindBestElevator",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (Elevator)method!.Invoke(controller, new object[] { request })!;

            Assert.That(result.Id, Is.EqualTo(1), "Should prefer same-direction elevator");
        }

        [Test]
        public void GenerateRandomRequest_ShouldLogAndAssignElevator()
        {
            var controller = CreateControllerWithMockedElevators();

            var method = typeof(ElevatorController).GetMethod("GenerateRandomRequest",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            method!.Invoke(controller, null);

            _mockLogger.Verify(l => l.Log(It.Is<string>(msg => msg.Contains("[CALL]"))), Times.Once);
            _mockLogger.Verify(l => l.Log(It.Is<string>(msg => msg.Contains("Assigned Elevator"))), Times.Once);
        }

        [Test]
        public async Task StartAsync_ShouldRunElevatorTasksWithoutThrowing()
        {
            var controller = CreateControllerWithMockedElevators();
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(100); // prevent infinite loop

            Assert.DoesNotThrowAsync(async () => await controller.StartAsync(cts.Token));
        }
    }
}
