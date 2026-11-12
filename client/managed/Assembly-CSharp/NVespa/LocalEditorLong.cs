namespace NVespa;

public class LocalEditorLong
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
			if (_value != value)
			{
				_value = value;
			}
		}
	}

	public LocalEditorLong(long i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
