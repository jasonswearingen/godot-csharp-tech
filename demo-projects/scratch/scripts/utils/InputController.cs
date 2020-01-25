using Godot;
using System;

public class InputController : Spatial
{
	// Declare member variables here. Examples:
	// private int a = 2;
	// private string b = "text";

	public float lookSensitivity = 1;

	private Vector3 _up = Vector3.Up;


	public Vector2 lookInput;
	///// <summary>
	///// this is actually the "backwards" vector of the matrix.  kinda dumb.
	///// </summary>
	//public Vector3 targetLookingAt;
	//public Vector3 worldUp = Vector3.Up;
	//public Vector3 targetUp = Vector3.Up;


	public Transform originalXform;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//var i = new Vector2(1, 1);
		//var j = new Vector2(1, 0);

		//GD.Print("project", i.Project(j));
		//GD.Print(i.Bounce(j));
		//GD.Print(i.Cross(j));
		//GD.Print(i.DirectionTo(j));
		//GD.Print(i.Dot(j));
		//GD.Print(i.Reflect(j));
		//GD.Print(i.Slide(j));
		//GD.Print(i.Tangent());

		//var x = new Vector3(1, 0, 1);
		//var y = new Vector3(1, 0, 0);
		//GD.Print(x.Cross(y));
		//GD.Print(x.DirectionTo(y));
		//GD.Print(x.Inverse());
		//GD.Print(x.MaxAxis());
		//GD.Print(x.Outer(y));
		//GD.Print(x.Project(y));
		//GD.Print(x.ToDiagonalMatrix());
		//Mathf.rad

		//targetForward = this.Transform.basis.Column2;
		//targetLookingAt = this.Transform.Xform(Vector3.Forward) * -1;





