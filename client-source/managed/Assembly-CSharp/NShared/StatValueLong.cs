using System;

namespace NShared;

public class StatValueLong : StatValue<long>
{
	private static readonly Random _random = new Random(DateTime.UtcNow.Millisecond);

	private readonly long _obf;

	public StatValueLong(long value)
	{
		_obf = (long)_random.Next() * (long)_random.Next();
		SetValue(value);
	}

	public override long GetValue()
	{
		return _value ^ _obf;
	}

	public override void SetValue(long value)
	{
		_value = value ^ _obf;
	}
}
