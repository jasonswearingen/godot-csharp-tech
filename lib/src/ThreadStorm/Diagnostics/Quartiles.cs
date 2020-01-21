using System;
using System.Collections.Generic;
using System.Text;

namespace ThreadStorm.Diagnostics
{

	/// <summary>
	/// use the static method .ComputeQuartiles() to generate
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct Quartiles<T>
	{
		public T q0;
		public T q1;
		public T q2;
		public T q3;
		public T q4;

		public int count;



		/// <summary>
		/// returns quartiles (0th, 1st, 2nd, 3rd, 4th, 5th.)
		/// 
		/// see: https://en.wikipedia.org/wiki/Quantile#Examples
		/// 
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




		public override string ToString()
		{
			return $"{q0} / {q1} / {q2}  / {q3} / {q4} ({count} samples)";
		}

		/// <summary>
		/// colors output string (using BBCode) if any number is 2 stddevs higher/lower than mean.
		/// </summary>
		/// <param name="toFloat"></param>
		/// <returns></returns>
		private string ToColorString(Func<T, float> toFloat, Func<T, string> toString)
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
			var sumQ = ((qf[1] * qf[1] * count * 0.5) + (qf[3] * qf[3] * count * 0.5) + (qf[2] * qf[2] * count)) / 2;
			var stdDev = Math.Sqrt((sumQ - (sum * sum) / count) * (1f / (count - 1)));

			var stdDev1Below = avg - stdDev;
			var stdDev1Above = avg + stdDev;

			var qs = new string[5];
			qs[0] = toString(q0);
			qs[1] = toString(q1);
			qs[2] = toString(q2);
			qs[3] = toString(q3);
			qs[4] = toString(q4);


			return $"{(qf[0] < stdDev1Below ? $"[color=blue]{qs[0]}[/color]" : qs[0])} / {q1} / {q2}  / {q3} / {q4} ({count} samples)";


		}




	}
}
