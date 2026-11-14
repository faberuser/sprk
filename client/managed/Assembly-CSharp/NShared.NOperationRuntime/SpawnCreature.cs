using NShared.NUtil;

namespace NShared.NOperationRuntime;

public class SpawnCreature : BaseOperationRuntime
{
	public enum SpawnPositionType
	{
		Myself,
		Ground,
		FixMyself
	}

	protected override void OnBegin()
	{
		base.OnBegin();
		BattleCreature firedCreature = base.OperationInfo.FiredCreature;
		if (firedCreature == null)
		{
			return;
		}
		int valueInt = base.OperationInfo.GetValueInt(0);
		sbyte star = firedCreature.CreateInfo.HeroInfo.Star;
		int valueInt2 = base.OperationInfo.GetValueInt(1);
		int valueInt3 = base.OperationInfo.GetValueInt(2);
		int num = base.OperationInfo.GetValueInt(3);
		SpawnPositionType valueEnum = base.OperationInfo.GetValueEnum<SpawnPositionType>(4);
		int valueInt4 = base.OperationInfo.GetValueInt(5);
		int valueInt5 = base.OperationInfo.GetValueInt(6);
		short level = firedCreature.CreateInfo.HeroInfo.Level;
		long playerId = base.OperationInfo.Creature.PlayerId;
		Vector3 position = base.OperationInfo.Creature.TransformController.Position;
		MonsterGradeType grade = base.OperationInfo.Creature.CreateInfo.Grade;
		switch (num)
		{
		case 0:
			num = base.OperationInfo.Creature.CreateInfo.Tier;
			break;
		case -1:
		{
			CampaignWaveData waveData = base.OperationInfo.BattleInstance.GetWaveData();
			if (waveData == null)
			{
				return;
			}
			num = waveData.Tier;
			break;
		}
		}
		for (int i = 0; i < valueInt2; i++)
		{
			int randomSeed = base.OperationInfo.Creature.NextRandom(100, "SpawnCreature " + base.OperationInfo.FiredSkillData);
			BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.Create(valueInt, star, level, randomSeed, num, grade, 0, valueInt3);
			bool inherited = SprkInheritSummonerStats(battleCreatureCreateInfo, firedCreature, valueInt3);
			if (inherited && SprkRefreshLivingSummon(base.OperationInfo.BattleInstance, base.OperationInfo.Creature.Uid, valueInt, battleCreatureCreateInfo.StatData))
			{
				continue;
			}
			switch (valueEnum)
			{
			case SpawnPositionType.Ground:
				battleCreatureCreateInfo.Position = new Vector3(valueInt4, position.My, valueInt5);
				break;
			case SpawnPositionType.FixMyself:
				battleCreatureCreateInfo.Position = position + new Vector3(valueInt4, 0L, valueInt5);
				break;
			default:
			{
				int num2 = base.OperationInfo.Creature.NextRandom(base.OperationInfo.Creature.CreatureData.SizeMm * 2, "SpawnCreature.randomOffsetX") - base.OperationInfo.Creature.CreatureData.SizeMm;
				int num3 = base.OperationInfo.Creature.NextRandom(base.OperationInfo.Creature.CreatureData.SizeMm * 2, "SpawnCreature.randomOffsetZ") - base.OperationInfo.Creature.CreatureData.SizeMm;
				num2 += valueInt4;
				num3 += valueInt5;
				battleCreatureCreateInfo.Position = position + new Vector3(num2, 0L, num3);
				break;
			}
			}
			BattleCreature battleCreature = base.OperationInfo.BattleInstance.CreatureContainer.Create(playerId, battleCreatureCreateInfo);
			battleCreature.TransformController.SetPosition(battleCreatureCreateInfo.Position, raiseEvent: true, SetPosition.SetPositionMethodType.Spawn, 0L);
			battleCreature.SetSummoner(base.OperationInfo.Creature.Uid);
			if (inherited && SprkHasArtifactSkill(battleCreatureCreateInfo.AddExtraSkill))
			{
				// Summoned mid-wave: run his wave-start skills (a shared artifact's) once now, as the
				// summoner's ran at the start of the wave.
				battleCreature.OnWaveStart(base.OperationInfo.BattleInstance.Snapshot.WaveIndex);
			}
		}
	}

