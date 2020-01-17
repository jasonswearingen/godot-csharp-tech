
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;











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



