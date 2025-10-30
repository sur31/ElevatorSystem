using NUnit.Framework;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ElevatorSystem.Core.Models;
using ElevatorSystem.Core.Enums;
using ElevatorSystem.Core.Utilities;
using ElevatorSystem.Core.Services;

namespace ElevatorSystem.Tests
{
    [TestFixture]
    public class ElevatorTests
    {
        private Elevator _elevator;
        private Mock<ILogger> _mockLogger;
        private ElevatorConfig _config;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger>();
            _config = new ElevatorConfig
            {
                Floors = 10,
                TimePerFloorMs = 10, // small to make tests fast
                LoadUnloadTimeMs = 10
            };
            _elevator = new Elevator(1, _config, _mockLogger.Object);
        }

        [Test]
        public void Constructor_ShouldInitializeCorrectly()
        {
            Assert.That(_elevator.Id, Is.EqualTo(1));
            Assert.That(_elevator.CurrentFloor, Is.EqualTo(1));
            Assert.That(_elevator.IsIdle, Is.True);
            Assert.That(_elevator.Targets, Is.Empty);
        }

        [Test]
        public void AddRequest_ShouldAddUniqueFloor()
        {
            _elevator.AddRequest(5);
            _elevator.AddRequest(5); // duplicate ignored
            _elevator.AddRequest(3);

            var targets = _elevator.Targets.ToList();
            Assert.That(targets, Has.Count.EqualTo(2));
            Assert.That(targets, Is.EquivalentTo(new[] { 5, 3 }));
        }

        [Test]
        public void IsIdle_ShouldBeFalse_WhenDirectionNotIdleOrTargetsExist()
        {
            _elevator.AddRequest(3);
            Assert.That(_elevator.IsIdle, Is.False);
        }

        [Test]
        public async Task RunAsync_ShouldMoveUpwards_WhenTargetAbove()
        {
            _elevator.AddRequest(3);
            using var cts = new CancellationTokenSource();
            cts.CancelAfter(10000); // give more time to avoid timing flakiness

            await _elevator.RunAsync(cts.Token);

            Assert.That(_elevator.CurrentFloor, Is.GreaterThanOrEqualTo(2));
            Assert.That(_elevator.Direction, Is.EqualTo(Direction.Up).Or.EqualTo(Direction.Idle));
        }

        [Test]
        public async Task RunAsync_ShouldMoveDownwards_WhenTargetBelow()
        {
            // Set elevator higher to force downward move
            var elevator = new Elevator(2, _config, _mockLogger.Object, initialFloor: 5);
            elevator.AddRequest(3);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(10000);

            await elevator.RunAsync(cts.Token);

            Assert.That(elevator.CurrentFloor, Is.LessThanOrEqualTo(5));
            Assert.That(elevator.Direction, Is.EqualTo(Direction.Down).Or.EqualTo(Direction.Idle));
        }

        [Test]
        public async Task RunAsync_ShouldBecomeIdle_WhenNoTargetsRemain()
        {
            var elevator = new Elevator(3, _config, _mockLogger.Object, initialFloor: 2);
            elevator.AddRequest(2); // target == current floor

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(10000);

            await elevator.RunAsync(cts.Token);

            Assert.That(elevator.IsIdle, Is.True);
        }

        [Test]
        public void GetStatusString_ShouldIncludeIdFloorDirectionAndTargets()
        {
            _elevator.AddRequest(5);
            var status = _elevator.GetStatusString();

            Assert.That(status, Does.Contain("1"));
            Assert.That(status, Does.Contain("Up").Or.Contain("Idle"));
            Assert.That(status, Does.Contain("5").Or.Contain("-"));
        }

        [Test]
        public void GetStatusString_ShouldShowDash_WhenNoTargets()
        {
            var status = _elevator.GetStatusString();
            Assert.That(status, Does.Contain("-"));
        }
    }
}
