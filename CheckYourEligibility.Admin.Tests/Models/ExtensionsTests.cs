using CheckYourEligibility.Admin.Models;
using FluentAssertions;


namespace CheckYourEligibility.Admin.Tests.Models
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
        public void GetUTCTime_With_Utc_Date_Should_Return_Same_Date()
        {
            // Arrange
            var utcDate = new DateTime(2026, 2, 10, 14, 0, 0, DateTimeKind.Utc);

            // Act
            var result = DateTimeExtensions.GetUTCTime(utcDate);

            // Assert
            result.Should().Be(utcDate);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void GetUTCTime_With_Local_BST_Date_Should_Convert_To_Utc()
        {
            // Arrange
            // 15 June 2026 11:00 BST = 10:00 UTC
            var localDate = new DateTime(2026, 6, 15, 11, 0, 0, DateTimeKind.Unspecified);

            // Act
            var result = DateTimeExtensions.GetUTCTime(localDate);

            // Assert
            result.Hour.Should().Be(10);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Test]
        public void ToLocalString12HourFormatReadable_Should_Format_GMT_Date_Correctly()
        {
            // Arrange
            var utcDate = new DateTime(2026, 2, 5, 14, 5, 0, DateTimeKind.Utc);

            // Act
            var result = utcDate.ToLocalString12HourFormatReadable();

            // Assert
            result.Should().Be("05 Feb 2026 02:05pm");
        }

        [Test]
        public void ToLocalString12HourFormatReadable_Should_Format_BST_Date_Correctly()
        {
            // Arrange
            var utcDate = new DateTime(2026, 7, 5, 14, 5, 0, DateTimeKind.Utc);

            // Act
            var result = utcDate.ToLocalString12HourFormatReadable();

            // Assert
            result.Should().Be("05 Jul 2026 03:05pm");
        }

        [Test]
        public void GetDateTimeOffsetFromString_In_Winter_Should_Have_Zero_Offset()
        {
            // Arrange
            var input = "2026-01-20 09:00";

            // Act
            var result = DateTimeExtensions.GetDateTimeOffsetFromString(input);

            // Assert
            result.Offset.Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void GetDateTimeOffsetFromString_In_Summer_Should_Have_One_Hour_Offset()
        {
            // Arrange
            var input = "2026-06-20 09:00";

            // Act
            var result = DateTimeExtensions.GetDateTimeOffsetFromString(input);

            // Assert
            result.Offset.Should().Be(TimeSpan.FromHours(1));
        }
    }

}
