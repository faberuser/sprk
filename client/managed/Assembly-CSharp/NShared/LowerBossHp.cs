namespace NShared;

public class LowerBossHp : IWinDecider
{
	private int _bossHpRatio;

	private WinDeciderType _winDeciderType;

	void IWinDecider.Init(WinDeciderType type, string[] values)
	{
		_bossHpRatio = ArgParser.GetValueInt(values, 0);
		_winDeciderType = type;
	}

	bool IWinDecider.Decide(BattleInstance battleInstance, out TeamType teamType)
	{
		teamType = TeamType.Left;
		if (battleInstance.IsBattleTimeEnd())
		{
			teamType = TeamType.Right;
			return true;
		}
		BattleCreature battleCreature = null;
		BattleCreatureList aliveCreatures = battleInstance.CreatureContainer.GetAliveCreatures();
		for (int i = 0; i < aliveCreatures.Count; i++)
		{
			BattleCreature battleCreature2 = aliveCreatures[i];
			if (battleCreature2 != null)
			{
				BattleCreatureCreateInfo createInfo = battleCreature2.CreateInfo;
				if (createInfo != null && createInfo.IsBoss)
				{
					battleCreature = battleCreature2;
					break;
				}
			}
		}
		if (battleCreature != null && battleCreature.StatController.HpRatioM <= _bossHpRatio)
		{
			teamType = TeamType.Left;
			return true;
		}
		if (battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Left) == 0)
		{
			teamType = TeamType.Right;
			return true;
		}
		if (battleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Right) == 0)
		{
			teamType = TeamType.Left;
			return true;
		}
		return false;
	}

	WinDeciderType IWinDecider.GetWinDeciderType()
	{
		return _winDeciderType;
	}
}
