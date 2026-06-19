using Godot;
using System;
using System.Collections.Generic;
using ProcGen;

[Tool]
public partial class DungeonGen : Node3D
{
	[ExportGroup("BorderProperties")]
	[Export]
	private int border_width; // x
	[Export]
	private int border_height; // z
	[Export]
	private int border_depth; // y
	[ExportGroup("RoomProperties")]
	[Export]
	private int min_num_rooms;
	[Export]
	private int max_num_rooms;

	private List<Room> roomLibrary;
	private List<Room> rooms;
	private Dictionary<RoomType, int> roomCount;

	public bool canBePlaces(Vector3I roomOrigin, Room room)
	{
		
	}
	public bool placeRoom(Vector3I roomOrigin, Room room)
	{
		return false;
	}
	public void placeRooms(int numRooms)
	{
		
	}
	public override void _Ready()
	{
		roomCount = new Dictionary<RoomType, int>();
		rooms = new List<Room>();
		foreach (Room room in roomLibrary)
		{
			RoomType roomType = room.getRoomType();
			roomCount.Add(roomType, 0);
			if (room.isRequired())
			{
				rooms.Add(room);
				roomCount[roomType] += 1;
			}
		}

		int requiredRoomCount = rooms.Count;
		int numRooms = new Random().Next(min_num_rooms - requiredRoomCount, max_num_rooms - requiredRoomCount);
	}

	public override void _Process(double delta)
	{
	}

	public DungeonGen()
	{
		GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe; 
	}
}
