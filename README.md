WORK IN PROGRESS
=============

my target is to build a csharp framework for high performance game logic.  

Current status
-------
**A dumpster fire**.  Feel free to critique, but avoid using "for real" until architecture and  design patterns are further established.

- done, kinda:
  - pubsub


ToC
===
- [WORK IN PROGRESS](#work-in-progress)
	- [Current status](#current-status)
- [ToC](#toc)
- [GOALS](#goals)
	- [High Performance](#high-performance)
	- [Simulation Engine](#simulation-engine)
- [REPO STRUCTURE](#repo-structure)
- [Important Notes](#important-notes)
- [Troubleshooting (and installing for first time)](#troubleshooting-and-installing-for-first-time)


GOALS
========
These are goals.  Not nessicarily what's available.

High Performance
------------
- Jobs System for multithreading
- Avoid Marshalling
  - CSharp-to-CSharp where possible reduces interop overhead
- Avoid heap allocations
  - reduce GC pressure
  - 

Simulation Engine
------------
- Use Godot for presentation
  - Headless "Sever" component should run without Godot
- Rules system
- world state
- multiplayer

REPO STRUCTURE
==============

- Git branch layout
   - ```master``` branch contains working builds, with at least some care taken to presentation.  
   - ```dev``` branch is my work in progress, beware.
- ```./lib/``` contains library code.
   - ```PubSub.cs``` a reasonably high performance Publish-Subscribe (*Observer Pattern*) based decoupled messaging system.
- ```./demo-projects/``` contains demos prototyping/showcasing the tech being developed.



Important Notes
====
- You probably need the Mono version of GoDot v3.2b5 (or newer?) to run.
- You need to install some nuget packages, check the wiki or ping me if you need help.



Troubleshooting (and installing for first time)
====
check this repo's wiki
