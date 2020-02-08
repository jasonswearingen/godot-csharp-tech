using Godot;
using System;

[Tool]
public class MoveTestScene : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		totalTime = TimeSpan.Zero;

		//add a fish via code, put it at (0,3,0)
		{
			if (fish != null)
			{
				RemoveChild(fish);
				fish.Dispose();
				fish = null;
			}
			//if (fish == null)
			{
				fish = new FishMoveTest();
				AddChild(fish);
				fish.Translate(new Vector3(-10, 0, -5));
				fish.Rotate(Vector3.Up, Mathf.Deg2Rad(45));
			}
		}
	}

	FishMoveTest fish;

	TimeSpan totalTime;

	public override void _Process(float delta)
	{
		totalTime += TimeSpan.FromSeconds(delta);
		if (totalTime >= TimeSpan.FromSeconds(15))
		{
			_Ready();
		}
		var steerTarget = FindNode("FishTarget") as Spatial;
		fish.steerTarget = steerTarget.Translation;


	}
}

public class FishMoveTest : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		//add a fish via code, put it at (0,3,0)
		{
			var mesh = GD.Load<Mesh>("res://asset/fish/Fish1.obj");

			GD.Print($"mesh aabb = {mesh.GetAabb().ToString("F2")}");

			var shader = GD.Load<Shader>("res://asset/fish/fish1.shader");
			var diffuse = GD.Load<Texture>("res://asset/fish/Fish1-diffuse_base.png");
			shader.SetDefaultTextureParam("texture_albedo", diffuse);
			fishMesh = new MeshInstance();
			fishMesh.Mesh = mesh;
			var shadMat = new ShaderMaterial();
			shadMat.Shader = shader;
			shadMat.SetShaderParam("texture_albedo", diffuse);
			fishMesh.MaterialOverride = shadMat;
			AddChild(fishMesh);
		}
		//reset location
		Translation = Vector3.Zero;
	}

	public Vector3 steerTarget;

	MeshInstance fishMesh;

	TimeSpan totalTime;

	int loopCount = 0;
	public override void _Process(float delta)
	{
		loopCount++;

		var localRight = Transform.basis.Column0;
		var localUp = Transform.basis.Column1;
		var localFwd = Transform.basis.Column2;


		Translate(Vector3.Forward * delta * 10);

		//var targetDiff = GlobalTransform.origin - steerTarget;

		var desiredXform = GlobalTransform.LookingAt(steerTarget, Vector3.Up);
		GlobalTransform = GlobalTransform.InterpolateWith(desiredXform, 1f * delta);

		//        Transform.Rotated(Vector3.Up)

		//Transform.
		//Transform.InterpolateWith

		if (loopCount % 100 == 0)
		{
			GD.Print($"{loopCount / 100}: localFwd={localFwd.ToString("F2")}");
		}

		//Transform.InterpolateWith

		//GD.Print("steerTarget", steerTarget);

	}
}






// private Random rand = new Random(0);

// 	private float minSpeed = 0.25f;
// 	private float maxSpeed = 2f;
// 	private float velocity;
// 	private Vector3 steerTarget;
// 	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
// 	public override void _Process(float delta)
// 	{
// 		steerTarget = new Vector3(1, 0, 0);

// 		accel += new Vector3{

//         }

// 		velocity = Vector3.Back;



// 		Translate(velocity * delta);




// 		if (Translation.Length() > 10)
// 		{
// 			Translation = Vector3.Zero;
// 		}
// 	}






