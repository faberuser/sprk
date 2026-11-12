using System;

namespace NShared;

[Flags]
public enum SkillCcFlag
{
	None = 0,
	IgnoreImmune = 1,
	IgnoreResist = 2
}
