using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadStorm.Diagnostics
{

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
}
