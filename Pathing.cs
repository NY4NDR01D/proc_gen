using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProcGen;

public static class Pathing
{
    public class Edge
    {
        public Room startRoom;
        public Room endRoom;

        public Edge(Room startRoom, Room endRoom )
        {
            this.startRoom = startRoom;
            this.endRoom = endRoom;
        }
    }
    
    private const int VERTICAL_COST = 5;

    public static int Manhattan(Vector3I originVec, Vector3I destVec, int verticalCost=VERTICAL_COST)
    {
        return Math.Abs(originVec.X - destVec.X) + Math.Abs(originVec.Y - destVec.Y) +
               verticalCost * Math.Abs(originVec.Z - destVec.Z);
    }
    public static int Manhattan(Room originRoom, Room destRoom, int verticalCost=VERTICAL_COST)
    {
        Vector3I originVec = originRoom.RoomOrigin;
        Vector3I destVec = destRoom.RoomOrigin;
        return Math.Abs(originVec.X - destVec.X) + Math.Abs(originVec.Y - destVec.Y) +
                     verticalCost * Math.Abs(originVec.Z - destVec.Z);
    }
    public static Edge GetNextEdge(HashSet<Room> visitedRooms, HashSet<Room> unvisitedRooms)
    {
        int lowestWeight = Int32.MaxValue;
        Room originRoom = new Room();
        Room nextRoom = new Room();
        foreach (Room room in visitedRooms)
        {
            foreach (Room unseenRoom in unvisitedRooms)
            {
                int dist = Manhattan(room, unseenRoom);
                if (dist < lowestWeight)
                {
                    originRoom = room;
                    nextRoom = unseenRoom;
                    lowestWeight = dist;
                }
            }
        }
        return new Edge(originRoom, nextRoom);
    }
    
    public static List<Edge> MST(List<Room> rooms, Room startRoom)
    {
        HashSet<Room> visitedRooms = new HashSet<Room>();
        HashSet<Room> unvisitedRooms = new HashSet<Room>(rooms);
        List<Edge> mstEdges = new List<Edge>();
        visitedRooms.Add(startRoom);
        unvisitedRooms.Remove(startRoom);
        while (unvisitedRooms.Count > 0)
        {
            Edge nextEdge = GetNextEdge(visitedRooms, unvisitedRooms);
            visitedRooms.Add(nextEdge.endRoom);
            unvisitedRooms.Remove(nextEdge.endRoom);
            mstEdges.Add(nextEdge);
        }
        return mstEdges;
    }

    public static List<Vector3I> reconstructPath(Dictionary<Vector3I, Vector3I> cameFrom, Vector3I current)
    {
        List<Vector3I> path = new List<Vector3I>();
        path.Add(current);
        while(cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }

    public static List<Vector3I> getNeighbours(Vector3I pos)
    {
        List<Vector3I> neighbours = new List<Vector3I>
        {
            pos + new Vector3I(1, 0, 0),
            pos + new Vector3I(-1, 0, 0),
            pos + new Vector3I(0, 1, 0),
            pos + new Vector3I(0, -1, 0),
            pos + new Vector3I(0, 0, 1),
            pos + new Vector3I(0, 0, -1)
        };
        return neighbours;
    }

    public static Vector3I getMinFScore(HashSet<Vector3I> openSet, Dictionary<Vector3I, int> fScores)
    {
        int minScore = Int32.MaxValue;
        Vector3I minVec = Vector3I.Zero;
        foreach (Vector3I vec in openSet)
        {
            int fScore = fScores.GetValueOrDefault(vec, Int32.MaxValue);
            if (fScore < minScore)
            {
                minScore = fScore;
                minVec = vec;
            }
        }

        return minVec;
    }
    
    public static List<Vector3I> AStar(Room startRoom, Room endRoom)
    {
        GD.Print("Starting AStar");
        HashSet<Vector3I> openSet = new HashSet<Vector3I>();
        Dictionary<Vector3I, Vector3I> cameFrom = new Dictionary<Vector3I, Vector3I>();
        openSet.Add(startRoom.RoomOrigin);

        Dictionary<Vector3I, int> gScore = new Dictionary<Vector3I, int>
        {
            {startRoom.RoomOrigin, 0}
        };
        Dictionary<Vector3I, int> fScore = new Dictionary<Vector3I, int>
        {
            {startRoom.RoomOrigin, Manhattan(startRoom, endRoom)}
        };

        while (openSet.Count > 0)
        {
            Vector3I current = getMinFScore(openSet, fScore);
            if (current.Equals(endRoom.RoomOrigin))
            {
                return reconstructPath(cameFrom, current);
            }

            openSet.Remove(current);

            foreach (Vector3I neighbour in getNeighbours(current))
            {
                int tentativeGScore = gScore.GetValueOrDefault(current, Int32.MaxValue) + 1;
                GD.Print(tentativeGScore);
                if (tentativeGScore < gScore.GetValueOrDefault(neighbour, Int32.MaxValue))
                {
                    cameFrom[neighbour] = current;
                    gScore[neighbour] = tentativeGScore;
                    fScore[neighbour] = tentativeGScore + Manhattan(neighbour, endRoom.RoomOrigin);
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                }
                
            }
        }
        return new List<Vector3I>();
    }
}