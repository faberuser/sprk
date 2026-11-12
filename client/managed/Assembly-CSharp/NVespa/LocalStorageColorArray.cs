using LitJson;
using UnityEngine;

namespace NVespa;

public class LocalStorageColorArray
{
	private NVespa.LocalStorageGenericArray<Color> _inner;

	public Color this[long index]
	{
		get
		{
			return _inner[index];
		}
		set
		{
			_inner[index] = value;
		}
	}

	public LocalStorageColorArray(Color i_default, string i_keyPrefix)
	{
		_inner = new NVespa.LocalStorageGenericArray<Color>(i_default, i_keyPrefix, GetLocalValue, SetLocalValue, EqualValue);
	}

	private Color GetLocalValue(string key, Color defaultValue)
	{
		if (!SprkTestPrefs.HasKey(key))
		{
			return defaultValue;
		}
		string text = SprkTestPrefs.GetString(key);
		if (String_Extension_Method.IsNullOrEmpty(text))
		{
			return defaultValue;
		}
		return JsonTools.ToColor(JsonMapper.ToObject(text), defaultValue);
	}

	private void SetLocalValue(string key, Color value)
	{
		SprkTestPrefs.SetString(key, JsonTools.FromColor(value).ToJson());
	}

	private bool EqualValue(Color lhs, Color rhs)
	{
		return lhs.Equals(rhs);
	}
}
