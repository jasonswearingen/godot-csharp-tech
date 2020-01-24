using Godot;
using System;

public class InputController : Spatial
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public float lookSensitivity = 1;

    private Vector3 _up = Vector3.Up;


    public Vector3 lookInput;
    /// <summary>
    /// this is actually the "backwards" vector of the matrix.  kinda dumb.
    /// </summary>
    public Vector3 targetLookingAt;
    public Vector3 worldUp = Vector3.Up;
    public Vector3 targetUp = Vector3.Up;


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
        targetLookingAt = this.Transform.Xform(Vector3.Forward) * -1;


        


        GD.Print("targetLookingAt", targetLookingAt, "xForward", this.Transform.Xform(Vector3.Forward),"fwd",this.Transform.basis.Column2);
        //Transform = Transform;
        originalXform = Transform;

    }

    //public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
    //{
    //    return new Vector3(
    //        vector1.y * vector2.z - vector1.z * vector2.y,
    //        vector1.z * vector2.x - vector1.x * vector2.z,
    //        vector1.x * vector2.y - vector1.y * vector2.x);
    //}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (lookInput != Vector3.Zero)
        {
            var xform = Transform;
            var localRight = Transform.basis.Column0;
            var localUp = Transform.basis.Column1;
            var localFwd = Transform.basis.Column2;
            var localPos = Transform.origin;

            var localIsNormalized = false;
            if(localRight.IsNormalized() && localUp.IsNormalized() && localFwd.IsNormalized())
            {
                localIsNormalized = true;
            }

            //targetForward = targetForward * -1;


            //move horizontal (left-right) == lookInput.X
            var targetHoriz = targetLookingAt.Cross(targetUp);
            var adjustHoriz = targetHoriz * (lookInput.x * delta * lookSensitivity);



            //GD.Print("targetForward", targetForward, "targetHoriz", targetHoriz,"adjustHoriz", adjustHoriz);

            var newTarget = targetLookingAt + adjustHoriz;
            newTarget = newTarget.Normalized();
            targetLookingAt = newTarget;



            

            var xformTarget = targetLookingAt + xform.origin;

            //xform.origin = Vector3.Zero;
            Transform = Transform.LookingAt(xformTarget, targetUp);
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

            GD.Print("targetForward", targetLookingAt, "origin", Transform.origin, "xformTarget", xformTarget);







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
        lookInput = new Vector3()
        {
            x = Input.GetActionStrength("ui_left") - Input.GetActionStrength("ui_right"),
            //z = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
            y = Input.GetActionStrength("ui_up") - Input.GetActionStrength("ui_down"),
        };
        //don't let movement exceed "1" length (but allow less)
        if (lookInput.LengthSquared() > 1f)
        {
            lookInput = lookInput.Normalized();
        }

        if (input.IsActionPressed("ui_down"))
        {
            GD.Print("resetting xform");
            Transform = originalXform;
            lookInput = Vector3.Zero;
        }

        if (input.IsActionPressed("ui_up"))
        {
            GD.Print("resetting xform");
            Transform = Godot.Transform.Identity;
            lookInput = Vector3.Zero;
        }



    }
}