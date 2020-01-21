// displays diagnostics info to the screen in a label.

//based off of https://github.com/lupoDharkael/godot-fps-label



using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using __MonoDiagLabel_internal;

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
	public Color fontShadowColor = new Color(1,1,1);


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
		label.AddColorOverride("font_color_shadow",fontShadowColor);
		

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

	private _PerfTimer totalFramePerf = new _PerfTimer("totalFrameMs");

	private _PerfSampler<int> fpsPerf = new _PerfSampler<int>("FPS");

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
				this.Offset = new Vector2((viewport_size.x - margin - label_size.x)/2, margin);
				label.GrowHorizontal = Control.GrowDirection.Both;
				label.GrowVertical = Control.GrowDirection.End;
				break;
		}
	}

	private Func<TimeSpan, float> _timeToFloat = (val) => (float)val.TotalMilliseconds;
	private Func<TimeSpan, string> _timeToString = (val) =>val.TotalMilliseconds.ToString("F1");

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


namespace __MonoDiagLabel_internal
{

	class _MonoDiagLabel_TreeEndHelper : Godot.Node
	{
		public _PerfTimer insideTreePerf = new _PerfTimer("insideTreeMs");
		public _PerfTimer outsideTreePerf = new _PerfTimer("outsideTreeMs");

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

	public static class _Stats
	{
		//public static float StandardDeviation(Span<float> data)
		//{
		//	float stdDev = 0;
		//	float sumAll = 0;
		//	float sumAllQ = 0;

		//	//Sum of x and sum of x²
		//	for (int i = 0; i < data.Length; i++)
		//	{
		//		double x = data[i];
		//		sumAll += x;
		//		sumAllQ += x * x;
		//	}

		//	//Mean (not used here)
		//	//double mean = 0;
		//	//mean = sumAll / (double)data.Length;

		//	//Standard deviation
		//	stdDev = System.Math.Sqrt(
		//		(sumAllQ -
		//		(sumAll * sumAll) / data.Length) *
		//		(1.0d / (data.Length - 1))
		//		);

		//	return stdDev;
		//}

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
				return $"{q0} / {q1} / {q2}  / {q3} / {q4} ({count} samples)";
			}