	private static readonly StatType[] SprkScaledSummonStats = new StatType[5]
	{
		StatType.PhysicalAttack,
		StatType.MagicalAttack,
		StatType.PhysicalDefense,
		StatType.MagicalDefense,
		StatType.MaxHp
	};

	// Live values (current HP/MP, unit counts, boss gauge) that are not stats of their own.
	private static readonly StatType[] SprkNotCopiedStats = new StatType[9]
	{
		StatType.Hp,
		StatType.HpRatioM,
		StatType.Mp,
		StatType.MpRatioM,
		StatType.AllyCount,
		StatType.EnemyCount,
		StatType.TargetableEnemyCount,
		StatType.BossCcCurGauge,
		StatType.BossCcCurGaugeR
	};

	// SPRK: a summon without stat rows of its own (Maya's beast Seves, creature 25004) takes the
	// summoner's live battle stats at the moment of the summon: her stat sheet plus every stat
	// change active on her (buffs, debuffs, passive gear and perk stats). ATK, DEF and Max HP are
	// scaled by the operation's ratio (per mille); every other stat is copied as is. The copy
	// becomes the summon's base stats, so his own buffs apply on top of it. Summons with their own
	// stat rows (all monster summons) are unchanged. Returns whether the stats were inherited.
	private static bool SprkInheritSummonerStats(BattleCreatureCreateInfo createInfo, BattleCreature summoner, int statRatio)
	{
		if (createInfo?.StatData == null || summoner?.StatController == null || createInfo.StatData.GetStat(StatType.MaxHp) > 0)
		{
			return false;
		}
		StatController statController = summoner.StatController;
		BattleDefineData battleDefine = summoner.BattleInstance?.BattleDefine;
		ExtraSkillInfo[] summonSkills = SprkSummonGearSkills(summoner.CreateInfo?.AddExtraSkill);
		// A shared artifact runs on the summon himself, so its stat changes on the summoner are not
		// copied (they would count twice).
		System.Func<IStatModifier, bool> skipArtifact = null;
		if (SprkHasArtifactSkill(summonSkills))
		{
			skipArtifact = (IStatModifier modifier) => modifier is BaseOperationRuntime runtime && runtime.OperationInfo != null && SprkIsArtifactSkill(runtime.OperationInfo.SkillIndex) && runtime.OperationInfo.FiredCreature == summoner;
		}
		StatData statData = new StatData();
		foreach (StatType statType in StatData.StatTypes)
		{
			if (statType == StatType.None || System.Array.IndexOf(SprkNotCopiedStats, statType) >= 0)
			{
				continue;
			}
			// Unattenuated, so the summon's own stat caps apply once to his total. A stat fixed by an
			// override (a gear skill fixing ATK Spd) is copied at its fixed value.
			if (!statController.GetOverrideStat(statType, out var value))
			{
				value = ((skipArtifact != null) ? statController.SprkGetFinalStatExcluding(statType, StatTypeAttribute.IsAllowNegativeStat(statType), skipArtifact) : statController.GetFinalStatNoCache(statType, StatTypeAttribute.IsAllowNegativeStat(statType), applyAttenuation: false));
			}
			if (battleDefine != null)
			{
				// The battle's per-mode stat scaling is applied to base stats again when read.
				int modStatRatioM = battleDefine.GetModStatRatioM(statType);
				value = ((modStatRatioM != 0) ? (value * 1000 / modStatRatioM) : 0);
			}
			if (value != 0)
			{
				statData.SetStat(statType, value);
			}
		}
		if (statRatio > 0)
		{
			foreach (StatType statType2 in SprkScaledSummonStats)
			{
				statData.ModifyStat(statType2, (long stat) => stat * statRatio / 1000);
			}
		}
		createInfo.StatData = statData;
		createInfo.AddExtraSkill = summonSkills;
		return true;
	}

	// SPRK: Tier 9 (Technomagic) gear skills the summon gets as skills of his own: those triggered by
	// his own HP, skill use, blocks and kills, plus the effects that are not stats (normal attack
	// DMG, regeneration). Gear skill indices are 81SS0FF (slot SS 01-04, family FF). The other
	// families only change stats, which the live stat copy already includes, or touch mana,
	// cooldowns or the whole team.
	private static readonly int[] SprkSummonGearSkillFamilies = new int[8]
	{
		5,  // ATK +% while HP is below a threshold
		12, // ATK +% per skill use (stacks)
		13, // DMG taken -% per skill use (stacks)
		14, // Block +% per block (stacks)
		19, // Heal Rate +% on skill use
		22, // ATK +% per kill (stacks)
		30, // Normal attack DMG +%
		33  // Heal % of Max HP every second
	};

