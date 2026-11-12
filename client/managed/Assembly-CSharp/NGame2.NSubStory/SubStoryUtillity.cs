using System;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NShared;

namespace NGame2.NSubStory;

public static class SubStoryUtillity
{
	public static bool IsSubStoryBattleType(int chapterIndex, int dungeonIndex)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (data.BattleType != BattleType.SubStory)
		{
			return data.BattleType == BattleType.SubStoryBoss;
		}
		return true;
	}

	public static bool IsSubStoryBattleType(EnterDungeonData enterDungeon)
	{
		if (enterDungeon == null)
		{
			return false;
		}
		int chapter = enterDungeon.Chapter;
		int originalDungeon = enterDungeon.OriginalDungeon;
		return IsSubStoryBattleType(chapter, originalDungeon);
	}

	public static BattleCreatureCreateInfo CreateBattleCreatureCreateInfo(CachedHeroInfo cachedHeroInfo, int scaleR, int tier, int weaponIndex, int[] addExtraSkills, int statRatio = 1000, bool isLeader = false)
	{
		if (cachedHeroInfo == null)
		{
			return null;
		}
		if (weaponIndex != 0 && GameRoot.Instance.ItemDataContainer.GetData(weaponIndex) != null)
		{
			EquipItemInfo equipItemInfo = new EquipItemInfo();
			equipItemInfo.ItemIndex = weaponIndex;
			equipItemInfo.SlotIndex = EquipPartType.Weapon.GetEquipSlotIndex();
			cachedHeroInfo.SetEquipItemInfo(EquipPartType.Weapon.GetEquipSlotIndex(), equipItemInfo);
		}
		BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(cachedHeroInfo, isLeader, isAi: false, null, null);
		if (battleCreatureCreateInfo == null)
		{
			return null;
		}
		StatData equippedHeroStat = StatCalculator.GetEquippedHeroStat(cachedHeroInfo);
		if (equippedHeroStat == null)
		{
			return null;
		}
		battleCreatureCreateInfo.StatData = equippedHeroStat;
		Random random = new Random(DateTime.UtcNow.Millisecond);
		battleCreatureCreateInfo.RandomSeed = random.Next();
		if (scaleR > 0)
		{
			battleCreatureCreateInfo.CreatureScaleR = scaleR;
		}
		if (tier > 0)
		{
			MonsterTierData tierData = GameRoot.Instance.MonsterTierDataContainer.GetData(tier);
			if (tierData != null && battleCreatureCreateInfo.StatData != null)
			{
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.PhysicalAttack, (long stat) => stat * tierData.PhysicalAttack * statRatio / 1000 / 1000);
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MagicalAttack, (long stat) => stat * tierData.MagicalAttack * statRatio / 1000 / 1000);
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.PhysicalDefense, (long stat) => stat * tierData.PhysicalDefense * statRatio / 1000 / 1000);
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MagicalDefense, (long stat) => stat * tierData.MagicalDefense * statRatio / 1000 / 1000);
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MaxHp, (long stat) => stat * tierData.MaxHp * statRatio / 1000 / 1000);
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.MaxMp, (long stat) => stat * tierData.MaxMp * statRatio / 1000 / 1000);
				battleCreatureCreateInfo.StatData.ModifyStat(StatType.AntiCcChance, (long stat) => stat * tierData.AntiCcChance * statRatio / 1000 / 1000);
			}
		}
		if (__Extension_Method_ICollection__.COUNT(addExtraSkills) > 0)
		{
			List<ExtraSkillInfo> list = new List<ExtraSkillInfo>();
			list.AddRangeFromIndex(addExtraSkills);
			if (__Extension_Method_ICollection__.COUNT(battleCreatureCreateInfo.AddExtraSkill) > 0)
			{
				list.AddRange(battleCreatureCreateInfo.AddExtraSkill);
			}
			battleCreatureCreateInfo.AddExtraSkill = list.ToArray();
		}
		return battleCreatureCreateInfo;
	}

	public static List<BattleCreatureCreateInfo> GetBattleCreatureInfos(EnterDungeonData enterDungeon)
	{
		List<ForcedHeroInfo> forcedHeroInfos = GetForcedHeroInfos(enterDungeon);
		List<BattleCreatureCreateInfo> list = new List<BattleCreatureCreateInfo>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(forcedHeroInfos); i++)
		{
			BattleCreatureCreateInfo battleCreatureCreateInfo = CreateBattleCreatureCreateInfo(forcedHeroInfos[i].CachedHeroInfo, forcedHeroInfos[i].HeroScaleR, forcedHeroInfos[i].HeroTier, forcedHeroInfos[i].WeaponIndex, forcedHeroInfos[i].AddExtraSkills, 1000, i == 0);
			if (battleCreatureCreateInfo != null)
			{
				list.Add(battleCreatureCreateInfo);
			}
		}
		return list;
	}

	public static List<ForcedHeroInfo> GetForcedHeroInfos(EnterDungeonData enterDungeon)
	{
		if (enterDungeon == null)
		{
			return null;
		}
		return GameRoot.Instance.SubStoryDataContainer.GetData(enterDungeon.Chapter, enterDungeon.OriginalDungeon)?.GetForcedHeroInfoList();
	}
}
