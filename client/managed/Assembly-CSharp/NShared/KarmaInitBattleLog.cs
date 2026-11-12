namespace NShared;

[JsonSerializable]
public class KarmaInitBattleLog : IBattleLogRuntime
{
	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.KarmaInit;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.KarmaManager.InitFilterClassTag();
		battleInstance.KarmaManager.InitSelectableCardList();
	}
}
