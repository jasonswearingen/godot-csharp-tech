using Godot;
using System;
using System.Linq;
using System.Collections.Concurrent;

using System.Runtime.InteropServices;

using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;



/// <summary>
/// Publish-Subscribe (Observer) Pattern.
/// 
/// <para>Designed for high performance messaging in games (written for Godot, but can be used elsewhere)</para>
/// 
/// <list type="number">
/// <listheader>Design considerations:</listheader>
/// <item>Thread safe:  can be published and/or subscribed simultaniously on multiple threads</item>
/// <item> Memory-Leak proof: disposal of subscribers will not leak memory</item>
/// <item>Alloc free:  Attaching Publishers and Subscribers allocates, as does queue resizes, but sending messages does not.   If this matters, be sure to send structs.</item>
/// </list>
/// 
/// <para>How to use:  </para>
/// 
/// <para>Future work:  add performance tests,  change queue-per-subscriber to a view into a shared buffer.  (reduce memory footprint and message copying)</para>
///
/// <code>
/// //Publisher: <para/>
/// var channel = pubSub.GetChannel{int}("myKey"); //create/get a channel for sending and recieving messages. <para/>
/// channel.Publish(1); <para/>
/// <para/>
/// //Subscriber:<para/>
/// var channel = pubSub.GetChannel{int}("myKey"); //create/get a channel for sending and recieving messages. <para/>
/// var queue = new ConcurrentQueue{int}(); //create a thread safe queue that messages will be put in. <para/>
/// channel.Subscribe(queue); //now whenever a publish occurs, the queue will get the message <para/>
///  <para/>
/// while(queue.TryDequeue(out var message)){ Console.Writeline(message); }  //do work with the message <para/>
/// </code>
/// </summary>
public class PubSub
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



	/// <summary>
	/// key = Pub key (anything).
	/// value = List<WR<Queue<V>> (Subscribers)
	/// </summary>
	/// <returns></returns>
	private readonly ConcurrentDictionary<string, object> _storage = new ConcurrentDictionary<string, object>();


	/// <summary>
	/// obtain a channel for sending and recieving messages.
	/// </summary>
	/// <typeparam name="TMessage">must be struct (to prevent GC allocations).  Pass a tuple if you want to pass an object</typeparam>
	/// <param name="key"></param>
	/// <returns></returns>
	public Channel<TMessage> GetChannel<TMessage>(string key) where TMessage : struct
	{

		var channel = _storage.GetOrAdd(key, (_key) => (new Channel<TMessage>(key)));

		return (Channel<TMessage>)channel;

	}

}

