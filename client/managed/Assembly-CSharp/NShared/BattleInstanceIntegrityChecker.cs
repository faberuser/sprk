using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NVespa;

namespace NShared;

public class BattleInstanceIntegrityChecker
{
	[JsonSerializable(Javascript = true)]
	public enum AbusingCategory
	{
		None,
		Timeout,
		HeroCount,
		InitialHeroInfo,
		HeroStat,
		WaveError,
		MonsterCount,
		WaveMonsterError,
		WaveMonsterStat,
		ResultAliveCountMismatch,
		ResultMismatch
	}

	public class CheckResult
	{
		public AbusingCategory AbusingCategory;

		public string Result;

		public int LeftValue;

		public int RightValue;

		public CheckResult(AbusingCategory a, string r, int lValue = 0, int rValue = 0)
		{
			AbusingCategory = a;
			Result = r;
			LeftValue = lValue;
			RightValue = rValue;
		}
	}

	protected const int DefaultTimeoutTimeMs = 15000;

	public static double HeroStatAllowedRate = 0.001;

	public static List<string> CoRunExcludeBattleTypes = new List<string>();

	public static bool IsHeroStatAllowedError { get; set; }

	public static bool OnCoRun { get; set; }

	public static BattleInstanceBuilder CreateBattleInstanceBuilderForCheck(int chapterIndex, int dungeonIndex, int difficulty, BattleInstanceExtraData extraData)
	{
		new DefeatAllEnemy();
		BattleInstanceBuilder battleInstanceBuilder = new BattleInstanceBuilder();
		battleInstanceBuilder.SetCampaignDungeon(chapterIndex, dungeonIndex, difficulty);
		battleInstanceBuilder.SetBattleEventContext(null);
		battleInstanceBuilder.EnableSnapshot(enable: false);
		battleInstanceBuilder.SetExtraData(extraData);
		return battleInstanceBuilder;
	}

