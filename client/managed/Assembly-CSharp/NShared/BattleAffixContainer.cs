using System.Collections.Generic;
using NVespa;

namespace NShared;

public class BattleAffixContainer
{
	private readonly BattleInstance _battleInstance;

	private readonly List<AffixStatDataBase> _listStatAffixDataBase = new List<AffixStatDataBase>();

	private readonly List<AffixSkillDataBase> _listSkillAffixDataBase = new List<AffixSkillDataBase>();

	public BattleAffixContainer(BattleInstance battleInstance)
	{
		_listStatAffixDataBase = new List<AffixStatDataBase>();
		_listSkillAffixDataBase = new List<AffixSkillDataBase>();
		_battleInstance = battleInstance;
	}

	private int GetEnviromentCreatureIndex()
	{
		return BattleConstant.EnvironmentCreatureInex;
	}

	public void Set(int[] affixIndices, int infinityLevel)
	{
		_listSkillAffixDataBase.Clear();
		_listStatAffixDataBase.Clear();
		if (affixIndices == null || affixIndices.Length == 0)
		{
			return;
		}
		for (int i = 0; i < affixIndices.Length; i++)
		{
			int key = affixIndices[i];
			AffixData data = GameRoot.Instance.AffixDataContainer.GetData(key);
			if (data == null)
			{
				VespaDebug.LogError("[AffixContainer] AffixData is null AffixIndex :" + key);
				continue;
			}
			switch (data.AffixType)
			{
			case EAffixType.Skill:
			{
				AffixSkillDataBase item3 = CreateAffixSkillDataBase(data);
				_listSkillAffixDataBase.Add(item3);
				break;
			}
			case EAffixType.Stat:
			{
				AffixStatDataBase item2 = CreateAffixStatDataBase(data);
				_listStatAffixDataBase.Add(item2);
				break;
			}
			case EAffixType.InfinityStat:
			{
				AffixStatDataBase item = CreateInfinityAffixStatDataBase(data, infinityLevel);
				_listStatAffixDataBase.Add(item);
				break;
			}
			}
		}
	}

	public List<IExtraSkillData> GetSkillAffixDataBase(TeamType teamType, int heroIndex, long uid)
	{
		if (heroIndex == GetEnviromentCreatureIndex())
		{
			return null;
		}
		List<IExtraSkillData> list = new List<IExtraSkillData>();
		for (int i = 0; i < _listSkillAffixDataBase.Count; i++)
		{
			AffixSkillDataBase affixSkillDataBase = _listSkillAffixDataBase[i];
			if (affixSkillDataBase != null && affixSkillDataBase.IsAddAble(teamType, heroIndex))
			{
				list.Add(affixSkillDataBase);
			}
		}
		return list;
	}

	public List<IConditionalExtraStatData> GetStatAffixDataBase(TeamType teamType, int heroIndex, long uid)
	{
		if (heroIndex == GetEnviromentCreatureIndex())
		{
			return null;
		}
		List<IConditionalExtraStatData> list = new List<IConditionalExtraStatData>();
		for (int i = 0; i < _listStatAffixDataBase.Count; i++)
		{
			AffixStatDataBase affixStatDataBase = _listStatAffixDataBase[i];
			if (affixStatDataBase != null && affixStatDataBase.IsAddAble(teamType, heroIndex))
			{
				list.Add(affixStatDataBase);
			}
		}
		return list;
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

	private AffixStatDataBase CreateAffixStatDataBase(AffixData data)
	{
		return new AffixStatDataBase
		{
			Index = data.Index,
			TeamType = data.TeamType,
			HeroIndices = data.HeroIndices,
			OptionType = data.OptionType,
			OptionValue = data.OptionValue
		};
	}

	private AffixStatDataBase CreateInfinityAffixStatDataBase(AffixData data, int infinityLevel)
	{
		int infinityIndex = data.InfinityIndex;
		int infinityStep = data.GetInfinityStep(infinityLevel);
		int sumValue = GameRoot.Instance.InfinityDataContainer.GetSumValue(infinityIndex, infinityStep);
		return new AffixStatDataBase
		{
			Index = data.Index,
			TeamType = data.TeamType,
			HeroIndices = data.HeroIndices,
			OptionType = data.OptionType,
			OptionValue = sumValue
		};
	}

	internal void AddAffixAfterSet(int[] affixIndexes, int affixLevel)
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
				continue;
			}
			switch (data.AffixType)
			{
			case EAffixType.Skill:
			{
				AffixSkillDataBase item2 = CreateAffixSkillDataBase(data, affixLevel);
				_listSkillAffixDataBase.Add(item2);
				break;
			}
			case EAffixType.Stat:
			{
				AffixStatDataBase item = CreateAffixStatDataBase(data);
				_listStatAffixDataBase.Add(item);
				break;
			}
			}
		}
	}

	public List<AffixSkillDataBase> GetAffixSkillList()
	{
		return _listSkillAffixDataBase;
	}

	public List<AffixStatDataBase> GetAffixStatList()
	{
		return _listStatAffixDataBase;
	}
}
