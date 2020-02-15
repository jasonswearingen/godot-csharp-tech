using System;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            //for (var x = 0; x < 1000; x++)
            //{

            //    var array = new ThreadStorm.Collections.NativeArray<Job>(x);
            //    var span = array.AsSpan();

            //    for (var i = 0; i < span.Length; i++)
            //    {
            //        span[i] = new Job() { order = x, id = i };
            //    }
            //}
            var buckets =new int[100];

            for(var i = 0; i < 10000; i+=3)
            {
                var hashValue = hash(i) % 100;
                buckets[hashValue]++;
            }
            //Console.WriteLine($"{buckets.}");

            Console.WriteLine("done!");
        }


        public static uint hash(int val)
        {
            unchecked
            {
                return (uint)((val * 2654435769) >> 1);
            }
            
        }
    }


    




    public struct Job
    {
        //TODO: support memoryAffinity

        /// <summary>
        /// lower = more important
        /// </summary>
        public int order;

        //public ValueTask target;

        public System.Runtime.InteropServices.GCHandle testHandle;

        /// <summary>
        /// id of this job.
        /// </summary>
        public long id;

        public string Execute()
        {
            return "whooo";
        }
    }

    public interface IJob
    {
        public TReturn Execute<TReturn>();

    }

}
