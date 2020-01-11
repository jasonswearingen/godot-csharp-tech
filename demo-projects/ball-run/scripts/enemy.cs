using Godot;
using System;

public class enemy : Area
{

	public static string PUBKEY_enemy_body_enter = "enemy_body_enter";
	//public (enemy sender,Node target) PUBDATA_enemy_body_enter;
	public struct PUBDATA_enemy_body_enter
	{
		public enemy sender;
		public Node target;

		public override string ToString()
		{
			return $"sender={sender}, target={target}";
		}
	}


	public SimManager simManager;
	public PubSub.Channel<PUBDATA_enemy_body_enter> pubChannel;

	public override void _Ready()
	{

		GD.PrintErr("enemy._Ready()");

		//For triggering signals (like collision detection) we will always need to marshal into C# from Godot-Native. 
		//This PubSub is more for "once we get that first signal, all game logic stays in C#".
		// The standard way of doing it is marshaling to script, and then script marshaling back to native for sending to other scripts,
		// then marshalling back to the engine to do something. So this at least saves 2 out of 4 marshals per signal.
		this.Connect("body_entered", this, nameof(_onEnter));

		simManager = this.GetNode<SimManager>("/root/Level/SimManager");
		pubChannel = simManager.pubSub.GetChannel<PUBDATA_enemy_body_enter>(PUBKEY_enemy_body_enter);

	}

	private void _onEnter(Node node)
	{
		pubChannel.Publish(new PUBDATA_enemy_body_enter() { sender = this, target = node });

	}




}