			/// <summary>
			/// colors output string (using BBCode) if any number is 2 stddevs higher/lower than mean.
			/// </summary>
			/// <param name="toFloat"></param>
			/// <returns></returns>
			private string ToColorString(Func<T,float> toFloat,Func<T,string>toString)
			{
				//UNUSED PLACEHOLDER:  I can't get richtextlabel working, but if I can, we can code colors when values are outside stddev.

				Span<float> qf = stackalloc float[5];
				qf[0] = toFloat(q0);
				qf[1] = toFloat(q1);
				qf[2] = toFloat(q2);
				qf[3] = toFloat(q3);
				qf[4] = toFloat(q4);

				//stddev calc from https://stackoverflow.com/questions/895929/how-do-i-determine-the-standard-deviation-stddev-of-a-set-of-values
				var avg = qf[2];
				var sum = ((qf[1] * count * 0.5) + (qf[3] * count * 0.5) + (qf[2] * count)) / 2;
				var sumQ = ((qf[1] * qf[1] * count * 0.5) + (qf[3] * qf[3] * count * 0.5) + (qf[2]*qf[2] * count)) / 2;
				var stdDev = Math.Sqrt((sumQ - (sum * sum) / count) * (1f / (count - 1)));

				var stdDev1Below = avg - stdDev;
				var stdDev1Above = avg + stdDev;

				var qs = new string[5];
				qs[0] = toString(q0);
				qs[1] = toString(q1);
				qs[2] = toString(q2);
				qs[3] = toString(q3);
				qs[4] = toString(q4);


				return $"{(qf[0]<stdDev1Below?$"[color=blue]{qs[0]}[/color]": qs[0])} / {q1} / {q2}  / {q3} / {q4} ({count} samples)";


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

	internal struct PerfInfo<T> : IComparable<PerfInfo<T>> where T : IComparable<T>
	{
		public T sample;
		public TimeSpan wall;

		public int CompareTo(PerfInfo<T> other)
		{
			return sample.CompareTo(other.sample);// (int)(sample.c - other.sample.Ticks);
		}

		public override string ToString()
		{
			return $"sample={sample}, wall={wall}";
		}
	}

	public class _PerfSampler<T> where T : unmanaged, IComparable<T>
	{



		/// <summary>
		/// tracks wall time, used for getting history over the last N seconds
		/// </summary>
		private Stopwatch wallTimer = new Stopwatch();


		/// <summary>
		/// stores in chrono order (first inserted to last inserted)
		/// </summary>
		private PerfInfo<T>[] _tempHist;

		private Queue<PerfInfo<T>> history;

		private int maxHistory;

		public string name;
		public _PerfSampler(string name, int maxHistory = 1000)
		{
			this.name = name;
			this.maxHistory = maxHistory;
			this.history = new Queue<PerfInfo<T>>(maxHistory);
			this._tempHist = new PerfInfo<T>[maxHistory];
			wallTimer.Start();
		}

		/// <summary>
		/// Store a sample for the current wallTime
		/// </summary>
		/// <param name="sampleValue"></param>
		public void Sample(T sampleValue)
		{
			//keep our hist under max size
			while (history.Count >= maxHistory)
			{
				history.Dequeue();
			}
			history.Enqueue(new PerfInfo<T>() { sample = sampleValue, wall = wallTimer.Elapsed });
		}


		public _Stats.Quartiles<T> GetHistory(int samples)
		{
			var actualSamples = Math.Min(history.Count, samples);
			return _GetHistoryHelper(0, actualSamples);
		}


		private _Stats.Quartiles<T> _GetHistoryHelper(int startIndex, int length)
		{
			if (length == 0)
			{
				return default;
			}
			history.CopyTo(_tempHist, 0);
			Array.Reverse(_tempHist, 0, history.Count);  //only referse length coppied by previous line
														 //var actualSamples = Math.Min(history.Count, samples);
			Array.Sort(_tempHist, startIndex, length);

			Span<T> _tempFloats = stackalloc T[length];
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

		public _Stats.Quartiles<T> GetHistory(TimeSpan wallInterval, TimeSpan startingFrom = default)
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
			return _QuartileToString(quart);
		}

		public string GetHistoryString(TimeSpan wallInterval, TimeSpan startingFrom = default)
		{
			var quart = GetHistory(wallInterval, startingFrom);
			return _QuartileToString(quart);
		}


		protected virtual string _QuartileToString(_Stats.Quartiles<T> hist)
		{
			return $"{name} Quantiles: {hist}";
		}


	}


	public class _PerfTimer : _PerfSampler<TimeSpan>
	{

		/// <summary>
		/// tracks time inside a sample
		/// </summary>
		private Stopwatch sampleTimer = new Stopwatch();

		public _PerfTimer(string name, int maxHistory = 1000) : base(name, maxHistory)
		{
		}

		public void BeginSample()
		{
			sampleTimer.Restart();
		}

		public TimeSpan EndSample()
		{
			sampleTimer.Stop();
			var sampleValue = sampleTimer.Elapsed;

			this.Sample(sampleValue);
			return sampleValue;

		}
		public void Pause()
		{
			sampleTimer.Stop();
		}

		public void Unpause()
		{
			sampleTimer.Start();
		}

		protected override string _QuartileToString(_Stats.Quartiles<TimeSpan> hist)
		{
			return $"{name} Quantiles: {hist.q0.TotalMilliseconds.ToString("F1") } / {hist.q1.TotalMilliseconds.ToString("F1") } / {hist.q2.TotalMilliseconds.ToString("F1") } / {hist.q3.TotalMilliseconds.ToString("F1") } / {hist.q4.TotalMilliseconds.ToString("F1") } ({hist.count} samples)";
		}


	}
}