using System;
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

            var takeDecision = _game.TakeDecision(new Clone(cloneFloor, clonePos, direction));    
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
                floors.Add(elevatorFloor, new Floor(elevatorPos));
        }

        for (int i = 0; i <= nbFloors; i++)
        {
            if (!floors.ContainsKey(i))
                floors[i] = new Floor();
        }

        floors.InitializeScanResults(width);
        _game = new Game(floors, exitFloor, exitPos, numberElevatorsToBuild);
        _game.SetGeneralProperties(nbRounds, nbTotalClones, width);
    }
}

public class Game
{
    private readonly Floors _floors;
    private readonly int _exitFloor;
    private readonly int _exitPosition;
    private int _elevatorsToBuild;
    private int _nbTotalClones;
    public int _maxWidth;
    private readonly ElevatorStrategy _elevatorStrategy;

    public Game(Floors floors, int exitFloor, int exitPosition, int elevatorsToBuild)
    {
        _floors = floors;
        _exitFloor = exitFloor;
        _exitPosition = exitPosition;
        _elevatorsToBuild = elevatorsToBuild;
        _elevatorStrategy = new ElevatorStrategy(this);
    }

    public string TakeDecision(Clone clone)
    {
        if (clone.ClonePos == -1 || _floors[clone.CloneFloor].ContainsElevatorAt(clone.ClonePos))
        {
            return "WAIT";
        }
        
        var floor = _floors[clone.CloneFloor];
        var simulationParameters = new SimulationParameters(clone, _elevatorsToBuild, _nbTotalClones);
        var shortPath = ShortPath(simulationParameters, true);

        if (shortPath.ShouldBuild)
        {
            return BuildElevator(floor, clone.ClonePos);
        }

        var cloneGoingToRightDirection = clone.Direction == shortPath.Direction;
        return cloneGoingToRightDirection ? "WAIT" : "BLOCK";
    }

    public MapReturn ShortPath(SimulationParameters parameters, bool root = false)
    {
        if (parameters.NbTotalClones < 0 || parameters.CloneFloor > _exitFloor)
            return new MapReturn(int.MaxValue, parameters.Direction);

        var floor = _floors[parameters.CloneFloor];
        var scanResult = floor.ScanResults[parameters.ClonePos];
        if (scanResult.IsBetterThan(parameters))
            return new MapReturn(int.MaxValue, Direction.Right);

        scanResult.Update(parameters);

        if (floor.ContainsElevatorAt(parameters.ClonePos))
            return WaitForElevator(parameters);
        
        var elevators = floor.ElevatorsByDirection(parameters.ClonePos, parameters.Direction);
        if (parameters.CloneFloor == _exitFloor)
        {
            return HandleLastFloor(parameters, elevators);
        }

        var left = new MapReturn(CalculateFor(parameters, elevators.ClosestLeft, Direction.Left), Direction.Left);
        var right = new MapReturn(CalculateFor(parameters, elevators.ClosestRight, Direction.Right), Direction.Right);

        var elevatorReturns = _elevatorStrategy.GetMasterElevators(parameters, elevators);
        var mapReturn = MinOf(elevatorReturns, left, right);

        //if (root)
        //{
        //    Console.Error.WriteLine($"left = {left.Format()}");
        //    Console.Error.WriteLine($"right = {right.Format()}");
        //    foreach (var e in elevatorReturns)
        //    {
        //        Console.Error.WriteLine($"elevator = {e.Format()}");
        //    }
        //}
        return mapReturn;
        
    }

    private MapReturn WaitForElevator(SimulationParameters simulationParameters)
    {
        var cost = 1;
        var path = ShortPath(simulationParameters.AboveSimulation(cost));
        return new MapReturn(cost + path.TotalTravel, path.Direction);
    }

    private MapReturn MinOf(List<MapReturn> elevators, params MapReturn[] returns)
    {
        elevators.AddRange(returns);
        return elevators.OrderBy(p => p.TotalTravel).ThenBy(p => p.ShouldBuild).First();
    }

