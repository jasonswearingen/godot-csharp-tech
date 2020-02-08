using Godot;
using System;
using System.Collections.Generic;

[Tool]
public class MeshDataToolTestScene : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.


	TimeSpan timer;

	ArrayMesh mesh;
	public override void _Ready()
	{
		GD.Print("REBUILDING COLOR MESH");

		var testNode = FindNode("Test") as MeshInstance;
		//var mesh = GD.Load<ArrayMesh>("res://asset/fish/Fish1.obj");
		mesh = testNode.Mesh as ArrayMesh;


		//var mdt = new MeshDataTool();
		//mdt.CreateFromSurface(mesh, 0);
		//_SetVertexColorToBarycentric(mdt);
		//mesh.SurfaceRemove(0);
		//mdt.CommitToSurface(mesh);
		//mdt.Dispose();


		var surfaceCount = mesh.GetSurfaceCount();
		var surfaceQueue = new Queue<MeshDataTool>();
		for (var i = 0; i < surfaceCount; i++)
		{
			var mdt = new MeshDataTool();
			mdt.CreateFromSurface(mesh, 0);
			_SetVertexColorToBarycentric(mdt);
			mesh.SurfaceRemove(0);
			surfaceQueue.Enqueue(mdt);
		}

		//replace our mesh with modified version
		while (surfaceQueue.Count > 0)
		{
			var mdt = surfaceQueue.Dequeue();
			mdt.CommitToSurface(mesh);
			mdt.Dispose();
		}

		testNode.Mesh = mesh;

		//GD.Print($"mark fails= {markFailCount}");
	}

	private static void _SetVertexColorToBarycentric(MeshDataTool mdt)
	{

		//try looping verticies
		{
			// var marked = new System.Collections.Generic.Dictionary<string, Color>();
			// var markFailCount = 0;
			// var markIndex = 0;
			// float vertexCount = mdt.GetVertexCount();

			// for (var i = 0; i < vertexCount; i++)
			// {
			// 	var vertex = mdt.GetVertex(i);
			// 	var vertId = vertex.ToString("F2");
			// 	// if (marked.TryGetValue(vertId, out var markedColor))
			// 	// {
			// 	// 	GD.Print($"MARK FAIL!  {vertId}");
			// 	// 	mdt.SetVertexColor(i, markedColor);
			// 	// 	markFailCount++;
			// 	// 	continue;
			// 	// }

			// 	var channel = markIndex % 3;
			// 	var color = new Color(0, 0, 0, 1);
			// 	switch (channel)
			// 	{
			// 		case 0:
			// 			color.r = 1;
			// 			break;
			// 		case 1:
			// 			color.g = 1;
			// 			break;
			// 		case 2:
			// 			color.b = 1;
			// 			break;
			// 		case 3:
			// 			color.r = 1;
			// 			color.g = 1;
			// 			break;
			// 		case 4:
			// 			color.r = 1;
			// 			color.b = 1;
			// 			break;
			// 		case 5:
			// 			color.b = 1;
			// 			color.g = 1;
			// 			break;
			// 		case 6:
			// 			color.r = 1;
			// 			color.g = 1;
			// 			color.b = 1;
			// 			break;
			// 		case 7:
			// 			color.r = 0;
			// 			color.g = 0;
			// 			color.b = 0;
			// 			break;

			// 	}
			// 	color = new Color(0, 0, 0, 1);
			// 	//marked.Add(vertId, color);
			// 	markIndex++;
			// 	mdt.SetVertexColor(i, color);
			// }

		}

		//private helper, gets the name of a color (assumes R,G,B, or Black)
		string _getColorName(Color color)
		{
			switch (color)
			{
				case Color c when c == Colors.Red:
					return "Red";
					break;
				case Color c when c == Colors.Green:
					return "Green";
					break;
				case Color c when c == Colors.Blue:
					return "Blue";
					break;
				case Color c when c == Colors.Black:
					return "Black";
					break;
				default:
					throw new Exception($"getColorName, unknown color={color}.  This helper function supports only Red, Green,Blue, or Black.");
			}
		}

		//loop all faces, assigning colors to each of it's 3 verticies.
		{

			//preprocess verticies of the mesh.
			//reset vertex color to black.  we do this in case the model already uses R,G, or B for vertex color, which can mess up our color assigning algorithm below (we check the color of the vertex to see if it's been set)
			var vertexCount = mdt.GetVertexCount();
			for (var vertIdx = 0; vertIdx < vertexCount; vertIdx++)
			{
				mdt.SetVertexColor(vertIdx, Colors.Black);
			}

			var faceCount = mdt.GetFaceCount();
			var faceColorChoices = new List<string>();
			for (var faceIdx = 0; faceIdx < faceCount; faceIdx++)
			{
				//the colors this faces' verticies will use. 
				faceColorChoices.Clear();
				faceColorChoices.Add("Red");
				faceColorChoices.Add("Green");
				faceColorChoices.Add("Blue");


				//first loop through all the faces' verticies and removed any colors already used.
				for (var faceVert = 0; faceVert < 3; faceVert++)
				{
					var vertIdx = mdt.GetFaceVertex(faceIdx, faceVert);
					//var vert = mdt.GetVertex(vertIdx);
					//var vertName = vert.ToString("F5");
					var vertColor = mdt.GetVertexColor(vertIdx);
					var vertColorName = _getColorName(vertColor);
					switch (vertColorName)
					{
						case "Black":
							//no color assigned to the vertex
							break;
						default:
							//remove the vertex's color from our choices (so the next loop through the faces's verticies wont pick it)
							var result = faceColorChoices.Remove(vertColorName);
							if (!result)
							{
								//color used twice in the vertex!  bad
								GD.Print($"failed, vert color used twice!!!!!  color={vertColorName} vertIdx={vertIdx}");
								//throw new Exception($"failed to set the faces vertex color properly.  color is used twice on the face.");
							}
							break;
					}
				}

				//this pass, assign all black verticies our remaining colors
				for (var faceVert = 0; faceVert < 3; faceVert++)
				{
					var vertIdx = mdt.GetFaceVertex(faceIdx, faceVert);
					//var vert = mdt.GetVertex(vertIdx);
					//var vertName = vert.ToString("F5");
					var vertColor = mdt.GetVertexColor(vertIdx);
					var vertColorName = _getColorName(vertColor);
					switch (vertColorName)
					{
						case "Black":
							//assign the vertex a color from our remaining
							var colorChoice = faceColorChoices[0];
							faceColorChoices.RemoveAt(0);
							mdt.SetVertexColor(vertIdx, Color.ColorN(colorChoice));
							break;
						default:
							//vertex already has a color assigned
							break;
					}


					//switch (faceVert)
					//{
					//	case 0:
					//		vertColor.r = 1;
					//		break;
					//	case 1:
					//		vertColor.g = 1;
					//		break;
					//	case 2:
					//		vertColor.b = 1;
					//		break;
					//}
					//mdt.SetVertexColor(vertIdx, vertColor);






					//if (vertColors.TryGetValue(vertName, out var _tempVertCol))
					//{
					//	mdt.getvertexc
					//	//if this vertex already has a color assigned to it
					//	vertColor = _tempVertCol;
					//}
					//else
					//{
					//	switch (priorVertColor)
					//	{
					//		case Color c when c == Colors.Black || c == Colors.Blue:
					//			vertColor = Colors.Red;
					//			break;
					//		case Color c when c == Colors.Red:
					//			vertColor = Colors.Green;
					//			break;
					//		case Color c when c == Colors.Green:
					//			vertColor = Colors.Blue;
					//			break;
					//	}

					//	mdt.SetVertexColor(vert0Idx, Colors.Red);
					//	lastVertCol = Colors.Red;
					//}

					////verify that color isn't already used for a vert of this face.
					//switch (vertColor)
					//{
					//	case Color c when c == Colors.Red:
					//		if (redUsed)
					//		{
					//			throw new Exception("red already used");
					//		}
					//		redUsed = true;
					//		break;
					//	case Color c when c == Colors.Green:
					//		if (greenUsed)
					//		{
					//			throw new Exception("green already used");
					//		}
					//		greenUsed = true;
					//		break;
					//	case Color c when c == Colors.Blue:
					//		if (blueUsed)
					//		{
					//			throw new Exception("blue already used");
					//		}
					//		blueUsed = true;
					//		break;
					//}



				}







			}
		}
	}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(float delta)
	{
		timer.Add(TimeSpan.FromSeconds(delta));

		if (Engine.EditorHint)
		{
			if (mesh == null || timer.TotalSeconds > 5)
			{
				timer = TimeSpan.Zero;
				_Ready();
			}
		}
	}
}
