using ElevatorSystem.Core.Utilities;
using NUnit.Framework;
using System.Linq;

namespace ElevatorSystem.Tests
{
    [TestFixture]
    public class ConsoleLoggerTests
    {
        [Test]
        public void Log_ShouldStoreMessages()
        {
            var logger = new ConsoleLogger();
            logger.Log("Test Message");

            var logs = logger.GetRecentLogs(1).ToList();
            Assert.That(logs.Any(l => l.Contains("Test Message")), Is.True);
        }

        [Test]
        public void GetRecentLogs_ShouldReturnLimitedEntries()
        {
            var logger = new ConsoleLogger();
            for (int i = 0; i < 10; i++)
                logger.Log($"Log {i}");

            var recent = logger.GetRecentLogs(5);
            Assert.That(recent.Count(), Is.EqualTo(5));
        }
    }
}
