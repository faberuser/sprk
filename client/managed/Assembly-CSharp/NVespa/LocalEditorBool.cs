namespace NVespa;

public class LocalEditorBool
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
			_value = value;
		}
	}

	public LocalEditorBool(bool i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
