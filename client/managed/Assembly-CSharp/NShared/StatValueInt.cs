using System;

namespace NShared;

public class StatValueInt : StatValue<int>
{
	private static readonly Random _random = new Random(DateTime.UtcNow.Millisecond);

	private readonly int _obf;

	public StatValueInt(int value)
	{
		_obf = _random.Next();
		SetValue(value);
	}

	public override long GetValue()
	{
		return _value ^ _obf;
	}

	public override void SetValue(long value)
	{
		_value = (int)value ^ _obf;
	}
}
