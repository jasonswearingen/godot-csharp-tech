using Godot;
using System;
using System.Linq;
using System.Collections.Concurrent;

using System.Runtime.InteropServices;








namespace Messaging
{


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



	public static class MQ
	{





		static ConcurrentDictionary<string, object> storage = new ConcurrentDictionary<string, object>();

		/// <summary>
		/// a one-off helper to enqueue a single message.  
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <typeparam name="V"></typeparam>
		public static void Enqueue<V>(string key, V value)
		{
			var queue = (ConcurrentQueue<V>)storage.GetOrAdd(key, (_key) => new ConcurrentQueue<V>());
			queue.Enqueue(value);
		}

	}


	public static class __ExtensionMethods
	{
		public static ConcurrentQueue<V> TestGetQueue<V>(this SingletonFactory sf, string key)
		{
			return null;
		}
	}





	/// <summary>
	/// access via .instance static member.
	/// </summary>
	public class SingletonFactory
	{

		public event Action TestEvent
		{
			add
			{

			}
			remove
			{

			}
		}


		//make 
		protected SingletonFactory() { }

		public static SingletonFactory instance = new SingletonFactory();
		public ConcurrentDictionary<string, object> _storage = new ConcurrentDictionary<string, object>();

		public ConcurrentQueue<V> GetQueue<V>(string key)
		{
			var queue = (ConcurrentQueue<V>)_storage.GetOrAdd(key, (_key) => new ConcurrentQueue<V>());
			return queue;
		}


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
	[FieldOffset(0)]
	public Vector3 val;
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
