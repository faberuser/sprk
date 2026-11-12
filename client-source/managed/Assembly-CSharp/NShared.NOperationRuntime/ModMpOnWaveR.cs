namespace NShared.NOperationRuntime;

public class ModMpOnWaveR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnWave };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
