using NShared;

public static class DamageFlag_ExtensionMethod
{
	public static bool HasFlag(this DamageFlag damageFlag, DamageFlag flag)
	{
		return (damageFlag & flag) != 0;
	}

	public static bool IsPhysical(this DamageFlag damageFlag)
	{
		return (damageFlag & DamageFlag.Physical) != 0;
	}

	public static bool IsMagical(this DamageFlag damageFlag)
	{
		return (damageFlag & DamageFlag.Magical) != 0;
	}

	public static DamageFlag CreateReflectedDamageFlag(this DamageFlag damageFlag, DamageFlag addedFlag)
	{
		DamageFlag damageFlag2 = addedFlag | DamageFlag.Reflected;
		if ((damageFlag2 & (DamageFlag.Physical | DamageFlag.Magical)) == 0)
		{
			damageFlag2 = (DamageFlag)((int)damageFlag2 | (damageFlag.IsPhysical() ? 1 : 2));
		}
		return damageFlag2;
	}
}
