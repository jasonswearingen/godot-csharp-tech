using Godot;
using System;

public class ProcCube : Spatial
{
	public override void _Ready()
	{
		var mesh = GD.Load<Mesh>("res://asset/cube/cube.obj");
		//var diffuse = ResourceLoader.Load<Texture>("res://asset/cube/cube-diffuse.png");


		//put together a meshInstance from code
		var meshInst = new MeshInstance();
		meshInst.Mesh = mesh;
		meshInst.MaterialOverride = GD.Load<ShaderMaterial>("res://asset/cube/cube.shaderMaterial.tres");
		meshInst.Translate(Vector3.Up * 3);
		AddChild(meshInst);


		//put together a ShaderMaterial via code		
		var shader = GD.Load<Shader>("res://asset/cube/cube.shader");
		var diffuse = GD.Load<Texture>("res://asset/cube/cube-diffuse.png");
		var meshInstWithShadMat = new MeshInstance();
		meshInstWithShadMat.Mesh = mesh;
		var shadMat = new ShaderMaterial();
		shadMat.Shader = shader;
		//need to explicitly set default shader params yourself when setting up a ShaderMaterial through code
		shadMat.SetShaderParam("albedo", Colors.White);
		shadMat.SetShaderParam("uv1_scale", Vector3.One);
		shadMat.SetShaderParam("uv1_offset", Vector3.Zero);
		shadMat.SetShaderParam("uv2_scale", Vector3.One);
		shadMat.SetShaderParam("uv2_offset", Vector3.Zero);
		shadMat.SetShaderParam("texture_albedo", diffuse);
		meshInstWithShadMat.MaterialOverride = shadMat;
		AddChild(meshInstWithShadMat);
	}
}
