namespace godot_csharp_tech.nodes
{

	// displays diagnostics info to the screen in a label.

	//based off of https://github.com/lupoDharkael/godot-fps-label




	using Godot;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Text;
	using _internal;
    using ThreadStorm.Diagnostics;

	/// <summary>
	/// # MonoDiagLabel<para/>
	///	I will eventually get around to posting this in the asset library, but if you are writing a C# game this label may be useful.
	///it's a port of the FPS Label to C# and adds more details, such as GC collection info.. <para/>
	///	Addionally, I hope this is a good reference for anyone who wants to create their own C# addon.<para/>
	///	add this to** the root node** of your scene to get advanced performance information, including GC details.<para/><para/>
	///## Quantiles (how to read label)<para/>
	///Framerates are displayed as Quantiles (0th to 4th).  <para/>
	///For example:  ```insideTreeMs Quantiles: 0.1 / 2.4 / 2.6 / 5.8 / 15.2 (803 samples)```<para/>
	///- ```insideTreeMs``` shows how much time is taken inside the scene tree, in milliseconds<para/>
	///- ```0.1```:  the 0th Quartile, this is the time taken by the fastest frame during the sample interval<para/>
	///- ```2.4```: the 1st Quartile, aprox 25% frames took less than 2.4ms, while 75% took more than 2.4ms.<para/>
	///- ```2.6```: the 2nd Quartile, IE the Median.  (50% frames took less than 2.6ms)<para/>
	///- ```5.8```: the 3rd Quartile, aprox 75% frames took less than 5.8ms.  25% more.<para/>
	///- ```15.2```: the 4th Quartile, the time that the slowest frame took<para/>
	///- ```(803 samples)``` how many frames were measured<para/>
	///	For more information on Quartiles, see: https://en.wikipedia.org/wiki/Quantile#Examples <para/><para/>
	///# update frequency <para/>
	///Samples are taken every frame (call to ```._Process()```).<para/>
	///You can customize the label update via the Node inspector (click on the node after you add it to your scene)<para/><para/>
	///# Performance note<para/>
	///-Best to have label update every 0.5 seconds or slower(default is 2 seconds).This is to reduce pressure on the GC(displaying text allocates junk string objects).
	/// </summary>
	[Tool]
	public class MonoDiagLabel : Godot.CanvasLayer
	{
		public enum POSITION
		{
			TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT, TOP_CENTER, CENTER
		}
		[Export]
		public POSITION position = POSITION.TOP_CENTER;
		[Export]
		public int margin = 5;

		[Export]
		public float labelUpdatesPerSecond = 0.5f;


		[Export]
		public bool showFrameInfo = true;
		[Export]
		public bool showPhysicInfo = true;
		[Export]
		public bool showGcInfo = true;

		[Export]
		public Color fontColor = new Color(0, 0, 0);
		[Export]
		public Color fontShadowColor = new Color(1, 1, 1);


		/// <summary>
		/// stopwatch that tracks when to update label
		/// </summary>
		public System.Diagnostics.Stopwatch labelUpdateStopwatch;
		public TimeSpan labelUpdateFrequency;
		public TimeSpan historyLength;

		private StringBuilder sb = new StringBuilder(500);

		public Label label;

		private bool isInitialized = false;

		private _MonoDiagLabel_TreeEndHelper treeEndHelper;

		public override void _Ready()
		{
			base._Ready();

			//if (label != null)
			//{
			//	RemoveChild(label);
			//	label.Dispose();
			//}
			//add label as child
			label = new Label();



			label.AddColorOverride("font_color", fontColor);
			label.AddColorOverride("font_color_shadow", fontShadowColor);


			AddChild(label);


			//if window resizes, update our position, only helps if Project --> Display --> Window --> Stretch --> Mode == "disabled", so not doing it by default
			//this.GetTree().Root.Connect("size_changed", this, "updatePosition");

			//set our initial position
			updatePosition();

			//set our update frequency
			this.labelUpdateFrequency = TimeSpan.FromSeconds(1 / labelUpdatesPerSecond);
			this.historyLength = TimeSpan.FromSeconds(labelUpdateFrequency.TotalSeconds * 10);
			this.labelUpdateStopwatch = System.Diagnostics.Stopwatch.StartNew();

			isInitialized = true;
			this.ProcessPriority = int.MinValue;




		}

		private PerfTimer totalFramePerf = new PerfTimer("totalFrameMs");

