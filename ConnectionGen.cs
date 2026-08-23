using System;

namespace ProcGen;
using Godot;
using System;
using System.Collections.Generic;

public partial class ConnectionGen : DungeonGen
{
    public ConnectionGen(int minNumRooms, int maxNumRooms, int bX, int bY, int bZ)
    {
        border_width = bX;
        border_depth = bY;
        border_height = bZ;
        min_num_rooms = minNumRooms;
        max_num_rooms = maxNumRooms;
        rng = new Random();
        library = GD.Load<PackedScene>("res://Library.tscn").Instantiate<Node3D>();
        roomLibrary = new List<Room>();
        roomTypes = Enum.GetValues(typeof(RoomType));
        foreach (MeshInstance3D mesh in library.GetChildren())
        {
            Room tempRoom = new Room(mesh, (RoomType)roomTypes.GetValue(rng.Next(roomTypes.Length)));
            roomLibrary.Add(tempRoom);
        }
        occupancy = new int[border_height, border_width, border_depth];
        roomCount = new Dictionary<RoomType, int>();
        foreach (RoomType roomType in roomTypes)
        {
            roomCount[roomType] = 0;
        }
        rooms = new List<Room>();
    }
    public override void _Ready()
    {
        baseNode = GetTree().GetCurrentScene();
        // if(!generate) return;
        foreach (Room room in roomLibrary)
        {
            RoomType roomType = room.getRoomType();
            // roomCount.Add(roomType, 0);
            if (room.isRequired())
            {
                // load of tries
                for (int i = 0; i < 100; i++)
                {
                    Vector3I roomPos = getRandomSpace(room);
                    if (placeRoom(roomPos, room)) break;
                }
            }
        }
        int requiredRoomCount = rooms.Count;
        GD.Print(requiredRoomCount);
        int numRooms = rng.Next(min_num_rooms - requiredRoomCount, max_num_rooms - requiredRoomCount);
        availableRooms = new HashSet<Room>(roomLibrary);
        iterateAvailableRooms();
        placeRooms(numRooms);
        List<Kruskal.Edge> edges = Kruskal.MST(rooms, rooms[0]);
        List<MeshInstance3D> lines = new List<MeshInstance3D>();
        foreach (Kruskal.Edge edge in edges)
        {
            MeshInstance3D line = Line(edge.startRoom.RoomOrigin, edge.endRoom.RoomOrigin);
            lines.Add(line);
        }
        generated = true;
    }
}