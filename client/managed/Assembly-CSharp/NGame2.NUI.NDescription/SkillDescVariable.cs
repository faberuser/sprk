using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NShared;
using NVespa;

namespace NGame2.NUI.NDescription;

public class SkillDescVariable : SmallPooling<SkillDescVariable>, IVariable, ISmallPoolingItem
{
	public SkillData SkillData;

	public int CreatureIndex;

	public int CreatureStar;

	public int CreatureTranscended;

	public int SkillLevel;

	public StatData MyselfStat;

	private bool _reservedMemberVariable;

	private RuntimeInstance _runtime;

	private StringKeyDictionary<IVariableValueBase> _dictionary = new StringKeyDictionary<IVariableValueBase>();

	void ISmallPoolingItem.OnCreateNew()
	{
		ReserveMemberVariable();
	}

	void ISmallPoolingItem.OnDestroy()
	{
	}

	void ISmallPoolingItem.OnCreate()
	{
	}

	void ISmallPoolingItem.OnRelease()
	{
		SkillData = null;
		MyselfStat = null;
		_runtime = null;
	}

	private void ReserveMemberVariable()
	{
		if (!_reservedMemberVariable)
		{
			_reservedMemberVariable = true;
			_dictionary.Add(VariableKeyword.SkillIndex, new ValueGenericFunc<decimal>(GetSkillIndex));
			_dictionary.Add(VariableKeyword.SkillLevel, new ValueGenericFunc<decimal>(GetSkillLevel));
			_dictionary.Add(VariableKeyword.CreatureIndex, new ValueGenericFunc<decimal>(GetCreatureIndex));
			_dictionary.Add(VariableKeyword.CreatureStar, new ValueGenericFunc<decimal>(GetCreatureStar));
			_dictionary.Add(VariableKeyword.CreatureTranscended, new ValueGenericFunc<decimal>(GetCreatureTranscended));
			_dictionary.Add(VariableKeyword.MyselfStat, new ValueGenericFunc<object>(GetMyselfStat));
		}
	}

	public bool Open(StatData statData, int creatureIndex, int creatureStar, int creatureEnhanced, int skillIndex, int skillLevel)
	{
		SkillData = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (SkillData == null)
		{
			return false;
		}
		CreatureIndex = creatureIndex;
		CreatureStar = creatureStar;
		CreatureTranscended = creatureEnhanced;
		SkillLevel = skillLevel;
		MyselfStat = statData;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(creatureIndex);
		if (data != null)
		{
			int skillSlotByIndex = data.GetSkillSlotByIndex(skillIndex, includeLinkedSkill: true);
			int equipOptionAddedSkillLevel = StatCalculator.GetEquipOptionAddedSkillLevel(statData, skillSlotByIndex);
			SkillLevel += equipOptionAddedSkillLevel;
		}
		return true;
	}

	void IVariable.Open(RuntimeInstance runtime)
	{
		_runtime = runtime;
	}

	IVariable IVariable.CaptureSelf()
	{
		return SmallPooling<SkillDescVariable>.CaptureInstance(this);
	}

	void IVariable.ReleaseSelf()
	{
		SmallPooling<SkillDescVariable>.ReleaseInstance(this);
	}

	RuntimeInstance IVariable.GetRuntimeInstance()
	{
		return _runtime;
	}

	IVariableValueBase IVariable.GetValueBase(StringKey key)
	{
		if (key.IsNullOrEmpty())
		{
			return null;
		}
		if (_dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return SingletonAuto<EnvironmentVariable>.instance.GetValueBase(key);
	}

	JsonData IVariable.DumpJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Variable Type", "SkillDesc");
		foreach (KeyValuePair<StringKey, IVariableValueBase> item in _dictionary.CreateDictionary())
		{
			JsonData value = item.Value.DumpJson();
			jsonData.AddField(item.Key.Value, value);
		}
		return jsonData;
	}

	private decimal GetCreatureIndex()
	{
		return CreatureIndex;
	}

	private decimal GetCreatureStar()
	{
		return CreatureStar;
	}

	private decimal GetCreatureTranscended()
	{
		return CreatureTranscended;
	}

	private decimal GetSkillIndex()
	{
		return (SkillData != null) ? SkillData.Index : 0;
	}

	private decimal GetSkillLevel()
	{
		return SkillLevel;
	}

	private StatData GetMyselfStat()
	{
		return MyselfStat;
	}
}
