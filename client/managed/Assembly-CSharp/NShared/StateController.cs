using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NShared.StateCondition;

namespace NShared;

public class StateController : SnapshotObject
{
	public enum AddModifierResult
	{
		Success,
		Fail,
		Immune,
		[AddModifierResult_CancelOperation]
		ImmuneAndCancel,
		[AddModifierResult_CancelOperation]
		LongerModifierExist
	}

	public class AddModifierResult_CancelOperationAttribute : Attribute
	{
	}

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<SnapshotList<IStateModifier>> _stateModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotList<IStateDurationModifier>> _stateDurationModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotList<StateFlagModifier>> _stateFlagModifierListHolder;

	private readonly SnapshotObjectMember<SnapshotList<IStateObserver>> _stateObserverListHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	protected SnapshotList<IStateModifier> _stateModifierList => _stateModifierListHolder.Get();

	protected SnapshotList<IStateDurationModifier> _stateDurationModifierList => _stateDurationModifierListHolder.Get();

	protected SnapshotList<StateFlagModifier> _stateFlagModifierList => _stateFlagModifierListHolder.Get();

	public StateController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_stateModifierListHolder = new SnapshotObjectMember<SnapshotList<IStateModifier>>(this);
		_stateDurationModifierListHolder = new SnapshotObjectMember<SnapshotList<IStateDurationModifier>>(this);
		_stateFlagModifierListHolder = new SnapshotObjectMember<SnapshotList<StateFlagModifier>>(this);
		_stateObserverListHolder = new SnapshotObjectMember<SnapshotList<IStateObserver>>(this);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_stateModifierListHolder.Alloc();
		_stateDurationModifierListHolder.Alloc();
		_stateFlagModifierListHolder.Alloc();
		_stateObserverListHolder.Alloc();
	}

	public void Update()
	{
		RemoveInvalidModifier();
	}

	public void Clear()
	{
		ClearModifier();
	}

	public string Dump()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IStateModifier item in (IEnumerable<IStateModifier>)_stateModifierList)
		{
			stringBuilder.AppendFormat("{0} (by {1}): Stack={2}, 남은시간(ms)={3}\n", item.GetStateData().StateType, item.GetFiredCreature(), item.GetStackCount(), item.GetRemainDurationMs());
		}
		return stringBuilder.ToString();
	}

	public AddModifierResult AddModifier(IStateModifier stateModifier, int addStackCount = 0)
	{
		if (stateModifier == null)
		{
			return AddModifierResult.Fail;
		}
		BattleCreature firedCreature = stateModifier.GetFiredCreature();
		int skillIndex = stateModifier.GetSkillIndex();
		stateModifier.GetOperationUid();
		OperationInfo operationInfo = stateModifier.GetOperationInfo();
		StateData stateData = stateModifier.GetStateData();
		if (stateData == null)
		{
			return AddModifierResult.Fail;
		}
		string stateType = stateData.StateType;
		bool flag = false;
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data != null && !Owner.SkillController.IsUsingForceImmuneSkill())
		{
			if (data.HasCcFlag(SkillCcFlag.IgnoreImmune))
			{
				flag = true;
			}
			else if (operationInfo != null && operationInfo.HasOperationFlag(OperationFlag.IgnoreImmune))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			bool cancel = false;
			if (!string.IsNullOrEmpty(stateType) && IsImmuneToState(stateType, out cancel))
			{
				if (!cancel)
				{
					return AddModifierResult.Immune;
				}
				return AddModifierResult.ImmuneAndCancel;
			}
			string text = stateData.Group;
			if (!string.IsNullOrEmpty(text) && IsImmuneToState(text, out cancel))
			{
				if (!cancel)
				{
					return AddModifierResult.Immune;
				}
				return AddModifierResult.ImmuneAndCancel;
			}
		}
		int num = 0;
		using TemporaryList<IStateModifier> temporaryList = default(TemporaryList<IStateModifier>);
		int num2 = 0;
		int tickedTimeMs = 0;
		if (operationInfo != null && operationInfo.HasOperationFlag(OperationFlag.OverrideAllState))
		{
			List<IStateModifier> allStateModifier = GetAllStateModifier(stateType);
			if (allStateModifier != null && allStateModifier.Count > 0)
			{
				for (int i = 0; i < allStateModifier.Count; i++)
				{
					IStateModifier stateModifier2 = allStateModifier[i];
					num2 += stateModifier2.GetStackCount();
					tickedTimeMs = stateModifier2.GetTickedTimeMs();
					num = Math.Max(num, stateModifier2.GetRemainDurationMs());
					temporaryList.Add(stateModifier2);
				}
			}
		}
		else
		{
			IStateModifier stateOfFiredCreature = GetStateOfFiredCreature(stateType, firedCreature);
			if (stateOfFiredCreature != null)
			{
				num2 = stateOfFiredCreature.GetStackCount();
				tickedTimeMs = stateOfFiredCreature.GetTickedTimeMs();
				num = stateOfFiredCreature.GetRemainDurationMs();
				temporaryList.Add(stateOfFiredCreature);
			}
		}
		if (stateData.StackDurationRuleType == StateStackDurationRuleType.Max && num > stateModifier.GetDurationMs())
		{
			return AddModifierResult.LongerModifierExist;
		}
		for (int j = 0; j < temporaryList.Count; j++)
		{
			RemoveModifier(temporaryList[j]);
		}
		addStackCount = ((addStackCount <= 0) ? 1 : addStackCount);
		int num3 = stateData.MaxStack(firedCreature);
		num3 = ((num3 > 0) ? num3 : int.MaxValue);
		int stackCount = Math.Min(num2 + addStackCount, num3);
		stateModifier.SetStackCount(stackCount);
		stateModifier.SetTickedTimeMs(tickedTimeMs);
		if (!CanMagicalSkillState(firedCreature, stateData))
		{
			Owner.SkillController.CancelMagicalSkill(skillIndex);
		}
		if (!CanPhysicalSkillState(firedCreature, stateData))
		{
			Owner.SkillController.CancelPhysicalSkill(skillIndex);
		}
		if (!CanBaseAttackState(firedCreature, stateData))
		{
			Owner.SkillController.CancelBaseAttack();
			Owner.SkillController.ResetBaseAttack();
		}
		if (!CanMoveState(firedCreature, stateData))
		{
			Owner.TransformController.StopMove(raiseEvent: true);
		}
		_stateModifierList.Add(stateModifier);
		OnStateChanged(stateModifier);
		return AddModifierResult.Success;
	}

	public bool HasModifier(IStateModifier stateModifier)
	{
		return _stateModifierList.Contains(stateModifier);
	}

	public bool RemoveModifier(IStateModifier stateModifier)
	{
		if (stateModifier == null)
		{
			return false;
		}
		StateData stateData = stateModifier.GetStateData();
		if (stateData == null)
		{
			return false;
		}
		if (stateData.IsSticky)
		{
			return false;
		}
		if (_stateModifierList.Remove(stateModifier))
		{
			string stateType = stateData.StateType;
			long operationUid = stateModifier.GetOperationUid();
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			int skillIndex = stateModifier.GetSkillIndex();
			ChangeState changeState = new ChangeState();
			changeState.CreatureUid = Owner.Uid;
			changeState.OperationUid = operationUid;
			changeState.SkillIndex = skillIndex;
			changeState.StateAdded = false;
			changeState.ChangedStateType = stateType;
			changeState.FinalStateType = GetAllStateType();
			changeState.FiredCreatureUid = firedCreature?.Uid ?? 0;
			Owner.RaiseBattleEvent(changeState);
			Owner.OperationController.RemoveOperationInfo(operationUid);
			AnnounceStateChangedToObserver(stateType);
			return true;
		}
		return false;
	}

	private bool ForceRemoveModifier(IStateModifier stateModifier)
	{
		if (stateModifier == null)
		{
			return false;
		}
		StateData stateData = stateModifier.GetStateData();
		if (stateData == null)
		{
			return false;
		}
		if (_stateModifierList.Remove(stateModifier))
		{
			string stateType = stateData.StateType;
			long operationUid = stateModifier.GetOperationUid();
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			int skillIndex = stateModifier.GetSkillIndex();
			ChangeState changeState = new ChangeState();
			changeState.CreatureUid = Owner.Uid;
			changeState.OperationUid = operationUid;
			changeState.SkillIndex = skillIndex;
			changeState.StateAdded = false;
			changeState.ChangedStateType = stateType;
			changeState.FinalStateType = GetAllStateType();
			changeState.FiredCreatureUid = firedCreature?.Uid ?? 0;
			Owner.RaiseBattleEvent(changeState);
			Owner.OperationController.ForceRemoveOperationInfo(operationUid);
			AnnounceStateChangedToObserver(stateType);
			return true;
		}
		return false;
	}

	private void RemoveInvalidModifier()
	{
		using TemporaryList<IStateModifier> temporaryList = default(TemporaryList<IStateModifier>);
		long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
		foreach (IStateModifier item in (IEnumerable<IStateModifier>)_stateModifierList)
		{
			if (!item.IsValid(currentTimeMs))
			{
				temporaryList.Add(item);
			}
		}
		for (int i = 0; i < temporaryList.Count; i++)
		{
			IStateModifier stateModifier = temporaryList[i];
			RemoveModifier(stateModifier);
		}
	}

	private void ClearModifier()
	{
		IStateModifier[] array = _stateModifierList.ToArray();
		foreach (IStateModifier stateModifier in array)
		{
			RemoveModifier(stateModifier);
		}
	}

	public void ChangeModifierStateCount(string stateType, BattleCreature firedCreature, int count)
	{
		if (count == 0)
		{
			return;
		}
		IList<IStateModifier> list = null;
		if (firedCreature != null)
		{
			IStateModifier stateOfFiredCreature = Owner.StateController.GetStateOfFiredCreature(stateType, firedCreature);
			if (stateOfFiredCreature == null)
			{
				return;
			}
			list = new IStateModifier[1] { stateOfFiredCreature };
		}
		else
		{
			list = Owner.StateController.GetAllStateModifier(stateType);
		}
		if (list == null || list.Count == 0)
		{
			return;
		}
		if (count > 0)
		{
			int num = count;
			for (int i = 0; i < list.Count; i++)
			{
				if (num <= 0)
				{
					break;
				}
				IStateModifier stateModifier = list[i];
				StateData stateData = stateModifier.GetStateData();
				if (stateData != null)
				{
					int stackCount = stateModifier.GetStackCount();
					int num2 = stateData.MaxStack(firedCreature);
					num2 = ((num2 > 0) ? num2 : int.MaxValue);
					int num3 = Math.Min(Math.Max(num2 - stackCount, 0), num);
					if (num3 != 0)
					{
						num -= num3;
						int stackCount2 = stackCount + num3;
						stateModifier.SetStackCount(stackCount2);
						OnStateChanged(stateModifier);
					}
				}
			}
			return;
		}
		int num4 = -count;
		for (int j = 0; j < list.Count; j++)
		{
			if (num4 <= 0)
			{
				break;
			}
			IStateModifier stateModifier2 = list[j];
			int stackCount3 = stateModifier2.GetStackCount();
			int num5 = Math.Min(stackCount3, num4);
			if (num5 != 0)
			{
				num4 -= num5;
				int num6 = stackCount3 - num5;
				if (num6 > 0)
				{
					stateModifier2.SetStackCount(num6);
					OnStateChanged(stateModifier2);
				}
				else
				{
					RemoveModifier(stateModifier2);
				}
			}
		}
	}

	public void OnStateChanged(IStateModifier stateModifier)
	{
		if (stateModifier == null)
		{
			return;
		}
		StateData stateData = stateModifier.GetStateData();
		if (stateData != null)
		{
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			int skillIndex = stateModifier.GetSkillIndex();
			string stateType = stateData.StateType;
			int stackCount = stateModifier.GetStackCount();
			ChangeState changeState = new ChangeState();
			changeState.CreatureUid = Owner.Uid;
			changeState.OperationUid = stateModifier.GetOperationUid();
			changeState.SkillIndex = skillIndex;
			changeState.StateAdded = true;
			changeState.ChangedStateType = stateType;
			changeState.FinalStateType = GetAllStateType();
			changeState.StackCount = stackCount;
			changeState.FiredCreatureUid = firedCreature?.Uid ?? 0;
			changeState.StackDurationRuleType = stateData.StackDurationRuleType;
			changeState.DurationMs = stateModifier.GetDurationMs();
			Owner.RaiseBattleEvent(changeState);
			AnnounceStateChangedToObserver(stateType);
			StackCountEqual condition = new StackCountEqual(stateType, stackCount);
			GameRoot.Instance.StateActionFactory.Process(Owner, condition, stateModifier);
			OnState onState = SnapshotObjectPool<OnState>.Alloc();
			onState.TriggerCreature = firedCreature;
			onState.TargetCreatures = new BattleCreature[1] { Owner };
			onState.SkillIndex = skillIndex;
			onState.StateType = stateType;
			onState.StateGroup = stateData.Group;
			Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onState);
			if (stateData.IsPositive(firedCreature))
			{
				OnBuff onBuff = SnapshotObjectPool<OnBuff>.Alloc();
				onBuff.TriggerCreature = firedCreature;
				onBuff.TargetCreatures = new BattleCreature[1] { Owner };
				onBuff.SkillIndex = skillIndex;
				Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onBuff);
			}
			if (stateData.IsNegative(firedCreature))
			{
				OnDebuff onDebuff = SnapshotObjectPool<OnDebuff>.Alloc();
				onDebuff.TriggerCreature = firedCreature;
				onDebuff.TargetCreatures = new BattleCreature[1] { Owner };
				onDebuff.SkillIndex = skillIndex;
				Owner.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onDebuff);
			}
		}
	}

	public void AddStateObserver(IStateObserver observer)
	{
		if (observer != null && !_stateObserverListHolder.Get().Contains(observer))
		{
			_stateObserverListHolder.Get().Add(observer);
		}
	}

	public bool RemoveStateObserver(IStateObserver observer)
	{
		return _stateObserverListHolder.Get().Remove(observer);
	}

	private void AnnounceStateChangedToObserver(string stateType)
	{
		for (int i = 0; i < _stateObserverListHolder.Get().Count; i++)
		{
			_stateObserverListHolder.Get()[i].OnStateChanged(stateType);
		}
	}

	public void AddDurationModifier(IStateDurationModifier modifier)
	{
		_stateDurationModifierList.Add(modifier);
	}

	public void RemoveDurationModifier(IStateDurationModifier modifier)
	{
		_stateDurationModifierList.Remove(modifier);
	}

	public int GetAddedStateDurationMs(string stateType)
	{
		StateData data = GameRoot.Instance.StateDataContainer.GetData(stateType);
		int num = 0;
		for (int i = 0; i < _stateDurationModifierList.Count; i++)
		{
			IStateDurationModifier stateDurationModifier = _stateDurationModifierList[i];
			string[] stateType2 = stateDurationModifier.GetStateType();
			StateFlag stateFlag = stateDurationModifier.GetStateFlag();
			if ((stateType2 != null && stateType2.Contains(stateType)) || (data != null && (data.GetStateFlag() & stateFlag) != StateFlag.None))
			{
				num += stateDurationModifier.GetAddedDurationMs();
			}
		}
		return num;
	}

	public int GetMultedStateDurationMs(string stateType)
	{
		StateData data = GameRoot.Instance.StateDataContainer.GetData(stateType);
		int num = 0;
		for (int i = 0; i < _stateDurationModifierList.Count; i++)
		{
			IStateDurationModifier stateDurationModifier = _stateDurationModifierList[i];
			string[] stateType2 = stateDurationModifier.GetStateType();
			StateFlag stateFlag = stateDurationModifier.GetStateFlag();
			if ((stateType2 != null && stateType2.Contains(stateType)) || (data != null && (data.GetStateFlag() & stateFlag) != StateFlag.None))
			{
				num += stateDurationModifier.GetMultedDurationMs();
			}
		}
		return num;
	}

	public void AddModifier(StateFlagModifier modifier)
	{
		if (!_stateFlagModifierList.Contains(modifier))
		{
			_stateFlagModifierList.Add(modifier);
		}
	}

	public bool RemoveModifier(StateFlagModifier modifier)
	{
		return _stateFlagModifierList.Remove(modifier);
	}

	private StateFlagModifier FindStateFlagModifier(string stateType, StateFlag flag)
	{
		StateFlagModifier result = null;
		for (int i = 0; i < _stateFlagModifierList.Count; i++)
		{
			StateFlagModifier stateFlagModifier = _stateFlagModifierList[i];
			if (string.Equals(stateFlagModifier.State, stateType, StringComparison.OrdinalIgnoreCase) && (stateFlagModifier.Flag & flag) != StateFlag.None)
			{
				result = stateFlagModifier;
			}
		}
		return result;
	}

	public string GetModifiedStateValue(string stateType, StateFlag flag, string originalValue)
	{
		StateFlagModifier stateFlagModifier = FindStateFlagModifier(stateType, flag);
		if (stateFlagModifier == null)
		{
			return originalValue;
		}
		return stateFlagModifier.Value;
	}

	public bool GetModifiedStateValue(string stateType, StateFlag flag, bool originalValue)
	{
		StateFlagModifier stateFlagModifier = FindStateFlagModifier(stateType, flag);
		if (stateFlagModifier == null)
		{
			return originalValue;
		}
		return string.Equals(stateFlagModifier.Value, "true", StringComparison.OrdinalIgnoreCase);
	}

	public long GetModifiedStateValue(string stateType, StateFlag flag, long originalValue)
	{
		StateFlagModifier stateFlagModifier = FindStateFlagModifier(stateType, flag);
		if (stateFlagModifier == null)
		{
			return originalValue;
		}
		long.TryParse(stateFlagModifier.Value, out var result);
		return result;
	}

	public int GetModifiedStateValue(string stateType, StateFlag flag, int originalValue)
	{
		return (int)GetModifiedStateValue(stateType, flag, (long)originalValue);
	}

	public short GetModifiedStateValue(string stateType, StateFlag flag, short originalValue)
	{
		return (short)GetModifiedStateValue(stateType, flag, (long)originalValue);
	}

	public sbyte GetModifiedStateValue(string stateType, StateFlag flag, sbyte originalValue)
	{
		return (sbyte)GetModifiedStateValue(stateType, flag, (long)originalValue);
	}

	public string[] GetAllStateType()
	{
		using TemporaryList<string> temporaryList = default(TemporaryList<string>);
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			string stateType = _stateModifierList[i].GetStateData().StateType;
			if (!temporaryList.Contains(stateType))
			{
				temporaryList.Add(stateType);
			}
		}
		return temporaryList.ToArray();
	}

	public IStateModifier GetStateOfFiredCreature(string stateType, BattleCreature firedCreature)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier.GetStateData().StateType == stateType && stateModifier.GetFiredCreature() == firedCreature)
			{
				return stateModifier;
			}
		}
		return null;
	}

	public List<IStateModifier> GetAllStateModifier(string stateType)
	{
		List<IStateModifier> list = new List<IStateModifier>();
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier.GetStateData().StateType == stateType)
			{
				list.Add(stateModifier);
			}
		}
		return list;
	}

	public int GetManaAddictLevel()
	{
		int num = 0;
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			StateData stateData = _stateModifierList[i].GetStateData();
			if (num < 1 && stateData.StateType == "ManaAddict_Lv1")
			{
				num = 1;
			}
			if (num < 2 && stateData.StateType == "ManaAddict_Lv2")
			{
				num = 2;
			}
			if (num < 3 && stateData.StateType == "ManaAddict_Lv3")
			{
				num = 3;
			}
		}
		return num;
	}

	public IStateModifier GetGroupStateOfFiredCreature(string stateGroup, BattleCreature firedCreature)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier.GetStateData().Group == stateGroup && stateModifier.GetFiredCreature() == firedCreature)
			{
				return stateModifier;
			}
		}
		return null;
	}

	public IList<IStateModifier> GetAllStateModifier(Func<IStateModifier, bool> match)
	{
		List<IStateModifier> list = null;
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (match(stateModifier))
			{
				if (list == null)
				{
					list = new List<IStateModifier>();
				}
				list.Add(stateModifier);
			}
		}
		return list;
	}

	public int GetStateStackCount(string stateType, BattleCreature firedCreature = null)
	{
		int num = 0;
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (firedCreature == null || stateModifier.GetFiredCreature() == firedCreature)
			{
				StateData stateData = stateModifier.GetStateData();
				if (stateData.StateType == stateType || stateData.Group == stateType)
				{
					num += stateModifier.GetStackCount();
				}
			}
		}
		return num;
	}

	public int GetStateMaxStackCount(string stateType, BattleCreature firedCreature = null)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			StateData stateData = _stateModifierList[i].GetStateData();
			if (stateData.StateType == stateType)
			{
				return stateData.MaxStack(firedCreature);
			}
		}
		return 0;
	}

	public IStateModifier PopStateOfFiredCreature(string stateType, BattleCreature firedCreature)
	{
		IStateModifier stateOfFiredCreature = GetStateOfFiredCreature(stateType, firedCreature);
		if (stateOfFiredCreature != null)
		{
			RemoveModifier(stateOfFiredCreature);
		}
		return stateOfFiredCreature;
	}

	public int ClearState(string stateType)
	{
		return RemoveState(stateType, 0);
	}

	public int RemoveState(string stateType, int maxCount)
	{
		List<IStateModifier> list = new List<IStateModifier>();
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			StateData stateData = stateModifier.GetStateData();
			if (stateData.StateType == stateType || stateData.Group == stateType)
			{
				list.Add(stateModifier);
				if (maxCount > 0 && list.Count >= maxCount)
				{
					break;
				}
			}
		}
		foreach (IStateModifier item in list)
		{
			RemoveModifier(item);
		}
		return list.Count;
	}

	public int ForceRemoveState(string stateType, int maxCount)
	{
		List<IStateModifier> list = new List<IStateModifier>();
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			StateData stateData = stateModifier.GetStateData();
			if (stateData.StateType == stateType || stateData.Group == stateType)
			{
				list.Add(stateModifier);
				if (maxCount > 0 && list.Count >= maxCount)
				{
					break;
				}
			}
		}
		foreach (IStateModifier item in list)
		{
			ForceRemoveModifier(item);
		}
		return list.Count;
	}

	public int RemoveNegativeState()
	{
		List<IStateModifier> list = new List<IStateModifier>();
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			StateData stateData = stateModifier.GetStateData();
			if (stateData != null && stateData.IsNegative(firedCreature))
			{
				list.Add(stateModifier);
			}
		}
		foreach (IStateModifier item in list)
		{
			RemoveModifier(item);
		}
		return list.Count;
	}

	public int GetNegativeStateCount()
	{
		int num = 0;
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			StateData stateData = stateModifier.GetStateData();
			if (stateData != null && stateData.IsNegative(firedCreature))
			{
				num++;
			}
		}
		return num;
	}

	public int RemovePositiveState()
	{
		List<IStateModifier> list = new List<IStateModifier>();
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			StateData stateData = stateModifier.GetStateData();
			if (stateData != null && stateData.IsPositive(firedCreature))
			{
				long operationUid = stateModifier.GetOperationUid();
				OperationInfo operationInfo = Owner.OperationController.GetOperationInfo(operationUid);
				if (operationInfo != null && !operationInfo.IsInfinitePassiveOperation())
				{
					list.Add(stateModifier);
				}
			}
		}
		foreach (IStateModifier item in list)
		{
			RemoveModifier(item);
		}
		return list.Count;
	}

	public void AddState(StateData stateData, int durationMs, BattleCreature triggeredCreature, int stackCount = 0)
	{
		if (stateData != null)
		{
			long expireTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs() + durationMs;
			DelegateStateModifier stateModifier = new DelegateStateModifier
			{
				StateData = stateData,
				FiredCreature = triggeredCreature,
				ExpireTimeMs = expireTimeMs
			};
			AddModifier(stateModifier, stackCount);
		}
	}

	public bool IsImmuneToState(string stateType)
	{
		bool cancel;
		return IsImmuneToState(stateType, out cancel);
	}

	public bool IsImmuneToState(string stateType, out bool cancel)
	{
		cancel = false;
		bool flag = false;
		StateData data = GameRoot.Instance.StateDataContainer.GetData(stateType);
		for (int i = 0; i < _stateDurationModifierList.Count; i++)
		{
			IStateDurationModifier stateDurationModifier = _stateDurationModifierList[i];
			ImmuneResult immuneResult = stateDurationModifier.IsImmune();
			if (immuneResult != ImmuneResult.None)
			{
				string[] stateType2 = stateDurationModifier.GetStateType();
				StateFlag stateFlag = stateDurationModifier.GetStateFlag();
				if ((stateType2 != null && stateType2.Contains(stateType)) || (data != null && data.HasFlag(stateFlag, Owner)))
				{
					flag = true;
					cancel = cancel || immuneResult == ImmuneResult.ImmuneAndCancel;
				}
			}
		}
		if (flag)
		{
			return true;
		}
		UsingSkillInfo usingSkillInfo = Owner.SkillController.UsingSkillInfo;
		if (usingSkillInfo != null)
		{
			string[] ignoreStateTypes = usingSkillInfo.IgnoreStateTypes;
			if (ignoreStateTypes != null && ignoreStateTypes.Contains(stateType))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInState(string stateType)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			StateData stateData = _stateModifierList[i].GetStateData();
			if (stateData.StateType == stateType || stateData.Group == stateType)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInAnyState(string[] stateTypes)
	{
		if (stateTypes == null || stateTypes.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < stateTypes.Length; i++)
		{
			if (IsInState(stateTypes[i]))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckAnyState(Func<IStateModifier, bool> checker)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier arg = _stateModifierList[i];
			if (checker(arg))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckAnyState(Func<BattleCreature, StateData, bool> checker)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier != null)
			{
				BattleCreature firedCreature = stateModifier.GetFiredCreature();
				StateData stateData = stateModifier.GetStateData();
				if (checker(firedCreature, stateData))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsImmuneToPhysicalCc()
	{
		return CheckAnyState((IStateModifier modifier) => !CanBePhysicalCcState(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool IsImmuneToMagicalCc()
	{
		return CheckAnyState((IStateModifier modifier) => !CanBeMagicalCcState(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool CannotMoveState()
	{
		return CheckAnyState((IStateModifier modifier) => !CanMoveState(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool IsImmuneToPhysicalAttack()
	{
		return CheckAnyState((IStateModifier modifier) => !CanBePhysicalDamagedState(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool IsImmuneToMagicalAttack()
	{
		return CheckAnyState((IStateModifier modifier) => !CanBeMagicalDamagedState(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool IsPhysicalAttackBlinded()
	{
		return CheckAnyState((IStateModifier modifier) => !CanHitPhysicalAttack(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool IsMagicalAttackBlinded()
	{
		return CheckAnyState((IStateModifier modifier) => !CanHitMagicalAttack(modifier.GetFiredCreature(), modifier.GetStateData()));
	}

	public bool CanPhysicalBlockAll()
	{
		return CheckAnyState((BattleCreature firedCreature, StateData stateData) => stateData?.CanPhysicalBlockAll(firedCreature) ?? false);
	}

	public bool CanMagicalBlockAll()
	{
		return CheckAnyState((BattleCreature firedCreature, StateData stateData) => stateData?.CanMagicalBlockAll(firedCreature) ?? false);
	}

	public bool CanPhysicalDodgeAll()
	{
		return CheckAnyState((BattleCreature firedCreature, StateData stateData) => stateData?.CanPhysicalDodgeAll(firedCreature) ?? false);
	}

	public bool CanMagicalDodgeAll()
	{
		return CheckAnyState((BattleCreature firedCreature, StateData stateData) => stateData?.CanMagicalDodgeAll(firedCreature) ?? false);
	}

	public bool CanUsePassiveSkill()
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier != null && !stateModifier.GetStateData().CanPassiveSkill)
			{
				return false;
			}
		}
		return true;
	}

	public bool RejectReservedSkill()
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier != null && stateModifier.GetStateData().RejectReservedSkill)
			{
				return true;
			}
		}
		return false;
	}

	public bool CheckSkillState(SkillData skillData, bool isBaseAttack)
	{
		if (skillData == null)
		{
			return false;
		}
		SkillAttrType attrType = skillData.AttrType;
		string[] disabledStateType = skillData.DisabledStateType;
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			StateData stateData = stateModifier.GetStateData();
			string stateType = stateData.StateType;
			string text = stateData.Group;
			BattleCreature firedCreature = stateModifier.GetFiredCreature();
			HashSet<string> ignoreStateTypeList = skillData.GetIgnoreStateTypeList(Owner);
			if (ignoreStateTypeList == null || ((string.IsNullOrEmpty(stateType) || !ignoreStateTypeList.Contains(stateType)) && (string.IsNullOrEmpty(text) || !ignoreStateTypeList.Contains(text))))
			{
				if (disabledStateType != null && ((!string.IsNullOrEmpty(stateType) && disabledStateType.Contains(stateType)) || (!string.IsNullOrEmpty(text) && disabledStateType.Contains(text))))
				{
					return false;
				}
				if (attrType == SkillAttrType.Magical && !CanMagicalSkillState(firedCreature, stateData))
				{
					return false;
				}
				if (attrType == SkillAttrType.Physical && !CanPhysicalSkillState(firedCreature, stateData))
				{
					return false;
				}
				if (isBaseAttack && !CanBaseAttackState(firedCreature, stateData))
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool CanBeTargetedState(BattleCreature firedCreature)
	{
		if (firedCreature == null)
		{
			return true;
		}
		bool isAlly = firedCreature.TeamController.IsFriendly(Owner.TeamController);
		return CanBeTargetedState(isAlly);
	}

	public bool CanBeTargetedState(bool isAlly)
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			StateData stateData = stateModifier.GetStateData();
			if ((isAlly && !CanBeTargetedByAllyState(stateModifier.GetFiredCreature(), stateData)) || (!isAlly && !CanBeTargetedByEnemyState(stateModifier.GetFiredCreature(), stateData)))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanUpdateCooltime()
	{
		for (int i = 0; i < _stateModifierList.Count; i++)
		{
			IStateModifier stateModifier = _stateModifierList[i];
			if (stateModifier != null)
			{
				StateData stateData = stateModifier.GetStateData();
				if (!CanUpdateCooltime(Owner, stateData))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool CanMoveState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanMove(firedCreature) ?? true;
	}

	public static bool CanPhysicalSkillState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanPhysicalSkill(firedCreature) ?? true;
	}

	public static bool CanMagicalSkillState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanMagicalSkill(firedCreature) ?? true;
	}

	public static bool CanBaseAttackState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBaseAttack(firedCreature) ?? true;
	}

	public static bool CanBeTargetedByAllyState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBeTargetedByAlly(firedCreature) ?? true;
	}

	public static bool CanBeTargetedByEnemyState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBeTargetedByEnemy(firedCreature) ?? true;
	}

	public static bool CanBePhysicalCcState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBePhysicalCc(firedCreature) ?? true;
	}

	public static bool CanBeMagicalCcState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBeMagicalCc(firedCreature) ?? true;
	}

	public static bool CanBePhysicalDamagedState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBePhysicalDamaged(firedCreature) ?? true;
	}

	public static bool CanBeMagicalDamagedState(BattleCreature firedCreature, StateData data)
	{
		return data?.CanBeMagicalDamaged(firedCreature) ?? true;
	}

	public static bool CanHitPhysicalAttack(BattleCreature firedCreature, StateData data)
	{
		return data?.CanHitPhysicalAttack(firedCreature) ?? true;
	}

	public static bool CanHitMagicalAttack(BattleCreature firedCreature, StateData data)
	{
		return data?.CanHitMagicalAttack(firedCreature) ?? true;
	}

	public static bool IsCcState(BattleCreature firedCreature, StateData data)
	{
		return data?.IsCc(firedCreature) ?? false;
	}

	public static bool CanUpdateCooltime(BattleCreature firedCreature, StateData data)
	{
		return data?.CanUpdateCooltime(firedCreature) ?? true;
	}
}
