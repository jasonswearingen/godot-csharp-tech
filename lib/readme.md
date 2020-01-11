
```PubSub.cs```
======
My first feature is "done", and **I would *greatly* appreciate feedback regarding usability**.  


Features
-------------
1. Thread safe: supports any number of publishers and subscribers
2. No allocations:  sending/receiving messages doesn't allocate (no GC pressure). 
3. Pull, not push:  Incoming Messages are enqueued for reading during the subscriber's update logic.
4. No marshaling:  stays in managed code land.


Godot already has this feature! (Signals)
-----
The built-in godot ```signals``` workflow marshals back-and-forth between native and managed code.  This PubSub is faster for C# to C# messaging. 

Check it out
-------------
* You can view the code here: https://github.com/jasonswearingen/godot-csharp-tools/blob/master/lib/PubSub.cs
* A demo project using it is here: https://github.com/jasonswearingen/godot-csharp-tools/tree/master/demo-projects/ball-run
  * The "enemy" publishes it's collisions [scripts/enemy.cs](https://github.com/jasonswearingen/godot-csharp-tools/blob/master/demo-projects/ball-run/scripts/enemy.cs)
  * and a child node of the player subscribes to it [script/SubExample.cs](https://github.com/jasonswearingen/godot-csharp-tools/blob/master/demo-projects/ball-run/scripts/SubExample.cs)


**This is for CSharp developers**:  maybe later we could interop this with GDScript but I want to get a solid C# framework firstly.

Example use
-------------
Best to look at the code posted in the "check it out", but:

```csharp
//Publisher:
var channel = pubSub.GetChannel<int>("myKey"); //create/get a channel for sending/receiving messages
channel.Publish(1);

//Subscriber:
var channel = pubSub.GetChannel<int>("myKey");  //create/get a channel for sending/receiving messages
var queue = new ConcurrentQueue<int>(); //create a thread safe queue that messages will be put in.
channel.Subscribe(queue); //now whenever a publish occurs, the queue will get the message

while(queue.TryDequeue(out var message)){ Console.Writeline(message); }  //do work with the message
```

My own "self critique"
-----
The subscriber needs to discover the PubSub Channel's ```key``` and ```message``` type.  I don't see an easy solution for this, so in my demo the subscriber reads static values from the publisher's class.   This works but is verbose.   Probably the workaround is to have a central registry (class) with all standard Channel keys+message types, but it's still pretty not ideal.  Ideally, you could subscribe to a channel and consume it's messages without searching for either the key or message type.


Please give feedback!
==============
- Raise an issue on GitHub,
- email me: jasons aat novalaf doot coom


