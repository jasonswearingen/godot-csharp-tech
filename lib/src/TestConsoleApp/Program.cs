using System;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");


            for (var x = 0; x < 1000; x++)
            {

                var array = new ThreadStorm.Collections.NativeArray<Job>(x);
                var span = array.AsSpan();
                
                for (var i = 0; i < span.Length; i++)
                {
                    span[i] = new Job() { order = x, id = i };
                }
            }

            Console.WriteLine("done!");
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
