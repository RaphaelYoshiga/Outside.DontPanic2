using System;
using System.Collections.Generic;
using System.Text;
using Shouldly;
using Xunit;

namespace Outside.DontPanic2UnitTests
{
    public class GameShould
    {
        [Fact]
        public void ChooseToBlock()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 1, 7)},
                {1, new Floor(1, 6) },
                {2, new Floor(2, 3)  }
            };
            var game = new Game(floors,2,3, 0);

            var decision = game.TakeDecision(0, 3, Direction.Left);

            decision.ShouldBe("BLOCK");
        }

        [Fact]
        public void ChooseToUTurn()
        {
            var floors = new Floors()
            {
                {0, new Floor(0)},
            };
            var game = new Game(floors, 0, 1, 0);

            var decision = game.TakeDecision(0, 3, Direction.Right);

            decision.ShouldBe("BLOCK");
        }
    }
}
