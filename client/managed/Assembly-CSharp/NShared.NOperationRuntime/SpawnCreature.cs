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
			SprkInheritSummonerStats(battleCreatureCreateInfo, firedCreature, valueInt3);
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

	// SPRK: a summon without stat rows of its own (Maya's beast Seves, creature
	// 25004) takes the summoner's battle stats. ATK, DEF and Max HP are scaled by
	// the operation's ratio (per mille); every other stat is inherited as is.
	// Summons with their own stat rows (all monster summons) are unchanged.
	private static void SprkInheritSummonerStats(BattleCreatureCreateInfo createInfo, BattleCreature summoner, int statRatio)
	{
		if (createInfo?.StatData == null || summoner?.CreateInfo?.StatData == null || createInfo.StatData.GetStat(StatType.MaxHp) > 0)
		{
			return;
		}
		StatData statData = new StatData();
		statData.AddStat(summoner.CreateInfo.StatData);
		if (statRatio > 0)
		{
			foreach (StatType statType in SprkScaledSummonStats)
			{
				statData.ModifyStat(statType, (long stat) => stat * statRatio / 1000);
			}
		}
		createInfo.StatData = statData;
	}
}
