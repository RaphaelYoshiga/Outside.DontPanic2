using System;
using Shouldly;
using Xunit;

namespace Outside.DontPanic2UnitTests
{
    public class FloorShould
    {
        [Theory]
        [InlineData(11, 10)]
        [InlineData(15, 10)]
        [InlineData(16, 20)]
        [InlineData(20, 20)]
        [InlineData(21, 20)]
        public void GetClosestElevator(int clonePos, int expectedElevator)
        {
            const int firstElevator = 10;
            const int secondElevator = 20;
            var floor = new Floor(1, firstElevator);
            floor.Elevators.Add(secondElevator);

            var closestElevatorTo = floor.ClosestElevatorTo(clonePos);

            closestElevatorTo.ShouldBe(expectedElevator);
        }
    }
}
