using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ThreadStorm.Diagnostics
{

	/// <summary>
	/// a general purpose performance counter.
	/// 
	/// reports results as quartiles: 0th, 1st,2nd, 3rd, 4th.   see https://en.wikipedia.org/wiki/Quantile for more information.
	/// </summary>
	/// <typeparam name="T"></typeparam>
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


		public Quartiles<T> GetHistory(int samples)
		{
			var actualSamples = Math.Min(history.Count, samples);
			return _GetHistoryHelper(0, actualSamples);
		}


		private Quartiles<T> _GetHistoryHelper(int startIndex, int length)
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

			var toReturn = Quartiles<T>.ComputeQuartiles(_tempFloats, length);


			return toReturn;

		}

		public Quartiles<T> GetHistory(TimeSpan wallInterval, TimeSpan startingFrom = default)
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


		protected virtual string _QuartileToString(Quartiles<T> hist)
		{
			return $"{name} Quantiles: {hist}";
		}


	}
}
