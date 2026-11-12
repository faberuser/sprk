namespace NVespa;

public class LocalStorageFloat
{
	private bool _init;

	private string _key;

	private float _value;

	public float Value
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
				SprkTestPrefs.SetFloat(_key, _value);
			}
		}
	}

	public LocalStorageFloat(float i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
		if (!_init)
		{
			_init = true;
			_value = SprkTestPrefs.GetFloat(_key, _value);
		}
	}
}
