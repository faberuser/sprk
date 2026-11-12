using NVespa;

namespace NShared;

public abstract class BaseOperationRuntime : SnapshotObject, IOperationRuntime, ISnapshotObject, IStateModifier, IStatObserver, IStateObserver
{
	protected readonly SnapshotObjectMember<OperationInfo> _operationHolder;

	protected readonly SnapshotObjectMember<StateData> _stateDataHolder;

	protected readonly SnapshotObjectMember<bool> _enabledHolder;

	public OperationInfo OperationInfo => _operationHolder.Get();

	public BaseOperationRuntime()
	{
		_operationHolder = new SnapshotObjectMember<OperationInfo>(this);
		_stateDataHolder = new SnapshotObjectMember<StateData>(this, null, GameRoot.Instance.StateDataContainer);
		_enabledHolder = new SnapshotObjectMember<bool>(this, value: false);
	}

	protected virtual void OnInit()
	{
	}

	protected virtual void OnBegin()
	{
	}

	protected virtual void OnEnd()
	{
	}

	protected virtual void OnTick()
	{
	}

	protected virtual void OnStackCountChanged()
	{
	}

	protected virtual bool IsValidState(long mainTimeMs)
	{
		return true;
	}

	public virtual string GetStateType()
	{
		return OperationInfo.State;
	}

	public virtual int GetAddStateCount()
	{
		return OperationInfo.AddStateCount;
	}

	public virtual void RecalculateOperationValue()
	{
	}

	private void UpdateOperationCondition()
	{
		bool flag = CheckConditionEnabled();
		if (flag != _enabledHolder.Get())
		{
			_enabledHolder.Set(flag);
			if (!flag && OperationInfo.HasOperationFlag(OperationFlag.ExpireOnConditionFail))
			{
				OperationInfo.Expired = true;
			}
			else
			{
				RecalculateOperationValue();
			}
		}
	}

	private bool CheckConditionEnabled()
	{
		if (OperationInfo == null)
		{
			return true;
		}
		FiredOperationInfo firedInfo = OperationInfo.FiredInfo;
		if (firedInfo == null)
		{
			return true;
		}
		OperationData operationData = OperationInfo.OperationData;
		if (operationData == null)
		{
			return true;
		}
		if (!OperationInfo.HasOperationFlag(OperationFlag.WatchCondition))
		{
			return true;
		}
		OperationConditionType conditionType = operationData.ConditionType;
		if (conditionType == OperationConditionType.None)
		{
			return true;
		}
		string[] conditionValues = operationData.ConditionValues;
		bool reverseCheck = operationData.ReverseCheck;
		return GameRoot.Instance.OperationConditionFactory.MeetCondition(conditionType, firedInfo, OperationInfo.Creature, conditionValues, reverseCheck);
	}

	void IOperationRuntime.Init(OperationInfo operationInfo)
	{
		_operationHolder.Set(operationInfo);
		string stateType = GetStateType();
		if (!string.IsNullOrEmpty(stateType))
		{
			StateData data = GameRoot.Instance.StateDataContainer.GetData(stateType);
			if (data != null)
			{
				_stateDataHolder.Set(data);
			}
			else
			{
				VespaDebug.LogError("StateData not found : " + stateType);
			}
		}
		OnInit();
	}

	void IOperationRuntime.OnBegin()
	{
		bool data = CheckConditionEnabled();
		_enabledHolder.Set(data);
		if (_stateDataHolder.Get() != null && EnumAttributeContainer<StateController.AddModifierResult, StateController.AddModifierResult_CancelOperationAttribute>.Has(OperationInfo.Creature.StateController.AddModifier(this, GetAddStateCount())))
		{
			OperationInfo.Expired = true;
		}
		else if (OperationInfo != null && OperationInfo.FiredInfo != null)
		{
			StatType[] recalculateValueByStats = OperationInfo.RecalculateValueByStats;
			if (recalculateValueByStats != null && recalculateValueByStats.Length != 0)
			{
				OperationInfo.Creature.StatController.AddStatObserver(this);
			}
			string[] recalculateValueByStates = OperationInfo.RecalculateValueByStates;
			if (recalculateValueByStates != null && recalculateValueByStates.Length != 0)
			{
				OperationInfo.Creature.StateController.AddStateObserver(this);
			}
			OnBegin();
		}
	}

	void IOperationRuntime.OnEnd()
	{
		if (_stateDataHolder.Get() != null)
		{
			OperationInfo.Creature.StateController.RemoveModifier(this);
		}
		StatType[] recalculateValueByStats = OperationInfo.RecalculateValueByStats;
		if (recalculateValueByStats != null && recalculateValueByStats.Length != 0)
		{
			OperationInfo.Creature.StatController.RemoveStatObserver(this);
		}
		string[] recalculateValueByStates = OperationInfo.RecalculateValueByStates;
		if (recalculateValueByStates != null && recalculateValueByStates.Length != 0)
		{
			OperationInfo.Creature.StateController.RemoveStateObserver(this);
		}
		OnEnd();
	}

	void IOperationRuntime.OnTick()
	{
		UpdateOperationCondition();
		OnTick();
	}

	StateData IStateModifier.GetStateData()
	{
		return _stateDataHolder.Get();
	}

	int IStateModifier.GetTickedTimeMs()
	{
		return OperationInfo.TickedTimeMs;
	}

	void IStateModifier.SetTickedTimeMs(int tickedTimeMs)
	{
		OperationInfo.TickedTimeMs = tickedTimeMs;
	}

	int IStateModifier.GetStackCount()
	{
		return OperationInfo.StackCount;
	}

	public void SetStackCount(int stackCount)
	{
		OperationInfo.StackCount = stackCount;
		OnStackCountChanged();
	}

	BattleCreature IStateModifier.GetFiredCreature()
	{
		return OperationInfo.FiredCreature;
	}

	int IStateModifier.GetSkillIndex()
	{
		return OperationInfo.SkillIndex;
	}

	long IStateModifier.GetOperationUid()
	{
		return OperationInfo.Uid;
	}

	int IStateModifier.GetOperationNo()
	{
		return OperationInfo.OperationNo;
	}

	bool IStateModifier.IsValid(long mainTimeMs)
	{
		return IsValidState(mainTimeMs);
	}

	OperationInfo IStateModifier.GetOperationInfo()
	{
		return OperationInfo;
	}

	int IStateModifier.GetDurationMs()
	{
		return OperationInfo.DurationMs;
	}

	int IStateModifier.GetRemainDurationMs()
	{
		return OperationInfo.DurationMs - OperationInfo.ElapsedTimeMs;
	}

	string IStateModifier.Dump()
	{
		return string.Empty;
	}

	void IStatObserver.OnStatChanged(StatType statType)
	{
		StatType[] recalculateValueByStats = OperationInfo.RecalculateValueByStats;
		if (recalculateValueByStats == null || recalculateValueByStats.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < recalculateValueByStats.Length; i++)
		{
			if (recalculateValueByStats[i] == statType)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			RecalculateOperationValue();
		}
	}

	void IStateObserver.OnStateChanged(string stateType)
	{
		string[] recalculateValueByStates = OperationInfo.RecalculateValueByStates;
		if (recalculateValueByStates == null || recalculateValueByStates.Length == 0)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < recalculateValueByStates.Length; i++)
		{
			if (recalculateValueByStates[i] == stateType)
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			RecalculateOperationValue();
		}
	}
}
