namespace NShared.NOperationRuntime;

public class ModMpOnWave : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnWave };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
