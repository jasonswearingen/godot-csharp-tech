using Godot;
using System;

[Tool]
public class MoveTestNode : MeshInstance
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	private Random rand = new Random(0);

	private float minSpeed = 0.25f;
	private float maxSpeed = 2f;
	private float velocity;
	private Vector3 steerTarget;
	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		steerTarget = new Vector3(1, 0, 0);

		// accel += new Vector3{

		//     }

		// velocity = Vector3.Back;



		// Translate(velocity * delta);




		if (Translation.Length() > 10)
		{
			Translation = Vector3.Zero;
		}
	}
}
