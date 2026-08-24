using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ProcGen;
	
[Tool]
public partial class DungeonGen : Node3D
{
	[Export] protected bool generate = false;
	[ExportGroup("BorderProperties")]
	[Export]
	protected int border_width; // x
	[Export]
	protected int border_height; // z
	[Export]
	protected int border_depth; // y
	[ExportGroup("RoomProperties")]
	[Export]
	protected int min_num_rooms;
	[Export]
	protected int max_num_rooms;

	protected bool generated = false;

	protected List<Room> roomLibrary;
	protected List<Room> rooms;
	protected List<Room> corridorLibrary;
	protected List<Room> corridors;
	protected Dictionary<RoomType, int> roomCount;
	protected HashSet<Room> availableRooms;
	protected int[,,] occupancy;
	protected Node baseNode;
	protected Random rng;
	protected Node3D library;
	protected Array roomTypes;

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
		int x = rng.Next(0,border_height - (room.Height + 1));
		int z = rng.Next(0,border_width - (room.Width + 1));
		int y = rng.Next(0,border_depth - (room.Depth + 1));
		return new Vector3I(z, y, x);
	}

	public bool canBePlaced(Vector3I roomOrigin, Room room)
	{
		int count = 0;
		for (int i = 0; i < room.Height; i++)
		{
			for (int j = 0; j < room.Width; j++)
			{
				for (int k = 0; k < room.Depth; k++)
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
		for (int i = 0; i < room.Height; i++)
		{
			for (int j = 0; j < room.Width; j++)
			{
				for (int k = 0; k < room.Depth; k++)
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

	public bool placeCorridor(Vector3I roomOrigin, Room room)
	{
		if (!canBePlaced(roomOrigin, room)) return false;
		
		room.RoomOrigin = roomOrigin;
		occupySpace(roomOrigin, room);
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
	public MeshInstance3D Line(Vector3 pos1, Vector3 pos2, Color? color = null)
	{
		var meshInstance = new MeshInstance3D();
		var immediateMesh = new ImmediateMesh();
		var material = new StandardMaterial3D();

		meshInstance.Mesh = immediateMesh;
		meshInstance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;

		immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
		immediateMesh.SurfaceAddVertex(pos1);
		immediateMesh.SurfaceAddVertex(pos2);
		immediateMesh.SurfaceEnd();

		material.ShadingMode = StandardMaterial3D.ShadingModeEnum.Unshaded;
		material.AlbedoColor = color ?? Colors.WhiteSmoke;

		baseNode.CallDeferred("add_child", meshInstance);

		return meshInstance;
	}
	public override void _Ready()
	{
		baseNode = GetTree().GetCurrentScene();
		ConnectionGen generator = new ConnectionGen(min_num_rooms, max_num_rooms, border_width, border_depth, border_height);
		baseNode.CallDeferred("add_child", generator);
	}

	public override void _Process(double delta)
	{
	}
}
