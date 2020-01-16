using Godot;
using System;
using System.Linq;
using static Godot.GD;
//using static Godot.Mathf;

using System.Collections.Concurrent;




public unsafe class Steve : KinematicBody
{


	public Vector3 velocity;//no need to ctor for structs = new Vector3()
	public static readonly float SPEED = 6;
	public static readonly float ACCELERATION = 5;

	public static readonly float GRAVITY = 9.8f;

	/// <summary>
	/// needed because very slow velocity messes up things like Node.Rotate()
	/// </summary>
	public static readonly float MINVELOCITY = 0.0001f;

	public static readonly float EPSILON = 0.00001f;// float.Epsilon * 2;


	private bool canJump = false;
	public override void _PhysicsProcess(float delta)
	{



		/// <summary>
		/// store our fall speed (gravity) because this will be destroyed by our motion computations...
		/// </summary>
		var fallSpeed = this.velocity.y;

		//! use a shorter/simpler way to control character than shown in video
		var motion = new Vector3()
		{
			x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
			z = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
		};

		if (motion.Length() > 1)
		{
			//clamp to length 1 (otherwise pressing diaganol would give you speedup)
			motion = motion.Normalized();
		}

		motion.z *= -1; //reverse z axis movement
		motion *= SPEED;

		//fake momentuym (speed up and down)
		if (motion.Length() > 0)
		{
			this.velocity = this.velocity.LinearInterpolate(motion, ACCELERATION * delta);
		}
		else
		{
			this.velocity = this.velocity.LinearInterpolate(Vector3.Zero, ACCELERATION * delta);
		}

		//restore gravity
		this.velocity.y = fallSpeed;

		//apply jump
		if (Input.IsActionPressed("ui_select") && canJump == true)
		{
			canJump = false;
			this.velocity.y += 5;// Input.GetActionStrength("ui_select") * 10;
		}


		//dampen miniscule velocities
		if (Mathf.Abs(this.velocity.x) < EPSILON)
		{
			this.velocity.x = 0;
		}
		if (Mathf.Abs(this.velocity.y) < EPSILON)
		{
			this.velocity.y = 0;
		}
		if (Mathf.Abs(this.velocity.z) < EPSILON)
		{
			this.velocity.z = 0;
		}

		//
		if (this.velocity.Length() > MINVELOCITY)
		{

			var velForRotations = this.velocity;
			velForRotations.y = 0;
			if (velForRotations.LengthSquared() > EPSILON) //if we are moving along the gameplay plane, simulate rotation....
			{
				var cross = velForRotations.Cross(Vector3.Up);
				var axis = cross.Normalized();
				this.GetNode<MeshInstance>("MeshInstance").Rotate(axis, Mathf.Deg2Rad(-velForRotations.Length()));
			}

			this.velocity.y -= GRAVITY * delta;
			var tryVel = this.velocity;
			this.velocity = this.MoveAndSlide(this.velocity, Vector3.Up);  //this.MoveAndCollide(motion * delta);

			if (canJump == false && velocity.y < 2)
			{
				for (var i = 0; i < this.GetSlideCount(); i++)
				{
					var col = this.GetSlideCollision(i);
					//GD.Print($"i={i} col={col.Normal}");
					//				GD.Print($"vel={this.velocity}, tryVel={tryVel}, col={col}");
					if ((col.Normal.y / col.Normal.Length()) > EPSILON)
					{
						//GD.Print($"ALLOW JUMP!  y=${col.Normal.y}, e=${EPSILON}");
						canJump = true;
					}
				}
			}



		}


	}

}
