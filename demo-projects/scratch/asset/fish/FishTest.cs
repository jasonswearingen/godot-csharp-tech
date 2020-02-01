using Godot;
using System;
using godot_csharp_tech.addons;

public class FishTest : Spatial
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


[Tool]
public class FishShaderControl : Node
{
	[Export]
	public float frequency = 5;

	[Export]
	public float fish_twist_frequency = -5;
	[Export]
	public float fish_s_frequency = 5;
	[Export]
	public float fish_yaw = 0.25f;
	[Export]
	public float fish_twist = 0.8f;
	[Export]
	public float fish_s = 0.38f;
	[Export]
	public float fish_head_z = -1.16f;
	[Export]
	public float fish_tail_z = 1;
	[Export]
	public Color fish_head_color = new Color(0, 0, 0);
	[Export]
	public Color fish_tail_color = new Color(1, 1, 1);

	private float timer = 0;
	[Export]
	private Vector3 fish_translate = new Vector3(0.1f, 0, 0);

	[Export]
	public float fish_speed = 1f;

	public ShaderMaterial fish_shaderMaterial;


	//public FishShaderControl(ShaderMaterial fish_shaderMaterial)
	//{
	//	this.fish_shaderMaterial = fish_shaderMaterial;
	//}


	public override void _Ready()
	{
		base._Ready();
		//try
		//{
		//	var parent = GetParent<MeshInstance>();
		//	this.fish_shaderMaterial = parent.MaterialOverride as ShaderMaterial;
		//}
		//catch(Exception ex)
		//{
		//	throw new Exception("must be attached as a child of MeshInstance");
		//}
		this._updateShader();
	}

	public void _updateShader()
	{
		if (fish_shaderMaterial == null)
		{
			return;
		}
		fish_shaderMaterial.SetShaderParam("frequency", frequency);
		fish_shaderMaterial.SetShaderParam("translate", fish_translate);
		fish_shaderMaterial.SetShaderParam("yaw_factor", fish_yaw);
		fish_shaderMaterial.SetShaderParam("twist_factor", fish_twist);
		fish_shaderMaterial.SetShaderParam("twist_factor_frequency", fish_twist_frequency);
		fish_shaderMaterial.SetShaderParam("s_factor", fish_s);
		fish_shaderMaterial.SetShaderParam("s_factor_frequency", fish_s_frequency);
		fish_shaderMaterial.SetShaderParam("head_z", fish_head_z);
		fish_shaderMaterial.SetShaderParam("head_color", fish_head_color);
		fish_shaderMaterial.SetShaderParam("tail_z", fish_tail_z);
		fish_shaderMaterial.SetShaderParam("tail_color", fish_tail_color);
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		timer += delta * fish_speed;
		if (fish_shaderMaterial != null)
		{
			fish_shaderMaterial.SetShaderParam("timer", timer);
		}


		if (Engine.EditorHint)
		{
			_updateShader();
		}

	}
}
