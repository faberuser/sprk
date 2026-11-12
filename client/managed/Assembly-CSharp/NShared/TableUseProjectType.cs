using System;

namespace NShared;

[Flags]
public enum TableUseProjectType
{
	None = 0,
	BattleServer = 1,
	GmsClient = 2,
	QueryEditor = 4,
	All = int.MaxValue
}
