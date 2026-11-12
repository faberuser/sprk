using System.Collections.Generic;
using NVespa;

namespace NShared;

public class ShardFlaskManager
{
	private const string SHARD_FLASK_SKILL_GROUP_NAME = "ShardGroup";

	private BattleInstance _battleInstance;

	private readonly List<AffixSkillDataBase> _flaskSkillList = new List<AffixSkillDataBase>();

	private List<BattleCreature> affectedCreatureList = new List<BattleCreature>();

	private long _expiredTime;

	private bool _enabled;

	public ShardFlaskManager(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
	}

	public void Set()
	{
		int flaskItemIndex = _battleInstance.ExtraData.FlaskItemIndex;
		int flaskItemCount = _battleInstance.ExtraData.FlaskItemCount;
		ShardItemData data = GameRoot.Instance.ShardItemDataContainer.GetData(flaskItemIndex);
		if (data == null)
		{
			return;
		}
		PunishmentGroupData dataWithChapterIndex = GameRoot.Instance.PunishmentGroupDataContainer.GetDataWithChapterIndex(_battleInstance.ChapterIndex);
		if (dataWithChapterIndex != null && dataWithChapterIndex.PunishmentDungeonType == EPunishmentDungeonType.Boss)
		{
			int[] affixIndexes = data.AffixIndexes;
			if (affixIndexes != null && affixIndexes.Length >= 1)
			{
				_expiredTime = data.Duration * flaskItemCount * 1000;
				AddFlaskExtraSkill(affixIndexes, flaskItemCount);
			}
		}
	}

	internal void AddFlaskExtraSkill(int[] affixIndexes, int affixLevel)
	{
		if (affixIndexes == null || affixIndexes.Length == 0)
		{
			return;
		}
		for (int i = 0; i < affixIndexes.Length; i++)
		{
			int key = affixIndexes[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data == null)
			{
				VespaDebug.LogError("[AffixContainer] AffixData is null AffixIndex :" + key);
			}
			else if (data.AffixType == EAffixType.Skill)
			{
				AffixSkillDataBase item = CreateAffixSkillDataBase(data, affixLevel);
				_flaskSkillList.Add(item);
			}
		}
		if (_flaskSkillList.Count > 0)
		{
			_enabled = true;
		}
	}

	private AffixSkillDataBase CreateAffixSkillDataBase(AffixData data, int skillLevel = 1)
	{
		return new AffixSkillDataBase
		{
			Index = data.Index,
			TeamType = data.TeamType,
			HeroIndices = data.HeroIndices,
			SkillIndex = data.OptionValue,
			SkillLevel = skillLevel
		};
	}

	public List<IExtraSkillData> GetFlaskSkillList(TeamType teamType, BattleCreature creature)
	{
		if (_battleInstance.GlobalTimer.GetCurrentTimeMs() > _expiredTime)
		{
			return null;
		}
		int creatureIndex = creature.CreatureIndex;
		if (creatureIndex == BattleConstant.EnvironmentCreatureInex)
		{
			return null;
		}
		List<IExtraSkillData> list = new List<IExtraSkillData>();
		for (int i = 0; i < _flaskSkillList.Count; i++)
		{
			AffixSkillDataBase affixSkillDataBase = _flaskSkillList[i];
			if (affixSkillDataBase != null && affixSkillDataBase.IsAddAble(teamType, creatureIndex))
			{
				list.Add(affixSkillDataBase);
				affectedCreatureList.Add(creature);
			}
		}
		return list;
	}

	public void Update(long globalTime)
	{
		if (_enabled && globalTime > _expiredTime)
		{
			RemoveAllFlaskSkill();
			ClearFlaskSkillList();
			_enabled = false;
		}
	}

	private void RemoveAllFlaskSkill()
	{
		foreach (BattleCreature affectedCreature in affectedCreatureList)
		{
			affectedCreature.StateController.ForceRemoveState("ShardGroup", 0);
		}
	}

	private void ClearFlaskSkillList()
	{
		_flaskSkillList.Clear();
	}
}
