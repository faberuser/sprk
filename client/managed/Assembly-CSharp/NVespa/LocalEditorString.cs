namespace NVespa;

public class LocalEditorString
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
			if (!(_value == value))
			{
				_value = value;
			}
		}
	}

	public LocalEditorString(string i_key)
	{
		_key = i_key;
		_value = string.Empty;
	}

	public LocalEditorString(string i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
