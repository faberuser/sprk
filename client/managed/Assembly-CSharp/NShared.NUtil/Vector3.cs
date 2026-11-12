using System;
using UnityEngine;

namespace NShared.NUtil;

[JsonSerializable(ManualField = true, Serialize = true)]
public struct Vector3 : IComparable, IComparable<Vector3>, IEquatable<Vector3>, IFormattable
{
	public long mx;

	public long my;

	public long mz;

	public static readonly Vector3 origin = new Vector3(0L, 0L, 0L);

	public static readonly Vector3 zero = new Vector3(0L, 0L, 0L);

	public static readonly Vector3 xAxis = new Vector3(1000L, 0L, 0L);

	public static readonly Vector3 right = new Vector3(1000L, 0L, 0L);

	public static readonly Vector3 left = new Vector3(-1000L, 0L, 0L);

	public static readonly Vector3 yAxis = new Vector3(0L, 1000L, 0L);

	public static readonly Vector3 up = new Vector3(0L, 1000L, 0L);

	public static readonly Vector3 down = new Vector3(0L, -1000L, 0L);

	public static readonly Vector3 zAxis = new Vector3(0L, 0L, 1000L);

	public static readonly Vector3 forward = new Vector3(0L, 0L, 1000L);

	public static readonly Vector3 back = new Vector3(0L, 0L, -1000L);

	private const string THREE_COMPONENTS = "Array must contain exactly three components , (x,y,z)";

	private const string NORMALIZE_0 = "Can not normalize a vector when it's magnitude is zero";

	private const string INTERPOLATION_RANGE = "Control parameter must be a value between 0 & 1";

	private const string NON_VECTOR_COMPARISON = "Cannot compare a Vector3 to a non-Vector3";

	private const string ARGUMENT_TYPE = "The argument provided is a type of ";

	private const string ARGUMENT_VALUE = "The argument provided has a value of ";

	private const string ARGUMENT_LENGTH = "The argument provided has a length of ";

	private const string NEGATIVE_MAGNITUDE = "The magnitude of a Vector3 must be a positive value, (i.e. greater than 0)";

	private const string ORAGIN_VECTOR_MAGNITUDE = "Cannot change the magnitude of Vector3(0,0,0)";

	private const string UNIT_VECTOR = "Unit vector composing of ";

	private const string POSITIONAL_VECTOR = "Positional vector composing of  ";

	private const string MAGNITUDE = " of magnitude ";

	[JsonSerializable]
	public long Mx
	{
		get
		{
			return mx;
		}
		set
		{
			mx = value;
		}
	}

	[JsonSerializable]
	public long My
	{
		get
		{
			return my;
		}
		set
		{
			my = value;
		}
	}

	[JsonSerializable]
	public long Mz
	{
		get
		{
			return mz;
		}
		set
		{
			mz = value;
		}
	}

