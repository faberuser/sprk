using NVespa;

namespace NShared;

public class PassiveSkillContainer : SnapshotObject
{
	private readonly SnapshotObjectMember<IndexableSnapshotDictionary<int, PassiveSkillInfo>> _passiveSkillInfoDictionaryHolder;

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<SnapshotList<IPassiveToggler>> _togglerListHolder;

	private readonly SnapshotObjectMember<SnapshotList<int>> _disabledOnStartListHolder;

	private readonly SnapshotObjectMember<bool> _modifierUpdatedHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public SnapshotList<IPassiveToggler> PassiveToggler => _togglerListHolder.Get();

	public bool ModifierUpdated => _modifierUpdatedHolder.Get();

	public PassiveSkillContainer()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_passiveSkillInfoDictionaryHolder = new SnapshotObjectMember<IndexableSnapshotDictionary<int, PassiveSkillInfo>>(this);
		_togglerListHolder = new SnapshotObjectMember<SnapshotList<IPassiveToggler>>(this);
		_disabledOnStartListHolder = new SnapshotObjectMember<SnapshotList<int>>(this);
		_modifierUpdatedHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	public void Init(BattleCreature creature)
	{
		_ownerHolder.Set(creature);
		_passiveSkillInfoDictionaryHolder.Alloc();
		_togglerListHolder.Alloc();
		_disabledOnStartListHolder.Alloc();
		_modifierUpdatedHolder.Set(data: true);
	}

	public void Update(int deltaTimeMs)
	{
		UpdateOnStartSkillCondition();
		UpdateSkillTriggerTimeMs();
	}

	public PassiveSkillInfo Register(SkillData skillData, int skillLevel)
	{
		if (skillData == null)
		{
			VespaDebug.LogError("Registering null skill data.");
			return null;
		}
		int index = skillData.Index;
		if (_passiveSkillInfoDictionaryHolder.Get().ContainsKey(index))
		{
			VespaDebug.LogError("Duplicate passive skill : Creature=" + Owner.CreatureData.Name + " Skill=" + skillData);
			return null;
		}
		CreatureTagType creatureTagType = skillData.CreatureTagType;
		if (creatureTagType != CreatureTagType.None && Owner.CreatureData.TagType != creatureTagType)
		{
			return null;
		}
		long randomTriggerTimeMs = GetRandomTriggerTimeMs(skillData);
		PassiveSkillInfo passiveSkillInfo = SnapshotObjectPool<PassiveSkillInfo>.Alloc();
		passiveSkillInfo.Init(skillData, skillLevel, randomTriggerTimeMs);
		_passiveSkillInfoDictionaryHolder.Get().TryAdd(index, passiveSkillInfo);
		if (skillData.TriggerType == SkillTriggerType.OnStart && !_disabledOnStartListHolder.Get().Contains(index))
		{
			_disabledOnStartListHolder.Get().Add(index);
		}
		return passiveSkillInfo;
	}

	public bool Unregister(PassiveSkillInfo passiveSkillInfo)
	{
		if (passiveSkillInfo == null)
		{
			return false;
		}
		int skillIndex = passiveSkillInfo.SkillIndex;
		if (_passiveSkillInfoDictionaryHolder.Get().Remove(skillIndex))
		{
			_disabledOnStartListHolder.Get().Remove(skillIndex);
			return true;
		}
		return false;
	}

	public bool IsPassiveSkillEnabled(int skillIndex)
	{
		return !_disabledOnStartListHolder.Get().Contains(skillIndex);
	}

	public int GetPassiveSkillCount()
	{
		return _passiveSkillInfoDictionaryHolder.Get().Count;
	}

	public PassiveSkillInfo GetPassiveSkillByIndex(int index)
	{
		return _passiveSkillInfoDictionaryHolder.Get().GetValueByIndex(index);
	}

	public TemporaryList<PassiveSkillInfo> GetPassiveSkillsByTrigger(SkillTriggerType triggerType, bool enabledOnly = true)
	{
		TemporaryList<PassiveSkillInfo> result = default(TemporaryList<PassiveSkillInfo>);
		for (int i = 0; i < _passiveSkillInfoDictionaryHolder.Get().Count; i++)
		{
			PassiveSkillInfo valueByIndex = _passiveSkillInfoDictionaryHolder.Get().GetValueByIndex(i);
			if (valueByIndex.TriggerType == triggerType && (!enabledOnly || GetPassiveEnabled(valueByIndex)))
			{
				result.Add(valueByIndex);
			}
		}
		return result;
	}

