//using Godot;
//using System;

//[Tool]
//public class FishControlFacade : FishShaderControl
//{
//	// Declare member variables here. Examples:
//	// private int a = 2;
//	// private string b = "text";

//	// Called when the node enters the scene tree for the first time.
//	public override void _Ready()
//	{
//		var parent = GetParent<MeshInstance>();
//		this.fish_shaderMaterial = parent.MaterialOverride as ShaderMaterial;		
//		base._Ready();



//	}

//	//public FishControlFacade():base()
//	//{
//	//	var parent = GetParent<MeshInstance>();
//	//	var fish_shaderMaterial = parent.MaterialOverride as ShaderMaterial;

//	//	base(fish_shaderMaterial);
//	//}




//	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//	public override void _Process(float delta)
//	{
//		base._Process(delta);

//		if (Engine.EditorHint && fish_shaderMaterial == null)
//		{

//			var parent = GetParent<MeshInstance>();
//			this.fish_shaderMaterial = parent.MaterialOverride as ShaderMaterial;
//			this._updateShader();

//		}

//	}
//}
