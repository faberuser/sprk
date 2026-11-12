using System;

namespace NShared;

[Flags]
public enum EKarmaRarity
{
	None = 1,
	Rare = 2,
	Epic = 4,
	Ancient = 8,
	Legend = 0x10
}
