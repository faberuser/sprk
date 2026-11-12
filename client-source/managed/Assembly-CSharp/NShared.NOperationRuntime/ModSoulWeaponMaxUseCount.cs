namespace NShared.NOperationRuntime;

public class ModSoulWeaponMaxUseCount : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SoulWeaponMaxUseCount };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