    private MapReturn HandleLastFloor(SimulationParameters simulationParameters, ElevatorReturn elevators)
    {
        if (simulationParameters.ClonePos == _exitPosition)
            return new MapReturn(1, simulationParameters.Direction);

        var returnDirection = _exitPosition > simulationParameters.ClonePos ? Direction.Right : Direction.Left;
        if (returnDirection == Direction.Right)
            return LastFloorReturnRight(simulationParameters, returnDirection, elevators.ClosestRight);

        return LastFloorReturnLeft(simulationParameters, returnDirection, elevators.ClosestLeft);
    }
    private MapReturn LastFloorReturnRight(SimulationParameters simulationParameters,
        Direction returnDirection, ClosestElevator elevator)
    {
        if (elevator != null && elevator.Position < _exitPosition)
            return new MapReturn(int.MaxValue, returnDirection);

        var distance = Floor.GetDistance(simulationParameters.ClonePos, _exitPosition, simulationParameters.Direction);
        return new MapReturn(distance, returnDirection);
    }

    private MapReturn LastFloorReturnLeft(SimulationParameters simulationParameters,
        Direction returnDirection, ClosestElevator elevator)
    {
        if (elevator != null && elevator.Position > _exitPosition)
            return new MapReturn(int.MaxValue, returnDirection);

        var distance = Floor.GetDistance(simulationParameters.ClonePos, _exitPosition, simulationParameters.Direction);
        return new MapReturn(distance, returnDirection);
    }

    private int CalculateFor(SimulationParameters simulationParameters, ClosestElevator elevator, Direction direction)
    {
        if (elevator == null)
            return int.MaxValue;

        var aboveWithWrongDirection = simulationParameters.AboveWithDirection(elevator, direction);
        var totalTravel = ShortPath(aboveWithWrongDirection).TotalTravel;
        if (totalTravel == int.MaxValue)
            return totalTravel;

        return elevator.Distance + totalTravel;
    }


    private string BuildElevator(Floor floor, int clonePos)
    {
        _elevatorsToBuild--;
        _nbTotalClones--;
        floor.Elevators.Add(clonePos);
        return "ELEVATOR";
    }

    public void SetGeneralProperties(int nbRounds, int nbTotalClones, int width = 10)
    {
        _maxWidth = width;
        _nbTotalClones = nbTotalClones;
    }
}

public class ElevatorStrategy
{
    private readonly Game _game;

    public ElevatorStrategy(Game game)
    {
        _game = game;
    }

    public List<MapReturn> GetMasterElevators(SimulationParameters simulationParameters, ElevatorReturn elevators)
    {
        var results = new List<MapReturn>();
        if (simulationParameters.ElevatorsToBuild > 0)
        {
            AddRight(simulationParameters, elevators, results);
            AddLeft(simulationParameters, elevators, results);

            var elevatorTotalTravel = ElevatorTotalTravel(simulationParameters.CloneAt(simulationParameters.ClonePos));
            var elevator = new MapReturn(elevatorTotalTravel, Direction.Right, true);
            results.Add(elevator);
        }

        return results;
    }

    private void AddLeft(SimulationParameters simulationParameters, ElevatorReturn elevators, List<MapReturn> results)
    {
        int maxRight = 0;
        if (elevators.ClosestLeft != null)
            maxRight = elevators.ClosestLeft.Position + 1;

        var newParams = simulationParameters.WrongDirectionHandle(simulationParameters.Direction == Direction.Left);
        for (int i = simulationParameters.ClonePos - 1; i >= maxRight; i--)
        {
            var distance = Floor.GetDistance(simulationParameters.ClonePos, i, simulationParameters.Direction);
            var cloneAt = newParams.CloneAt(i, distance);
            var elevatorTotalTravel = distance + ElevatorTotalTravel(cloneAt);

            var x = new MapReturn(elevatorTotalTravel, Direction.Left);
            results.Add(x);
        }
    }

