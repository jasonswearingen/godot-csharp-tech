// displays diagnostics info to the screen in a label.

//based off of https://github.com/lupoDharkael/godot-fps-label



using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

[Tool]
public class MonoDiagLabel : Godot.CanvasLayer
{

	class _MonoDiagLabel_TreeEndHelper : Godot.Node
	{

		public MonoDiagLabel parent;


		public _PerfTimer treeTimer;

		public TimeSpan lastTreeElapsed;
		public _PerfTimer outsideTreeTimer;


		public _MonoDiagLabel_TreeEndHelper(TimeSpan sampleInterval, TimeSpan historyLength)
		{
			treeTimer = new _PerfTimer(sampleInterval, historyLength);
			outsideTreeTimer = new _PerfTimer(sampleInterval, historyLength);
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

			//exec at tree end

			treeTimer.Pause();
			treeTimer.Lap();

			outsideTreeTimer.Unpause();
		}


		public void OnTreeProcessStart()
		{
			outsideTreeTimer.Pause();
			outsideTreeTimer.Lap();

			treeTimer.Unpause();
		}


	}

	public class _PerfTimer
	{


		Stopwatch timer = new Stopwatch();

		public TimeSpan measureInterval;
		public TimeSpan historyLength;


		TimeSpan min;
		TimeSpan max;
		int count;
		TimeSpan total;

		public List<(TimeSpan min, TimeSpan max, int count, TimeSpan total)> history = new List<(TimeSpan min, TimeSpan max, int count, TimeSpan total)>();

		public _PerfTimer(TimeSpan sampleInterval, TimeSpan historyLength)
		{
			this.measureInterval = sampleInterval;
			this.historyLength = historyLength;
		}

		public void Restart()
		{
			timer.Restart();
			min = TimeSpan.Zero;
			max = TimeSpan.Zero;
			count = 0;
			total = TimeSpan.Zero;
		}

		public void Pause()
		{
			timer.Stop();
		}
		public void Unpause()
		{
			timer.Start();
		}

		public void Lap()
		{
			var elapsed = timer.Elapsed;
			if (min == TimeSpan.Zero || min > elapsed)
			{
				min = elapsed;
			}
			if (max < elapsed)
			{
				max = elapsed;
			}
			count++;
			total += elapsed;
			if (total >= measureInterval)
			{
				_ArchiveHelper();
				Restart();
			}
		}

		private void _ArchiveHelper()
		{
			var maxSamples = (int)(historyLength.Ticks / measureInterval.Ticks);
			history.Insert(0, (min, max, count, total));
			if (history.Count > maxSamples)
			{
				history.RemoveRange(maxSamples, history.Count - maxSamples);
			}
		}

		public (TimeSpan min, TimeSpan max, int count, TimeSpan total) Report(TimeSpan targetInterval)
		{
			(TimeSpan min, TimeSpan max, int count, TimeSpan total) toReturn = default;
			var index = 0;
			while (toReturn.total < targetInterval && history.Count > index)
			{
				var sample = history[index];
				if (toReturn.min == TimeSpan.Zero || toReturn.min > sample.min)
				{
					toReturn.min = sample.min;
				}
				if (toReturn.max < sample.max)
				{
					toReturn.max = sample.max;
				}
				toReturn.count += sample.count;
				toReturn.total += sample.total;
				index++;
			}

			return toReturn;
		}

		public string ReportToString(TimeSpan targetInterval)
		{
			return ReportToString(this.Report(targetInterval));
		}
		public static string ReportToString((TimeSpan min, TimeSpan max, int count, TimeSpan total) tuple)
		{
			var tupAvg = tuple.total.TotalMilliseconds / tuple.count;
			return $"Ms Min/Avg/Max/Tot= {tuple.min.TotalMilliseconds.ToString("F2")} / {tupAvg.ToString("F2")} / {tuple.max.TotalMilliseconds.ToString("F2")} / {tuple.total.TotalMilliseconds.ToString("F2")} ({tuple.count} samples)";
		}
	}
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
	public TimeSpan historyLength;

	private StringBuilder sb = new StringBuilder(500);

	public Label label;

	private bool isInitialized = false;

	private _MonoDiagLabel_TreeEndHelper treeEndHelper;

	public override void _Ready()
	{
		base._Ready();

		//if window resizes, update our position
		this.GetTree().Root.Connect("size_changed", this, "updatePosition");

		//set our initial position
		//        _setLabelText();
		updatePosition();


		//set our update frequency
		this.updateFrequency = TimeSpan.FromMilliseconds(1000 / updatesPerSecond);
		this.historyLength = TimeSpan.FromSeconds(updateFrequency.TotalSeconds * 10);
		this.sw = System.Diagnostics.Stopwatch.StartNew();

		//this.renderTime.timer = new Stopwatch();
		framePerf = new _PerfTimer(updateFrequency, historyLength);
		framePerf.Restart();

		isInitialized = true;
		this.ProcessPriority = int.MinValue;




	}
	//private System.Diagnostics.Stopwatch renderTimer = new System.Diagnostics.Stopwatch();


	private _PerfTimer framePerf;

	//private (Stopwatch timer, TimeSpan min, TimeSpan max, int count, TimeSpan total) renderTime;

	public override void _Process(float delta)
	{
		base._Process(delta);

		if (isInitialized == false || Engine.EditorHint == true)
		{
			//needed because editor sometimes runs this code, even while _Ready() hasn't been called.
			return;
		}

		if (treeEndHelper == null)
		{
			//so we can track in-tree vs out-tree times
			treeEndHelper = new _MonoDiagLabel_TreeEndHelper(updateFrequency, historyLength);
			this.GetTree().Root.AddChild(treeEndHelper);
			return;
		}
		treeEndHelper.OnTreeProcessStart();

		//System.Threading.Thread.Sleep(10);




		framePerf.Lap();

		////store info in min/max frame time
		//    var frameElapsed = renderTime.timer.Elapsed;
		//renderTime.timer.Restart();
		//renderTime.count++;
		//renderTime.total += frameElapsed;
		//if (renderTime.max < frameElapsed)
		//{
		//    renderTime.max = frameElapsed;
		//}
		//if (renderTime.min == TimeSpan.Zero || renderTime.min > frameElapsed)
		//{
		//    renderTime.min = frameElapsed;
		//}



		if (this.sw.Elapsed >= this.updateFrequency)
		{
			this.sw.Restart();
			_setLabelText();

			////reset renderTime counters
			//renderTime.min = TimeSpan.Zero;
			//renderTime.max = TimeSpan.Zero;
			//renderTime.count = 0;
			//renderTime.total = TimeSpan.Zero;

		}




	}



	/// <summary>
	/// sets position of the label based on user preferences, and sets label to grow in the approriate direction based on content length
	/// </summary>
	public void updatePosition()
	{
		if (label != null)
		{
			RemoveChild(label);
			label.Dispose();
		}
		//add label as child
		label = new Label();
		AddChild(label);

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
			sb.Append($"FPS ={ Engine.GetFramesPerSecond()}  FrameDetail= {framePerf.ReportToString(this.updateFrequency)}");
			//var min = renderTime.min.TotalMilliseconds.ToString("F1");
			//var avg = (renderTime.total.TotalMilliseconds / renderTime.count).ToString("F1");
			//var max = renderTime.max.TotalMilliseconds.ToString("F1");

			//sb.Append($"FPS={Engine.GetFramesPerSecond()} ({Engine.GetFramesDrawn()} Total Frames)");
			//sb.Append($"\nFrame time (in Ms) over last second: Min/Avg/Max= {min} / {avg} / {max}");
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

