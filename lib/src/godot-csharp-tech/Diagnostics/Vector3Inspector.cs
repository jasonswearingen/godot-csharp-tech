using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace godot_csharp_tech.Diagnostics
{

	[StructLayout(LayoutKind.Explicit)]
	struct Vector3Inspector
	{
		[FieldOffset(0)]
		public Vector3 val;
		[FieldOffset(0)]
		public FloatInspector x;
		[FieldOffset(4)]
		public FloatInspector y;
		[FieldOffset(8)]
		public FloatInspector z;

		public override string ToString()
		{

			return String.Format("x=[{0}], \n\ty=[{1}], \n\tz=[{2}]", x, y, z);


		}

	}
}
