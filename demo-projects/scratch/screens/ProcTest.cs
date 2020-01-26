using Godot;
using System;
using godot_csharp_tech.addons;

public class ProcTest : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		//add perf info label
		AddChild(new MonoDiagLabel()
		{
			position = MonoDiagLabel.POSITION.TOP_CENTER,
		});

		//setup a moveable camera
		var inputControl = new InputController();
		inputControl.Translate(new Vector3(0, 0, 3));
		inputControl.AddChild(new Camera());
		AddChild(inputControl);

		var dirLight = new DirectionalLight();
		dirLight.Rotation = Vector3.One;//rotate aprox 57' in xyz
		AddChild(dirLight);


        
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}
