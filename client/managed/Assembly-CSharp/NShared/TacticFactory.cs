namespace NShared;

public class TacticFactory
{
	public static IBattleTactic Create(CampaignTacticData tacticData)
	{
		if (tacticData == null)
		{
			return null;
		}
		return Create(tacticData.TacticType);
	}

	private static IBattleTactic Create(BattleTacticType type)
	{
		return type switch
		{
			BattleTacticType.BossCcGauge => new BossCcGauge(), 
			BattleTacticType.TimerSkill => new TimerSkill(), 
			_ => null, 
		};
	}
}
