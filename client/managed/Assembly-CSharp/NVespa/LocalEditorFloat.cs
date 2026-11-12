namespace NVespa;

public class LocalEditorFloat
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
			if (_value != value)
			{
				_value = value;
			}
		}
	}

	public LocalEditorFloat(float i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