	// Family 38 ("Dodge +X, heal on dodge"): only its heal-on-dodge passive, since the dodge stat
	// is already copied. Every slot's family 38 uses this passive.
	private const int SprkDodgeGearFamily = 38;

	private const int SprkDodgeHealPassive = 8101138;

	// SPRK: the summoner's artifact is shared with the summon (artifact skills are 8000AAAL: artifact
	// AAA, level L; 8000999L and 8001AAAL likewise). Artifacts that also act on allies are not
	// shared, as a second copy would buff the team twice; their key is (index - 80000000) / 10.
	private const int SprkArtifactSkillMin = 80000000;

	private const int SprkArtifactSkillMax = 80100000;

	private static readonly int[] SprkTeamArtifacts = new int[16]
	{
		37, 48, 67, 502, 506, 507, 508, 514, 520, 523, 524, 910, 920, 921, 1001, 1003
	};

	private static bool SprkIsArtifactSkill(int skillIndex)
	{
		return skillIndex >= SprkArtifactSkillMin && skillIndex < SprkArtifactSkillMax;
	}

	private static bool SprkHasArtifactSkill(ExtraSkillInfo[] skills)
	{
		return skills != null && System.Array.Exists(skills, (ExtraSkillInfo e) => e != null && SprkIsArtifactSkill(e.SkillIndex));
	}

	private static ExtraSkillInfo[] SprkSummonGearSkills(ExtraSkillInfo[] summonerSkills)
	{
		if (summonerSkills == null)
		{
			return null;
		}
		System.Collections.Generic.List<ExtraSkillInfo> result = new System.Collections.Generic.List<ExtraSkillInfo>();
		foreach (ExtraSkillInfo skill in summonerSkills)
		{
			if (skill == null)
			{
				continue;
			}
			if (SprkIsArtifactSkill(skill.SkillIndex))
			{
				if (System.Array.IndexOf(SprkTeamArtifacts, (skill.SkillIndex - SprkArtifactSkillMin) / 10) < 0)
				{
					result.Add(new ExtraSkillInfo(skill.SkillIndex, skill.SkillLevel));
				}
				continue;
			}
			if (skill.SkillIndex < 8101000 || skill.SkillIndex >= 8105000)
			{
				continue;
			}
			int family = skill.SkillIndex % 1000;
			if (System.Array.IndexOf(SprkSummonGearSkillFamilies, family) >= 0)
			{
				result.Add(new ExtraSkillInfo(skill.SkillIndex, skill.SkillLevel));
			}
			else if (family == SprkDodgeGearFamily && !result.Exists((ExtraSkillInfo e) => e.SkillIndex == SprkDodgeHealPassive))
			{
				result.Add(new ExtraSkillInfo(SprkDodgeHealPassive, skill.SkillLevel));
			}
		}
		return (result.Count > 0) ? result.ToArray() : null;
	}

	// SPRK: when the summoner summons again while her summon of this creature is alive (Maya's S2
	// with Seves on the field), he is not duplicated: his base stats are replaced by a fresh copy
	// of her live stats instead. Returns whether a living summon was refreshed.
	private static bool SprkRefreshLivingSummon(BattleInstance battleInstance, int summonerUid, int creatureIndex, StatData statData)
	{
		if (battleInstance?.CreatureContainer == null || statData == null)
		{
			return false;
		}
		bool refreshed = false;
		BattleCreatureList children = battleInstance.CreatureContainer.GetChildCreatureList(summonerUid, aliveOnly: true);
		for (int i = 0; i < children.Count; i++)
		{
			BattleCreature summon = children[i];
			if (summon == null || summon.CreatureIndex != creatureIndex)
			{
				continue;
			}
			summon.CreateInfo.StatData = statData;
			foreach (StatType statType in StatData.StatTypes)
			{
				summon.StatController.MakeStatDirty(statType);
			}
			long maxHp = summon.StatController.GetMaxHp();
			if (summon.StatController.Hp > maxHp)
			{
				summon.StatController.SetHp(maxHp);
			}
			refreshed = true;
		}
		return refreshed;
	}
}
