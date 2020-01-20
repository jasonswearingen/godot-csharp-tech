﻿// displays diagnostics info to the screen in a label.

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


		//public _PerfTimer treeInsideTimer;

		//public TimeSpan lastTreeElapsed;
		//public _PerfTimer treeOutsideTimer;

		public _PerfTimer2 insideTreePerf = new _PerfTimer2("insideTree");
		public _PerfTimer2 outsideTreePerf = new _PerfTimer2("outsideTree");

		public TimeSpan sampleInterval;


		public _MonoDiagLabel_TreeEndHelper(TimeSpan sampleInterval, TimeSpan historyLength)
		{
			//treeInsideTimer = new _PerfTimer(sampleInterval, historyLength);
			//treeOutsideTimer = new _PerfTimer(sampleInterval, historyLength);

			this.sampleInterval = sampleInterval;
		}

		public override void _Ready()
		{
			base._Ready();

			//make update last
			this.ProcessPriority = int.MaxValue;

			//this.treeInsideTimer.Restart();
			//this.treeOutsideTimer.Restart();

		}
		public override void _Process(float delta)
		{
			base._Process(delta);

			//exec at tree end

			//treeInsideTimer.Pause();
			//treeInsideTimer.Lap();

			//treeOutsideTimer.Unpause();

			insideTreePerf.EndSample();
			outsideTreePerf.BeginSample();
		}


		public void OnTreeProcessStart()
		{
			//treeOutsideTimer.Pause();
			//treeOutsideTimer.Lap();

			//treeInsideTimer.Unpause();

			outsideTreePerf.EndSample();
			insideTreePerf.BeginSample();

		}


	}

	public static class _Stats
	{
		public struct Quartiles<T>
		{
			public T q0;
			public T q1;
			public T q2;
			public T q3;
			public T q4;

			public int count;

			public override string ToString()
			{
				return $"{q0} / {q1} / {q2}  / {q3} / {q4}  ({count} samples)";
			}
		}

		/// <summary>
		/// Return the quartile values of an ordered set of doubles
		///   assume the sorting has already been done.
		///   
		/// This actually turns out to be a bit of a PITA, because there is no universal agreement 
		///   on choosing the quartile values. In the case of odd values, some count the median value
		///   in finding the 1st and 3rd quartile and some discard the median value. 
		///   the two different methods result in two different answers.
		///   The below method produces the arithmatic mean of the two methods, and insures the median
		///   is given it's correct weight so that the median changes as smoothly as possible as 
		///   more data ppints are added.
		///    
		/// This method uses the following logic:
		/// 
		/// ===If there are an even number of data points:
		///    Use the median to divide the ordered data set into two halves. 
		///    The lower quartile value is the median of the lower half of the data. 
		///    The upper quartile value is the median of the upper half of the data.
		///    
		/// ===If there are (4n+1) data points:
		///    The lower quartile is 25% of the nth data value plus 75% of the (n+1)th data value.
		///    The upper quartile is 75% of the (3n+1)th data point plus 25% of the (3n+2)th data point.
		///    
		///===If there are (4n+3) data points:
		///   The lower quartile is 75% of the (n+1)th data value plus 25% of the (n+2)th data value.
		///   The upper quartile is 25% of the (3n+2)th data point plus 75% of the (3n+3)th data point.
		/// 
		/// </summary>
		static public Quartiles<float> ComputeQuartiles(Span<float> afVal, int length)
		{
			int iSize = length;
			int iMid = iSize / 2; //this is the mid from a zero based index, eg mid of 7 = 3;

			var toReturn = new Quartiles<float>();

			toReturn.count = length;
			//q0 and q4
			toReturn.q0 = afVal[0];
			toReturn.q4 = afVal[length - 1];

			if (iSize % 2 == 0)
			{
				//================ EVEN NUMBER OF POINTS: =====================
				//even between low and high point
				toReturn.q2 = (afVal[iMid - 1] + afVal[iMid]) / 2;

				int iMidMid = iMid / 2;

				//easy split 
				if (iMid % 2 == 0)
				{
					toReturn.q1 = (afVal[iMidMid - 1] + afVal[iMidMid]) / 2;
					toReturn.q3 = (afVal[iMid + iMidMid - 1] + afVal[iMid + iMidMid]) / 2;
				}
				else
				{
					toReturn.q1 = afVal[iMidMid];
					toReturn.q3 = afVal[iMidMid + iMid];
				}
			}
			else if (iSize == 1)
			{
				//================= special case, sorry ================
				toReturn.q1 = afVal[0];
				toReturn.q2 = afVal[0];
				toReturn.q3 = afVal[0];
			}
			else
			{
				//odd number so the median is just the midpoint in the array.
				toReturn.q2 = afVal[iMid];

				if ((iSize - 1) % 4 == 0)
				{
					//======================(4n-1) POINTS =========================
					int n = (iSize - 1) / 4;
					toReturn.q1 = (afVal[n - 1] * .25f) + (afVal[n] * .75f);
					toReturn.q3 = (afVal[3 * n] * .75f) + (afVal[3 * n + 1] * .25f);
				}
				else if ((iSize - 3) % 4 == 0)
				{
					//======================(4n-3) POINTS =========================
					int n = (iSize - 3) / 4;

					toReturn.q1 = (afVal[n] * .75f) + (afVal[n + 1] * .25f);
					toReturn.q3 = (afVal[3 * n + 1] * .25f) + (afVal[3 * n + 2] * .75f);
				}
			}

			return toReturn;
		}

		/// <summary>
		/// returns quartiles (0th, 1st, 2nd, 3rd, 4th, 5th.)
		/// 
		/// see: https://en.wikipedia.org/wiki/Quantile#Examples
		/// 
		/// This generic overload returns discrete values:  the items at those quartile indicies rounded down.  (no averaging of values)
		/// the float based overload will average results.
		/// </summary>
		static public Quartiles<T> ComputeQuartiles<T>(Span<T> afVal, int length)
		{
			int iSize = length;
			int iMid = iSize / 2; //this is the mid from a zero based index, eg mid of 7 = 3;

			var toReturn = new Quartiles<T>();

			toReturn.count = length;
			//q0 and q4
			toReturn.q0 = afVal[0];
			toReturn.q4 = afVal[length - 1];

			toReturn.q2 = afVal[iMid];

			int iMidMid = iMid / 2;
			toReturn.q1 = afVal[iMidMid];
			toReturn.q3 = afVal[iMid + iMidMid];



			return toReturn;


		}
	}

	public class _PerfTimer2
	{


		private struct PerfInfo : IComparable<PerfInfo>
		{
			public TimeSpan sample;
			public TimeSpan wall;

			public int CompareTo(PerfInfo other)
			{
				return (int)(sample.Ticks - other.sample.Ticks);
			}

			public override string ToString()
			{
				return $"sample={sample}, wall={wall}";
			}
		}
		/// <summary>
		/// tracks wall time, used for getting history over the last N seconds
		/// </summary>
		private Stopwatch wallTimer = new Stopwatch();
		/// <summary>
		/// tracks time inside a sample
		/// </summary>
		private Stopwatch sampleTimer = new Stopwatch();

		private Queue<PerfInfo> history;

		private int maxHistory;

		private string name;
		public _PerfTimer2(string name, int maxHistory = 1000)
		{
			this.name = name;
			this.maxHistory = maxHistory;
			this.history = new Queue<PerfInfo>(maxHistory);
			this._tempHist = new PerfInfo[maxHistory];
			wallTimer.Start();
		}

		public void BeginSample()
		{
			sampleTimer.Restart();
		}

		public TimeSpan EndSample()
		{
			sampleTimer.Stop();
			var toReturn = sampleTimer.Elapsed;

			//keep our hist under max size
			while (history.Count >= maxHistory)
			{
				history.Dequeue();
			}
			history.Enqueue(new PerfInfo() { sample = toReturn, wall = wallTimer.Elapsed });

			return toReturn;

		}
		public void Pause()
		{
			sampleTimer.Stop();
		}

		public void Unpause()
		{
			sampleTimer.Start();
		}

		/// <summary>
		/// stores in chrono order (first inserted to last inserted)
		/// </summary>
		private PerfInfo[] _tempHist;
		public _Stats.Quartiles<TimeSpan> GetHistory(int samples)
		{
			var actualSamples = Math.Min(history.Count, samples);
			return _GetHistoryHelper(0, actualSamples);
		}


		private _Stats.Quartiles<TimeSpan> _GetHistoryHelper(int startIndex, int length)
		{
			if (length == 0)
			{
				return default;
			}
			history.CopyTo(_tempHist, 0);
			Array.Reverse(_tempHist, 0, history.Count);  //only referse length coppied by previous line
																									 //var actualSamples = Math.Min(history.Count, samples);
			Array.Sort(_tempHist, startIndex, length);

			Span<TimeSpan> _tempFloats = stackalloc TimeSpan[length];
			var endIndex = startIndex + length;
			var tempIndex = 0;
			for (var i = startIndex; i < endIndex; i++)
			{
				_tempFloats[tempIndex] = _tempHist[i].sample;
				tempIndex++;
			}

			var toReturn = _Stats.ComputeQuartiles(_tempFloats, length);


			return toReturn;

		}

		public _Stats.Quartiles<TimeSpan> GetHistory(TimeSpan wallInterval, TimeSpan startingFrom = default)
		{


			history.CopyTo(_tempHist, 0);
			Array.Reverse(_tempHist, 0, history.Count); //only referse length coppied by previous line

			var now = wallTimer.Elapsed;

			var startIndexTime = now - startingFrom;
			var endIndexTime = startIndexTime - wallInterval; //working our way backwards through time


			//get indexes for stard/end
			var startIndex = 0;
			var length = 0;
			for (var i = 0; i < history.Count; i++)
			{
				var current = _tempHist[i];
				if (current.wall > startIndexTime)
				{
					startIndex++;
					continue;
				}

				if (current.wall < endIndexTime)
				{
					break;
				}
				length++;
			}

			return _GetHistoryHelper(startIndex, length);

		}

		public string GetHistoryString(int samples)
		{
			var quart = GetHistory(samples);
			return QuartileToString(quart);
		}

		public string GetHistoryString(TimeSpan wallInterval, TimeSpan startingFrom = default)
		{
			var quart = GetHistory(wallInterval, startingFrom);
			return QuartileToString(quart);
		}

		public string QuartileToString(_Stats.Quartiles<TimeSpan> hist)
		{
			return $"{name} Quantiles: {hist.q0.TotalMilliseconds.ToString("F1") } / {hist.q1.TotalMilliseconds.ToString("F1") } / {hist.q2.TotalMilliseconds.ToString("F1") } / {hist.q3.TotalMilliseconds.ToString("F1") } / {hist.q4.TotalMilliseconds.ToString("F1") } / ({hist.count} samples)";
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

		public List<(TimeSpan min, TimeSpan max, int count, TimeSpan total, TimeSpan pause)> history = new List<(TimeSpan min, TimeSpan max, int count, TimeSpan total, TimeSpan pause)>();

		public _PerfTimer(TimeSpan sampleInterval, TimeSpan historyLength)
		{
			this.measureInterval = sampleInterval;
			this.historyLength = historyLength;
		}

		Stopwatch pauseTimer = new Stopwatch();

		public void Restart()
		{
			pauseTimer.Reset();
			timer.Restart();
			min = TimeSpan.Zero;
			max = TimeSpan.Zero;
			count = 0;
			total = TimeSpan.Zero;
		}

		public void Pause()
		{
			pauseTimer.Start();
			//timer.Stop();
		}
		public void Unpause()
		{
			//timer.Start();
			pauseTimer.Stop();
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
			history.Insert(0, (min, max, count, total: total, pause: pauseTimer.Elapsed));
			if (history.Count > maxSamples)
			{
				history.RemoveRange(maxSamples, history.Count - maxSamples);
			}
		}

		public (TimeSpan min, TimeSpan max, int count, TimeSpan total, TimeSpan pause) Report(TimeSpan targetInterval)
		{
			(TimeSpan min, TimeSpan max, int count, TimeSpan total, TimeSpan pause) toReturn = default;
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
				toReturn.pause += sample.pause;
				index++;
			}
			toReturn.total -= toReturn.pause;
			return toReturn;
		}

		public string ReportToString(TimeSpan targetInterval)
		{
			return ReportToString(this.Report(targetInterval));
		}
		public static string ReportToString((TimeSpan min, TimeSpan max, int count, TimeSpan total, TimeSpan pause) tuple)
		{
			var tupAvg = tuple.total.TotalMilliseconds / tuple.count;
			return $"Ms Min/Avg/Max/Tot= {tuple.min.TotalMilliseconds.ToString("F2")} / {tupAvg.ToString("F2")} / {tuple.max.TotalMilliseconds.ToString("F2")} / {tuple.total.TotalMilliseconds.ToString("F2")} ({tuple.count} samples, {tuple.pause.TotalMilliseconds.ToString("F2")} paused)";
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
	public float labelUpdatesPerSecond = 0.5f;

	[Export]
	public float samplesPerSecond = 2;

	[Export]
	public bool showFrameInfo = true;
	[Export]
	public bool showPhysicInfo = true;
	[Export]
	public bool showGcInfo = true;

	public System.Diagnostics.Stopwatch sw;
	public TimeSpan labelUpdateFrequency;
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
		this.labelUpdateFrequency = TimeSpan.FromSeconds(1 / labelUpdatesPerSecond);
		this.historyLength = TimeSpan.FromSeconds(labelUpdateFrequency.TotalSeconds * 10);
		this.sw = System.Diagnostics.Stopwatch.StartNew();

		//this.renderTime.timer = new Stopwatch();
		//framePerf = new _PerfTimer(updateFrequency, historyLength);
		//framePerf.Restart();

		isInitialized = true;
		this.ProcessPriority = int.MinValue;




	}
	//private System.Diagnostics.Stopwatch renderTimer = new System.Diagnostics.Stopwatch();


	//private _PerfTimer framePerf;

	private _PerfTimer2 totalFramePerf = new _PerfTimer2("totalFrame");


	//private (Stopwatch timer, TimeSpan min, TimeSpan max, int count, TimeSpan total) renderTime;

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

		if (treeEndHelper == null)
		{
			//so we can track in-tree vs out-tree times
			treeEndHelper = new _MonoDiagLabel_TreeEndHelper(labelUpdateFrequency, historyLength);
			this.GetTree().Root.AddChild(treeEndHelper);
			return;
		}
		treeEndHelper.OnTreeProcessStart();

		//System.Threading.Thread.Sleep(10);




		//framePerf.Lap();

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



		if (this.sw.Elapsed >= this.labelUpdateFrequency)
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
			sb.Append($"FPS ={ Engine.GetFramesPerSecond()}");//  FrameDetail= {framePerf.ReportToString(this.updateFrequency)}");
			sb.Append($"\n{totalFramePerf.GetHistoryString(this.labelUpdateFrequency)}");
			sb.Append($"\n{treeEndHelper.insideTreePerf.GetHistoryString(this.labelUpdateFrequency)}");
			sb.Append($"\n{treeEndHelper.outsideTreePerf.GetHistoryString(this.labelUpdateFrequency)}");
			sb.Append($"\n");
			//sb.Append($"\nInside Tree ={treeEndHelper.treeInsideTimer.ReportToString(this.updateFrequency)}  Outside Tree ={treeEndHelper.treeOutsideTimer.ReportToString(this.updateFrequency)}");
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

