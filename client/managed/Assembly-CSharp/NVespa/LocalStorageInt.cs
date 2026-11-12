namespace NVespa;

public class LocalStorageInt
{
	private bool _init;

	private string _key;

	private int _value;

	public int Value
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
				SprkTestPrefs.SetInt(_key, _value);
			}
		}
	}

	public LocalStorageInt(int i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
		if (!_init)
		{
			_init = true;
			_value = SprkTestPrefs.GetInt(_key, _value);
		}
	}

	public void Remove()
	{
		SprkTestPrefs.DeleteKey(_key);
	}
}
