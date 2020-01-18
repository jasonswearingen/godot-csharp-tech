// displays diagnostics info to the screen in a label.

//based off of https://github.com/lupoDharkael/godot-fps-label



using Godot;
using System;
using System.Text;

[Tool]
public class MonoDiagLabel : Godot.CanvasLayer
{
	public enum POSITION
	{
		TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT, CENTER
	}
	[Export]
	public POSITION position = POSITION.TOP_RIGHT;
	[Export]
	public int margin = 5;

	[Export]
	public int updatesPerSecond = 2;

	[Export]
	public bool showFrameInfo = true;
	[Export]
	public bool showPhysicInfo = true;
	[Export]
	public bool showGcInfo = true;

	public System.Diagnostics.Stopwatch sw;
	public TimeSpan updateFrequency;

	private StringBuilder sb = new StringBuilder(500);

	public Label label;

	private bool isInitialized = false;

	public override void _Ready()
	{
		base._Ready();
		//add label as child
		label = new Label();


		//this.grow


		AddChild(label);
		//if window resizes, update our position
		this.GetTree().Root.Connect("size_changed", this, "updatePosition");

		//set our initial position
		//        _setLabelText();
		updatePosition();


		//set our update frequency
		this.updateFrequency = TimeSpan.FromMilliseconds(1000 / updatesPerSecond);
		this.sw = System.Diagnostics.Stopwatch.StartNew();
		isInitialized = true;


	}

	/// <summary>
	/// sets position of the label based on user preferences, and sets label to grow in the approriate direction based on content length
	/// </summary>
	public void updatePosition()
	{
		var viewport_size = GetViewport().Size;
		var label_size = label.RectSize;


		switch (this.position)
		{
			case POSITION.TOP_LEFT:
				this.Offset = new Vector2(margin, margin);
				label.GrowHorizontal = Control.GrowDirection.End;
				label.GrowVertical = Control.GrowDirection.End;
				break;
			case POSITION.BOTTOM_LEFT:
				this.Offset = new Vector2(margin, viewport_size.y - margin - label_size.y);
				label.GrowHorizontal = Control.GrowDirection.End;
				label.GrowVertical = Control.GrowDirection.Begin;
				break;
			case POSITION.TOP_RIGHT:
				this.Offset = new Vector2(viewport_size.x - margin - label_size.x, margin);
				label.GrowHorizontal = Control.GrowDirection.Begin;
				label.GrowVertical = Control.GrowDirection.End;
				break;
			case POSITION.BOTTOM_RIGHT:
				this.Offset = new Vector2(viewport_size.x - margin - label_size.x, viewport_size.y - margin - label_size.y);
				label.GrowHorizontal = Control.GrowDirection.Begin;
				label.GrowVertical = Control.GrowDirection.Begin;
				break;
			case POSITION.CENTER:
				this.Offset = (viewport_size - label_size) / 2;
				label.GrowHorizontal = Control.GrowDirection.Both;
				label.GrowVertical = Control.GrowDirection.Both;
				break;
		}


	}


	private void _setLabelText()
	{
		sb.Clear();

		if (showFrameInfo)
		{
			sb.Append($"FPS={Engine.GetFramesPerSecond()}  ({Engine.GetFramesDrawn()} Total Frames)");
		}
		if (showPhysicInfo)
		{
			var phyInterpFrac = Engine.GetPhysicsInterpolationFraction();
			var phyInFrame = Engine.IsInPhysicsFrame();
			var phyItterPerSec = Engine.IterationsPerSecond;

			var activeObjects = PhysicsServer.GetProcessInfo(PhysicsServer.ProcessInfo.ActiveObjects);
			var collisionPairs = PhysicsServer.GetProcessInfo(PhysicsServer.ProcessInfo.CollisionPairs);
			var islandCount =PhysicsServer.GetProcessInfo(PhysicsServer.ProcessInfo.IslandCount);

			sb.Append($"\nPhysics: Itter/sec={phyItterPerSec} inPhyFrame={phyInFrame} interpFrac={phyInterpFrac} activeObj={activeObjects} colPairs={collisionPairs} islands={islandCount}");
		}
		if (showGcInfo)
		{
			var totMem = GC.GetTotalMemory(false) / 1024;
			var gcCount = new int[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };

			sb.Append($"\nGC: Collects/Gen={string.Join(",", gcCount)}   ({totMem}K Total Alloc)");
		}



		label.Text = sb.ToString();



	}

	public override void _Process(float delta)
	{
		base._Process(delta);



		if (
						isInitialized && Engine.EditorHint == false  //needed because editor sometimes runs this code, even while _Ready() hasn't been called.
				&& this.sw.Elapsed >= this.updateFrequency)
		{
			this.sw.Restart();

			_setLabelText();

		}




	}
}

