
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;




/// <summary>
/// Emulates a WeakReference<> but of type ```STRUCT```.
/// IMPORTANT:  you must call ```.Free()``` when done, otherwise leaks occur.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct WeakHandle<T>
{

	public GCHandle handle;

	public WeakHandle(object target)
	{
		handle = GCHandle.Alloc(target, GCHandleType.Weak);
	}

	public T Target
	{
		get { return (T)handle.Target; }
	}

	public void Free()
	{
		handle.Free();
	}
}






/// <summary>
/// Manages a simulation
/// </summary>
public class SimManager2
{
	public ConcurrentDictionary<string, object> _messageQueue = new ConcurrentDictionary<string, object>();

	/// <summary>
	/// obtain a message queue  (shared consumption of messages)
	/// </summary>
	/// <typeparam name="TMessage"></typeparam>
	/// <param name="key"></param>
	/// <returns></returns>
	public ConcurrentQueue<TMessage> GetMessageQueue<TMessage>(string key) where TMessage : struct
	{
		var queue = _messageQueue.GetOrAdd(key, (_key) => new ConcurrentQueue<TMessage>());
		return (ConcurrentQueue<TMessage>)queue;
		
	}




}


namespace JobSystem
{

	public class JobManager
	{



		public void test()
		{
			//System.Threading.ThreadPool.


		}
	}

	public class JobDetails
	{
		//public  List<Job> dependsOn;
		//public  List<Job> holdingUp;

		public  int input;


		/// <summary>
		/// tasks with same grouping are run in the same thread, sequentially.  only run on other threads if no other work is available for them.
		/// </summary>
		public  string memoryAffinity;

		/// <summary>
		/// lower = more important
		/// </summary>
		public  int order;

		public  Action  target;

		/// <summary>
		/// id of this job.
		/// </summary>
		public readonly long id;

	}

	public interface IJob
	{
		void Execute(JobDetails jobDetails);
	}

	

	namespace _internal
	{

	}
}



//public static class MessageQueue




[StructLayout(LayoutKind.Explicit)]
struct FloatInspector
{
	[FieldOffset(0)]
	public float val;


	[FieldOffset(0)]
	public int bitfield;


	// public FloatInspector(float value)
	// {
	// 	this = new FloatInspector();
	// 	val = value;
	// }

	public int Sign
	{
		get
		{
			return bitfield >> 31;
		}
		set
		{
			bitfield |= value << 31;
		}
	}
	public int Exponent
	{
		get
		{
			return (bitfield & 0x7f800000) >> 23;
			// var e = bitfield & 0x7f800000;
			// e >>= 23;
			// return e;
		}
		set
		{
			bitfield |= (value << 23) & 0x7f800000;
		}
	}

	public int Mantissa
	{
		get
		{
			return bitfield & 0x007fffff;
		}
		set
		{
			bitfield |= value & 0x007fffff;
		}
	}

	public override string ToString()
	{
		var floatBytes = BitConverter.GetBytes(this.val);
		var bitBytes = BitConverter.GetBytes(this.bitfield);
		var floatBytesStr = string.Join(",", floatBytes.Select(b => b.ToString("X")));
		var bitBytesStr = string.Join(",", bitBytes.Select(b => b.ToString("X")));


		//return String.Format("FLOAT={0:G9} RT={0:R} FIELD={1:G},  HEXTEST={1:X8}, fBytes={2}, bBytes={3}", this.val, this.bitfield, floatBytesStr, bitBytesStr); //HEXTEST={1:X}, 
		return String.Format("FLOAT={0:G9} BITFIELD={1:G},  HEX={1:X}, S={2}, E={3:X}, M={4:X}", this.val, this.bitfield, this.Sign, this.Exponent, this.Mantissa); //HEXTEST={1:X}, 
	}


}

[StructLayout(LayoutKind.Explicit)]
struct Vector3Inspector
{
	//[FieldOffset(0)]
	//public Vector3 val;
	[FieldOffset(0)]
	public FloatInspector x;
	[FieldOffset(4)]
	public FloatInspector y;
	[FieldOffset(8)]
	public FloatInspector z;

	public override string ToString()
	{

		return String.Format("x=[{0}], \n\ty=[{1}], \n\tz=[{2}]", x, y, z);


	}

}
