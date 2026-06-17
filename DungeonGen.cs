using Godot;
using System;
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

	
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public DungeonGen()
	{
		GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe; 
	}
}
