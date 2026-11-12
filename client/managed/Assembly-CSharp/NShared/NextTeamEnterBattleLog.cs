using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

[JsonSerializable]
public class NextTeamEnterBattleLog : IBattleLogRuntime
{
	public long PlayerId { get; set; }

	public BattleCreatureCreateInfo[] CreateInfos { get; set; }

	public WaveCenterPositionInfo[] WaveCenterPositionInfos { get; set; }

	public int[] PrevTeamUidList { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.NextTeamEnterBattleLog;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		if (PrevTeamUidList != null)
		{
			int[] prevTeamUidList = PrevTeamUidList;
			foreach (int creatureUid in prevTeamUidList)
			{
				battleInstance.CreatureContainer.GetCreature(creatureUid).OnBeginResting(raiseDestroyEvent: true);
				battleInstance.CreatureContainer.Destroy(creatureUid, raiseEvent: true);
			}
		}
		if (CreateInfos == null)
		{
			VespaDebug.LogError("NextTeamEntereBattleLog - CreateInfos is null.");
			return;
		}
		if (WaveCenterPositionInfos == null || WaveCenterPositionInfos.Length == 0)
		{
			VespaDebug.LogError("NextTeamEntereBattleLog - WaveCenterPositionInfos is null or size zero.");
			return;
		}
		WaveCenterPositionInfo[] waveCenterPositionInfos = WaveCenterPositionInfos;
		foreach (WaveCenterPositionInfo waveCenterPositionInfo in waveCenterPositionInfos)
		{
			BattleCreatureList aliveCreaturesOfTeam = battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Right, waveCenterPositionInfo.GroupIndex);
			aliveCreaturesOfTeam = BattleCreatureContainer.SortCreatureByWavePosition(aliveCreaturesOfTeam);
			List<Vector3> waveReadyPosition = BattleModeHelper.GetWaveReadyPosition(waveCenterPositionInfo.WaveCenter, aliveCreaturesOfTeam.Count, TeamType.Right);
			int num = 0;
			for (int j = 0; j < aliveCreaturesOfTeam.Count; j++)
			{
				BattleCreature battleCreature = aliveCreaturesOfTeam[j];
				Vector3 waveCenter = waveCenterPositionInfo.WaveCenter;
				waveCenter = ((battleCreature.CreateInfo.OffsetMx == 0 && battleCreature.CreateInfo.OffsetMz == 0) ? waveReadyPosition[num++] : new Vector3(waveCenterPositionInfo.Mx + battleCreature.CreateInfo.OffsetMx, 0L, waveCenterPositionInfo.Mz + battleCreature.CreateInfo.OffsetMz));
				battleInstance.BattleArea.ValidateByGroupIndex(ref waveCenter, battleCreature.CreateInfo.GroupIndex);
				battleCreature.TransformController.SetPosition(waveCenter, raiseEvent: true, SetPosition.SetPositionMethodType.Spawn, 0L);
			}
		}
		BattleCreatureCreateInfo[] createInfos = CreateInfos;
		foreach (BattleCreatureCreateInfo createInfo in createInfos)
		{
			battleInstance.CreatureContainer.Create(PlayerId, createInfo);
		}
		waveCenterPositionInfos = WaveCenterPositionInfos;
		foreach (WaveCenterPositionInfo waveCenterPositionInfo2 in waveCenterPositionInfos)
		{
			BattleCreatureList aliveCreaturesOfTeam2 = battleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamType.Left, waveCenterPositionInfo2.GroupIndex);
			if (aliveCreaturesOfTeam2 != null && aliveCreaturesOfTeam2.Count != 0)
			{
				List<Vector3> waveReadyPosition2 = BattleModeHelper.GetWaveReadyPosition(waveCenterPositionInfo2.WaveCenter, aliveCreaturesOfTeam2.Count, TeamType.Left);
				aliveCreaturesOfTeam2 = BattleCreatureContainer.SortCreatureByWavePosition(aliveCreaturesOfTeam2);
				int num2 = 0;
				for (int k = 0; k < aliveCreaturesOfTeam2.Count; k++)
				{
					BattleCreature battleCreature2 = aliveCreaturesOfTeam2[k];
					Vector3 waveCenter2 = waveCenterPositionInfo2.WaveCenter;
					waveCenter2 = ((battleCreature2.CreateInfo.OffsetMx == 0 && battleCreature2.CreateInfo.OffsetMz == 0) ? waveReadyPosition2[num2++] : new Vector3(waveCenterPositionInfo2.Mx + battleCreature2.CreateInfo.OffsetMx, 0L, waveCenterPositionInfo2.Mz + battleCreature2.CreateInfo.OffsetMz));
					Vector3 position = waveCenter2 + Vector3.left * 15L;
					battleInstance.BattleArea.ValidateByGroupIndex(ref position, battleCreature2.CreateInfo.GroupIndex);
					battleCreature2.TransformController.SetPosition(position, raiseEvent: true, SetPosition.SetPositionMethodType.Spawn, 0L);
					battleInstance.BattleArea.ValidateByGroupIndex(ref waveCenter2, battleCreature2.CreateInfo.GroupIndex);
					battleCreature2.TransformController.SetPosition(waveCenter2, raiseEvent: true, SetPosition.SetPositionMethodType.WaveReady, 0L);
				}
			}
		}
		battleInstance.OnWaveReadyBattle();
	}
}
