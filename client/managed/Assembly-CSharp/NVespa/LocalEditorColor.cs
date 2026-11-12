using UnityEngine;

namespace NVespa;

public class LocalEditorColor
{
	private bool _init;

	private string _key;

	private Color _value;

	public Color Value
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

	public LocalEditorColor(Color i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
