﻿using System;
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
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Left));

            decision.ShouldBe("BLOCK");
        }

        [Fact]
        public void ChooseToBlockForAnotherElevator()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 3)},
                {1, new Floor(1) },
                {2, new Floor(2)  }
            };
            var game = new Game(floors, 2, 3, 1);
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 2, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 3, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 3, Direction.Right));
            decision.ShouldBe("WAIT");
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
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 4, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(1, 4, Direction.Right));
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
                {4, new Floor(4)  }
            };
            var game = new Game(floors, 4, 6, 2);
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 4, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(1, 4, Direction.Right));
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(new Clone(1, 4, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(2, 4, Direction.Right));
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
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 4, Direction.Right));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(1, 4, Direction.Right));
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(new Clone(2, 4, Direction.Right));
            decision.ShouldBe("BLOCK");

            decision = game.TakeDecision(new Clone(2, 3, Direction.Left));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(3, 3, Direction.Left));
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(new Clone(4, 3, Direction.Left));
            decision.ShouldBe("BLOCK");
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
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Left));

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

            var game = new Game(floors, 4, 3, 4);
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Left));
            decision.ShouldBe("ELEVATOR");

            decision = game.TakeDecision(new Clone(1, 3, Direction.Left));
            decision.ShouldBe("ELEVATOR");
        }

        [Fact]
        public void HandleTrap()
        {
            var floors = new Floors()
            {
                {0, new Floor(0, 1) },
                {1, new Floor(3) }
            };

            var game = new Game(floors, 1, 0, 1);
            game.SetGeneralProperties(100, 10);
            
            var decision = game.TakeDecision(new Clone(0, 3, Direction.Left));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 2, Direction.Left));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 1, Direction.Left));
            decision.ShouldBe("WAIT");

            decision = game.TakeDecision(new Clone(0, 0, Direction.Left));
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
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Right));

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
            game.SetGeneralProperties(100, 10);

            var decision = game.TakeDecision(new Clone(0, 3, Direction.Right));

            decision.ShouldBe("BLOCK");
        }
    }
}
