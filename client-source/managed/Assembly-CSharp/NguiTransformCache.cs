using NVespa;
using UnityEngine;

public class NguiTransformCache
{
	private bool _hasChanged;

	private Matrix4x4Fast _localToWorld;

	private Matrix4x4Fast _worldToLocal;

	public bool hasChanged
	{
		get
		{
			Refresh();
			return _hasChanged;
		}
		set
		{
			Refresh();
			_hasChanged = value;
		}
	}

	public Transform Value { get; private set; }

	public Transform parent => Value.parent;

	public Vector3 localPosition
	{
		get
		{
			return Value.localPosition;
		}
		set
		{
			Value.localPosition = value;
		}
	}

	public Quaternion localRotation => Value.localRotation;

	public Vector3 localScale
	{
		get
		{
			return Value.localScale;
		}
		set
		{
			Value.localScale = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return Value.position;
		}
		set
		{
			Value.position = value;
		}
	}

	public Quaternion rotation => Value.rotation;

	public Vector3 lossyScale => Value.lossyScale;

	public Matrix4x4 localToWorldMatrix => Value.localToWorldMatrix;

	public Matrix4x4 worldToLocalMatrix => Value.worldToLocalMatrix;

	public NguiTransformCache(Transform tm)
	{
		Value = tm;
		Setup();
	}

	public Vector3 TransformPoint(Vector3 v)
	{
		Refresh();
		return _localToWorld.MultiplyPoint3x4(v);
	}

	public Vector3 TransformPoint(float x, float y, float z)
	{
		Refresh();
		return _localToWorld.MultiplyPoint3x4(new Vector3(x, y, z));
	}

	public Vector3 InverseTransformPoint(Vector3 v)
	{
		Refresh();
		return _worldToLocal.MultiplyPoint3x4(v);
	}

	private void Refresh()
	{
		if (Value.hasChanged)
		{
			Value.hasChanged = false;
			_hasChanged = true;
			Setup();
		}
	}

	private void Setup()
	{
		_localToWorld.Reset(Value.localToWorldMatrix);
		_worldToLocal.Reset(Value.worldToLocalMatrix);
	}
}
