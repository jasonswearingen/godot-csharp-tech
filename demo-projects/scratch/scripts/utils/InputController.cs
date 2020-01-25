using Godot;
using System;


internal class _MouseInputHandler : Node
{

}


//this simple code works if you need a basic camera
//    //side-by-side:
//    //this.GlobalRotate(up, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity);
//    this.Rotate(up, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity); //use fixed up, not transforms up
//    //up-down:
//    this.Rotate(right, Mathf.Deg2Rad(lookInput.z) * delta * lookSensitivity);
// the following code is a more complex solution allowing looking "past" the up vector
public class InputController : Spatial
{
	[Export]
	public Vector2 lookSensitivityMouse = Vector2.One;

	/// <summary>
	/// set to true to use a tradtional gimbal camera.  no roll, but restricts camera from looking past straight up/down
	/// </summary>
	[Export]
	public bool useGimbalCamera = false;
	/// <summary>
	/// if useGimbalCamera==true, how many degrees to restrict camera view angles when looking up or down.  useful to prevent artifacts (camera flipping direction)
	/// </summary>
	public float gimcamDeadzoneDeg = 0.5f;

	private Vector3 _up = Vector3.Up;


	public Vector2 lookInputKeyboard;
	public override void _Ready()
	{
	}

	[Export]
	public bool freecamRemoveRoll = true;


	public float rollMaxStabilizePerSecond = Mathf.Pi * 2 * 2;
	public float rollStabilizeSpeedFactor = 10f;
	public float rollSnapAngle = 0.1f;


	private Vector3 _freeCamRemoveRollHelper(float delta, ref Vector3 localUp)
	{
		if (freecamRemoveRoll != true)
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

		//if (angle < rollSnapAngle)
		//{
		//	newUp = _up;
		//}

		//newUp = localUp;


	//	GD.Print(" localUp =", localUp.ToString("F2")
	//, " angle=", angle.ToString("F2")
	//, " appliedAngle=", appliedAngle.ToString("F2")
	//, " upDiff=", upDiff.ToString("F2")
	//, " lenDiff=", lenDiff.ToString("F2")
	//, " newUp=", newUp.ToString("F2"));

		return newUp;

	}
	public override void _Process(float delta)
	{
		//get transform components:  https://community.khronos.org/t/get-direction-from-transformation-matrix-or-quat/65502/2
		var xform = Transform;
		var localRight = Transform.basis.Column0;
		var localUp = Transform.basis.Column1;
		if (useGimbalCamera == true)
		{
			localUp = _up;
		}
		
		var localFwd = Transform.basis.Column2; //alt approach:  localFwd = this.Transform.Xform(Vector3.Forward) - Transform.origin;
		var localPos = Transform.origin;
#if DEBUG
		var scale = Transform.basis.Scale;
		if (scale.IsEqualApprox(Vector3.One) == false)
		{
			throw new Exception($"scale={scale:F2}.  non 1 scale is unexpected.  need to refactor the basis components to account for this.");
		}
#endif

		//this is actually the "backwards" vector of the matrix.  kinda dumb.
		var localLookDir = localFwd * -1;


		var lookInput = lookInputKeyboard + lookInputMouse;
		lookInput *= -1; // have to flip because our "look at" vector is actually behind, not in front.  dumb but ok
		lookInputMouse = Vector2.Zero;  //clear out mouse when done reading it, as it doesn't auto-clear like keyboard input.



		if (freecamRemoveRoll == true)
		{
			var newUp = _freeCamRemoveRollHelper(delta, ref localUp);
			Transform = Transform.LookingAt(localLookDir + xform.origin, newUp);
		}


		if (lookInput == Vector2.Zero)
		{
		}
		else
		{


			var localIsNormalized = false;
			if (localRight.IsNormalized() && localUp.IsNormalized() && localFwd.IsNormalized())
			{
				localIsNormalized = true;
			}
			GD.Print("localIsNormalized=", localIsNormalized);

			//var newUp = localUp;
			//if (rollStabilize == true)
			//{
			//	newUp = _stabalizeUpHelper(delta, ref localUp);
			//}
					   
			//look horizontal (left-right) == lookInput.X
			var targetHoriz = localLookDir.Cross(localUp);
			var adjustHoriz = targetHoriz * (lookInput.x * delta * lookSensitivityMouse.x);

			//look vertical (up-down) == lookInput.Y
			var targetVert = localLookDir.Cross(localRight);
			var adjustVert = targetVert * (lookInput.y * delta * lookSensitivityMouse.y);


					   
			var newTarget = localLookDir + adjustHoriz + adjustVert;

			/////////////////////////
			//adjust gimbal camera properties
			if (useGimbalCamera == true)
			{

				//ensure that the gimbal camera isn't pointing too far up or down
				{
					//helper to check if input is moving the angle too far up/down
					bool _isOverGimbalMaxAngle(ref Vector3 _target, ref Vector3 _up, float _minAngle)
					{
						var angle = Mathf.Rad2Deg(_target.AngleTo(_up));
						if (angle < _minAngle || angle > 180 - _minAngle)
						{
							return true;
						}
						return false;
					}
					if (_isOverGimbalMaxAngle(ref newTarget, ref _up, gimcamDeadzoneDeg))
					{
						//over max angle, so try only horiz
						newTarget = localLookDir + adjustHoriz;
						if (_isOverGimbalMaxAngle(ref newTarget, ref _up, gimcamDeadzoneDeg))
						{
							//over max angle, so try only vert components
							newTarget = localLookDir + adjustVert;
							if (_isOverGimbalMaxAngle(ref newTarget, ref _up, gimcamDeadzoneDeg))
							{
								//over max angle, so don't set.
								newTarget = localLookDir;
							}
						}
					}
				}
			}


			if (newTarget.LengthSquared() > 1)
			{
				newTarget = newTarget.Normalized();
			}
			localLookDir = newTarget;





			var xformTarget = localLookDir + xform.origin;



			//xform.origin = Vector3.Zero;
			Transform = Transform.LookingAt(xformTarget, localUp);







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

	public Vector2 lookInputMouse;
	public override void _UnhandledInput(InputEvent input)
	{
		base._UnhandledInput(input);

		//! use a shorter/simpler way to control character than shown in video
		lookInputKeyboard = new Vector2()
		{
			x = Input.GetActionStrength("ui_left") - Input.GetActionStrength("ui_right"),
			//z = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
			y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
		};

		//handle mouse input
		if (input is InputEventMouseMotion)
		{
			var mouse = input as InputEventMouseMotion;
			var relMove = mouse.Relative;
			GD.Print($"relMove={relMove.ToString("F2")}");
			lookInputMouse = -mouse.Relative;
		}


		//don't let movement exceed "1" length (but allow less)
		if (lookInputKeyboard.LengthSquared() > 1f)
		{
			lookInputKeyboard = lookInputKeyboard.Normalized();
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
