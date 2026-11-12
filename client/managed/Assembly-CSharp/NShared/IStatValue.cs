using System;

namespace NShared;

public interface IStatValue
{
	Type GetValueType();

	long GetValue();

	void SetValue(long value);
}
