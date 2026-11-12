using System.Collections.Generic;
using NShared.NCampaign.NCampaignState;

namespace NShared.NCampaign;

public class CampaignUpdater : BattleModeUpdater
{
	public List<CreatureCreateBattleLog> DelayedCreatureLogList { get; private set; }

	public List<List<BattleCreatureCreateInfo>> EachWaveEachTeamBattleCreatureCreateInfoList { get; private set; }

	public CampaignUpdater(BattleInstance battleInstance, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, List<IBattleLogRuntime> initialBattleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer = null)
		: base(battleInstance, chapterIndex, dungeonIndex, difficulty, scenario)
	{
		DelayedCreatureLogList = new List<CreatureCreateBattleLog>();
		WaveCreate waveCreate = new WaveCreate();
		waveCreate.SetUpdater(this);
		waveCreate.SetInitialBattleLog(initialBattleLogList, randomSeedByMonth, projectileGroupIndicesFromServer);
		StartState(waveCreate);
	}

	public void AddEachWaveEachTeamBattleCreatureCreateInfo(List<BattleCreatureCreateInfo> infoList)
	{
		if (infoList != null && infoList.Count > 0)
		{
			if (EachWaveEachTeamBattleCreatureCreateInfoList == null)
			{
				EachWaveEachTeamBattleCreatureCreateInfoList = new List<List<BattleCreatureCreateInfo>>();
				EachWaveEachTeamBattleCreatureCreateInfoList.Clear();
			}
			EachWaveEachTeamBattleCreatureCreateInfoList.Add(infoList);
		}
	}

	public static PlayerEnterBattleLog CreateNpcEnterBattleLog(long accountId, MatchedAccountInfo matchedNpcInfo, TeamType teamType = TeamType.Right, bool isAi = true)
	{
		PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog
		{
			PlayerCreateInfo = new BattlePlayerCreateInfo
			{
				Id = accountId,
				TeamType = teamType
			}
		};
		Dictionary<int, CachedHeroInfo>.ValueCollection values = matchedNpcInfo.HeroInfos.Values;
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		foreach (CachedHeroInfo item in values)
		{
			StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(item);
			BattleCreatureCreateInfo battleCreatureCreateInfo = new BattleCreatureCreateInfo();
			battleCreatureCreateInfo.IsAi = isAi;
			battleCreatureCreateInfo.HeroInfo = item;
			battleCreatureCreateInfo.StatData = equippedHeroStat;
			if (matchedNpcInfo.ArenaType == MatchArenaType.GuildArena)
			{
				battleCreatureCreateInfo.AddExtraSkill = BattleCreatureCreateInfo.GetAddExtraSkill(item);
				battleCreatureCreateInfo.AccountBuffs = matchedNpcInfo.AccountBuffs;
				battleCreatureCreateInfo.GuildSkills = matchedNpcInfo.GuildSkills;
				battleCreatureCreateInfo.GuildArenaBuffDatasBases = matchedNpcInfo.GuildArenaBuffDataBases;
				battleCreatureCreateInfo.ClassBuffDataBases = matchedNpcInfo.ClassBuffDataBases;
				battleCreatureCreateInfo.FirstSkillSlotIndex = item.FirstSkillSlotIndex;
				battleCreatureCreateInfo.ExtraStats = matchedNpcInfo.ExtraStatDataBases;
				battleCreatureCreateInfo.PetStats = matchedNpcInfo.PetStatDataBases;
			}
			list.Add(battleCreatureCreateInfo);
		}
		playerEnterBattleLog.CreatureCreateInfos = list.ToArray();
		return playerEnterBattleLog;
	}
}
