namespace NShared.NOperationRuntime;

public class ModThreatGainR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.ThreatGain };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
