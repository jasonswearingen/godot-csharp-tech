# WORK IN PROGRESS
The end goal is a high performance multithreaded game framework + logic "engine" that uses [Godot](https://godotengine.org/) for presentation.


# ToC
- [WORK IN PROGRESS](#work-in-progress)
- [ToC](#toc)
- [Current status](#current-status)
  - [Done](#done)
- [GOALS](#goals)
  - [High Performance](#high-performance)
  - [Simulation Engine](#simulation-engine)
- [REPO STRUCTURE](#repo-structure)
- [Important Notes](#important-notes)
- [Troubleshooting (and installing for first time)](#troubleshooting-and-installing-for-first-time)
- [Please give feedback!](#please-give-feedback)
- [some background reading....](#some-background-reading)





# Current status
**A dumpster fire**.  Feel free to critique, but avoid using "for real" until architecture and  design patterns are further established.  If you want to use the things here, probably the best would be to clone the repo and open the ```scratch``` project in godot.  Adding this stuff to your existing c# project is doable, if you are pretty good with fixing .net build issues (mismatched nuget packages, retargeting runtimes, etc). 

Main branch is semi-stable.  Dev branch might not build.



## Done
- ```PubSub``` system:  [./lib/src/Threadstorm/Messaging](./lib/src/Threadstorm/Messaging) 
  - a reasonably high performance Publish-Subscribe (*Observer Pattern*) based decoupled messaging system.
- ```MonoDiagLabel``` Addon for Godot: [./lib/src/godot-csharp-tech/addons/MonoDiagLabel](./lib/src/godot-csharp-tech/addons/MonoDiagLabel)
  - Provides runtime diag data on framerate, GC collections, etc.
- Bonus stuff:
  - [./demo-projects\ball-run](./demo-projects\ball-run) project shows how to reference a library project directly (not static linking the DLL).   Useful for buid verification and refactoring
  - [./lib\src\godot-csharp-tech\addons\MonoDiagLabel](./lib\src\godot-csharp-tech\addons\MonoDiagLabel) shows how to create a Godot Addon via CSharp



GOALS
========
These are goals.  Not nessicarily what's available.  See the above "done" section.

High Performance
------------
- Jobs System for multithreading
  - Threadsafe by default.
- Avoid Marshalling
  - CSharp-to-CSharp where possible reduces interop overhead
- Optimize (minimize) garbage collections:
  1. prefer allocations of large struct arrays (bulk structs):  less objects for GC to walk
  2. avoid references in bulk structs   (use indexes instead): less refs for GC to walk
  3. use WeakReference<T> when you really need to

Simulation Engine
------------
- Use Godot for presentation
  - "Sever" component should work with a headless version of Godot, and optionally without Godot at all (full CLR app).
- Rules system
- world state
- multiplayer

REPO STRUCTURE
==============

- Git branch layout
   - ```master``` branch contains working builds, with at least some care taken to presentation.  
   - ```dev``` branch is my work in progress, beware.
- ```./lib/src``` contains library code. 
   - ```PubSub.cs``` 
- ```./demo-projects/``` contains demos prototyping/showcasing the tech being developed.



Important Notes
====
- You probably need the Mono version of GoDot v3.2b6 (or newer?) to run.
- You need to install some nuget packages, check the wiki or ping me if you need help.





Troubleshooting (and installing for first time)
====
check this repo's wiki


Please give feedback!
==============
- Raise an issue on GitHub,
- email me: jasons aat novalaf doot coom


# some background reading....

- If you don't know what a job system is, It's a way to write asynchronous systems in a thread-safe, scaleable way.  Here are some high-level overview videos from a game-engine point of view:  [Unity (2018)](https://youtu.be/kwnb9Clh2Is), [Destiny (2015)](https://www.gdcvault.com/play/1022164/Multithreading-the-Entire-Destiny), and [Intel (2010)](https://www.youtube.com/watch?v=1sAR3WHzJEM).
  - more intel vids here: https://gamedev.stackexchange.com/questions/2116/what-are-the-best-resources-on-multi-threaded-game-or-game-engine-design-and-dev

In the past, I tended to use managed allocation pools to reuse objects, but with mobile devices still having pretty weak garbage collectors (Mono), and [gen2 collections being slow with tons of objects](https://stackoverflow.com/a/15294458), it seems Structs+Unmanaged is the way to go with Job System workflows.

Trying to avoid re-inventing the wheel as much as possible.  Some general questions so far...

* **unmanaged memory allocator** I know I can use [Marshal.AllocHGlobal()](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.allochglobal?view=netframework-4.8) but if there's a less naive strategy please let me know.  I don't think [stackalloc](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/stackalloc) is helpful in this situation because the allocated structs will need to be read/wrote on different threads, and the memory being allocated may be quite a lot.
* **unmanaged heap** Unity made their Jobs to be custom structs (dev makes a struct, at long as it inherits from *IJob*)  This sounds like a good idea, but I'm struggling to figure out how to put these custom struct jobs into a queue for the JobSystem to work on.  I'm guessing putting the struct on an unmanaged heap and the queue having a pointer to each.

* **Native collections**  An unmanaged array is easy enough to make, and an expandable could be done, by allocating another chunk internally... is that the best way?  What about more advanced structures?  For example, an unmanaged MultiMap (Hashmap allowing for multiple values per key) would be a very useful collection. [Here are some examples of basic unmanaged collections](https://github.com/jacksondunstan/NativeCollections) Though I'm not sure I'll use those, as they seem to require Unity's secret unmanaged allocator to do the heavy lifting.
