using System;

namespace NVespa;

public class LocalStorageDateTime
{
	private bool _init;

	private string _key;

	private DateTime _value;

	public DateTime Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			_value = value;
			SprkTestPrefs.SetString(_key, Vespa_DateTime_Extension_Method.ToNetworkString(_value));
		}
	}

	public LocalStorageDateTime(DateTime i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	private void Init()
	{
		if (!_init)
		{
			_init = true;
			if (Vespa_String_Extension_Method2.TryParseNetworkTime(SprkTestPrefs.GetString(_key, string.Empty), out var o_dateTime))
			{
				_value = o_dateTime;
			}
		}
	}
}
