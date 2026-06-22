using Godot;
using System;

public partial class CharacterBody3d : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	private Node3D _head;
	private Camera3D camera;
	private float _cameraAngle = 0F;
	private float _mouseSensitivity = 0.1F;
	private float _moveSpeed = 20F;
	

	public override void _Ready()
	{
		camera = this.GetNode<Camera3D>("%Camera3D");
		camera.Current = true;
		Input.MouseMode = Input.MouseModeEnum.Captured;
		_head = GetNode<Node3D>("Head");

	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		// Handle Jump.
		if (Input.IsActionJustPressed("go_up") )
		{
			velocity.Y += JumpVelocity;
		}
		if (Input.IsActionJustPressed("go_down") )
		{
			velocity.Y -= JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		Basis cameraDir = camera.GlobalTransform.Basis;
		Godot.Vector3 direction = (cameraDir * new Godot.Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _Input(InputEvent @event)
	{
		if (InputMap.EventIsAction(@event, "escape") && @event.IsPressed())
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		if (@event is not InputEventMouseMotion motion) return;

		_head.RotateY(Mathf.DegToRad(-motion.Relative.X * _mouseSensitivity));
		float change = -motion.Relative.Y * _mouseSensitivity;

		if (!((change + _cameraAngle) < 90F) || !((change + _cameraAngle) > -90F)) return;
		
		camera.RotateX(Mathf.DegToRad(change));
		_cameraAngle += change;	
	}
}
