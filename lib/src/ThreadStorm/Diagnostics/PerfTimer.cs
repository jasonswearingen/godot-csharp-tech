using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ThreadStorm.Diagnostics
{

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

		protected override string _QuartileToString(Quartiles<TimeSpan> hist)
		{
			return $"{name} Quantiles: {hist.q0.TotalMilliseconds.ToString("F1") } / {hist.q1.TotalMilliseconds.ToString("F1") } / {hist.q2.TotalMilliseconds.ToString("F1") } / {hist.q3.TotalMilliseconds.ToString("F1") } / {hist.q4.TotalMilliseconds.ToString("F1") } ({hist.count} samples)";
		}


	}
}
