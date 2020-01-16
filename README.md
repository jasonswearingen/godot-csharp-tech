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





# Current status
**A dumpster fire**.  Feel free to critique, but avoid using "for real" until architecture and  design patterns are further established.

## Done
- ```PubSub``` system.   
  - a reasonably high performance Publish-Subscribe (*Observer Pattern*) based decoupled messaging system.
  - Found at ```lib/src/Threadstorm/Messaging```
  -  [additional docs in it's readme](./lib/src/ThreadStorm/Messaging/readme.md).
- ```MonoDiagLabel``` Addon for Godot
  - Provides runtime diag data on framerate, GC collections, etc.
  - Found at ```lib\src\godot-csharp-tech\addons\MonoDiagLabel```
- Bonus stuff:
  - ```demo-projects\ball-run``` project shows how to reference a library project directly (not static linking the DLL).   Useful for buid verification
  - ```lib\src\godot-csharp-tech\addons\MonoDiagLabel``` shows how to create a Godot Addon via CSharp



GOALS
========
These are goals.  Not nessicarily what's available.  See the above "done" section.

High Performance
------------
- Jobs System for multithreading
  - Threadsafe by default.
- Avoid Marshalling
  - CSharp-to-CSharp where possible reduces interop overhead
- Avoid heap allocations
  - reduce GC pressure
  - 

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
