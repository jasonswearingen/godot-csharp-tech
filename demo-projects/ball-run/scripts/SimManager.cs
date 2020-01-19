using Godot;
using System;
using ThreadStorm.Messaging;

public class SimManager : Node
{

	public PubSub pubSub = new PubSub();

	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//PhysicsServer.

		//var instance = VisualServer.create
	}

	public override void _Process(float delta)
	{
		//GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, false, false);
	}
}
