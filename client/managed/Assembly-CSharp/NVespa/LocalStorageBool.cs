namespace NVespa;

public class LocalStorageBool
{
	private bool _init;

	private string _key;

	private bool _value;

	public bool Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			Init();
			_value = value;
			SprkTestPrefs.SetInt(_key, _value ? 1 : 0);
		}
	}

	public LocalStorageBool(bool i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
		if (!_init)
		{
			_init = true;
			int num = SprkTestPrefs.GetInt(_key, _value ? 1 : 0);
			_value = num > 0;
		}
	}
}
