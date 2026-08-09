using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProcGen;

[Tool]
public partial class DungeonGen : Node3D
{
	[Export] private bool generate = false;
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

	private bool generated = false;

	private List<Room> roomLibrary;
	private List<Room> rooms;
	private Dictionary<RoomType, int> roomCount;
	private HashSet<Room> availableRooms;
	private int[,,] occupancy;
	private Node baseNode;
	private Random rng;
	private Node3D library;
	private Array roomTypes;

	public void clearRooms()
	{
		rooms.Clear();
		roomCount.Clear();
		foreach (Node room in baseNode.GetChildren())
		{
			room.QueueFree();
		}
		Array.Clear(occupancy);
	}

	public Vector3I getRandomSpace(Room room)
	{
		int x = rng.Next(room.Height/2,border_height - room.Height/2);
		int z = rng.Next(room.Width/2,border_width - room.Width/2);
		int y = rng.Next(room.Depth / 2,border_depth - room.Depth/2);
		return new Vector3I(z, y, x);
	}

	public bool canBePlaced(Vector3I roomOrigin, Room room)
	{
		int count = 0;
		for (int i = (int)Math.Ceiling(-room.Height/2f); i < room.Height/2f; i++)
		{
			for (int j = (int)Math.Ceiling(-room.Width/2f); j < room.Width/2f; j++)
			{
				for (int k = (int)Math.Ceiling(-room.Depth/2f); k < room.Depth/2f; k++)
				{
					count++;
					// GD.Print($"X: {roomOrigin.X + j} Y: {roomOrigin.Y + k} Z: {roomOrigin.Z + i}");
					if (occupancy[roomOrigin.Z + i, roomOrigin.X + j, roomOrigin.Y + k] != 0)
					{
						// GD.Print($"X: {roomOrigin.X + j} Y: {roomOrigin.Y + k} Z: {roomOrigin.Z + i}");
						GD.Print("Space occupied");
						return false;
					}
				}
			}
		}
		GD.Print($"Count: {count}");
		return true;
	}

	public void occupySpace(Vector3I roomOrigin, Room room)
	{
		for (int i = (int)Math.Ceiling(-room.Height/2f); i < room.Height/2f; i++)
		{
			for (int j = (int)Math.Ceiling(-room.Width/2f); j < room.Width/2f; j++)
			{
				for (int k = (int)Math.Ceiling(-room.Depth/2f); k < room.Depth/2f; k++)
				{
					occupancy[Math.Max(0, roomOrigin.X + i - 1), Math.Max(0, roomOrigin.Z + j - 1) , Math.Max(0, roomOrigin.Y + k - 1)] = 1;
					occupancy[roomOrigin.Z + i, roomOrigin.X + j, roomOrigin.Y + k] = 1;
					occupancy[Math.Min(roomOrigin.X + i + 1, border_height - 1), Math.Min(roomOrigin.Z + j + 1, border_width - 1), Math.Min(roomOrigin.Y + k + 1, border_depth - 1)] = 1;
				}
			}
		}
	}
	public bool placeRoom(Vector3I roomOrigin, Room room)
	{
		if (!canBePlaced(roomOrigin, room)) return false;
		
		room.RoomOrigin = roomOrigin;
		roomCount[room.getRoomType()] += 1;
		occupySpace(roomOrigin, room);
		iterateAvailableRooms();
		room.Name = roomOrigin.ToString();
		room.placeRoom(roomOrigin);
		// baseNode.AddChild(room);
		baseNode.CallDeferred("add_child", room);
		rooms.Add(room);
		return true;
	}

	public void iterateAvailableRooms()
	{
		HashSet<Room> roomsLeft = new HashSet<Room>();
		foreach (Room room in availableRooms)
		{
			if (roomCount[room.getRoomType()] < room.getMaxDuplicates())
			{
				roomsLeft.Add(room);
			}
		}

		availableRooms = roomsLeft;
	}
	
	public void placeRooms(int numRooms)
	{
		for (int i = 0; i < numRooms; i++)
		{
			Room roomTemplate = availableRooms.ElementAt(rng.Next(availableRooms.Count));
			Room room = roomTemplate.CopyRoom();
			for (int j = 0; j < 100; j++)
			{
				Vector3I roomOrigin = getRandomSpace(room);
				if (placeRoom(roomOrigin, room)) break;
			}
		}

	}
	public override void _Ready()
	{
		rng = new Random();
		baseNode = GetTree().GetCurrentScene();
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
		int numRooms = rng.Next(min_num_rooms - requiredRoomCount, max_num_rooms - requiredRoomCount);
		availableRooms = new HashSet<Room>(roomLibrary);
		iterateAvailableRooms();
		placeRooms(numRooms);
		generated = true;
	}

	public override void _Process(double delta)
	{
		return;
		if (!generate)
		{
			if (generated)
			{
				clearRooms();
			}
		
			return;
		}

		if (!generated)
		{
			if (generate)
			{
				
				foreach (MeshInstance3D mesh in library.GetChildren())
				{
					Room tempRoom = new Room(mesh, (RoomType)roomTypes.GetValue(rng.Next(roomTypes.Length)));
					roomLibrary.Add(tempRoom);
				}
				foreach (Room room in roomLibrary)
				{
					RoomType roomType = room.getRoomType();
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
				int numRooms = rng.Next(min_num_rooms - requiredRoomCount, max_num_rooms - requiredRoomCount);
				availableRooms = new HashSet<Room>(roomLibrary);
				iterateAvailableRooms();
				placeRooms(numRooms);
				generated = true;
			}
		}
		// if (!generated)
		// {
		// 	foreach (Room room in roomLibrary)
		// 	{
		// 		RoomType roomType = room.getRoomType();
		// 		roomCount.Add(roomType, 0);
		// 		if (room.isRequired())
		// 		{
		// 			// load of tries
		// 			for (int i = 0; i < 100; i++)
		// 			{
		// 				Vector3I roomPos = getRandomSpace(room);
		// 				if (placeRoom(roomPos, room)) break;
		// 			}
		// 		}
		// 	}
		//
		// 	int requiredRoomCount = rooms.Count;
		// 	int numRooms = rng.Next(min_num_rooms - requiredRoomCount, max_num_rooms - requiredRoomCount);
		// 	availableRooms = new HashSet<Room>(rooms);
		// 	placeRooms(numRooms);
		// 	generated = true;
		// }
	}
}
