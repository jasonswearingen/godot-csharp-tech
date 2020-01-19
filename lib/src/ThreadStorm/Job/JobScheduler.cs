using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ThreadStorm.Job
{

    //TODO:  look into  Entitas-CSharp for some ECS architecture ideas, maybe also jobs, but probably not.  https://github.com/sschmid/Entitas-CSharp

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
