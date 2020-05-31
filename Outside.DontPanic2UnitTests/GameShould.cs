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
            var game = new Game(floors, 2, 3, 0);

            var decision = game.TakeDecision(0, 3, Direction.Left);

            decision.ShouldBe("BLOCK");
        }

        [Fact]
        public void OnlyBuildInRightFloors()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 4)},
                {1, new Floor(1) },
                {2, new Floor(2,6)  }
            };
            var game = new Game(floors, 2, 6, 1);

            var decision = game.TakeDecision(0, 3, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(0, 4, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(1, 4, Direction.Right);
            decision.ShouldBe("ELEVATOR");
        }

        [Fact]
        public void OnlyBuildInRightFloors2()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 4)},
                {1, new Floor(1) },
                {2, new Floor(2) },
                {3, new Floor(3,6)  },
                {4, new Floor(4,6)  }
            };
            var game = new Game(floors, 4, 6, 2);

            var decision = game.TakeDecision(0, 3, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(0, 4, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(1, 4, Direction.Right);
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(1, 4, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(2, 4, Direction.Right);
            decision.ShouldBe("ELEVATOR");
        }

        [Fact]
        public void OnlyBuildInRightFloors3()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 4)},
                {1, new Floor(1) },
                {2, new Floor(2, 3) },
                {3, new Floor(3)  },
                {4, new Floor(4)  }
            };
            var game = new Game(floors, 4, 6, 2);

            var decision = game.TakeDecision(0, 3, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(0, 4, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(1, 4, Direction.Right);
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(1, 4, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(2, 4, Direction.Right);
            decision.ShouldBe("BLOCK");

            decision = game.TakeDecision(2, 3, Direction.Right);
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(3, 3, Direction.Right);
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(3, 3, Direction.Right);
            decision.ShouldBe("WAIT");
        }

        [Fact]
        public void ChooseToBuild()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 1, 7)},
                {1, new Floor(1, 6) },
                {2, new Floor(2, 3)  }
            };
            var game = new Game(floors, 1, 3, 1);

            var decision = game.TakeDecision(0, 3, Direction.Left);

            decision.ShouldBe("ELEVATOR");
        }

        [Fact]
        public void BuildOneElevatorPerFloor()
        {
            var floors = new Floors()
            {
                {0, new Floor(0) },
                {1, new Floor(1) },
                {2, new Floor(2) },
                {3, new Floor(3) },
                {4, new Floor(3) }
            };

            var game = new Game(floors, 4, 3, 1);
            
            var decision = game.TakeDecision(0, 3, Direction.Left);
            decision.ShouldBe("ELEVATOR");
        }

        [Fact]
        public void ChooseToWait()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 1, 7)},
                {1, new Floor(1, 6) },
                {2, new Floor(2, 3)  }
            };
            var game = new Game(floors, 2, 3, 0);

            var decision = game.TakeDecision(0, 3, Direction.Right);

            decision.ShouldBe("WAIT");
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
