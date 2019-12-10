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
    private static int _exitFloor;
    private static int _exitPos;
    private static int _numberElevatorsToBuild;
    private static Floors _floors;

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
            string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

            var floor = GetFloor(cloneFloor);
            if (clonePos == -1)
            {
                Console.WriteLine("WAIT");
                continue;
            }
            var closestElevatorTo = floor.ClosestElevatorTo(clonePos);

            if (closestElevatorTo < 0 && _numberElevatorsToBuild > 0)
            {
                BuildElevator(floor, clonePos);
                continue;
            }

            var correctDirection = clonePos - closestElevatorTo < 0 ? "RIGHT" : "LEFT";
            Console.Error.WriteLine($"Clone X: {clonePos} at floor: {cloneFloor}, {closestElevatorTo}, {correctDirection}");
            var cloneGoingToRightDirection = direction == correctDirection || clonePos == closestElevatorTo;
            var format = cloneGoingToRightDirection ? "WAIT" : "BLOCK";
            Console.Error.WriteLine(format);
            Console.WriteLine(format);
        }
    }

    private static void Setup(string[] inputs)
    {
        int nbFloors = int.Parse(inputs[0]); // number of floors
        int width = int.Parse(inputs[1]); // width of the area

        _floors = new Floors();
        for (int i = 0; i < nbFloors; i++)
        {
            _floors.Add(i, new Floor(i, width));
        }

        int nbRounds = int.Parse(inputs[2]); // maximum number of rounds
        _exitFloor = int.Parse(inputs[3]);
        _exitPos = int.Parse(inputs[4]);
        int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
        _numberElevatorsToBuild = int.Parse(inputs[6]);
        int nbElevators = int.Parse(inputs[7]); // number of elevators

        _floors = new Floors();
        for (int i = 0; i < nbElevators; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
            int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor

            _floors[elevatorFloor].Elevators.Add(elevatorPos);
        }
    }

    private static void BuildElevator(Floor floor, int clonePos)
    {
        Console.WriteLine("ELEVATOR");
        _numberElevatorsToBuild--;

        floor.Elevators.Add(clonePos);
        Console.Error.WriteLine($"Floor: {floor.Y}");
    }

    private static Floor GetFloor(int cloneFloor)
    {
        if (cloneFloor == _exitFloor)
            return new Floor(_exitPos, _exitPos);

        if (_floors.ContainsKey(cloneFloor))
            return _floors[cloneFloor];

        var newFloor = new Floor(cloneFloor, -1);
        _floors.Add(cloneFloor, newFloor);
        return newFloor;
    }
}

internal class Floors : Dictionary<int, Floor>
{
}

public class Floor
{

    public int Y { get; }
    public int MaxX { get; set; }
    public List<int> Elevators { get; set; } = new List<int>();

    public Floor(int y, int maxX)
    {
        Y = y;
        MaxX = maxX;
    }

    public Floor(int y)
    {
        Y = y;
    }

    public int ClosestElevatorTo(int clonePos)
    {
        if (!Elevators.Any())
            return -1;

        return Elevators.OrderBy(elevator => GetDistance(clonePos, elevator))
            .First();
    }

    private static int GetDistance(int clonePos, int elevator)
    {
        var distance = elevator - clonePos;
        if (distance < 0)
            return -distance;

        return distance;
    }
}