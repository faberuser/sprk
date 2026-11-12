using System;

namespace NShared;

public class SkillDisablerTypeAttribute : Attribute
{
	public bool DisableAutoUse { get; set; }

	private static SkillDisablerTypeAttribute GetOperationAttribute(SkillDisablerType disablerType)
	{
		return EnumAttributeContainer<SkillDisablerType, SkillDisablerTypeAttribute>.Get(disablerType);
	}

	public static bool IsDisableAutoUse(SkillDisablerType disablerType)
	{
		return GetOperationAttribute(disablerType)?.DisableAutoUse ?? false;
	}
}
