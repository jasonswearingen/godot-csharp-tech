using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ThreadStorm.Runtime
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
}
