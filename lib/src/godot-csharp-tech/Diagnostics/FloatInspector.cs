using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace godot_csharp_tech.Diagnostics
{


	/// <summary>
	/// allows extraction of the Sign, Exponent, and Mantissa from a float.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	struct FloatInspector
	{
		[FieldOffset(0)]
		public float val;


		[FieldOffset(0)]
		public int bitfield;


		// public FloatInspector(float value)
		// {
		// 	this = new FloatInspector();
		// 	val = value;
		// }

		public int Sign
		{
			get
			{
				return bitfield >> 31;
			}
			set
			{
				bitfield |= value << 31;
			}
		}
		public int Exponent
		{
			get
			{
				return (bitfield & 0x7f800000) >> 23;
				// var e = bitfield & 0x7f800000;
				// e >>= 23;
				// return e;
			}
			set
			{
				bitfield |= (value << 23) & 0x7f800000;
			}
		}

		public int Mantissa
		{
			get
			{
				return bitfield & 0x007fffff;
			}
			set
			{
				bitfield |= value & 0x007fffff;
			}
		}

		public override string ToString()
		{
			var floatBytes = BitConverter.GetBytes(this.val);
			var bitBytes = BitConverter.GetBytes(this.bitfield);
			var floatBytesStr = string.Join(",", floatBytes.Select(b => b.ToString("X")));
			var bitBytesStr = string.Join(",", bitBytes.Select(b => b.ToString("X")));


			//return String.Format("FLOAT={0:G9} RT={0:R} FIELD={1:G},  HEXTEST={1:X8}, fBytes={2}, bBytes={3}", this.val, this.bitfield, floatBytesStr, bitBytesStr); //HEXTEST={1:X}, 
			return String.Format("FLOAT={0:G9} BITFIELD={1:G},  HEX={1:X}, S={2}, E={3:X}, M={4:X}", this.val, this.bitfield, this.Sign, this.Exponent, this.Mantissa); //HEXTEST={1:X}, 
		}


	}

}
