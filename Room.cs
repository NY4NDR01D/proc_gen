using Godot;
using System;
using ProcGen;

public partial class Room : Node3D
{
	private int height;

	private int width;

	private int depth;
	[Export] private MeshInstance3D roomMesh;

	private Vector3I roomOrigin;

	public Vector3I RoomOrigin
	{
		get => roomOrigin;
		set => roomOrigin = value;
	}

	[Export]
	private bool requiredRoom = false;
	private int numDoors;
	[Export]
	private int maxDuplicates = 1;

	private RoomType roomType;

	public int Height => height;

	public int Width => width;

	public int Depth => depth;

	public RoomType getRoomType()
	{
		return roomType;
	}
	public bool isRequired()
	{
		return requiredRoom;
	}

	public int getMaxDuplicates()
	{
		return maxDuplicates;
	}

	public Room CopyRoom()
	{
		Room room = new Room();
		return room;
	}

	public Room()
	{
		Vector3 size = roomMesh.GetAabb().Size;
		width = (int)size.X;
		height = (int)size.Z;
		depth = (int)size.Y;
	}
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
