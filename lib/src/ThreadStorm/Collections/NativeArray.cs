using System;
using System.Collections.Generic;
using System.Text;
using ThreadStorm.Runtime._Advanced;

namespace ThreadStorm.Collections
{
    /// <summary>
    /// allocates a fixed size array from unmanaged memory.
    /// 
    /// must call .Free() when done.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct NativeArray<T> where T: unmanaged
    {
        readonly public IntPtr ptr;
        readonly public int bytes;
        readonly public int length;
        public NativeArray(int length)
        {
            var tuple = NativeAllocator.Alloc<T>(length);

            this.ptr = tuple.ptr;
            this.bytes = tuple.bytes;
            this.length = length;
        }

        unsafe public Span<T> AsSpan() => new Span<T>(ptr.ToPointer(), length);
        unsafe public ReadOnlySpan<T> AsReadOnlySpan() => new ReadOnlySpan<T>(ptr.ToPointer(), length);

        public void Free()
        {
            NativeAllocator.Free(this.ptr);
        }
    }

}
