namespace NShared.NOperationRuntime;

public class ModHpRatioOnWave : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpRatioOnWave };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
