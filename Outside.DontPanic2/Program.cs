﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

class Player
{
    private static Game _game;

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');
        Setup(inputs);

        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
            int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
            var direction = inputs[2] == "LEFT" ? Direction.Left : Direction.Right; // direction of the leading clone: LEFT or RIGHT

            var takeDecision = _game.TakeDecision(cloneFloor, clonePos, direction);    
            Console.WriteLine(takeDecision);
        }
    }

    private static void Setup(string[] inputs)
    {
        int nbFloors = int.Parse(inputs[0]); // number of floors
        int width = int.Parse(inputs[1]); // width of the area
        
        int nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        var exitFloor = int.Parse(inputs[3]);
        var exitPos = int.Parse(inputs[4]);
        int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        var numberElevatorsToBuild = int.Parse(inputs[6]);
        int nbElevators = int.Parse(inputs[7]); // number of elevators

        var floors = new Floors();
        for (int i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor

            if (floors.ContainsKey(elevatorFloor))
                floors[elevatorFloor].Elevators.Add(elevatorPos);
            else
                floors.Add(elevatorFloor, new Floor(elevatorFloor, elevatorPos));
        }

        for (int i = 0; i <= nbFloors; i++)
        {
            if (!floors.ContainsKey(i))
                floors[i] = new Floor(i);
        }

        _game = new Game(floors, exitFloor, exitPos, numberElevatorsToBuild);
        _game.SetGeneralProperties(nbRounds, nbTotalClones);
    }
   
}

public enum Direction
{
    Left,
    Right
}

public struct MapReturn
{
    public int TotalTravel { get; }
    public Direction Direction { get; }
    public bool ShouldBuild { get; }

    public MapReturn(int totalTravel, Direction direction, bool shouldBuild = false)
    {
        TotalTravel = totalTravel;
        Direction = direction;
        ShouldBuild = shouldBuild;
    }
}

public class Game
{
    private readonly Floors _floors;
    private readonly int _exitFloor;
    private readonly int _exitPosition;
    private int _elevatorsToBuild;

    public Game(Floors floors, int exitFloor, int exitPosition, int elevatorsToBuild)
    {
        _floors = floors;
        _exitFloor = exitFloor;
        _exitPosition = exitPosition;
        _elevatorsToBuild = elevatorsToBuild;
    }

    public string TakeDecision(int cloneFloor, int clonePos, Direction direction)
    {
        if (clonePos == -1)
        {
            return "WAIT";
        }
        var floor = _floors[cloneFloor];
        if (floor.Elevators.Any(p => p == clonePos))
        {
            Console.Error.WriteLine("Llll");
            return "WAIT";
        }

        Console.Error.WriteLine($"Clone Floor: {cloneFloor}");
        if (_elevatorsToBuild > 0 && !floor.Elevators.Any())
        {
            Console.Error.WriteLine($"Built From not any elevator");
            return BuildElevator(floor, clonePos);
        }
            

        var shortPath = ShortPath(cloneFloor, clonePos, direction, _elevatorsToBuild);
        if (shortPath.ShouldBuild)
            return BuildElevator(floor, clonePos);

        var cloneGoingToRightDirection = direction == shortPath.Direction;
        return cloneGoingToRightDirection ? "WAIT" : "BLOCK";
    }

    private MapReturn ShortPath(int cloneFloor, int clonePos, Direction direction, int elevatorsToBuild)
    {
        var exitFloor = _floors[cloneFloor];
        var elevators = exitFloor.ElevatorsByDirection(clonePos, direction);
        if (cloneFloor == _exitFloor)
        {
            return HandleLastFloor(clonePos, direction, elevators);
        }

        var left = CalculateFor(cloneFloor, elevators.ClosestLeft, Direction.Left, elevatorsToBuild);
        var right = CalculateFor(cloneFloor, elevators.ClosestRight, Direction.Right, elevatorsToBuild);
        var elevator = ElevatorTotalTravel(cloneFloor, clonePos, direction, elevatorsToBuild);

        if (left <= right && left <= elevator)
            return new MapReturn(left, Direction.Left);
        if(right <= elevator)
            return new MapReturn(right, Direction.Right);

        return new MapReturn(elevator, direction, true);
    }

