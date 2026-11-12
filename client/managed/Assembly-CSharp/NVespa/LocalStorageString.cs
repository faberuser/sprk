namespace NVespa;

public class LocalStorageString
{
	private bool _init;

	private string _key;

	private string _value;

	public string Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			Init();
			if (!(_value == value))
			{
				_value = value;
				SprkTestPrefs.SetString(_key, _value);
			}
		}
	}

	public LocalStorageString(string i_key)
	{
		_key = i_key;
		_value = string.Empty;
	}

	public LocalStorageString(string i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
		if (!_init)
		{
			_init = true;
			_value = SprkTestPrefs.GetString(_key, _value);
		}
	}

	public void Remove()
	{
		SprkTestPrefs.DeleteKey(_key);
	}
}
