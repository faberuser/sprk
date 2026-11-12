using System;

namespace NShared;

public abstract class StatValue<T> : IStatValue
{
	protected T _value;

	public abstract long GetValue();

	public abstract void SetValue(long value);

	Type IStatValue.GetValueType()
	{
		return typeof(T);
	}

	long IStatValue.GetValue()
	{
		return GetValue();
	}

	void IStatValue.SetValue(long value)
	{
		SetValue(value);
	}
}
