namespace NShared.NOperationRuntime;

public class ModHpOnWave : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpOnWave };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
