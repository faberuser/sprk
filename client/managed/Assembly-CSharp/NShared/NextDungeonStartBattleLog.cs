using System.Collections.Generic;
using NShared.NUtil;

namespace NShared;

[JsonSerializable]
public class NextDungeonStartBattleLog : IBattleLogRuntime
{
	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.NextDungeonStartBattleLog;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		int difficulty = battleInstance.Difficulty;
		bool isScenario = battleInstance.IsScenario;
		battleInstance.SetCampaignDungeon(ChapterIndex, DungeonIndex, difficulty, isScenario);
		string fieldName = GameRoot.Instance.CampaignDungeonDataContainer.GetFieldName(ChapterIndex, DungeonIndex);
		CampaignFieldData data = GameRoot.Instance.CampaignFieldDataContainer.GetData(fieldName);
		if (data == null)
		{
			return;
		}
		List<BattleAreaElementData> battleAreaDataList = data.GetBattleAreaDataList();
		if (battleAreaDataList == null)
		{
			return;
		}
		foreach (BattleAreaElementData item in battleAreaDataList)
		{
			int num = ((item != null) ? ((item.AreaTopM + item.AreaBottomM) / 2) : 0);
			int num2 = -(item?.CenterBiasM ?? 0);
			Vector3 center = new Vector3(num2, 0L, num);
			BattleCreatureList aliveCreaturesOfTeam = battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Left, item.BattleAreaGroupIndex);
			List<Vector3> waveReadyPosition = BattleModeHelper.GetWaveReadyPosition(center, aliveCreaturesOfTeam.Count, TeamType.Left);
			battleInstance.BattleArea.GetBattleAreaByGroupIndex(item.BattleAreaGroupIndex)?.YawCreaturePositionList(waveReadyPosition);
			aliveCreaturesOfTeam = BattleCreatureContainer.SortCreatureByWavePosition(aliveCreaturesOfTeam);
			int num3 = 0;
			for (int i = 0; i < aliveCreaturesOfTeam.Count; i++)
			{
				BattleCreature battleCreature = aliveCreaturesOfTeam[i];
				Vector3 position = waveReadyPosition[num3++];
				battleInstance.BattleArea.ValidateByGroupIndex(ref position, battleCreature.CreateInfo.GroupIndex);
				if (battleInstance.KarmaManager.Enabled)
				{
					battleCreature.TransformController.SetPosition(position, raiseEvent: true, SetPosition.SetPositionMethodType.WaveReady, 0L);
				}
				else
				{
					battleCreature.TransformController.SetPosition(position, raiseEvent: true, SetPosition.SetPositionMethodType.DungeonChangeMove, 0L);
				}
			}
		}
	}
}