	public long Magnitude
	{
		get
		{
			return (long)Math.Sqrt(SumComponentSqrs());
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value", value, "The magnitude of a Vector3 must be a positive value, (i.e. greater than 0)");
			}
			if (this == new Vector3(0L, 0L, 0L))
			{
				throw new ArgumentException("Cannot change the magnitude of Vector3(0,0,0)", "this");
			}
			this *= value / Magnitude;
		}
	}

	public long MagnitudeSqr => SumComponentSqrs();

	public long magnitude => Magnitude;

	public bool approximatelyZero
	{
		get
		{
			if (mx == 0L && my == 0L)
			{
				return mz == 0;
			}
			return false;
		}
	}

	public long[] Array
	{
		get
		{
			return new long[3] { mx, my, mz };
		}
		set
		{
			if (value.Length == 3)
			{
				mx = value[0];
				my = value[1];
				mz = value[2];
				return;
			}
			throw new ArgumentException("Array must contain exactly three components , (x,y,z)");
		}
	}

	public long this[int index]
	{
		get
		{
			return index switch
			{
				0 => mx, 
				1 => my, 
				2 => mz, 
				_ => throw new ArgumentException("Array must contain exactly three components , (x,y,z)", "index"), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				mx = value;
				break;
			case 1:
				my = value;
				break;
			case 2:
				mz = value;
				break;
			default:
				throw new ArgumentException("Array must contain exactly three components , (x,y,z)", "index");
			}
		}
	}

	public Vector3 normalized => Normalize(this);

	public static implicit operator UnityEngine.Vector3(Vector3 v)
	{
		return new UnityEngine.Vector3((float)v.mx / 1000f, (float)v.my / 1000f, (float)v.mz / 1000f);
	}

	public Vector3(UnityEngine.Vector3 v)
	{
		mx = (long)(v.x * 1000f);
		my = (long)(v.y * 1000f);
		mz = (long)(v.z * 1000f);
	}

	public Vector3(long x, long y, long z)
	{
		mx = x;
		my = y;
		mz = z;
	}

	public Vector3(long[] xyz)
	{
		mx = 0L;
		my = 0L;
		mz = 0L;
		Array = xyz;
	}

	public Vector3(Vector3 v1)
	{
		mx = 0L;
		my = 0L;
		mz = 0L;
		mx = v1.mx;
		my = v1.my;
		mz = v1.mz;
	}

	public static Vector3 operator +(Vector3 v1, Vector3 v2)
	{
		return new Vector3(v1.mx + v2.mx, v1.my + v2.my, v1.mz + v2.mz);
	}

	public static Vector3 operator -(Vector3 v1, Vector3 v2)
	{
		return new Vector3(v1.mx - v2.mx, v1.my - v2.my, v1.mz - v2.mz);
	}

	public static Vector3 operator *(Vector3 v1, long s2)
	{
		return new Vector3(v1.mx * s2, v1.my * s2, v1.mz * s2);
	}

	public static Vector3 operator *(long s1, Vector3 v2)
	{
		return v2 * s1;
	}

	public static Vector3 operator /(Vector3 v1, long s2)
	{
		return new Vector3(v1.mx / s2, v1.my / s2, v1.mz / s2);
	}

	public static Vector3 operator -(Vector3 v1)
	{
		return new Vector3(-v1.mx, -v1.my, -v1.mz);
	}

	public static Vector3 operator +(Vector3 v1)
	{
		return new Vector3(v1.mx, v1.my, v1.mz);
	}

	public static bool operator <(Vector3 v1, Vector3 v2)
	{
		return v1.SumComponentSqrs() < v2.SumComponentSqrs();
	}

	public static bool operator >(Vector3 v1, Vector3 v2)
	{
		return v1.SumComponentSqrs() > v2.SumComponentSqrs();
	}

	public static bool operator <=(Vector3 v1, Vector3 v2)
	{
		return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
	}

	public static bool operator >=(Vector3 v1, Vector3 v2)
	{
		return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
	}

	public static bool operator ==(Vector3 v1, Vector3 v2)
	{
		if (v1.mx == v2.mx && v1.my == v2.my)
		{
			return v1.mz == v2.mz;
		}
		return false;
	}

	public static bool operator !=(Vector3 v1, Vector3 v2)
	{
		return !(v1 == v2);
	}

	public static Vector3 CrossProduct(Vector3 v1, Vector3 v2)
	{
		return new Vector3(v1.my * v2.mz - v1.mz * v2.my, v1.mz * v2.mx - v1.mx * v2.mz, v1.mx * v2.my - v1.my * v2.mx);
	}

	public Vector3 CrossProduct(Vector3 other)
	{
		return CrossProduct(this, other);
	}

	public static long DotProduct(Vector3 v1, Vector3 v2)
	{
		return v1.mx * v2.mx + v1.my * v2.my + v1.mz * v2.mz;
	}

	public long DotProduct(Vector3 other)
	{
		return DotProduct(this, other);
	}

	public static long MixedProduct(Vector3 v1, Vector3 v2, Vector3 v3)
	{
		return DotProduct(CrossProduct(v1, v2), v3);
	}

	public long MixedProduct(Vector3 other_v1, Vector3 other_v2)
	{
		return DotProduct(CrossProduct(this, other_v1), other_v2);
	}

	public static Vector3 Normalize(Vector3 v1)
	{
		if (v1.Magnitude == 0L)
		{
			throw new DivideByZeroException("Can not normalize a vector when it's magnitude is zero");
		}
		long num = v1.Magnitude;
		return new Vector3(v1.mx * 1000 / num, v1.my * 1000 / num, v1.mz * 1000 / num);
	}

	public void Normalize()
	{
		this = Normalize(this);
	}

	public static Vector3 Interpolate(Vector3 v1, Vector3 v2, int controlM, bool allowExtrapolation)
	{
		if (!allowExtrapolation && (controlM > 1000 || controlM < 0))
		{
			throw new ArgumentOutOfRangeException("control", controlM, "Control parameter must be a value between 0 & 1\nThe argument provided has a value of " + controlM);
		}
		return new Vector3((v1.mx * (1000 - controlM) + v2.mx * controlM) / 1000, (v1.my * (1000 - controlM) + v2.my * controlM) / 1000, (v1.mz * (1000 - controlM) + v2.mz * controlM) / 1000);
	}

	public static Vector3 Interpolate(Vector3 v1, Vector3 v2, int controlM)
	{
		return Interpolate(v1, v2, controlM, allowExtrapolation: false);
	}

	public Vector3 Interpolate(Vector3 other, int controlM)
	{
		return Interpolate(this, other, controlM);
	}

	public Vector3 Interpolate(Vector3 other, int controlM, bool allowExtrapolation)
	{
		return Interpolate(this, other, controlM);
	}

	public static long Distance(Vector3 v1, Vector3 v2)
	{
		return (long)Math.Sqrt(DistanceSqr(v1, v2));
	}

	public static long DistanceSqr(Vector3 v1, Vector3 v2)
	{
		return (v1.mx - v2.mx) * (v1.mx - v2.mx) + (v1.my - v2.my) * (v1.my - v2.my) + (v1.mz - v2.mz) * (v1.mz - v2.mz);
	}

	public static long DistanceSqrWithoutY(Vector3 v1, Vector3 v2)
	{
		return (v1.mx - v2.mx) * (v1.mx - v2.mx) + (v1.mz - v2.mz) * (v1.mz - v2.mz);
	}

	public static long DistanceWithoutY(Vector3 v1, Vector3 v2)
	{
		return (long)Math.Sqrt(DistanceSqrWithoutY(v1, v2));
	}

	public long Distance(Vector3 other)
	{
		return Distance(this, other);
	}

	public static int AngleM(Vector3 v1, Vector3 v2)
	{
		return Angle(v1, v2) * 1000;
	}

	public static int Angle(Vector3 v1, Vector3 v2)
	{
		return (int)(Math.Acos((double)Normalize(v1).DotProduct(Normalize(v2)) / 1000000.0) * 180.0 / Math.PI);
	}

	public int AngleM(Vector3 other)
	{
		return AngleM(this, other);
	}

	public static Vector3 Max(Vector3 v1, Vector3 v2)
	{
		if (v1 >= v2)
		{
			return v1;
		}
		return v2;
	}

	public Vector3 Max(Vector3 other)
	{
		return Max(this, other);
	}

	public static Vector3 Min(Vector3 v1, Vector3 v2)
	{
		if (v1 <= v2)
		{
			return v1;
		}
		return v2;
	}

	public Vector3 Min(Vector3 other)
	{
		return Min(this, other);
	}

	public static Vector3 Yaw(Vector3 v1, int degreeM)
	{
		long x = (long)((double)v1.mz * Math.Sin((double)degreeM / 1000.0)) + (long)((double)v1.mx * Math.Cos((double)degreeM / 1000.0));
		long y = v1.my;
		long z = (long)((double)v1.mz * Math.Cos((double)degreeM / 1000.0)) - (long)((double)v1.mx * Math.Sin((double)degreeM / 1000.0));
		return new Vector3(x, y, z);
	}

	public void Yaw(int degreeM)
	{
		this = Yaw(this, degreeM);
	}

	public void YawPoint(Vector3 point, int radian)
	{
		mx -= point.mx;
		mz -= point.mz;
		long num = (long)((double)mz * Math.Sin((double)radian / 1000.0)) + (long)((double)mx * Math.Cos((double)radian / 1000.0)) + point.mx;
		long num2 = (long)((double)mz * Math.Cos((double)radian / 1000.0)) - (long)((double)mx * Math.Sin((double)radian / 1000.0)) + point.mz;
		mx = num;
		mz = num2;
	}

	public static Vector3 Pitch(Vector3 v1, int degreeM)
	{
		long x = v1.mx;
		long y = (long)((double)v1.my * Math.Cos((double)degreeM / 1000.0)) - (long)((double)v1.mz * Math.Sin((double)degreeM / 1000.0));
		long z = (long)((double)v1.my * Math.Sin((double)degreeM / 1000.0)) + (long)((double)v1.mz * Math.Cos((double)degreeM / 1000.0));
		return new Vector3(x, y, z);
	}

	public void Pitch(int degreeM)
	{
		this = Pitch(this, degreeM);
	}

	public static Vector3 Roll(Vector3 v1, int degreeM)
	{
		long x = (long)((double)v1.mx * Math.Cos((double)degreeM / 1000.0)) - (long)((double)v1.my * Math.Sin((double)degreeM / 1000.0));
		long y = (long)((double)v1.mx * Math.Sin((double)degreeM / 1000.0)) + (long)((double)v1.my * Math.Cos((double)degreeM / 1000.0));
		long z = v1.mz;
		return new Vector3(x, y, z);
	}

	public void Roll(int degreeM)
	{
		this = Roll(this, degreeM);
	}

	public static long Abs(Vector3 v1)
	{
		return v1.Magnitude;
	}

	public long Abs()
	{
		return Magnitude;
	}

	public static long SumComponents(Vector3 v1)
	{
		return v1.mx + v1.my + v1.mz;
	}

	public long SumComponents()
	{
		return SumComponents(this);
	}

	public static long SumComponentSqrs(Vector3 v1)
	{
		return SqrComponents(v1).SumComponents();
	}

	public long SumComponentSqrs()
	{
		return SumComponentSqrs(this);
	}

	public static Vector3 SqrtComponents(Vector3 v1)
	{
		return new Vector3((long)Math.Sqrt(v1.mx), (long)Math.Sqrt(v1.my), (long)Math.Sqrt(v1.mz));
	}

	public void SqrtComponents()
	{
		this = SqrtComponents(this);
	}

	public static Vector3 SqrComponents(Vector3 v1)
	{
		return new Vector3(v1.mx * v1.mx, v1.my * v1.my, v1.mz * v1.mz);
	}

	public void SqrComponents()
	{
		this = SqrtComponents(this);
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToVerbString()
	{
		string text = null;
		text = ((!IsUnitVector()) ? (text + "Positional vector composing of  ") : (text + "Unit vector composing of "));
		text += $"( x={mx}, y={my}, z={mz} )";
		return text + " of magnitude " + Magnitude;
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (format == null || format == "")
		{
			return $"({mx}, {my}, {mz})";
		}
		char c = format[0];
		string text = null;
		if (format.Length > 1)
		{
			text = format.Substring(1);
		}
		return c switch
		{
			'x' => mx.ToString(text, formatProvider), 
			'y' => my.ToString(text, formatProvider), 
			'z' => mz.ToString(text, formatProvider), 
			_ => $"({mx.ToString(format, formatProvider)}, {my.ToString(format, formatProvider)}, {mz.ToString(format, formatProvider)})", 
		};
	}

	public override int GetHashCode()
	{
		return (int)((mx + my + mz) % int.MaxValue);
	}

	public override bool Equals(object other)
	{
		if (other is Vector3)
		{
			return (Vector3)other == this;
		}
		return false;
	}

	public bool Equals(Vector3 other)
	{
		return other == this;
	}

	public int CompareTo(Vector3 other)
	{
		if (this < other)
		{
			return -1;
		}
		if (this > other)
		{
			return 1;
		}
		return 0;
	}

	public int CompareTo(object other)
	{
		if (other is Vector3)
		{
			return CompareTo((Vector3)other);
		}
		throw new ArgumentException("Cannot compare a Vector3 to a non-Vector3\nThe argument provided is a type of " + other.GetType().ToString(), "other");
	}

	public static bool IsUnitVector(Vector3 v1)
	{
		return v1.Magnitude == 1000;
	}

	public bool IsUnitVector()
	{
		return IsUnitVector(this);
	}

	public static bool IsBackFace(Vector3 normal, Vector3 lineOfSight)
	{
		return normal.DotProduct(lineOfSight) < 0;
	}

	public bool IsBackFace(Vector3 lineOfSight)
	{
		return IsBackFace(this, lineOfSight);
	}

	public static bool IsPerpendicular(Vector3 v1, Vector3 v2)
	{
		return v1.DotProduct(v2) == 0;
	}

	public bool IsPerpendicular(Vector3 other)
	{
		return IsPerpendicular(this, other);
	}
}
