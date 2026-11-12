using System;

namespace NShared;

public class StatValueShort : StatValue<short>
{
	private static readonly Random _random = new Random(DateTime.UtcNow.Millisecond);

	private readonly short _obf;

	public StatValueShort(short value)
	{
		_obf = (short)_random.Next();
		SetValue(value);
	}

	public override long GetValue()
	{
		return _value ^ _obf;
	}

	public override void SetValue(long value)
	{
		_value = (short)((short)value ^ _obf);
	}
}
