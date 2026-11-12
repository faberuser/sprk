namespace NShared.NOperationRuntime;

public class ModRecoverPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.RecoverPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
