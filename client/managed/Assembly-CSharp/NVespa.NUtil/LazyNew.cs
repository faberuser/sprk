namespace NVespa.NUtil;

public class LazyNew<T> where T : class, new()
{
	private T _value;

	public T Value
	{
		get
		{
			if (_value == null)
			{
				_value = new T();
			}
			return _value;
		}
	}
}
