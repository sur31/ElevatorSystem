using ElevatorSystem.Core.Enums;
using ElevatorSystem.Core.Models;
using ElevatorSystem.Core.Services;
using ElevatorSystem.Core.Utilities;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ElevatorSystem.Tests
{
    [TestFixture]
    public class ElevatorTests
    {
        private ElevatorConfig _config;
        private Mock<ILogger> _loggerMock;

        [SetUp]
        public void Setup()
        {
            _config = new ElevatorConfig { Floors = 10, TimePerFloorMs = 5, LoadUnloadTimeMs = 5 };
            _loggerMock = new Mock<ILogger>();
        }

        [Test]
        public void Constructor_ShouldInitializeValues()
        {
            var elevator = new Elevator(1, _config, _loggerMock.Object, 3);
            Assert.That(elevator.Id, Is.EqualTo(1));
            Assert.That(elevator.CurrentFloor, Is.EqualTo(3));
            Assert.That(elevator.IsIdle, Is.True);
        }

        [Test]
        public void AddRequest_ShouldAddUniqueFloor()
        {
            var elevator = new Elevator(1, _config, _loggerMock.Object);
            elevator.AddRequest(5);
            elevator.AddRequest(5);
            Assert.That(elevator.Targets.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task RunAsync_ShouldMoveElevatorUp()
        {
            var elevator = new Elevator(1, _config, _loggerMock.Object, 1);
            elevator.AddRequest(3);

            using var cts = new CancellationTokenSource(100);
            await elevator.RunAsync(cts.Token);

            Assert.That(elevator.CurrentFloor, Is.GreaterThan(1));
        }

        [Test]
        public void GetStatusString_ShouldReturnFormattedString()
        {
            var elevator = new Elevator(2, _config, _loggerMock.Object, 4);
            elevator.AddRequest(6);
            var status = elevator.GetStatusString();
            Assert.That(status, Does.Contain("2"));
            Assert.That(status, Does.Contain("4"));
        }
    }
}