		private PerfSampler<int> fpsPerf = new PerfSampler<int>("FPS");

		public override void _Process(float delta)
		{
			base._Process(delta);

			if (isInitialized == false || Engine.EditorHint == true)
			{
				//needed because editor sometimes runs this code, even while _Ready() hasn't been called.
				return;
			}

			totalFramePerf.EndSample();
			totalFramePerf.BeginSample();

			fpsPerf.Sample((int)Engine.GetFramesPerSecond());

			if (treeEndHelper == null)
			{
				//so we can track in-tree vs out-tree times
				treeEndHelper = new _MonoDiagLabel_TreeEndHelper(labelUpdateFrequency, historyLength);
				this.GetTree().Root.AddChild(treeEndHelper);
				return;
			}
			treeEndHelper.OnTreeProcessStart();




			if (this.labelUpdateStopwatch.Elapsed >= this.labelUpdateFrequency)
			{
				this.labelUpdateStopwatch.Restart();
				_setLabelText();
			}

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
					this.label.RectPosition = new Vector2(margin, viewport_size.y - margin - label_size.y);
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
				case POSITION.TOP_CENTER:
					this.Offset = new Vector2((viewport_size.x - margin - label_size.x) / 2, margin);
					label.GrowHorizontal = Control.GrowDirection.Both;
					label.GrowVertical = Control.GrowDirection.End;
					break;
			}
		}

		private Func<TimeSpan, float> _timeToFloat = (val) => (float)val.TotalMilliseconds;
		private Func<TimeSpan, string> _timeToString = (val) => val.TotalMilliseconds.ToString("F1");

		private void _setLabelText()
		{
			sb.Clear();

			if (showFrameInfo)
			{
				sb.Append($"{fpsPerf.GetHistoryString(this.labelUpdateFrequency)}");
				sb.Append($"\n{totalFramePerf.GetHistoryString(this.labelUpdateFrequency) }");
				sb.Append($"\n{treeEndHelper.insideTreePerf.GetHistoryString(this.labelUpdateFrequency)}");
				sb.Append($"\n{treeEndHelper.outsideTreePerf.GetHistoryString(this.labelUpdateFrequency)}");
				sb.Append($"\n");
			}
			if (showPhysicInfo)
			{
				var phyInterpFrac = Engine.GetPhysicsInterpolationFraction();
				var phyInFrame = Engine.IsInPhysicsFrame();
				var phyItterPerSec = Engine.IterationsPerSecond;

				var activeObjects = PhysicsServer.GetProcessInfo(PhysicsServer.ProcessInfo.ActiveObjects);
				var collisionPairs = PhysicsServer.GetProcessInfo(PhysicsServer.ProcessInfo.CollisionPairs);
				var islandCount = PhysicsServer.GetProcessInfo(PhysicsServer.ProcessInfo.IslandCount);

				sb.Append($"\nPhysics: Itter/sec={phyItterPerSec} inPhyFrame={phyInFrame} interpFrac={phyInterpFrac.ToString("F2")} activeObj={activeObjects} colPairs={collisionPairs} islands={islandCount}");
			}
			if (showGcInfo)
			{
				var totMem = GC.GetTotalMemory(false) / 1024;
				var gcCount = new int[] { GC.CollectionCount(0), GC.CollectionCount(1), GC.CollectionCount(2) };

				sb.Append($"\nGC: Collects/Gen={string.Join(",", gcCount)}   ({totMem}K Total Alloc)");
			}



			label.Text = sb.ToString();



		}

	}


	namespace _internal
	{

		class _MonoDiagLabel_TreeEndHelper : Godot.Node
		{
			public PerfTimer insideTreePerf = new PerfTimer("insideTreeMs");
			public PerfTimer outsideTreePerf = new PerfTimer("outsideTreeMs");

			public TimeSpan sampleInterval;


			public _MonoDiagLabel_TreeEndHelper(TimeSpan sampleInterval, TimeSpan historyLength)
			{
				this.sampleInterval = sampleInterval;
			}

			public override void _Ready()
			{
				base._Ready();

				//make update last
				this.ProcessPriority = int.MaxValue;
			}
			public override void _Process(float delta)
			{
				base._Process(delta);

				insideTreePerf.EndSample();
				outsideTreePerf.BeginSample();
			}


			public void OnTreeProcessStart()
			{
				outsideTreePerf.EndSample();
				insideTreePerf.BeginSample();
			}
		}

	}


}