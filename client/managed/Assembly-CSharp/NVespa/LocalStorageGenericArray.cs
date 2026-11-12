using System;
using System.Collections.Generic;

namespace NVespa;

internal class LocalStorageGenericArray<T>
{
	private string _keyPrefix;

	private T _default;

	private Dictionary<long, T> _valueDic = new Dictionary<long, T>();

	private Func<string, T, T> _getFunc;

	private Action<string, T> _setAction;

	private Func<T, T, bool> _equalFunc;

	public T this[long index]
	{
		get
		{
			T value = _default;
			if (_valueDic.TryGetValue(index, out value))
			{
				return value;
			}
			value = Vespa_System_Action_Extension_Method.fire(_getFunc, _keyPrefix + index, _default);
			_valueDic[index] = value;
			return value;
		}
		set
		{
			if (!_valueDic.TryGetValue(index, out var value2) || !Vespa_System_Action_Extension_Method.fire(_equalFunc, value2, value))
			{
				Vespa_System_Action_Extension_Method.fire(_setAction, _keyPrefix + index, value);
				_valueDic[index] = value;
			}
		}
	}

	public int Count
	{
		get
		{
			if (_valueDic != null)
			{
				return _valueDic.Count;
			}
			return 0;
		}
	}

	public LocalStorageGenericArray(T i_default, string i_keyPrefix, Func<string, T, T> getFunc, Action<string, T> setAction, Func<T, T, bool> equalFunc)
	{
		_keyPrefix = i_keyPrefix;
		_default = i_default;
		_getFunc = getFunc;
		_setAction = setAction;
		_equalFunc = equalFunc;
	}
}
