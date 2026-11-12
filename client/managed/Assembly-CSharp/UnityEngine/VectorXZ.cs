using System;

namespace UnityEngine;

[Serializable]
public struct VectorXZ
{
	public const float kEpsilon = 1E-05f;

	public float x;

	public float z;

	public static readonly VectorXZ one = new VectorXZ(1f, 1f);

	public static readonly VectorXZ right = new VectorXZ(1f, 0f);

	public static readonly VectorXZ up = new VectorXZ(0f, 1f);

	public static readonly VectorXZ zero = new VectorXZ(0f, 0f);

	public Vector2 vec2
	{
		get
		{
			return new Vector2(x, z);
		}
		set
		{
			x = value.x;
			z = value.y;
		}
	}

	public float magnitude => vec2.magnitude;

	public VectorXZ normalized => new VectorXZ
	{
		vec2 = vec2.normalized
	};

	public float sqrMagnitude => vec2.sqrMagnitude;

	public VectorXZ(float i_x, float i_z)
	{
		x = i_x;
		z = i_z;
	}

	public static VectorXZ operator -(VectorXZ a)
	{
		return new VectorXZ(0f - a.x, 0f - a.z);
	}

	public static VectorXZ operator -(VectorXZ a, VectorXZ b)
	{
		return new VectorXZ(a.x - b.x, a.z - b.z);
	}

	public static bool operator !=(VectorXZ lhs, VectorXZ rhs)
	{
		return lhs.vec2 != rhs.vec2;
	}

	public static VectorXZ operator *(float d, VectorXZ a)
	{
		return new VectorXZ(a.x * d, a.z * d);
	}

	public static VectorXZ operator *(VectorXZ a, float d)
	{
		return new VectorXZ(a.x * d, a.z * d);
	}

	public static VectorXZ operator /(VectorXZ a, float d)
	{
		return new VectorXZ(a.x / d, a.z / d);
	}

	public static VectorXZ operator +(VectorXZ a, VectorXZ b)
	{
		return new VectorXZ(a.x + b.x, a.z + b.z);
	}

	public static bool operator ==(VectorXZ lhs, VectorXZ rhs)
	{
		return lhs.vec2 == rhs.vec2;
	}

	public static float Angle(VectorXZ from, VectorXZ to)
	{
		return Vector2.Angle(from.vec2, to.vec2);
	}

	public static VectorXZ ClampMagnitude(VectorXZ vector, float maxLength)
	{
		return new VectorXZ
		{
			vec2 = Vector2.ClampMagnitude(vector.vec2, maxLength)
		};
	}

	public static float Distance(VectorXZ a, VectorXZ b)
	{
		return Vector2.Distance(a.vec2, b.vec2);
	}

	public static float Dot(VectorXZ lhs, VectorXZ rhs)
	{
		return Vector2.Dot(lhs.vec2, rhs.vec2);
	}

	public override bool Equals(object other)
	{
		if (!(other is VectorXZ vectorXZ))
		{
			return false;
		}
		if (x.Equals(vectorXZ.x))
		{
			return z.Equals(vectorXZ.z);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return vec2.GetHashCode();
	}

	public static VectorXZ Lerp(VectorXZ from, VectorXZ to, float t)
	{
		return new VectorXZ(Mathf.Lerp(from.x, to.x, t), Mathf.Lerp(from.z, to.z, t));
	}

	public static VectorXZ Max(VectorXZ lhs, VectorXZ rhs)
	{
		return new VectorXZ(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.z, rhs.z));
	}

	public static VectorXZ Min(VectorXZ lhs, VectorXZ rhs)
	{
		return new VectorXZ(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.z, rhs.z));
	}

	public static VectorXZ MoveTowards(VectorXZ current, VectorXZ target, float maxDistanceDelta)
	{
		return new VectorXZ
		{
			vec2 = Vector2.MoveTowards(current.vec2, target.vec2, maxDistanceDelta)
		};
	}

	public void Set(float new_x, float new_z)
	{
		x = new_x;
		z = new_z;
	}

	public float SqrMagnitude()
	{
		return vec2.SqrMagnitude();
	}

	public static float SqrMagnitude(VectorXZ a)
	{
		return Vector2.SqrMagnitude(a.vec2);
	}

	public override string ToString()
	{
		return vec2.ToString();
	}

	public string ToString(string format)
	{
		return vec2.ToString(format);
	}

	public Vector3 ToVec3()
	{
		return new Vector3(x, 0f, z);
	}

	public Vector3 ToVec3(float i_y)
	{
		return new Vector3(x, i_y, z);
	}
}
