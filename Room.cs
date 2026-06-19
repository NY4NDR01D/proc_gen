using Godot;
using System;
using ProcGen;

public partial class Room : Node3D
{
	private int height;

	private int width;

	private int depth;

	private Vector3I roomOrigin;
	[Export]
	private bool requiredRoom = false;
	private int numDoors;
	[Export]
	private int maxDuplicates = 1;

	private RoomType roomType;

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
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}
}
