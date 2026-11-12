namespace NShared;

[JsonSerializable]
public class KarmaSelectCardBattleLog : IBattleLogRuntime
{
	public int SelectSlot { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.KarmaSelectCard;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		KarmaManager karmaManager = battleInstance.KarmaManager;
		if (karmaManager.Enabled)
		{
			karmaManager.SelectSlot(SelectSlot);
		}
	}
}
