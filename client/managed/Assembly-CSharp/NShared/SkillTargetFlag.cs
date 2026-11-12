using System;

namespace NShared;

[Flags]
public enum SkillTargetFlag
{
	None = 0,
	ExcludeBoss = 1,
	IgnoreRangeCheckOnFire = 2,
	ExcludePrevFiredTarget = 4,
	FilterOnCheckOnly = 8
}
