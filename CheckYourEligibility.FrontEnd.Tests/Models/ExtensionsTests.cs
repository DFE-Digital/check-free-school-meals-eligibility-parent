using CheckYourEligibility.FrontEnd.Models;
using FluentAssertions;

namespace CheckYourEligibility.FrontEnd.Tests.Models
{
    [TestFixture]
    public class DateTimeExtensionsTests
    {
        [Test]
        public void GetLocalTime_With_UTC_Date_In_Winter_Should_Return_GMT_Time()
        {
            // Arrange
            var utcDate = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeExtensions.GetLocalTime(utcDate);

            // Assert
            result.Hour.Should().Be(10);
            result.Minute.Should().Be(30);
        }

        [Test]
        public void GetLocalTime_With_UTC_Date_In_Summer_Should_Return_BST_Time()
        {
            // Arrange
            var utcDate = new DateTime(2026, 6, 15, 10, 30, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeExtensions.GetLocalTime(utcDate);

            // Assert
            result.Hour.Should().Be(11);
            result.Minute.Should().Be(30);
        }

        [Test]
        public void ToLocalString12HourFormatReadable_Should_Format_GMT_Date_Correctly()
        {
            // Arrange
            var utcDate = new DateTime(2026, 2, 5, 14, 5, 0, DateTimeKind.Utc);

            // Act
            var result = utcDate.ToLocalString12HourFormatReadableWithAt();

            // Assert
            result.Should().Be("05 Feb 2026 at 2:05pm");
        }

        [Test]
        public void ToLocalString12HourFormatReadable_Should_Format_BST_Date_Correctly()
        {
            // Arrange
            var utcDate = new DateTime(2026, 7, 5, 14, 5, 0, DateTimeKind.Utc);

            // Act
            var result = utcDate.ToLocalString12HourFormatReadableWithAt();

            // Assert
            result.Should().Be("05 Jul 2026 at 3:05pm");
        }
    }
}
