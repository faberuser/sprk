using System;

namespace NShared;

[Flags]
public enum DamageMeterFlag
{
	None = 0,
	ShowAi = 1,
	ShowAllPlayer = 2,
	ShowAll = 4
}
