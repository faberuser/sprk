using System;

namespace NVespa;

public class WeakRef<T> : IWeakRef where T : class
{
	private WeakReference _value;

	object IWeakRef.Obj => Value;

	public T Value
	{
		get
		{
			if (_value == null)
			{
				return null;
			}
			if (!_value.IsAlive)
			{
				_value = null;
				return null;
			}
			return _value.Target as T;
		}
		set
		{
			if (value == null)
			{
				_value = null;
			}
			else
			{
				_value = new WeakReference(value);
			}
		}
	}

	public WeakRef()
	{
	}

	public WeakRef(T This)
	{
		_value = new WeakReference(This);
	}
}
