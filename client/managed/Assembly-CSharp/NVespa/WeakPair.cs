using System;

namespace NVespa;

public class WeakPair<TKey, TValue> where TKey : class
{
	internal WeakReference _key;

	internal TValue _value;

	public TKey Key
	{
		get
		{
			if (_key == null)
			{
				return null;
			}
			if (!_key.IsAlive)
			{
				return null;
			}
			return _key.Target as TKey;
		}
	}

	public TValue Value => _value;
}
