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
		int x = rng.Next(border_height - room.Height);
		int z = rng.Next(border_width - room.Width);
		int y = rng.Next(border_depth - room.Depth);
		GD.Print("vector:", x, y, z);
		return new Vector3I(x, y, z);
	}

	public bool canBePlaced(Vector3I roomOrigin, Room room)
	{
		for (int i = 0; i < room.Height; i++)
		{
			for (int j = 0; j < room.Width; j++)
			{
				for (int k = 0; k < room.Depth; k++)
				{
					if (occupancy[roomOrigin.X + i, roomOrigin.Z + j, roomOrigin.Y + k] != 0) return false;
				}
			}
		}
		return true;
	}

	public void occupySpace(Vector3I roomOrigin, Room room)
	{
		for (int i = 0; i < room.Height; i++)
		{
			for (int j = 0; j < room.Width; j++)
			{
				for (int k = 0; k < room.Depth; k++)
				{
					GD.Print("Occupying X: ", roomOrigin.X + i);
					GD.Print("Occupying Y: ", roomOrigin.Y + i);
					GD.Print("Occupying Z: ", roomOrigin.Z + i);
					occupancy[roomOrigin.X + i, roomOrigin.Z + j, roomOrigin.Y + k] = 1;
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
		GD.Print(room.RoomMesh);
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
			GD.Print("Available rooms count: ", availableRooms.Count);
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
		GD.Print(roomLibrary.Count);
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
				GD.Print("roomLibrary size: ", roomLibrary.Count);
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
				GD.Print(requiredRoomCount);
				GD.Print(min_num_rooms);
				int numRooms = rng.Next(min_num_rooms - requiredRoomCount, max_num_rooms - requiredRoomCount);
				GD.Print(numRooms);
				availableRooms = new HashSet<Room>(roomLibrary);
				iterateAvailableRooms();
				GD.Print("AvailableRooms: ", availableRooms.Count);
				placeRooms(numRooms);
				GD.Print(baseNode.GetChildCount());
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
