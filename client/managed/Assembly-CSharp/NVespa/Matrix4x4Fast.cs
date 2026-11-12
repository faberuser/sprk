using UnityEngine;

namespace NVespa;

public struct Matrix4x4Fast
{
	public enum EFastType
	{
		Normal,
		ScaleAndTranslate,
		TranslateOnly,
		ScaleOnly,
		None
	}

	public Matrix4x4 Value;

	private bool _initFastType;

	private EFastType _fastType;

	public Vector3 Translate => new Vector3(Value.m03, Value.m13, Value.m23);

	public Vector3 Scale => new Vector3(Value.m00, Value.m11, Value.m22);

	public EFastType FastType
	{
		get
		{
			InitFastType();
			return _fastType;
		}
	}

	public Matrix4x4Fast(Matrix4x4 matrix)
	{
		Value = matrix;
		_initFastType = false;
		_fastType = EFastType.Normal;
	}

	public void Reset(Matrix4x4 matrix)
	{
		Value = matrix;
		_initFastType = false;
		_fastType = EFastType.Normal;
	}

	public void Reset(Matrix4x4 parent, Matrix4x4 child)
	{
		Value = parent * child;
		_initFastType = false;
		_fastType = EFastType.Normal;
	}

	public Vector3 MultiplyPoint3x4(Vector3 v)
	{
		return FastType switch
		{
			EFastType.TranslateOnly => new Vector3(v.x + Value.m03, v.y + Value.m13, v.z + Value.m23), 
			EFastType.ScaleOnly => new Vector3(v.x * Value.m00, v.y * Value.m11, v.z * Value.m22), 
			EFastType.ScaleAndTranslate => new Vector3(v.x * Value.m00 + Value.m03, v.y * Value.m11 + Value.m13, v.z * Value.m22 + Value.m23), 
			EFastType.None => v, 
			EFastType.Normal => Value.MultiplyPoint3x4(v), 
			_ => Value.MultiplyPoint3x4(v), 
		};
	}

	private void InitFastType()
	{
		if (!_initFastType)
		{
			_initFastType = true;
			SetFastType();
		}
	}

	private void SetFastType()
	{
		if (Value.m01 < -1E-07f || Value.m01 > 1E-07f || Value.m02 < -1E-07f || Value.m02 > 1E-07f || Value.m10 < -1E-07f || Value.m10 > 1E-07f || Value.m12 < -1E-07f || Value.m12 > 1E-07f || Value.m20 < -1E-07f || Value.m20 > 1E-07f || Value.m21 < -1E-07f || Value.m21 > 1E-07f || Value.m30 < -1E-07f || Value.m30 > 1E-07f || Value.m31 < -1E-07f || Value.m31 > 1E-07f || Value.m32 < -1E-07f || Value.m32 > 1E-07f || Value.m33 < 0.99999f || Value.m33 > 1.00001f)
		{
			_fastType = EFastType.Normal;
			return;
		}
		bool num = Value.m03 < -1E-07f || Value.m03 > 1E-07f || Value.m13 < -1E-07f || Value.m13 > 1E-07f || Value.m23 < -1E-07f || Value.m23 > 1E-07f;
		bool flag = Value.m00 < 0.99999f || Value.m00 > 1.00001f || Value.m11 < 0.99999f || Value.m11 > 1.00001f || Value.m22 < 0.99999f || Value.m22 > 1.00001f;
		if (num)
		{
			_fastType = (flag ? EFastType.ScaleAndTranslate : EFastType.TranslateOnly);
		}
		else
		{
			_fastType = (flag ? EFastType.ScaleOnly : EFastType.None);
		}
	}

	private void SetFastType3()
	{
		if (Value.m01 < -1E-07f || Value.m01 > 1E-07f || Value.m02 < -1E-07f || Value.m02 > 1E-07f || Value.m10 < -1E-07f || Value.m10 > 1E-07f || Value.m12 < -1E-07f || Value.m12 > 1E-07f || Value.m20 < -1E-07f || Value.m20 > 1E-07f || Value.m21 < -1E-07f || Value.m21 > 1E-07f || Value.m30 < -1E-07f || Value.m30 > 1E-07f || Value.m31 < -1E-07f || Value.m31 > 1E-07f || Value.m32 < -1E-07f || Value.m32 > 1E-07f || Value.m33 < 0.99999f || Value.m33 > 1.00001f)
		{
			_fastType = EFastType.Normal;
			return;
		}
		bool num = Value.m03 < -1E-07f || Value.m03 > 1E-07f || Value.m13 < -1E-07f || Value.m13 > 1E-07f || Value.m23 < -1E-07f || Value.m23 > 1E-07f;
		bool flag = Value.m00 < 0.99999f || Value.m00 > 1.00001f || Value.m11 < 0.99999f || Value.m11 > 1.00001f || Value.m22 < 0.99999f || Value.m22 > 1.00001f;
		if (num)
		{
			_fastType = (flag ? EFastType.ScaleAndTranslate : EFastType.TranslateOnly);
		}
		else
		{
			_fastType = (flag ? EFastType.ScaleOnly : EFastType.None);
		}
	}

