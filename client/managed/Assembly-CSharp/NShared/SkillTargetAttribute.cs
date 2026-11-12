using System;

namespace NShared;

public class SkillTargetAttribute : Attribute
{
	public bool IsFriendly;

	public bool IsRandom;

	public bool ExcludeMe;

	public bool CheckDeadTarget;
}
