namespace NVespa;

public class LocalEditorEnum<T> where T : struct
{
	private bool _init;

	private string _key;

	private T _value;

	public T Value
	{
		get
		{
			Init();
			return _value;
		}
		set
		{
			if (!(EnumTools.GetName(_value) == EnumTools.GetName(value)))
			{
				_value = value;
			}
		}
	}

	public LocalEditorEnum(string i_key)
	{
		_key = i_key;
		_value = default(T);
	}

	public LocalEditorEnum(T i_default, string i_key)
	{
		_key = i_key;
		_value = i_default;
	}

	public void Init()
	{
	}
}
