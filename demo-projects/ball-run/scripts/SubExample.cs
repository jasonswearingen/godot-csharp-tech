using Godot;
using System;

using System.Collections.Concurrent;

public class SubExample : Node
{

	public ConcurrentQueue<enemy.PUBDATA_enemy_body_enter> SUB_enemyBodyEnter = new ConcurrentQueue<enemy.PUBDATA_enemy_body_enter>();
	public SimManager simManager;
	public unsafe override void _Ready()
	{

		simManager = this.GetNode<SimManager>("/root/Level/SimManager");
		simManager.pubSub.GetChannel<enemy.PUBDATA_enemy_body_enter>(enemy.PUBKEY_enemy_body_enter).Subscribe(SUB_enemyBodyEnter);

	}

	public override void _Process(float delta)
	{
		var count = 0;
		while (SUB_enemyBodyEnter.TryDequeue(out var data))
		{
			count++;
			//GD.Print($"{count} steve got sub from enemy= {data}");
		}

	}

}
