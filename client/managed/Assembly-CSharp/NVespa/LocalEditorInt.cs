namespace NVespa;

public class LocalEditorInt
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
			if (_value != value)
			{
				_value = value;
			}
		}
	}

	public LocalEditorInt(int i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