	private void SetFastType2()
	{
		if (Value.m01 < -1E-07f || Value.m01 > 1E-07f || Value.m02 < -1E-07f || Value.m02 > 1E-07f || Value.m10 < -1E-07f || Value.m10 > 1E-07f || Value.m12 < -1E-07f || Value.m12 > 1E-07f || Value.m20 < -1E-07f || Value.m20 > 1E-07f || Value.m21 < -1E-07f || Value.m21 > 1E-07f || Value.m30 < -1E-07f || Value.m30 > 1E-07f || Value.m31 < -1E-07f || Value.m31 > 1E-07f || Value.m32 < -1E-07f || Value.m32 > 1E-07f || Value.m33 < 0.99999f || Value.m33 > 1.00001f)
		{
			_fastType = EFastType.Normal;
			return;
		}
		bool num = Value.m03 < -1E-07f || Value.m03 > 1E-07f || Value.m13 < -1E-07f || Value.m13 > 1E-07f || Value.m23 < -1E-07f || Value.m23 > 1E-07f;
		bool flag = Value.m00 < 0.99999f || Value.m00 > 1.00001f || Value.m11 < 0.99999f || Value.m11 > 1.00001f || Value.m22 < 0.99999f || Value.m22 > 1.00001f;
		if (num)
		{
			_fastType = (flag ? EFastType.ScaleAndTranslate : EFastType.TranslateOnly);
		}
		else
		{
			_fastType = (flag ? EFastType.ScaleOnly : EFastType.None);
		}
	}

	private void SetFastType4()
	{
		if (Value.m01 < -1E-07f || Value.m01 > 1E-07f || Value.m02 < -1E-07f || Value.m02 > 1E-07f || Value.m10 < -1E-07f || Value.m10 > 1E-07f || Value.m12 < -1E-07f || Value.m12 > 1E-07f || Value.m20 < -1E-07f || Value.m20 > 1E-07f || Value.m21 < -1E-07f || Value.m21 > 1E-07f || Value.m30 < -1E-07f || Value.m30 > 1E-07f || Value.m31 < -1E-07f || Value.m31 > 1E-07f || Value.m32 < -1E-07f || Value.m32 > 1E-07f || Value.m33 < 0.99999f || Value.m33 > 1.00001f)
		{
			_fastType = EFastType.Normal;
			return;
		}
		bool num = Value.m03 < -1E-07f || Value.m03 > 1E-07f || Value.m13 < -1E-07f || Value.m13 > 1E-07f || Value.m23 < -1E-07f || Value.m23 > 1E-07f;
		bool flag = Value.m00 < 0.99999f || Value.m00 > 1.00001f || Value.m11 < 0.99999f || Value.m11 > 1.00001f || Value.m22 < 0.99999f || Value.m22 > 1.00001f;
		if (num)
		{
			_fastType = (flag ? EFastType.ScaleAndTranslate : EFastType.TranslateOnly);
		}
		else
		{
			_fastType = (flag ? EFastType.ScaleOnly : EFastType.None);
		}
	}

	private void SetFastType_orig()
	{
		if (!Mathf.Approximately(Value.m01, 0f) || !Mathf.Approximately(Value.m02, 0f) || !Mathf.Approximately(Value.m10, 0f) || !Mathf.Approximately(Value.m12, 0f) || !Mathf.Approximately(Value.m20, 0f) || !Mathf.Approximately(Value.m21, 0f) || !Mathf.Approximately(Value.m30, 0f) || !Mathf.Approximately(Value.m31, 0f) || !Mathf.Approximately(Value.m32, 0f) || !Mathf.Approximately(Value.m33, 1f))
		{
			_fastType = EFastType.Normal;
			return;
		}
		bool num = !Mathf.Approximately(Value.m03, 0f) || !Mathf.Approximately(Value.m13, 0f) || !Mathf.Approximately(Value.m23, 0f);
		bool flag = !Mathf.Approximately(Value.m00, 1f) || !Mathf.Approximately(Value.m11, 1f) || !Mathf.Approximately(Value.m22, 1f);
		if (num)
		{
			_fastType = (flag ? EFastType.ScaleAndTranslate : EFastType.TranslateOnly);
		}
		else
		{
			_fastType = (flag ? EFastType.ScaleOnly : EFastType.None);
		}
	}
}
