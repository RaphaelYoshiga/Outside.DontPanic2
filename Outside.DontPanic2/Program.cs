using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

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

        _game = new Game(floors, exitFloor, exitPos, numberElevatorsToBuild);
        _game.SetGeneralProperties(nbRounds, nbTotalClones);
    }
   
}

public enum Direction
{
    Left,
    Right
}

internal class Game
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
        var floor = GetFloor(cloneFloor);
        if (clonePos == -1)
            return "WAIT";

        var closestElevatorTo = floor.ClosestElevatorTo(clonePos, direction);
        if (closestElevatorTo < 0 && _elevatorsToBuild > 0)
            return BuildElevator(floor, clonePos);

        var correctDirection = clonePos - closestElevatorTo < 0 ? Direction.Right : Direction.Left;
        var cloneGoingToRightDirection = direction == correctDirection || clonePos == closestElevatorTo;
        return cloneGoingToRightDirection ? "WAIT" : "BLOCK";
    }
    private string BuildElevator(Floor floor, int clonePos)
    {
        _elevatorsToBuild--;
        floor.Elevators.Add(clonePos);
        return "ELEVATOR";
    }

    private Floor GetFloor(int cloneFloor)
    {
        if (cloneFloor == _exitFloor)
            return new Floor(_exitFloor, _exitPosition);

        if (_floors.ContainsKey(cloneFloor))
            return _floors[cloneFloor];

        var newFloor = new Floor(cloneFloor, -1);
        _floors.Add(cloneFloor, newFloor);
        return newFloor;
    }

    public void SetGeneralProperties(int nbRounds, int nbTotalClones)
    {
    }
}

internal class Floors : Dictionary<int, Floor>
{
}

public class Floor
{

    public int Y { get; }

    public List<int> Elevators { get; set; } = new List<int>();

    public Floor(int y, int elevatorPos)
    {
        Y = y;

        Elevators.Add(elevatorPos);
    }

    public int ClosestElevatorTo(int clonePos, Direction direction)
    {
        if (!Elevators.Any())
            return -1;

        return Elevators.OrderBy(elevator => GetDistance(clonePos, elevator, direction))
            .First();
    }

    private static int GetDistance(int clonePos, int elevator, Direction direction)
    {
        var elevatorDirection = clonePos - elevator < 0 ? Direction.Right : Direction.Left;
        var isCloneGoingToRightDirection = elevatorDirection == direction;

        var distance = elevator - clonePos;
        var abs = Math.Abs(distance);
        return isCloneGoingToRightDirection ? abs : abs + 3;
    }
}