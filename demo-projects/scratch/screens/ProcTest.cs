using Godot;
using System;
using godot_csharp_tech.addons;
using System.Runtime.InteropServices;

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


		var procMM = new ProcMultimesh();
		AddChild(procMM);




	}

	
}



public class ProcMultimesh : MultiMeshInstance
{

	private Vector3[] xforms;
	
	
	public override void _Ready()
	{
		base._Ready();


		var mesh = GD.Load<Mesh>("res://asset/fish/Fish1.obj");
		var shader = GD.Load<Shader>("res://asset/fish/fish1.shader");
		var diffuse = GD.Load<Texture>("res://asset/fish/Fish1-diffuse_base.png");
		shader.SetDefaultTextureParam("texture_albedo", diffuse);


		var mm = new MultiMesh();
		mm.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		mm.ColorFormat = MultiMesh.ColorFormatEnum.None;
		mm.CustomDataFormat = MultiMesh.CustomDataFormatEnum.None;
		

		this.Multimesh = new MultiMesh();
		this.Multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3d;
		this.Multimesh.CustomDataFormat = MultiMesh.CustomDataFormatEnum.Float;
		this.Multimesh.Mesh = mesh;
		//set shader
		{
			var shadMat = new ShaderMaterial();
			shadMat.Shader = shader;
			shadMat.SetShaderParam("texture_albedo", diffuse);
			this.MaterialOverride = shadMat;
		}
		this.Multimesh.InstanceCount =  10000;  //need to do this after setting the TransformFormat
										   //mm.InstanceCount = 1000;
										   //mm.VisibleInstanceCount = -1;


		//set initial placement
		var visibleCount = this.Multimesh.VisibleInstanceCount;
		if (visibleCount == -1)
		{
			visibleCount = this.Multimesh.InstanceCount;
		}

		//this vector3 array is actually a transform array (4 vector 3's)
		xforms = this.Multimesh.TransformArray;
		//so lets cast it into that!
		var spanV3 = new Span<Vector3>(xforms);
		var spanXf = MemoryMarshal.Cast<Vector3, Transform>(spanV3);


		//put all the fish into a grid
		{
			var seperationDistance = 5;
			var dim = Math.Pow(visibleCount, 1.0 / 3);
			GD.Print($"visibleCount={visibleCount}, xformsLen={xforms.Length}, dim={dim}");
			var i = 0;
			for (var x = 0; x < dim; x++)
			{
				for (var z = 0; z < dim; z++)
				{
					for (var y = 0; y < dim; y++)
					{
						if (i >= visibleCount)
						{
							break; //need this because of rounding with our dim variable.
						}
						var loc = new Vector3((x * seperationDistance), y * seperationDistance, z * seperationDistance);
						spanXf[i].origin = loc; //loc offset 3 is the position in a transform.  we could assign directly to array like this: //xforms[(i * 4) + 3] = loc;
						//this.Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, loc)); //instead of updating 1 at a time, we update all after the loop is done.
						i++;
						
					}
				}
			}
		}
		this.Multimesh.TransformArray = xforms;

	}

	private Random rand = new Random(0);
	public override void _Process(float delta)
	{
		base._Process(delta);

		var visibleCount = this.Multimesh.VisibleInstanceCount;
		if (visibleCount == -1)
		{
			visibleCount = this.Multimesh.InstanceCount;
		}

		//var xforms = this.Multimesh.TransformArray;  //we don't need to get this again, since our code is the only thing that changes it. 
		var spanV3 = new Span<Vector3>(xforms);
		var spanXf = MemoryMarshal.Cast<Vector3, Transform>(spanV3);

		for (var i = 0; i < visibleCount; i++)
		{
			//var relXf = new Transform()


			var deltaLoc = new Vector3((float)(rand.NextDouble() - 0.5), (float)(rand.NextDouble() - 0.5), (float)(rand.NextDouble() - 0.5)) / 10;
			var curLoc = xforms[(i * 4) + 3];
			var newLoc = curLoc + deltaLoc;
			//xforms[(i * 4) + 3] = newLoc;
			spanXf[i].origin = newLoc;
			//this.Multimesh.SetInstanceTransform(i, new Transform(Basis.Identity, newLoc));  //instead of updating 1 at a time, we update all after the loop is done.
		}
		this.Multimesh.TransformArray = xforms;
	}

}
