namespace godot_csharp_tech.nodes
{
	/////////////////////////////////////////
	///Input Controller, allows for keyboard and mouse inputs
	///example use:  attach a camera as a child node
	///There are two camera types.   traditional "gimbal" camera (done), and a freeCam (experimental) that can look past vertical, and still corrects for roll.
	///this is still a work in progress.
	///
	///
	///references for researching later:
	///https://github.com/Sombresonge/Third-Person-Controller/blob/master/ThirdPersonController/Controller.gd
	///https://github.com/tavurth/godot-simple-fps-camera/blob/master/Camera.gd
	///https://gamedev.stackexchange.com/questions/136174/im-rotating-an-object-on-two-axes-so-why-does-it-keep-twisting-around-the-thir
	///https://github.com/mrdev023/Godot-Orbit-Camera/blob/master/addons/orbit_camera/orbit_camera.gd
	///https://github.com/Goutte/godot-trackball-camera/blob/master/addons/goutte.camera.trackball/trackball_camera.gd
	///https://github.com/TWew/CADLikeOrbit_Camera/blob/master/CADLikeOrbit_Camera.gd
	///https://www.gamedev.net/tutorials/programming/math-and-physics/a-simple-quaternion-based-camera-r1997/









	using Godot;
	using System;



	//this simple code works if you need a basic camera
	//    //side-by-side:
	//    //this.GlobalRotate(up, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity);
	//    this.Rotate(up, Mathf.Deg2Rad(lookInput.x) * delta * lookSensitivity); //use fixed up, not transforms up
	//    //up-down:
	//    this.Rotate(right, Mathf.Deg2Rad(lookInput.z) * delta * lookSensitivity);
	// the following code is a more complex solution allowing looking "past" the up vector
	[Tool]
	public class InputController : Spatial
	{

		/// <summary>
		/// set to true to use a tradtional gimbal camera.  no roll, but restricts camera from looking past straight up/down
		/// </summary>
		[Export]
		public bool useGimbalCamera = false;
		/// <summary>
		/// if useGimbalCamera==true, how many degrees to restrict camera view angles when looking up or down.  useful to prevent artifacts (camera flipping direction)
		/// </summary>
		public float gimcamDeadzoneDeg = 1.0f;

		private Vector3 _worldUp = Vector3.Up;

		private _MouseInputHandler mouseInput;
		private _KeyboardInputHandler keyboardInput;

		/// <summary>
		/// node you want controlled by this inputController.   if not set, defaults to itself.
		/// </summary>
		public Spatial nodeToControl;
		public override void _Ready()
		{
			mouseInput = new _MouseInputHandler();
			AddChild(mouseInput);
			keyboardInput = new _KeyboardInputHandler();
			AddChild(keyboardInput);
			if(nodeToControl == null)
			{
				nodeToControl = this;
			}
		}

		[Export]
		public bool freecamRemoveRoll = true;


		//public float rollMaxStabilizePerSecond = Mathf.Pi * 2 * 2;
		//public float rollStabilizeSpeedFactor = 10f;
		//public float rollSnapAngle = 0.1f;


		[Export]
		float freecamRollFixDegPerSec = 360;

		[Export]
		float freecamRollFixAcceleration = 2;

		[Export]
		float freecamPoleFreeZoneDeg = 30f;

