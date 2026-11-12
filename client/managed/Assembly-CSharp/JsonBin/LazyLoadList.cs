using System.Collections.Generic;

namespace JsonBin;

public struct LazyLoadList<T> where T : class, ILoadJson, new()
{
	private static object _lockObj = new object();

	private List<T> _value;

	private JsonRead _jsonRead;

	public List<T> Value
	{
		get
		{
			if (_value != null)
			{
				return _value;
			}
			if (!_jsonRead.IsValid)
			{
				return null;
			}
			lock (_lockObj)
			{
				if (_value != null)
				{
					return _value;
				}
				_value = new List<T>();
				_value.LoadListFromJson(_jsonRead);
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
