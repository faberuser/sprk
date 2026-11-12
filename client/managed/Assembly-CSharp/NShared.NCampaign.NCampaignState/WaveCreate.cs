using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NCampaign.Karma;
using NShared.NUtil;

namespace NShared.NCampaign.NCampaignState;

public class WaveCreate : CampaignState
{
	private readonly List<BattleLog> _enterPlayerBattleLogList = new List<BattleLog>();

	public void SetInitialBattleLog(IList<IBattleLogRuntime> enterPlayerBattleLogs, int randomSeedByMonth, int[] projectileGroupIndicesFromServer)
	{
		if (enterPlayerBattleLogs == null)
		{
			return;
		}
		string fieldName = GameRoot.Instance.CampaignDungeonDataContainer.GetFieldName(_updater.ChapterIndex, _updater.DungeonIndex);
		CampaignFieldData data = GameRoot.Instance.CampaignFieldDataContainer.GetData(fieldName);
		if (data == null)
		{
			return;
		}
		List<BattleAreaElementData> battleAreaDataList = data.GetBattleAreaDataList();
		if (battleAreaDataList == null || battleAreaDataList.Count == 0)
		{
			return;
		}
		CampaignWaveData data2 = GameRoot.Instance.CampaignWaveDataContainer.GetData(_updater.ChapterIndex, _updater.DungeonIndex, _updater.Difficulty, _updater.Scenario, _updater.WaveIndex);
		foreach (IBattleLogRuntime enterPlayerBattleLog in enterPlayerBattleLogs)
		{
			if (!(enterPlayerBattleLog is PlayerEnterBattleLog { CreatureCreateInfos: var creatureCreateInfos } playerEnterBattleLog))
			{
				continue;
			}
			if (creatureCreateInfos != null)
			{
				List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
				for (int i = 0; i < battleAreaDataList.Count; i++)
				{
					BattleAreaElementData battleAreaData = battleAreaDataList[i];
					if (battleAreaData == null)
					{
						continue;
					}
					List<BattleCreatureCreateInfo> list2 = new List<BattleCreatureCreateInfo>();
					list2.AddRange(Array.FindAll(creatureCreateInfos, (BattleCreatureCreateInfo info) => info.GroupIndex == battleAreaData.BattleAreaGroupIndex));
					if (list2.Count != 0)
					{
						if (data2 != null)
						{
							AddAllyWaveMember(data2, battleAreaData.BattleAreaGroupIndex, list2);
						}
						int num = (battleAreaData.AreaTopM + battleAreaData.AreaBottomM) / 2;
						int centerBiasM = battleAreaData.CenterBiasM;
						TeamType teamType = ((playerEnterBattleLog.PlayerCreateInfo != null) ? playerEnterBattleLog.PlayerCreateInfo.TeamType : TeamType.Left);
						int num2 = ((teamType == TeamType.Left) ? (-centerBiasM) : centerBiasM);
						Vector3 center = new Vector3(num2, 0L, num);
						int angle = battleAreaData.Angle;
						if (angle != 0)
						{
							int waveAreaMaxMx = GetWaveAreaMaxMx(data2, battleAreaData.BattleAreaGroupIndex);
							int yAxisRadian = (int)((double)angle * Math.PI / 180.0 * 1000.0);
							Vector3 standardPos = new Vector3(waveAreaMaxMx, 0L, num);
							BattleCreatureContainer.SortHeroSpawnPosition(list2, teamType, center, standardPos, yAxisRadian);
						}
						else
						{
							BattleCreatureContainer.SortHeroSpawnPosition(list2, teamType, center);
						}
						list.AddRange(list2);
					}
				}
				if (data2 != null && data2.AddHeroExtraSkill != null && data2.AddHeroExtraSkill.Length != 0)
				{
					AddHeroExtraSkill(list, data2);
				}
				playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
			}
			BattleLog item = _updater.BattleInstance.BattleLogContainer.AddLog(enterPlayerBattleLog);
			_enterPlayerBattleLogList.Add(item);
		}
		BattleLog item2 = _updater.BattleInstance.BattleLogContainer.AddLog(new WaveCreateBattleLog
		{
			RandomSeedByMonth = randomSeedByMonth,
			ProjectileGroupIndicesFromServer = projectileGroupIndicesFromServer
		});
		_enterPlayerBattleLogList.Add(item2);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (_enterPlayerBattleLogList.All((BattleLog e) => e.Processed))
		{
			if (_updater is EclipseOfflineModeUpdater)
			{
				(_updater as EclipseOfflineModeUpdater).Set_EclipseStatus(EclipseStatus.BattleStart);
			}
			if (_updater is KarmaUpdater)
			{
				_updater.BattleInstance.BattleLogContainer.AddLog(new KarmaInitBattleLog());
			}
			EndState(new WaveInit());
		}
	}
}
