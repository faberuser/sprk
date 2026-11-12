namespace NShared.NOperationRuntime;

public class ModThreatGain : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.ThreatGain };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