		private Vector3 _freeCamRemoveRollHelper(float delta, ref Vector3 localUp, ref Vector3 targetUp, ref Vector3 lookDir)
		{
			if (freecamRemoveRoll != true)
			{
				return localUp;
			}





			var cross = localUp.Cross(targetUp);
			var angle = localUp.AngleTo(targetUp);



			var lookAngleToTargetUp = lookDir.AngleTo(targetUp);

			var freeZoneUp = Mathf.Deg2Rad(freecamPoleFreeZoneDeg);
			var freeZoneDown = Mathf.Deg2Rad(180 - freecamPoleFreeZoneDeg);

			if (lookDir.z > 0)
			{
				//looking backwards
				if (lookAngleToTargetUp < freeZoneUp || lookAngleToTargetUp > freeZoneDown)
				{
					return localUp;
				}
			}




			var angleToFix = (Mathf.Deg2Rad(freecamRollFixDegPerSec) + (angle * freecamRollFixAcceleration)) * delta;

			if (angleToFix > angle)
			{
				return targetUp;
			}
			//angleToFix = Mathf.Min(angleToFix, angle);

			var toReturn = localUp.Rotated(cross, angleToFix);

			return toReturn;



		}
		public override void _Process(float delta)
		{

			

			//get transform components:  https://community.khronos.org/t/get-direction-from-transformation-matrix-or-quat/65502/2
			//var xform = Transform;
			var localRight = nodeToControl.Transform.basis.Column0;
			var localUp = nodeToControl.Transform.basis.Column1;

			var selectedUp = useGimbalCamera ? _worldUp : localUp;

			var localFwd = nodeToControl.Transform.basis.Column2; //alt approach:  localFwd = this.Transform.Xform(Vector3.Forward) - Transform.origin;
			var localPos = nodeToControl.Transform.origin;
#if DEBUG
			var scale = nodeToControl.Transform.basis.Scale;
			if (scale.IsEqualApprox(Vector3.One) == false)
			{
				throw new Exception($"scale={scale:F2}.  non 1 scale is unexpected.  need to refactor the basis components to account for this.");
			}
			if (localRight.IsNormalized() && localUp.IsNormalized() && localFwd.IsNormalized())
			{
			}
			else
			{
				throw new Exception($"transform not normalized.={nodeToControl.Transform.ToString("F2")}.  need to refactor the basis components to account for this.");
			}
#endif

			//this is actually the "backwards" vector of the matrix.  kinda dumb.
			var localLookDir = localFwd * -1;

			_ProcessHelper_Look(delta, localUp, ref selectedUp, localPos, ref localLookDir);

			///////////////////////
			//MOVEMENT

			var moveInput = keyboardInput.moveState * delta;
			//move after look, because our move code changes localPos, which would mess up look code if we did that first.
			if (moveInput != Vector3.Zero)
			{
				localPos += localRight * moveInput.x;
				localPos += localLookDir * moveInput.z;
				localPos += localUp * moveInput.y;
				var tmpTransform = nodeToControl.Transform;
				tmpTransform.origin = localPos;
				nodeToControl.Transform = tmpTransform;
			}

		}

		private void _ProcessHelper_Look(float delta, Vector3 localUp, ref Vector3 selectedUp, Vector3 localPos, ref Vector3 localLookDir)
		{
			var lookInput = keyboardInput.lookState + mouseInput.lastMouseMovement;
			//lookInput *= -1; // have to flip because our "look at" vector is actually behind, not in front.  dumb but ok
			mouseInput.lastMouseMovement = Vector2.Zero;  //clear out mouse when done reading it, as it doesn't auto-clear like keyboard input.



			if (lookInput == Vector2.Zero)
			{
				if (freecamRemoveRoll == true)
				{
					var newUp = _freeCamRemoveRollHelper(delta, ref selectedUp, ref _worldUp, ref localLookDir);
					var tmpTransform = nodeToControl.Transform.LookingAt(localLookDir + localPos, newUp);
					//tmpTransform.origin = localPos;
					nodeToControl.Transform = tmpTransform;
					//Transform.SetLookAt(localPos, localLookDir + localPos, newUp);
					//Transform = new Transform(localRight, newUp, localLookDir *-1, localPos);
				}
			}
			else
			{

				////////////////////////////////////////
				//// ========= HANDLE MOVEMENT


				//HORIZONTAL:  need to get a new right vector from our "targetUp", 
				//because gimbalCamera's up isn't our local Transform's up.
				//if we used local transform up while in gimbalCam mode, you would rotate faster towards the poles (bad)
				var selectedRight = localLookDir.Cross(selectedUp);
				var adjustHoriz = selectedRight * (lookInput.x * delta);

				//VERTICAL:  we can just use our local transform's up/
				//if we were in gimbal mode and used worldup, we'd get slower as we move towards the poles (bad)
				var adjustVert = localUp * lookInput.y * delta;

				//GD.Print($"localRight2 {targetRight.ToString("F2")}  localRight {localRight.ToString("F2")}  localDown {localDown.ToString("F2")}  localUp {localUp.ToString("F2")}");



				var targetLookDir = localLookDir + adjustHoriz + adjustVert;

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
						if (_isOverGimbalMaxAngle(ref targetLookDir, ref selectedUp, gimcamDeadzoneDeg))
						{
							//over max angle, so try only horiz
							targetLookDir = localLookDir + adjustHoriz;
							if (_isOverGimbalMaxAngle(ref targetLookDir, ref selectedUp, gimcamDeadzoneDeg))
							{
								//over max angle, so try only vert components
								targetLookDir = localLookDir + adjustVert;
								if (_isOverGimbalMaxAngle(ref targetLookDir, ref selectedUp, gimcamDeadzoneDeg))
								{
									//over max angle, so don't set.
									targetLookDir = localLookDir;
								}
							}
						}
					}
				}