		//GD.Print("targetLookingAt", targetLookingAt, "xForward", this.Transform.Xform(Vector3.Forward),"fwd",this.Transform.basis.Column2);
		//Transform = Transform;
		originalXform = Transform;

	}

	[Export]
	public bool rollStabilize = true;

	//public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
	//{
	//    return new Vector3(
	//        vector1.y * vector2.z - vector1.z * vector2.y,
	//        vector1.z * vector2.x - vector1.x * vector2.z,
	//        vector1.x * vector2.y - vector1.y * vector2.x);
	//}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public float rollMaxStabilizePerSecond = Mathf.Pi * 2 *2;
	public float rollStabilizeSpeedFactor = 10f;
	public float rollSnapAngle = 0.1f;
	private Vector3 _stabalizeUpHelper(float delta, ref Vector3 localUp)
	{
		if (rollStabilize != true)
		{
			return localUp;
		}

		var upDiff = _up - localUp;
		//var upDiffLen = upDiff.Length() / 2;// Mathf.Clamp(upDiff.Length(), 0, 1);
		//var newUp = _up.LinearInterpolate(localUp, upDiffLen);
		var newUp = (_up + localUp) / 2;


		var angle = _up.AngleTo(localUp);
		var lenDiff = upDiff.Length();
		var cross = localUp.Cross(_up);
		//var appliedAngle = Mathf.Clamp(10 * angle * delta, 0,2* Mathf.Pi *delta);
		var appliedAngle = Mathf.Clamp(angle * rollStabilizeSpeedFactor, 0, rollMaxStabilizePerSecond) * delta;		
		newUp = localUp.Rotated(cross, appliedAngle);

		if (angle < rollSnapAngle)
		{
			newUp = _up;
		}

		//newUp = localUp;


		GD.Print(" localUp =", localUp.ToString("F2")
	, " angle=", angle.ToString("F2")
	, " appliedAngle=", appliedAngle.ToString("F2")
	, " upDiff=", upDiff.ToString("F2")
	, " lenDiff=", lenDiff.ToString("F2")
	, " newUp=", newUp.ToString("F2"));

		return newUp;

	}
	public override void _Process(float delta)
	{
		var xform = Transform;
		var localRight = Transform.basis.Column0;
		var localUp = Transform.basis.Column1;
		var localFwd = Transform.basis.Column2; //alt approach:  localFwd = this.Transform.Xform(Vector3.Forward) - Transform.origin;
		var localPos = Transform.origin;

		//this is actually the "backwards" vector of the matrix.  kinda dumb.
		var localLookingAt = localFwd * -1;


		if (lookInput == Vector2.Zero)
		{
			if (rollStabilize == true)
			{
				var	newUp = _stabalizeUpHelper(delta, ref localUp);
				Transform = Transform.LookingAt(localLookingAt + xform.origin, newUp);

			}
		}
		else
		{


			var localIsNormalized = false;
			if (localRight.IsNormalized() && localUp.IsNormalized() && localFwd.IsNormalized())
			{
				localIsNormalized = true;
			}
			GD.Print("localIsNormalized=", localIsNormalized);


			//targetForward = targetForward * -1;


			//look horizontal (left-right) == lookInput.X
			var targetHoriz = localLookingAt.Cross(localUp);
			var adjustHoriz = targetHoriz * (lookInput.x * delta * lookSensitivity);

			//look vertical (up-down) == lookInput.Y
			var targetVert = localLookingAt.Cross(localRight);
			var adjustVert = targetVert * (lookInput.y * delta * lookSensitivity);



			//GD.Print("targetForward", targetForward, "targetHoriz", targetHoriz,"adjustHoriz", adjustHoriz);

			var newTarget = localLookingAt + adjustHoriz + adjustVert;
			newTarget = newTarget.Normalized();
			localLookingAt = newTarget;





			var xformTarget = localLookingAt + xform.origin;


			var newUp = localUp;
			if (rollStabilize == true)
			{
				newUp = _stabalizeUpHelper(delta, ref localUp);
			}

			//xform.origin = Vector3.Zero;
			Transform = Transform.LookingAt(xformTarget, newUp);







			//xform.origin = Transform.origin;
			//Transform = xform;


			//var quat = new Quat(targetUp, Vector3.Forward.AngleTo(newForward));
			//var basis = new Basis()

			//Transform = new Transform(targetHoriz, targetUp, targetHoriz, Transform.origin);
			//Transform = new Transform(quat, Transform.origin);

			//var xf = Godot.Transform.Identity.LookingAt(targetForward,targetUp);
			//var xfDirection = xf.Xform(Vector3.Forward);
			//xf.origin = Transform.origin;

			//Transform = xf;

			//GD.Print("targetForward", targetLookingAt, "origin", Transform.origin, "xformTarget", xformTarget);







			//Transform.SetLookAt(Transform.origin, newForward, targetUp);


			//this.Transform = this.Transform.Rotated(targetForward.Cross(newForward), 1);

			//this.Transform = this.Transform.LookingAt(newForward, targetUp);


			//var xform = Transform.LookingAt(newForward, targetUp);

			//var right = xform.basis.Column0;
			//var up = xform.basis.Column1;
			//var fwd = xform.basis.Column2;
			//var pos = xform.origin;

			//GD.Print("fwd", fwd, "up", up, "pos", pos, "fwdX", this.Transform.Xform(Vector3.Forward));



			//this.Transform = xform;
			//targetLookingAt = newForward;





			//targetForward += lookInput * delta * lookSensitivity;
			//targetForward = targetForward.Normalized();

			//var right = targetForward.Cross(targetUp);

			//targetForward.Project
			////new up


			//var xform = Transform.LookingAt(targetForward, targetUp);
			//this.Transform = xform;



			//this.Rotate(targ, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity);
			//this.Rotate(right, Mathf.Deg2Rad(lookInput.z) * delta * lookSensitivity);


		}

		//return;
		//if (lookInput != Vector3.Zero)        
		//{   

		//    var xform = this.Transform;

		//    var right = xform.basis.Column0;
		//    var up = xform.basis.Column1;
		//    //up = _up;
		//    var fwd = xform.basis.Column2;

		//    //side-by-side:
		//    //this.GlobalRotate(up, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity);
		//    this.Rotate(up, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity);
		//    this.Rotate(right, Mathf.Deg2Rad(lookInput.z) * delta * lookSensitivity);



		//    //need to reset up
		//    //var localX = xform.Rotated(right, Mathf.Deg2Rad(lookInput.z) * delta * lookSensitivity);
		//    var localX = this.Transform;
		//    //localX.LookingAt
		//    //localX.basis.Column1 = up;  //doesn't work: corrupts xform
		//    //.

		//    this.Transform = localX;
		//    //this.Transform  basis.Column1 = up;
		//}


	}

	public override void _UnhandledInput(InputEvent input)
	{
		base._UnhandledInput(input);

		//! use a shorter/simpler way to control character than shown in video
		lookInput = new Vector2()
		{
			x = Input.GetActionStrength("ui_left") - Input.GetActionStrength("ui_right"),
			//z = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
			y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
		};
		lookInput *= -1; // have to flip because our "look at" vector is actually behind, not in front.  dumb but ok

		//don't let movement exceed "1" length (but allow less)
		if (lookInput.LengthSquared() > 1f)
		{
			lookInput = lookInput.Normalized();
		}

		//if (input.IsActionPressed("ui_down"))
		//{
		//    GD.Print("resetting xform");
		//    Transform = originalXform;
		//    lookInput = Vector3.Zero;
		//}

		//if (input.IsActionPressed("ui_up"))
		//{
		//    GD.Print("resetting xform");
		//    Transform = Godot.Transform.Identity;
		//    lookInput = Vector3.Zero;
		//}



	}
}