	public static CheckResult Run(BattleInstanceBuilder builder, IList<CachedHeroInfo> leftHeroInfos, IList<CachedHeroInfo> rightHeroInfos, IList<ResultCreatureInfo> leftInitialCreatures, IList<ResultCreatureInfo> rightInitialCreatures, IList<BaseBattleLog> battleLogList, IList<ResultCreatureInfo> resultCreatureInfos, out List<ResultCreatureInfo> verifiedCreatureInfoList, Func<string, PlayerEnterBattleLog> parser, Func<string, WaveReadyBattleLog> waveReadyParser, int indexOfBattleLogList, int timeoutTimeMs = 15000)
	{
		verifiedCreatureInfoList = null;
		if (battleLogList == null)
		{
			return new CheckResult(AbusingCategory.None, "MissingBattleLog");
		}
		IEnumerable<BaseBattleLog> enumerable = battleLogList.Where((BaseBattleLog e) => e.Type == BattleLogType.PlayerEnter);
		if (enumerable == null || enumerable.Count() == 0)
		{
			return new CheckResult(AbusingCategory.None, "EnterPlayerBattleLogNotFound");
		}
		PlayerEnterBattleLog playerEnterBattleLog = null;
		PlayerEnterBattleLog playerEnterBattleLog2 = null;
		foreach (BaseBattleLog item in enumerable)
		{
			try
			{
				PlayerEnterBattleLog playerEnterBattleLog3 = parser(item.Data);
				if (playerEnterBattleLog3 == null)
				{
					continue;
				}
				BattlePlayerCreateInfo playerCreateInfo = playerEnterBattleLog3.PlayerCreateInfo;
				if (playerCreateInfo == null)
				{
					continue;
				}
				if (playerEnterBattleLog3.CreatureCreateInfos != null)
				{
					BattleCreatureCreateInfo[] creatureCreateInfos = playerEnterBattleLog3.CreatureCreateInfos;
					foreach (BattleCreatureCreateInfo battleCreatureCreateInfo in creatureCreateInfos)
					{
						if (battleCreatureCreateInfo.HeroInfo != null)
						{
							battleCreatureCreateInfo.HeroInfo.Validate();
						}
					}
				}
				if (playerCreateInfo.TeamType == TeamType.Left)
				{
					playerEnterBattleLog = playerEnterBattleLog3;
				}
				else
				{
					playerEnterBattleLog2 = playerEnterBattleLog3;
				}
			}
			catch (Exception ex)
			{
				VespaDebug.LogError("Error while parsing player-enter : " + ex);
			}
		}
		List<WaveReadyBattleLog> list = null;
		IEnumerable<BaseBattleLog> enumerable2 = battleLogList.Where((BaseBattleLog e) => e.Type == BattleLogType.WaveReady);
		if (enumerable2 != null)
		{
			foreach (BaseBattleLog item2 in enumerable2)
			{
				try
				{
					WaveReadyBattleLog waveReadyBattleLog = waveReadyParser(item2.Data);
					if (waveReadyBattleLog != null)
					{
						list = list ?? new List<WaveReadyBattleLog>();
						list.Add(waveReadyBattleLog);
					}
				}
				catch (Exception ex2)
				{
					VespaDebug.LogError("Error while parsing wave-ready : " + ex2);
				}
			}
		}
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(builder.ChapterIndex, builder.DungeonIndex);
		BattleType battleType = data.BattleType;
		if (list != null && list.Count > 0)
		{
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				WaveReadyBattleLog waveReadyBattleLog2 = list[num2];
				int waveIndex = waveReadyBattleLog2.WaveIndex;
				int chapterIndex = waveReadyBattleLog2.ChapterIndex;
				int dungeonIndex = waveReadyBattleLog2.DungeonIndex;
				DungeonDifficultyType difficulty = waveReadyBattleLog2.Difficulty;
				if (battleType != BattleType.Eclipse && battleType != BattleType.KarmaRaid && indexOfBattleLogList == -1)
				{
					if (chapterIndex != builder.ChapterIndex)
					{
						return new CheckResult(AbusingCategory.WaveError, "ChapterIndexMismatch_" + waveIndex + "_" + chapterIndex + "_" + builder.ChapterIndex);
					}
					if (dungeonIndex != builder.DungeonIndex)
					{
						return new CheckResult(AbusingCategory.WaveError, "DungeonIndexMismatch_" + waveIndex + "_" + dungeonIndex + "_" + builder.DungeonIndex);
					}
					if (difficulty != (DungeonDifficultyType)builder.Dfficulty)
					{
						string[] obj = new string[6]
						{
							"DifficultyMismatch_",
							waveIndex.ToString(),
							"_",
							null,
							null,
							null
						};
						int num = (int)difficulty;
						obj[3] = num.ToString();
						obj[4] = "_";
						obj[5] = builder.Dfficulty.ToString();
						return new CheckResult(AbusingCategory.WaveError, string.Concat(obj));
					}
				}
				CampaignWaveData data2 = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario: false, waveIndex);
				if (data2 == null)
				{
					return new CheckResult(AbusingCategory.WaveError, "WaveDataNotFound_" + waveIndex);
				}
				List<CampaignWaveData.CreatureDetailInfo> creatureList = data2.GetCreatureList();
				List<CampaignWaveData.CreatureDetailWritable> list2 = new List<CampaignWaveData.CreatureDetailWritable>();
				foreach (CampaignWaveData.CreatureDetailInfo item3 in creatureList)
				{
					if (item3.DelayMs == 0)
					{
						list2.Add(item3.CloneWritable());
					}
				}
				if (battleType == BattleType.KarmaRaid)
				{
					for (int num3 = 0; num3 < list2.Count; num3++)
					{
						CampaignWaveData.CreatureDetailWritable creatureDetailWritable = list2[num3];
						if (creatureDetailWritable != null)
						{
							List<int> list3 = new List<int>();
							int punishmentGroupIndex = GameRoot.Instance.KarmaDungeonDataContainer.GetDataByChapterDungeon(chapterIndex, dungeonIndex).PunishmentGroupIndex;
							IList<int> allEnemyPowerUpSkills = GameRoot.Instance.KarmaEnemyPowerUpDataContainer.GetAllEnemyPowerUpSkills(punishmentGroupIndex);
							int[] unlockSkills = GameRoot.Instance.KarmaEnemyUnlockSkillDataContainer.GetUnlockSkills(punishmentGroupIndex, waveReadyBattleLog2.MaxStageLevel);
							if (allEnemyPowerUpSkills != null && allEnemyPowerUpSkills.Count > 0)
							{
								list3.AddRange(allEnemyPowerUpSkills);
							}
							if (unlockSkills != null && unlockSkills.Length != 0)
							{
								list3.AddRange(unlockSkills);
							}
							if (creatureDetailWritable._AddExtraSkill == null)
							{
								creatureDetailWritable._AddExtraSkill = list3.ToArray();
								continue;
							}
							IEnumerable<int> source = creatureDetailWritable.AddExtraSkill.Concat(list3);
							creatureDetailWritable._AddExtraSkill = source.ToArray();
						}
					}
				}
				int num4 = list2?.Count ?? 0;
				int num5 = ((waveReadyBattleLog2.MonsterCreateInfos != null) ? waveReadyBattleLog2.MonsterCreateInfos.Length : 0);
				if (num4 != num5)
				{
					return new CheckResult(AbusingCategory.MonsterCount, "WaveMonsterCountMismatch_" + waveIndex + "_" + num5 + "_" + num4, num5, num5);
				}
				for (int num6 = 0; num6 < num4; num6++)
				{
					BattleCreatureCreateInfo battleCreatureCreateInfo2 = waveReadyBattleLog2.MonsterCreateInfos[num6];
					BattleCreatureCreateInfo battleCreatureCreateInfo3 = BattleCreatureCreateInfo.Create(list2[num6], battleCreatureCreateInfo2.RandomSeed);
					if (battleCreatureCreateInfo3 == null)
					{
						return new CheckResult(AbusingCategory.WaveMonsterError, "NullWaveMonsterCreateInfo_" + waveIndex + "_" + num6);
					}
					Tuple<string, int, int> compareResult = BattleCreatureCreateInfo.GetCompareResult(battleCreatureCreateInfo2, battleCreatureCreateInfo3);
					if (compareResult != null && compareResult.Item1.Length > 0)
					{
						return new CheckResult(AbusingCategory.WaveMonsterStat, "MonsterInfoMismatch_" + waveIndex + "_" + num6 + "_" + compareResult.Item1, compareResult.Item2, compareResult.Item3);
					}
				}
			}
		}
		if (leftHeroInfos != null && leftHeroInfos.Count > 0)
		{
			for (int num7 = 0; num7 < leftHeroInfos.Count; num7++)
			{
				leftHeroInfos[num7].Validate();
			}
		}
		if (rightHeroInfos != null && rightHeroInfos.Count > 0)
		{
			for (int num8 = 0; num8 < rightHeroInfos.Count; num8++)
			{
				rightHeroInfos[num8].Validate();
			}
		}
		List<PlayerEnterBattleLog> list4 = new List<PlayerEnterBattleLog>();
		if (playerEnterBattleLog != null)
		{
			list4.Add(playerEnterBattleLog);
		}
		if (playerEnterBattleLog2 != null)
		{
			list4.Add(playerEnterBattleLog2);
		}
		IList<CachedHeroInfo>[] array = new IList<CachedHeroInfo>[2] { leftHeroInfos, rightHeroInfos };
		IList<ResultCreatureInfo>[] array2 = new IList<ResultCreatureInfo>[2] { leftInitialCreatures, rightInitialCreatures };
		for (int num9 = 0; num9 < list4.Count(); num9++)
		{
			PlayerEnterBattleLog playerEnterBattleLog4 = list4[num9];
			IList<CachedHeroInfo> list5 = array[num9];
			IList<ResultCreatureInfo> rhs = array2[num9];
			int num10 = list5?.Count ?? 0;
			int num11 = ((playerEnterBattleLog4 != null && playerEnterBattleLog4.CreatureCreateInfos != null) ? playerEnterBattleLog4.CreatureCreateInfos.Length : 0);
			if (playerEnterBattleLog4.BattleCheckCreatureCreateInfos != null)
			{
				num11 += playerEnterBattleLog4.BattleCheckCreatureCreateInfos.Length;
			}
			if (num11 != num10)
			{
				return new CheckResult(AbusingCategory.HeroCount, "BattleLogCreatureCreateInfoError_" + num11 + "_" + num10, num11, num10);
			}
			if (num10 > 0)
			{
				if (!CheckInitialCreatureInfo(playerEnterBattleLog4.InitialCreatureInfos, rhs))
				{
					return new CheckResult(AbusingCategory.InitialHeroInfo, "BattleLogInitialCreatureInfoMismatch");
				}
				Tuple<string, int, int> tuple = CheckCreatureStat(playerEnterBattleLog4.CreatureCreateInfos, list5);
				if (tuple != null)
				{
					return new CheckResult(AbusingCategory.HeroStat, tuple.Item1, tuple.Item2, tuple.Item3);
				}
			}
		}
		if (OnCoRun)
		{
			if (CoRunExcludeBattleTypes.FindIndex((string x) => x.Equals(battleType.ToString())) > -1)
			{
				return null;
			}
			BattleInstance battleInstance = null;
			bool timeout = false;
			IEnumerator enumerator3 = coRun(builder, battleLogList, int.MaxValue, delegate(BattleInstance b, bool t)
			{
				battleInstance = b;
				timeout = t;
			}, timeoutTimeMs);
			while (enumerator3.MoveNext())
			{
			}
			if (battleInstance == null)
			{
				return new CheckResult(AbusingCategory.None, "NullBattleInstance");
			}
			if (timeout)
			{
				return new CheckResult(AbusingCategory.Timeout, "Timeout");
			}
			verifiedCreatureInfoList = GetCreatureInfoList(battleInstance);
			Tuple<AbusingCategory, string, int, int> tuple2 = CheckCreatureResult(resultCreatureInfos, verifiedCreatureInfoList);
			if (tuple2 != null)
			{
				return new CheckResult(tuple2.Item1, "CreatureResult_" + tuple2.Item2, tuple2.Item3, tuple2.Item4);
			}
		}
		return null;
	}

	private static bool CheckInitialCreatureInfo(IList<ResultCreatureInfo> lhs, IList<ResultCreatureInfo> rhs)
	{
		List<ResultCreatureInfo> list = new List<ResultCreatureInfo>();
		List<ResultCreatureInfo> list2 = new List<ResultCreatureInfo>();
		if (lhs != null)
		{
			list.AddRange(lhs);
			list.Sort((ResultCreatureInfo l, ResultCreatureInfo r) => l.Key.CompareTo(r.Key));
		}
		if (rhs != null)
		{
			list2.AddRange(rhs);
			list2.Sort((ResultCreatureInfo l, ResultCreatureInfo r) => l.Key.CompareTo(r.Key));
		}
		if (list.Count != list2.Count)
		{
			return false;
		}
		for (int num = 0; num < list.Count; num++)
		{
			if (ResultCreatureInfo.IsEqual(list[num], list2[num]) != null)
			{
				return false;
			}
		}
		return true;
	}

	private static Tuple<string, int, int> CheckCreatureStat(IList<BattleCreatureCreateInfo> creatureCreateInfos, IList<CachedHeroInfo> heroInfos)
	{
		foreach (BattleCreatureCreateInfo creatureCreateInfo in creatureCreateInfos)
		{
			CachedHeroInfo heroInfo = creatureCreateInfo.HeroInfo;
			if (heroInfo == null)
			{
				continue;
			}
			int heroIndex = heroInfo.HeroIndex;
			CachedHeroInfo cachedHeroInfo = heroInfos.FirstOrDefault((CachedHeroInfo e) => e.HeroIndex == heroIndex);
			if (cachedHeroInfo == null)
			{
				return new Tuple<string, int, int>("NotOwnedHero:" + heroIndex, 0, 0);
			}
			string notEqualDesc = string.Empty;
			if (!IsEqualForBattle(heroInfo, cachedHeroInfo, out notEqualDesc))
			{
				return new Tuple<string, int, int>("Hero:" + heroIndex + ":" + notEqualDesc, 0, 0);
			}
			StatData statData = creatureCreateInfo.StatData;
			if (statData == null)
			{
				return new Tuple<string, int, int>("HeroStatNull:" + heroIndex, 0, 0);
			}
			StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(cachedHeroInfo);
			StatType differentStatType = StatType.None;
			if (StatData.IsEqual(statData, equippedHeroStat, out differentStatType))
			{
				continue;
			}
			long stat = statData.GetStat(differentStatType);
			long stat2 = equippedHeroStat.GetStat(differentStatType);
			if (IsHeroStatAllowedError)
			{
				int num = (int)System.Math.Abs(stat - stat2);
				int num2 = System.Math.Max((int)((double)stat2 * HeroStatAllowedRate), 0);
				if (num < num2)
				{
					continue;
				}
			}
			return new Tuple<string, int, int>("HeroStatMismatch:" + heroIndex + ":" + differentStatType.ToString() + ":" + stat + ":" + stat2, (int)stat, (int)stat2);
		}
		return null;
	}

	public static bool IsEqualForBattle(HeroInfo lhs, HeroInfo rhs, out string notEqualDesc)
	{
		notEqualDesc = string.Empty;
		if (lhs == null || rhs == null)
		{
			notEqualDesc = ((lhs == null) ? "LhsNull" : "RhsNull");
			return false;
		}
		if (lhs.HeroIndex != rhs.HeroIndex)
		{
			notEqualDesc = "Index_" + lhs.HeroIndex + "_" + rhs.HeroIndex;
			return false;
		}
		if (lhs.Level != rhs.Level)
		{
			notEqualDesc = "Level_" + lhs.Level + "_" + rhs.Level;
			return false;
		}
		if (lhs.Star != rhs.Star)
		{
			notEqualDesc = "Star_" + lhs.Star + "_" + rhs.Star;
			return false;
		}
		if (lhs.SkillLevel1 != rhs.SkillLevel1)
		{
			notEqualDesc = "Skill1_" + lhs.SkillLevel1 + "_" + rhs.SkillLevel1;
			return false;
		}
		if (lhs.SkillLevel2 != rhs.SkillLevel2)
		{
			notEqualDesc = "Skill2_" + lhs.SkillLevel2 + "_" + rhs.SkillLevel2;
			return false;
		}
		if (lhs.SkillLevel3 != rhs.SkillLevel3)
		{
			notEqualDesc = "Skill3_" + lhs.SkillLevel3 + "_" + rhs.SkillLevel3;
			return false;
		}
		if (lhs.SkillLevel4 != rhs.SkillLevel4)
		{
			notEqualDesc = "Skill4_" + lhs.SkillLevel4 + "_" + rhs.SkillLevel4;
			return false;
		}
		if (lhs.SkillLevel5 != rhs.SkillLevel5)
		{
			notEqualDesc = "Skill5_" + lhs.SkillLevel5 + "_" + rhs.SkillLevel5;
			return false;
		}
		foreach (EquipSlotData allEquipSlotDatum in EquipSlotHelper.GetAllEquipSlotData())
		{
			int equipItemSlotIndex = lhs.GetEquipItemSlotIndex(allEquipSlotDatum.Index);
			int equipItemSlotIndex2 = rhs.GetEquipItemSlotIndex(allEquipSlotDatum.Index);
			if (equipItemSlotIndex != equipItemSlotIndex2)
			{
				notEqualDesc = "EquipSlot" + allEquipSlotDatum.EquipPartType.ToString() + "_" + equipItemSlotIndex + "_" + equipItemSlotIndex2;
				return false;
			}
		}
		if (lhs.SkillExtend1 != rhs.SkillExtend1)
		{
			notEqualDesc = "SkillExt1_" + lhs.SkillExtend1 + "_" + rhs.SkillExtend1;
			return false;
		}
		if (lhs.SkillExtend2 != rhs.SkillExtend2)
		{
			notEqualDesc = "SkillExt2_" + lhs.SkillExtend2 + "_" + rhs.SkillExtend2;
			return false;
		}
		if (lhs.SkillExtend3 != rhs.SkillExtend3)
		{
			notEqualDesc = "SkillExt3_" + lhs.SkillExtend3 + "_" + rhs.SkillExtend3;
			return false;
		}
		if (lhs.SkillExtend4 != rhs.SkillExtend4)
		{
			notEqualDesc = "SkillExt4_" + lhs.SkillExtend4 + "_" + rhs.SkillExtend4;
			return false;
		}
		if (lhs.SkillExtend5 != rhs.SkillExtend5)
		{
			notEqualDesc = "SkillExt5_" + lhs.SkillExtend5 + "_" + rhs.SkillExtend5;
			return false;
		}
		if (!HeroInfo.IntegrityTranscendSkill(lhs, rhs, out var error))
		{
			notEqualDesc = error;
			return false;
		}
		if (lhs.Transcended != rhs.Transcended)
		{
			notEqualDesc = "Transcended_" + lhs.Transcended + "_" + rhs.Transcended;
			return false;
		}
		return true;
	}

	public static IEnumerator coRun(BattleInstanceBuilder builder, IList<BaseBattleLog> battleLogList, int yieldIntervalTimeMs, Action<BattleInstance, bool> onEnd, int timeoutTimeMs = 15000)
	{
		BattleInstance battleInstance = builder.Build<BattleInstance>();
		if (battleInstance == null)
		{
			onEnd?.Invoke(null, arg2: false);
			yield break;
		}
		battleInstance.BattleLogContainer.Clear();
		battleInstance.BattleLogContainer.AddReceivedLog(battleLogList);
		DateTime replayStartedTime = DateTime.UtcNow;
		DateTime lastUpdatedTime = DateTime.UtcNow;
		float yieldIntervalTime = (float)yieldIntervalTimeMs / 1000f;
		using (new SnapshotWritableBlock())
		{
			while (!battleInstance.IsBattleEnded())
			{
				DateTime utcNow = DateTime.UtcNow;
				battleInstance.UpdateTick(battleInstance.SnapshotEnabled);
				SnapshotObjectPoolContainer.FlushPendingRelease();
				_ = (utcNow - replayStartedTime).TotalMilliseconds;
				if ((utcNow - lastUpdatedTime).TotalSeconds >= (double)yieldIntervalTime)
				{
					lastUpdatedTime = utcNow;
					yield return null;
				}
			}
		}
		double totalSeconds = (DateTime.UtcNow - replayStartedTime).TotalSeconds;
		float num = (float)battleInstance.GlobalTimer.GetCurrentTimeMs() / 1000f;
		if (totalSeconds > 0.0)
		{
			_ = (double)num / totalSeconds;
		}
		onEnd(battleInstance, arg2: false);
	}

	public static Tuple<AbusingCategory, string, int, int> CheckCreatureResult(IList<ResultCreatureInfo> lhs, IList<ResultCreatureInfo> rhs)
	{
		List<ResultCreatureInfo> list = new List<ResultCreatureInfo>();
		List<ResultCreatureInfo> list2 = new List<ResultCreatureInfo>();
		if (lhs != null)
		{
			foreach (ResultCreatureInfo lh in lhs)
			{
				if (lh.Hp > 0)
				{
					list.Add(lh);
				}
			}
		}
		if (rhs != null)
		{
			foreach (ResultCreatureInfo rh in rhs)
			{
				if (rh.Hp > 0)
				{
					list2.Add(rh);
				}
			}
		}
		if (list.Count != list2.Count)
		{
			return new Tuple<AbusingCategory, string, int, int>(AbusingCategory.ResultAliveCountMismatch, "AliveCountMismatch_" + list.Count + "_" + list2.Count, list.Count, list2.Count);
		}
		if (list.Count > 0)
		{
			list.Sort((ResultCreatureInfo l, ResultCreatureInfo r) => l.Key.CompareTo(r.Key));
			list2.Sort((ResultCreatureInfo l, ResultCreatureInfo r) => l.Key.CompareTo(r.Key));
			for (int num = 0; num < list.Count; num++)
			{
				ResultCreatureInfo resultCreatureInfo = list[num];
				ResultCreatureInfo resultCreatureInfo2 = list2[num];
				Tuple<string, int, int> tuple = ResultCreatureInfo.IsEqual(resultCreatureInfo, resultCreatureInfo2);
				if (tuple != null)
				{
					string i_item = "CreatureMismatch_" + ((resultCreatureInfo != null) ? ("Hero_" + resultCreatureInfo.Index + "_" + tuple.Item1 + "_" + tuple.Item2) : "NULL") + "_" + ((resultCreatureInfo2 != null) ? ("Hero_" + resultCreatureInfo2.Index + "_" + tuple.Item1 + "_" + tuple.Item3) : "NULL");
					return new Tuple<AbusingCategory, string, int, int>(AbusingCategory.ResultMismatch, i_item, tuple.Item2, tuple.Item3);
				}
			}
		}
		return null;
	}

	public static List<ResultCreatureInfo> GetCreatureInfoList(BattleInstance battleInstance, int teamId = -1)
	{
		HashSet<int> hashSet = new HashSet<int>();
		BattleCreatureList creatures = battleInstance.CreatureContainer.GetCreatures();
		List<ResultCreatureInfo> list = new List<ResultCreatureInfo>();
		for (int i = 0; i < creatures.Count; i++)
		{
			BattleCreature battleCreature = creatures[i];
			hashSet.Add(battleCreature.Uid);
			int teamId2 = battleCreature.TeamController.TeamId;
			if (teamId < 0 || teamId2 == teamId)
			{
				ResultCreatureInfo resultCreatureInfo = CreateCreatureInfo(battleCreature);
				if (resultCreatureInfo != null)
				{
					list.Add(resultCreatureInfo);
				}
			}
		}
		Dictionary<int, ResultCreatureInfo> destroyedCreatureInfos = battleInstance.CreatureContainer.GetDestroyedCreatureInfos();
		if (destroyedCreatureInfos != null)
		{
			foreach (KeyValuePair<int, ResultCreatureInfo> item in destroyedCreatureInfos)
			{
				int key = item.Key;
				ResultCreatureInfo value = item.Value;
				if (hashSet.Contains(key))
				{
					VespaDebug.LogError("Alive creature exists in destroyed-list!");
				}
				else if (teamId < 0 || value.TeamId == teamId)
				{
					list.Add(value);
				}
			}
		}
		return SprkConquest.ScoreBoss(list, battleInstance);
	}

	public static ResultCreatureInfo CreateCreatureInfo(BattleCreature creature)
	{
		if (creature == null)
		{
			return null;
		}
		if (creature.SummonerUid != 0)
		{
			return null;
		}
		int teamId = creature.TeamController.TeamId;
		return new ResultCreatureInfo
		{
			Index = creature.CreatureIndex,
			TeamId = teamId,
			Key = ResultCreatureInfo.GetKey(creature.CreatureIndex, creature.CreateInfo.WaveSlotIndex, teamId),
			Hp = creature.StatController.Hp,
			Mp = creature.StatController.Mp,
			MaxHp = creature.StatController.GetMaxHp(),
			MaxMp = creature.StatController.GetMaxMp(),
			GivedDamage = GetGivedDamage(creature)
		};
	}

	private static long GetGivedDamage(BattleCreature creature)
	{
		if (creature == null || creature.StatController.GetMinHp() == 0L)
		{
			return 0L;
		}
		return creature.BattleInstance.Snapshot.StatisticsManager.TotalDamagedMeter(creature.Uid);
	}
}