				if (targetLookDir.LengthSquared() > 1)
				{
					targetLookDir = targetLookDir.Normalized();
				}

				var targetLookPosition = targetLookDir + localPos;

				if (freecamRemoveRoll == true)
				{
					selectedUp = _freeCamRemoveRollHelper(delta, ref selectedUp, ref _worldUp, ref targetLookDir);
				}

				var tmpTransform = nodeToControl.Transform.LookingAt(targetLookPosition, selectedUp);
				//tmpTransform.origin = localPos;
				nodeToControl.Transform = tmpTransform;

				//Transform = new Transform(selectedRight, selectedUp, targetLookDir * -1, localPos);
				//Transform.SetLookAt(localPos, targetLookPosition, selectedUp);





			}
		}

		[Tool]
		public class _MouseInputHandler : Node
		{

			[Export]
			public Vector2 lookSensitivityMouse = Vector2.One;

			[Export]
			public bool captureMouse = true;

			/// <summary>
			/// after reading this, you should set it to zero  (not automatically reset when input stops)
			/// 
			/// relative to Vector3.forward  (0,0,-1)
			/// right = +x
			/// up = +y
			/// </summary>
			public Vector2 lastMouseMovement;

			public override void _Ready()
			{
				if (captureMouse && Engine.EditorHint==false) 
				{
					Input.SetMouseMode(Input.MouseMode.Captured);
				}
			}

			public override void _Process(float delta)
			{


			}
			public override void _UnhandledInput(InputEvent input)
			{

				base._UnhandledInput(input);

				if (Engine.EditorHint)
				{
					return;
				}

				if (input.IsActionPressed("global_mouse_capture"))
				{
					if (Input.GetMouseMode() == Input.MouseMode.Captured)
					{
						Input.SetMouseMode(Input.MouseMode.Visible);
					}
					else
					{
						Input.SetMouseMode(Input.MouseMode.Captured);
					}
				}

				//handle mouse input
				if (input is InputEventMouseMotion)
				{
					var mouse = input as InputEventMouseMotion;
					var relMove = mouse.Relative;
					//GD.Print($"relMove={relMove.ToString("F2")}");
					relMove.y *= -1;  //mouse flips up/down
					relMove *= lookSensitivityMouse;
					lastMouseMovement = relMove;
				}
			}

		}

		[Tool]
		public class _KeyboardInputHandler : Node
		{

			/// <summary>
			/// current state of inputs to "game_look_x" and "game_look_y"  inputs
			/// right = +x
			/// up = +y
			/// </summary>
			public Vector2 lookState;
			public Vector3 moveState;



			[Export]
			public float moveSensitivity = 10f;//0.01f;
			public override void _Ready()
			{

				



				//InputMap.AddAction("p1_move_fwd");
				//InputMap.ActionAddEvent("p1_move_fwd", new InputEventKey() { Scancode = 'w' });


			}

			public override void _Process(float delta)
			{


			}

			public override void _UnhandledInput(InputEvent input)
			{
				base._UnhandledInput(input);

				if (Engine.EditorHint)
				{
					return;
				}


				//! use a shorter/simpler way to control character than shown in video
				lookState = new Vector2()
				{
					x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left"),
					y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
				};

				////don't let movement exceed "1" length (but allow less)
				//if (lookInputKeyboard.LengthSquared() > 1f)
				//{
				//	lookInputKeyboard = lookInputKeyboard.Normalized();
				//}

				moveState = new Vector3()
				{
					x = Input.GetActionStrength("p1_move_right") - Input.GetActionStrength("p1_move_left"),
					z = Input.GetActionStrength("p1_move_forward") - Input.GetActionStrength("p1_move_backward"),
					y = Input.GetActionStrength("p1_move_up") - Input.GetActionStrength("p1_move_down"),
				};
				moveState *= (1 + Input.GetActionStrength("p1_move_fast")) * moveSensitivity;
			}



		}


	}
}