    private void AddRight(SimulationParameters simulationParameters, ElevatorReturn elevators, List<MapReturn> results)
    {
        int maxRight = _game._maxWidth;
        if (elevators.ClosestRight != null)
            maxRight = elevators.ClosestRight.Position - 1;

        var newParams = simulationParameters.WrongDirectionHandle(simulationParameters.Direction == Direction.Right);
        for (int i = simulationParameters.ClonePos + 1; i <= maxRight; i++)
        {
            var distance = Floor.GetDistance(simulationParameters.ClonePos, i, simulationParameters.Direction);
            var cloneAt = newParams.CloneAt(i, distance);
            var pathDistance = i + ElevatorTotalTravel(cloneAt) + distance;
            var x = new MapReturn(pathDistance, simulationParameters.Direction);
            results.Add(x);
        }
    }

    private int ElevatorTotalTravel(SimulationParameters simulationParameters)
    {
        var cost = 3;
        var newParameters = simulationParameters.ElevatorBuilt(cost);
        var totalTravel = _game.ShortPath(newParameters).TotalTravel;
        if (totalTravel == int.MaxValue)
            return int.MaxValue;

        
        return cost + totalTravel;
    }
}
public class Floors : Dictionary<int, Floor>
{
    public void InitializeScanResults(int width)
    {
        foreach (var item in this)
        {
            for (int i = 0; i <= width; i++)
            {
                item.Value.ScanResults[i] = new ScanResult(new SimulationParameters(item.Key, i, Direction.Right, 0, 0, int.MaxValue));
            }
        }
    }
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
        IsCloneGoingToRightDirection = elevatorDirection == direction;

        return Floor.GetDistance(clonePos, elevator, direction);
    }

    public int Position { get; }

    public int Distance { get; }
    public bool IsCloneGoingToRightDirection { get; private set; }

}

public class Floor
{
    public HashSet<int> Elevators { get; set; } = new HashSet<int>();
    public Dictionary<int, ScanResult> ScanResults { get; set; } =new Dictionary<int, ScanResult>();
    public ElevatorReturn ElevatorsByDirection(int clonePos, Direction direction)
    {
        var elevatorsReturn = new ElevatorReturn();
        var lookup = Elevators.ToLookup(p => p > clonePos ? Direction.Right : Direction.Left, x => new ClosestElevator(x, clonePos, direction));
        if (lookup.Contains(Direction.Right))
            elevatorsReturn.ClosestRight = lookup[Direction.Right].Where(p => p.Position > clonePos).OrderBy(p => p.Distance).FirstOrDefault();
        if (lookup.Contains(Direction.Left))
            elevatorsReturn.ClosestLeft = lookup[Direction.Left].Where(p => p.Position < clonePos).OrderBy(p => p.Distance).FirstOrDefault();

        return elevatorsReturn;
    }

    public Floor(params int[] elevatorPos)
    {
        foreach (var elevator in elevatorPos)
        {
            Elevators.Add(elevator);
        }
    }

    public static int GetDistance(int clonePos, int position, Direction direction)
    {
        var elevatorDirection = clonePos - position < 0 ? Direction.Right : Direction.Left;
        var isCloneGoingToRightDirection = elevatorDirection == direction;
        var distance = position - clonePos;
        var abs = Math.Abs(distance);
        return isCloneGoingToRightDirection ? abs : abs + 3;
    }

    public bool ContainsElevatorAt(int index)
    {
        return Elevators.Contains(index);
    }
}


public class SimulationParameters
{
    public SimulationParameters(int cloneFloor, int clonePos, Direction direction, int elevatorsToBuild, int nbTotalClones, int cost)
    {
        CloneFloor = cloneFloor;
        ClonePos = clonePos;
        Direction = direction;
        ElevatorsToBuild = elevatorsToBuild;
        NbTotalClones = nbTotalClones;
        Cost = cost;
    }

