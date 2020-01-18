using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ThreadStorm.Runtime._Advanced
{
    /// <summary>
    /// Facade/Placeholder implementation of a native allocator.
    /// 
    /// //TODO: should be replaced with a more efficient pooled allocator in the future, such as described in the remarks section
    /// </summary>
    /// <remarks>
    /// can start with this c# pool allocator: https://jacksondunstan.com/articles/3770
    /// if that doesn't work out can try.....
    /// 
    /// there's a c# implementation of douglea alloc:  http://gee.cs.oswego.edu/dl/html/malloc.html
    /// ----   c# version here: https://github.com/gosub-com/DlMalloc
    /// ----   this is fast for single threaded use, but slow for multithread.
    /// another option would be to port  https://github.com/mjansson/rpmalloc
    /// ------  C++, but relatively simple code and high multithreaded performance
    /// another, completely managed option is: BufferManager: https://stackoverflow.com/a/13344090
    /// -------  https://referencesource.microsoft.com/#System.ServiceModel/System/ServiceModel/Channels/BufferManager.cs
    /// -------  https://referencesource.microsoft.com/#System.ServiceModel.Internals/System/Runtime/InternalBufferManager.cs
    /// 
    /// 
    /// </remarks>
    public static class NativeAllocator
    {
        //TODO:  in #DEV, track allocations and be sure they are disposed before app ends.   (can use reflection to inspect callsite)
        public static IntPtr Alloc(int bytes)
        {
            return Marshal.AllocHGlobal(bytes);
        }

        public static void Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }


        public static (IntPtr ptr, int bytes) Alloc<T>(int length) where T : unmanaged
        {
            var type = typeof(T);
            if(!_structLengthCache.TryGetValue(type,out var sizeOf))
            {
                //sizeOf = sizeof(T);  //can NOT use because unmanaged memory size may be different.  see: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/sizeof
                sizeOf = Marshal.SizeOf(type);  
                _structLengthCache.Add(type, sizeOf);
            }

            var bytes = length * sizeOf;
            var ptr = Marshal.AllocHGlobal(bytes);
            //var span = new Span<T>(ptr.ToPointer(), length);
            
            return (ptr, bytes);
        }

        /// <summary>
        /// per-thread cache of struct lengths (used by .Alloc{T}() method)
        /// </summary>
        [ThreadStatic]
        private static Dictionary<Type, int> _structLengthCache = new Dictionary<Type, int>();
    }

    
}
