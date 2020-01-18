using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThreadStorm.Job
{
    public class JobScheduler
    {



        //public void Schedule(Job job, Span<int> dependsOn, Span<>)

    }



    public struct Job
    {
        //TODO: support memoryAffinity

            /// <summary>
            /// lower = more important
            /// </summary>
        public int order;

        public ValueTask target;

        /// <summary>
        /// id of this job.
        /// </summary>
        public readonly long id;
    }
}