    public SimulationParameters(Clone clone, int elevatorsToBuild, int nbTotalClones)
    {
        CloneFloor = clone.CloneFloor;
        ClonePos = clone.ClonePos;
        Direction = clone.Direction;
        ElevatorsToBuild = elevatorsToBuild;
        NbTotalClones = nbTotalClones;
        Cost = 0;
    }


    public int CloneFloor { get;}
    public int ClonePos { get; }
    public Direction Direction { get; }
    public int ElevatorsToBuild { get; }
    public int NbTotalClones { get; }
    public int Cost { get; }

    public SimulationParameters ElevatorBuilt(int cost)
    {
        return new SimulationParameters(CloneFloor + 1, ClonePos, Direction, ElevatorsToBuild - 1, NbTotalClones - 1, Cost + cost);
    }

    public SimulationParameters AboveSimulation(int cost)
    {
        return new SimulationParameters(CloneFloor + 1, ClonePos, Direction, ElevatorsToBuild, NbTotalClones, Cost + cost);
    }

    public SimulationParameters CloneAt(int newPosition, int distance = 0)
    {
        return new SimulationParameters(CloneFloor, newPosition, Direction, ElevatorsToBuild, NbTotalClones, Cost + distance);
    }

    public SimulationParameters WrongDirectionHandle(bool elevatorIsCloneGoingToRightDirection)
    {
        var totalClones = NbTotalClones;
        if (!elevatorIsCloneGoingToRightDirection)
            totalClones--;

        return new SimulationParameters(CloneFloor, ClonePos, Direction, ElevatorsToBuild, totalClones, Cost);
    }

    public SimulationParameters AboveWithDirection(ClosestElevator elevator, Direction direction)
    {
        var totalClones = NbTotalClones;
        if (!elevator.IsCloneGoingToRightDirection)
            totalClones--;

        return new SimulationParameters(CloneFloor + 1, elevator.Position, direction, ElevatorsToBuild, totalClones, elevator.Distance);
    }


    public string CacheKey()
    {
        return $"{CloneFloor}-{ClonePos}-{NbTotalClones}-{Direction}-{ElevatorsToBuild}";
    }
}

public enum Direction
{
    Left,
    Right
}

public class ScanResult
{
    public ScanResult(SimulationParameters previousParams)
    {
        Cost = previousParams.Cost;
        TotalClones = previousParams.NbTotalClones;
        ElevatorsToBuild = previousParams.ElevatorsToBuild;
    }

    public int Cost { get; set; }
    public int TotalClones { get; set; }
    public int ElevatorsToBuild { get; set; }

    public bool IsBetterThan(SimulationParameters parameters)
    {
        return Cost < parameters.Cost &&
               TotalClones >= parameters.NbTotalClones &&
               ElevatorsToBuild >= parameters.ElevatorsToBuild;
    }

    public void Update(SimulationParameters parameters)
    {
        Cost = parameters.Cost;
        TotalClones = parameters.NbTotalClones;
        ElevatorsToBuild = parameters.ElevatorsToBuild;
    }
}

public struct MapReturn
{
    public int TotalTravel { get; }
    public Direction Direction { get; }
    public bool ShouldBuild { get; }

    public MapReturn(int totalTravel, Direction direction, bool shouldBuild = false)
    {
        TotalTravel = totalTravel < 0 ? int.MaxValue : totalTravel;
        Direction = direction;
        ShouldBuild = shouldBuild;
    }

    public string Format()
    {
        return $"ShortPath {TotalTravel}, {Direction}, {ShouldBuild}";
    }
}

public class Clone
{
    public Clone(int cloneFloor, int clonePos, Direction direction)
    {
        CloneFloor = cloneFloor;
        ClonePos = clonePos;
        Direction = direction;
    }

    public int CloneFloor { get; private set; }
    public int ClonePos { get; private set; }
    public Direction Direction { get; private set; }
}