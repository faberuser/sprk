using System;

namespace NShared;

[Flags]
public enum StatAttributeFlag
{
	None = 0,
	Positive = 1,
	Negative = 2,
	Percent = 4,
	AllowNegative = 8
}
