using UnityEngine;

namespace NVespa;

public class LocalEditorVector3
{
	private bool _init;

	private string _key;

	private Vector3 _value;

	public Vector3 Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			if (!_value.Equals(value))
			{
				_value = value;
			}
		}
	}

	public LocalEditorVector3(Vector3 i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
