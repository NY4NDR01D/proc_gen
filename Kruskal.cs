using System;
using System.Collections.Generic;
using Godot;

namespace ProcGen;

public static class Kruskal
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
}