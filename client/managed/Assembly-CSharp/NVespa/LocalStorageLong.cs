namespace NVespa;

public class LocalStorageLong
{
	private bool _init;

	private string _key;

	private long _value;

	public long Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			Init();
			if (_value != value)
			{
				_value = value;
				SprkTestPrefs.SetString(_key, _value.ToString());
			}
		}
	}

	public LocalStorageLong(long i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
		if (!_init)
		{
			_init = true;
			if (long.TryParse(SprkTestPrefs.GetString(_key, _value.ToString()), out var result))
			{
				_value = result;
			}
		}
	}
}
