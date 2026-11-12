using NShared;

public static class RightMenuButtonType_Extension
{
	public static int GetEffectIndex(this ERightMenuButton type)
	{
		return EnumAttributeContainer<ERightMenuButton, RightMenuButtonAttribute>.Get(type)?.EffectIndex ?? (-1);
	}
}