    private int ElevatorTotalTravel(int cloneFloor, int clonePos, Direction direction, int elevatorsToBuild)
    {
        if (elevatorsToBuild > 0)
        {
            var totalTravel = ShortPath(cloneFloor + 1, clonePos, direction, elevatorsToBuild - 1).TotalTravel;
            if(totalTravel == int.MaxValue)
                return int.MaxValue;

            return 1 + totalTravel;
        }

        return int.MaxValue;
    }

    private MapReturn HandleLastFloor(int clonePos, Direction direction, ElevatorReturn elevators)
    {
        if (clonePos == _exitPosition)
            return new MapReturn(0, direction);

        var returnDirection = _exitPosition > clonePos ? Direction.Right : Direction.Left;
        if (returnDirection == Direction.Right)
        {
            if(elevators.ClosestRight != null && elevators.ClosestRight.Position  < _exitPosition )
                return new MapReturn(int.MaxValue, returnDirection);

            return new MapReturn(_floors[_exitFloor].GetDistance(clonePos, _exitPosition, direction), returnDirection);
        }

        if (elevators.ClosestLeft != null &&  elevators.ClosestLeft.Position > _exitPosition)
            return new MapReturn(int.MaxValue, returnDirection);

        return new MapReturn(_floors[_exitFloor].GetDistance(clonePos, _exitPosition, direction), returnDirection);
    }

    private int CalculateFor(int cloneFloor, ClosestElevator elevator, Direction direction, int elevatorsToBuild)
    {
        if (elevator == null)
            return int.MaxValue;

        if (cloneFloor == _exitFloor)
            return elevator.Distance;

        var totalTravel = ShortPath(cloneFloor + 1, elevator.Position, direction, elevatorsToBuild).TotalTravel;
        if (totalTravel == int.MaxValue)
            return totalTravel;

        return elevator.Distance + totalTravel;
    }


    private string BuildElevator(Floor floor, int clonePos)
    {
        _elevatorsToBuild--;
        floor.Elevators.Add(clonePos);
        Console.Error.WriteLine($"Built elevator at: {floor.Y}, pos: {clonePos}");
        return "ELEVATOR";
    }

    public void SetGeneralProperties(int nbRounds, int nbTotalClones)
    {
    }
}

public class Floors : Dictionary<int, Floor>
{
}

public class ElevatorReturn
{
    public ClosestElevator ClosestLeft { get; set; }
    public ClosestElevator ClosestRight { get; set; }
}

public class ClosestElevator
{
    public ClosestElevator(int position, int clonePosition, Direction direction)
    {
        Position = position;
        Distance = GetDistance(clonePosition, position, direction);
    }
    private int GetDistance(int clonePos, int elevator, Direction direction)
    {
        var elevatorDirection = clonePos - elevator < 0 ? Direction.Right : Direction.Left;
        var isCloneGoingToRightDirection = elevatorDirection == direction;

        var abs = Math.Abs(elevator - clonePos);
        return isCloneGoingToRightDirection ? abs : abs + 3;
    }

    public int Position { get; }

    public int Distance { get; }
}

public class Floor
{
    public int Y { get; }

    public List<int> Elevators { get; set; } = new List<int>();

    public ElevatorReturn ElevatorsByDirection(int clonePos, Direction direction)
    {
        var elevatorsReturn = new ElevatorReturn();
        var lookup = Elevators.ToLookup(p => p > clonePos ? Direction.Right : Direction.Left, x => new ClosestElevator(x, clonePos, direction));
        if (lookup.Contains(Direction.Right))
            elevatorsReturn.ClosestRight = lookup[Direction.Right].OrderBy(p => p.Distance).First();
        if (lookup.Contains(Direction.Left))
            elevatorsReturn.ClosestLeft = lookup[Direction.Left].OrderBy(p => p.Distance).First();

        return elevatorsReturn;
    }

    public Floor(int y, params int[] elevatorPos)
    {
        Y = y;

        Elevators.AddRange(elevatorPos);
    }

    public int ClosestElevatorTo(int clonePos, Direction direction)
    {
        if (!Elevators.Any())
            return -1;

        return Elevators.OrderBy(elevator => GetDistance(clonePos, elevator, direction))
            .First();
    }

    public int GetDistance(int clonePos, int elevator, Direction direction)
    {
        var elevatorDirection = clonePos - elevator < 0 ? Direction.Right : Direction.Left;
        var isCloneGoingToRightDirection = elevatorDirection == direction;

        var distance = elevator - clonePos;
        var abs = Math.Abs(distance);
        return isCloneGoingToRightDirection ? abs : abs + 3;
    }
}