	public PassiveSkillInfo GetPassiveSkill(int skillIndex)
	{
		for (int i = 0; i < _passiveSkillInfoDictionaryHolder.Get().Count; i++)
		{
			PassiveSkillInfo valueByIndex = _passiveSkillInfoDictionaryHolder.Get().GetValueByIndex(i);
			if (valueByIndex.SkillIndex == skillIndex)
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public void AddPassiveToggler(IPassiveToggler toggler)
	{
		if (toggler != null && !PassiveToggler.Contains(toggler))
		{
			PassiveToggler.Add(toggler);
		}
	}

	public bool RemovePassiveToggler(IPassiveToggler toggler)
	{
		if (toggler == null)
		{
			return false;
		}
		return PassiveToggler.Remove(toggler);
	}

	public bool GetPassiveEnabled(PassiveSkillInfo skillInfo)
	{
		if (skillInfo == null)
		{
			return false;
		}
		return GetPassiveEnabled(skillInfo.SkillData);
	}

	public bool GetPassiveEnabled(SkillData skillData)
	{
		if (!IsToggleEnabled(skillData))
		{
			return false;
		}
		if (!Owner.SkillController.IsConditionActive(Owner, skillData))
		{
			return false;
		}
		return true;
	}

	public bool IsToggleEnabled(SkillData skillData)
	{
		if (skillData == null)
		{
			return false;
		}
		bool result = !skillData.TriggerDisabled;
		for (int i = 0; i < PassiveToggler.Count; i++)
		{
			IPassiveToggler passiveToggler = PassiveToggler[i];
			if (passiveToggler.GetSkillIndex() == skillData.Index)
			{
				result = passiveToggler.GetEnabled();
			}
		}
		return result;
	}

	public void UpdateOnStartSkillCondition()
	{
		for (int i = 0; i < _passiveSkillInfoDictionaryHolder.Get().Count; i++)
		{
			PassiveSkillInfo valueByIndex = _passiveSkillInfoDictionaryHolder.Get().GetValueByIndex(i);
			if (valueByIndex == null || (valueByIndex.TriggerType != SkillTriggerType.OnStart && !valueByIndex.SkillData.HasTriggerFlag(SkillTriggerFlag.UpdateSkillCondition)))
			{
				continue;
			}
			int skillIndex = valueByIndex.SkillIndex;
			SkillData skillData = valueByIndex.SkillData;
			bool flag = !_disabledOnStartListHolder.Get().Contains(skillIndex);
			bool passiveEnabled = GetPassiveEnabled(valueByIndex);
			if (passiveEnabled == flag)
			{
				continue;
			}
			if (passiveEnabled)
			{
				OnStart onStart = SnapshotObjectPool<OnStart>.Alloc();
				onStart.TriggerCreature = Owner;
				_disabledOnStartListHolder.Get().Remove(skillIndex);
				Owner.SkillController.UseSkill(skillData, null, onStart);
				continue;
			}
			_disabledOnStartListHolder.Get().Add(skillIndex);
			BattleInstance battleInstance = Owner.BattleInstance;
			for (int j = 0; j < battleInstance.CreatureContainer.GetCreatureCount(); j++)
			{
				BattleCreature creatureByIndex = battleInstance.CreatureContainer.GetCreatureByIndex(j);
				Owner.CancelMyOperation(creatureByIndex, passiveOnly: false, skillData.Index);
			}
		}
	}

	public void UpdateSkillTriggerTimeMs()
	{
		if (_modifierUpdatedHolder != null && _modifierUpdatedHolder.Get())
		{
			if (Owner != null && Owner.SkillController != null && Owner.SkillController.ModifierContainer != null && Owner.SkillController.ModifierContainer.IsReadyModifier())
			{
				RecalculateTimerSkill();
			}
			_modifierUpdatedHolder.Set(data: false);
		}
	}

	public long GetRandomTriggerTimeMs(SkillData skillData)
	{
		int num = 1000;
		if (skillData != null && skillData.TriggerType == SkillTriggerType.OnTimer)
		{
			int value = 0;
			if (((ITriggerValueGetter)skillData).GetTriggerValue(Owner, 0, out value))
			{
				int value2 = 0;
				num = ((!((ITriggerValueGetter)skillData).GetTriggerValue(Owner, 1, out value2)) ? Owner.NextRandom(value, "RANDOM_TRIGGER_TIME " + skillData) : Owner.NextRandom(value, value2, "RANDOM_TRIGGER_TIME " + skillData));
			}
		}
		return num;
	}

	public void RecalculateTimerSkill()
	{
		using TemporaryList<PassiveSkillInfo> temporaryList = GetPassiveSkillsByTrigger(SkillTriggerType.OnTimer, enabledOnly: false);
		for (int i = 0; i < temporaryList.Count; i++)
		{
			PassiveSkillInfo passiveSkillInfo = temporaryList[i];
			passiveSkillInfo.TriggerStartTimeMs = GetRandomTriggerTimeMs(passiveSkillInfo.SkillData);
		}
	}

	public TemporaryList<SkillData> UpdateTriggeredTimerSkillList(int deltaTimeMs)
	{
		TemporaryList<SkillData> result = default(TemporaryList<SkillData>);
		using TemporaryList<PassiveSkillInfo> temporaryList = GetPassiveSkillsByTrigger(SkillTriggerType.OnTimer, enabledOnly: false);
		for (int i = 0; i < temporaryList.Count; i++)
		{
			PassiveSkillInfo passiveSkillInfo = temporaryList[i];
			SkillData skillData = passiveSkillInfo.SkillData;
			if (skillData == null)
			{
				continue;
			}
			if (skillData.HasTriggerFlag(SkillTriggerFlag.UseRelativeTimer))
			{
				if (GetPassiveEnabled(passiveSkillInfo))
				{
					passiveSkillInfo.UpdateTriggerTime(deltaTimeMs);
					if (passiveSkillInfo.IsTriggerTimePassed())
					{
						passiveSkillInfo.InitTriggerTime(GetRandomTriggerTimeMs(skillData));
						result.Add(skillData);
					}
				}
				continue;
			}
			passiveSkillInfo.UpdateTriggerTime(deltaTimeMs);
			if (passiveSkillInfo.IsTriggerTimePassed())
			{
				passiveSkillInfo.InitTriggerTime(GetRandomTriggerTimeMs(skillData));
				if (GetPassiveEnabled(passiveSkillInfo))
				{
					result.Add(skillData);
				}
			}
		}
		return result;
	}
}
