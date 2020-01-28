using Godot;
using System;
using godot_csharp_tech.addons;

[Tool]
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


		var mesh = GD.Load<Mesh>("res://asset/fish/Fish Perch.obj");
		var sm = GD.Load<ShaderMaterial>("res://asset/fish/fish.shadermaterial.tres");
		//mesh.SurfaceSetMaterial(0, sm);
		//mesh.SurfaceSetMaterial(1, sm);

		var meshInstance = new MeshInstance();
		meshInstance.Mesh = mesh;
		meshInstance.MaterialOverride = sm;
		AddChild(meshInstance);
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
}


//public class FishShaderControl : Node
//{
//	[Export]
//	//public float frequency
//}


public class ProcMultimesh : MultiMeshInstance
{
	public override void _Ready()
	{
		base._Ready();

		var mm = new MultiMesh();
		mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		mm.ColorFormat = MultiMesh.ColorFormatEnum.None;
		mm.CustomDataFormat = MultiMesh.CustomDataFormatEnum.None;
		mm.InstanceCount = 1000;
		mm.VisibleInstanceCount = -1;

		//mm.Mesh = = GD.Load<Mesh>("res://asset/model/export/Fish Perch.obj");
		//mm.Mesh.PAT
	}
}