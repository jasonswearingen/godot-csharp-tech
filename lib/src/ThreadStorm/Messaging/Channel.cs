using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ThreadStorm.Messaging
{

	/// <summary>
	/// Represents a channel of communication between Publisher(s) and Subscriber(s).   
	/// Only one kind of message may be sent per Channel.
	/// </summary>
	/// <typeparam name="TMessage">must be struct (to prevent GC allocations).  Pass a tuple if you want to pass an object</typeparam>
	public class Channel<TMessage> where TMessage : struct
	{
		private ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

		private List<WeakReference<ConcurrentQueue<TMessage>>> _subscribers = new List<WeakReference<ConcurrentQueue<TMessage>>>();

		/// <summary>
		/// key used to identify this channel
		/// </summary>
		public readonly string key;

		private List<WeakReference<ConcurrentQueue<TMessage>>> GetSubscribers()
		{
			//Interlocked.MemoryBarrier(); //because we may lock if we use ConcurrentBag instead of a list. becuase concurrentbag requires recreation to remove contents
			return _subscribers;
		}


		public Channel(string key)
		{
			this.key = key;
		}




		/// <summary>
		/// send a message to subscribers
		/// </summary>
		/// <param name="message"></param>
		public void Publish(TMessage message)
		{
			var _markSubsToClean = false;

			rwLock.EnterReadLock();

			var subs = GetSubscribers();

			try
			{
				foreach (var weak in subs)
				{
					if (weak.TryGetTarget(out var sub))
					{

						//foreach (var message in messages)
						{
							sub.Enqueue(message);
						}
					}
					else
					{
						_markSubsToClean = true;
					}
				}
			}
			finally
			{
				rwLock.ExitReadLock();
			}

			if (_markSubsToClean == true)
			{
				CleanSubs();
			}
		}


		/// <summary>
		/// helper to only run the clean code once at a time.
		/// </summary>
		private object _cleanLock = new object();

		/// <summary>
		/// removes any inactive references, and optionally removes a subscriber on-demand.
		/// </summary>
		/// <param name="subToRemove"></param>
		private void CleanSubs(ConcurrentQueue<TMessage> subToRemove = null)
		{
			if (Monitor.TryEnter(_cleanLock)) //only 1 clean needs to ever execute at a time.  if others request it, they can skip.
			{
				try
				{
					//get exclusive access to the list as writing to it is not thread safe
					rwLock.EnterWriteLock();
					try
					{

						//////concurrentBag workflow
						////var oldSubs = GetSubscribers<V>();
						////var activeSubs = oldSubs.Where((wr) => wr.TryGetTarget(out var sub) == true && sub != toRemove);
						////_subscribers = new List<WeakReference<ConcurrentQueue<V>>>(activeSubs);


						//list workflow
						var subs = GetSubscribers();
						subs.RemoveAll((wr) => wr.TryGetTarget(out var sub) == false || sub == subToRemove);

					}
					finally
					{
						rwLock.ExitWriteLock();
					}
				}
				finally
				{
					Monitor.Exit(_cleanLock);
				}
			}

		}

		/// <summary>
		/// register a thread-safe queue that will recieve messages
		/// </summary>
		/// <param name="messagesTarget"></param>
		public void Subscribe(ConcurrentQueue<TMessage> messagesTarget)
		{
			rwLock.EnterWriteLock();
			try
			{
				var subs = GetSubscribers();
				subs.Add(new WeakReference<ConcurrentQueue<TMessage>>(messagesTarget));
			}
			finally
			{
				rwLock.ExitWriteLock();
			}

		}

	}
}
