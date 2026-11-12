using System.Collections.Generic;
using NShared.NArea;
using NShared.NUtil;

namespace NShared;

[JsonSerializable]
public class WaveReadyBattleLog : IBattleLogRuntime
{
	[JsonSerializable]
	public class CreatureReadyPosition
	{
		public int CreatureUid { get; set; }

		public Vector3 ReadyPosition { get; set; }
	}

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public DungeonDifficultyType Difficulty { get; set; }

	public bool Scenario { get; set; }

	public int WaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int MaxStageLevel { get; set; }

	public int TotalWaveIndex { get; set; }

	public BattleCreatureCreateInfo[] MonsterCreateInfos { get; set; }

	public BattleCreatureCreateInfo[] AllianceCreateInfos { get; set; }

	public WaveCenterPositionInfo[] WaveCenterPositionInfos { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveReady;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		SprkConquest.ApplySeed(this, battleInstance);
		battleInstance.SetChapterDungeonIndex(ChapterIndex, DungeonIndex);
		battleInstance.SetWaveIndex(WaveIndex, TotalWaveIndex);
		battleInstance.SetMaxWaveIndex(MaxWaveIndex);
		battleInstance.SetMaxStageLevel(MaxStageLevel);
		battleInstance.SetWaveCenter(WaveCenterPositionInfos);
		CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(ChapterIndex, DungeonIndex, Difficulty, Scenario, WaveIndex);
		if (data != null && data.AreaSizeMx > 0 && WaveCenterPositionInfos != null && WaveCenterPositionInfos.Length != 0)
		{
			for (int i = 0; i < WaveCenterPositionInfos.Length; i++)
			{
				WaveCenterPositionInfo waveCenterPositionInfo = WaveCenterPositionInfos[i];
				if (waveCenterPositionInfo != null)
				{
					int num = 0;
					if (waveCenterPositionInfo.GroupIndex == data.AreaGroupIndex)
					{
						num = data.AreaSizeMx;
					}
					else if (waveCenterPositionInfo.GroupIndex == data.AreaGroupIndex2)
					{
						num = data.AreaSizeMx2;
					}
					long num2 = waveCenterPositionInfo.Mx - num / 2;
					long num3 = waveCenterPositionInfo.Mx + num / 2;
					if (battleInstance.BattleArea.GetBattleAreaByGroupIndex(waveCenterPositionInfo.GroupIndex) != null)
					{
						battleInstance.BattleArea.InitMxByGroupIndex((int)num2, (int)num3, waveCenterPositionInfo.GroupIndex);
					}
				}
			}
		}
		else
		{
			ExtendArea battleAreaByGroupIndex = battleInstance.BattleArea.GetBattleAreaByGroupIndex(0);
			if (battleAreaByGroupIndex == null)
			{
				battleInstance.BattleArea.InitByGroupIndex(int.MinValue, int.MaxValue, int.MinValue, int.MaxValue, 0);
			}
			else if (battleAreaByGroupIndex != null && battleAreaByGroupIndex.MinMx == battleAreaByGroupIndex.MaxMx)
			{
				battleInstance.BattleArea.InitMxByGroupIndex(int.MinValue, int.MaxValue, 0);
			}
		}
		if (MonsterCreateInfos != null)
		{
			BattleCreatureCreateInfo[] monsterCreateInfos = MonsterCreateInfos;
			foreach (BattleCreatureCreateInfo createInfo in monsterCreateInfos)
			{
				battleInstance.CreatureContainer.Create(0L, createInfo);
			}
		}
		if (AllianceCreateInfos != null)
		{
			BattlePlayer playerByTeam = battleInstance.PlayerContainer.GetPlayerByTeam(TeamType.Left);
			BattleCreatureCreateInfo[] monsterCreateInfos = AllianceCreateInfos;
			foreach (BattleCreatureCreateInfo createInfo2 in monsterCreateInfos)
			{
				battleInstance.CreatureContainer.Create(playerByTeam.Id, createInfo2);
			}
		}
		if (WaveCenterPositionInfos == null || WaveCenterPositionInfos.Length == 0)
		{
			return;
		}
		TeamType[] array = new TeamType[2]
		{
			TeamType.Left,
			TeamType.Right
		};
		foreach (TeamType teamType in array)
		{
			for (int k = 0; k < WaveCenterPositionInfos.Length; k++)
			{
				WaveCenterPositionInfo waveCenterPositionInfo2 = WaveCenterPositionInfos[k];
				if (waveCenterPositionInfo2 == null)
				{
					continue;
				}
				BattleCreatureList aliveCreaturesOfTeam = battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(teamType, waveCenterPositionInfo2.GroupIndex);
				if (aliveCreaturesOfTeam != null && aliveCreaturesOfTeam.Count != 0)
				{
					List<Vector3> waveReadyPosition = BattleModeHelper.GetWaveReadyPosition(waveCenterPositionInfo2.WaveCenter, aliveCreaturesOfTeam.Count, teamType, battleInstance.GetBattleFormation());
					battleInstance.BattleArea.GetBattleAreaByGroupIndex(waveCenterPositionInfo2.GroupIndex)?.YawCreaturePositionList(waveReadyPosition);
					aliveCreaturesOfTeam = BattleCreatureContainer.SortCreatureByWavePosition(aliveCreaturesOfTeam);
					int num4 = 0;
					for (int l = 0; l < aliveCreaturesOfTeam.Count; l++)
					{
						BattleCreature battleCreature = aliveCreaturesOfTeam[l];
						Vector3 waveCenter = waveCenterPositionInfo2.WaveCenter;
						waveCenter = ((battleCreature.CreateInfo.OffsetMx == 0 && battleCreature.CreateInfo.OffsetMz == 0) ? waveReadyPosition[num4++] : new Vector3(waveCenterPositionInfo2.Mx + battleCreature.CreateInfo.OffsetMx, 0L, waveCenterPositionInfo2.Mz + battleCreature.CreateInfo.OffsetMz));
						battleInstance.BattleArea.ValidateByGroupIndex(ref waveCenter, battleCreature.CreateInfo.GroupIndex);
						battleCreature.TransformController.SetPosition(waveCenter, raiseEvent: true, SetPosition.SetPositionMethodType.WaveReady, 0L);
					}
				}
			}
		}
		battleInstance.OnWaveReadyBattle();
	}
}
