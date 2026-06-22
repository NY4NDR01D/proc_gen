using Godot;
using System;
using ProcGen;

public partial class Room : Node3D
{
	private int height;

	private int width;

	private int depth;
	private MeshInstance3D roomMesh;

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
	private int maxDuplicates = 100;

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
		Room room = new Room(this.roomMesh, this.roomType);
		return room;
	}

	public void placeRoom(Vector3 pos)
	{
		this.SetPosition(pos);	
	}

	public MeshInstance3D RoomMesh => roomMesh;

	public Room()
	{
		roomMesh = new MeshInstance3D();
		roomType = RoomType.BasicRoom;
		Vector3 size = roomMesh.GetAabb().Size;
		width = (int)size.X;
		height = (int)size.Z;
		depth = (int)size.Y;
	}
	public Room(MeshInstance3D mesh, RoomType roomStyle)
	{
		roomMesh = (MeshInstance3D)mesh.Duplicate();
		roomType = roomStyle;
		Vector3 size = roomMesh.GetAabb().Size;
		width = (int)size.X;
		height = (int)size.Z;
		depth = (int)size.Y;
	}
	public override void _Ready()
	{
		CallDeferred("add_child", roomMesh);
	}

	public override void _Process(double delta)
	{
	}
}
