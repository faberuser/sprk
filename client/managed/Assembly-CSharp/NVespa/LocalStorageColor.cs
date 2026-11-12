using LitJson;
using UnityEngine;

namespace NVespa;

public class LocalStorageColor
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
			Init();
			if (!_value.Equals(value))
			{
				_value = value;
				SprkTestPrefs.SetString(_key, JsonTools.FromColor(_value).ToJson());
			}
		}
	}

	public LocalStorageColor(Color i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
		if (_init)
		{
			return;
		}
		_init = true;
		if (SprkTestPrefs.HasKey(_key))
		{
			string text = SprkTestPrefs.GetString(_key, string.Empty);
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				JsonData i_json = JsonMapper.ToObject(text);
				_value = JsonTools.ToColor(i_json, _value);
			}
		}
	}
}
