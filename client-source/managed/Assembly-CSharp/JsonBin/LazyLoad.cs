namespace JsonBin;

public struct LazyLoad<T> where T : class, ILoadJson, new()
{
	private static object _lockObj = new object();

	private T _value;

	private JsonRead _jsonRead;

	public T Value
	{
		get
		{
			if (_value != null)
			{
				return _value;
			}
			lock (_lockObj)
			{
				if (_value != null)
				{
					return _value;
				}
				_value = new T();
				if (_jsonRead.IsValid)
				{
					_value.Load(_jsonRead);
				}
				return _value;
			}
		}
	}

	public void SetJsonRead(JsonRead jsonRead)
	{
		_jsonRead = jsonRead;
		_value = null;
	}
}
