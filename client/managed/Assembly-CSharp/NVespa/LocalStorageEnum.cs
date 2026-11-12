using System;

namespace NVespa;

public class LocalStorageEnum<T> where T : struct
{
	private bool _init;

	private string _key;

	private T _value;

	public T Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			Init();
			if (!(EnumTools.GetName(_value) == EnumTools.GetName(value)))
			{
				_value = value;
				SprkTestPrefs.SetString(_key, EnumTools.GetName(_value));
			}
		}
	}

	public LocalStorageEnum(string i_key)
	{
		_key = i_key;
		_value = default(T);
	}

	public LocalStorageEnum(T i_default, string i_key)
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
		try
		{
			string str = SprkTestPrefs.GetString(_key, EnumTools.GetName(_value));
			_value = EnumTools.Parse(str, default(T));
		}
		catch (Exception)
		{
			_value = default(T);
		}
	}
}
