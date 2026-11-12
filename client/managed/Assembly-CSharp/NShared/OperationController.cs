using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NBattleTacticEvent;
using NShared.StateCondition;
using NVespa;

namespace NShared;

public class OperationController : SnapshotObject, IOperationController
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<IndexableSnapshotDictionary<long, OperationInfo>> _operationDictionaryHolder;

	private readonly SnapshotObjectMember<SnapshotList<DelayedOperationInfo>> _delayedOperationListHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public IndexableSnapshotDictionary<long, OperationInfo> OperationDictionary => _operationDictionaryHolder.Get();

	public SnapshotList<DelayedOperationInfo> DelayedOperationList => _delayedOperationListHolder.Get();

	public OperationController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_operationDictionaryHolder = new SnapshotObjectMember<IndexableSnapshotDictionary<long, OperationInfo>>(this);
		_delayedOperationListHolder = new SnapshotObjectMember<SnapshotList<DelayedOperationInfo>>(this);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_operationDictionaryHolder.Alloc();
		_delayedOperationListHolder.Alloc();
	}

	public void Clear(bool clearPassive)
	{
		List<long> list = new List<long>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (clearPassive || !valueByIndex.IsInfinitePassiveOperation())
			{
				list.Add(valueByIndex.Uid);
			}
		}
		foreach (long item in list)
		{
			RemoveOperationInfo(item);
		}
		for (int num = DelayedOperationList.Count - 1; num >= 0; num--)
		{
			OperationInfo operationInfo = DelayedOperationList[num].OperationInfo;
			if (clearPassive || !operationInfo.IsInfinitePassiveOperation())
			{
				DelayedOperationList.RemoveAt(num);
			}
		}
	}

	public void ClearEnemyOperationOnWaveEnd()
	{
		List<long> list = new List<long>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			long keyByIndex = OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (valueByIndex.FiredCreature.TeamController.TeamId != Owner.TeamController.TeamId && !valueByIndex.HasOperationFlag(OperationFlag.PersistOnWaveEnd))
			{
				list.Add(keyByIndex);
			}
		}
		foreach (long item in list)
		{
			RemoveOperationInfo(item);
		}
	}

	public void ClearExpireOnWaveEndOperation()
	{
		bool flag = Owner.BattleInstance.Snapshot.WaveIndex == Owner.BattleInstance.Snapshot.TotalWaveIndex;
		List<long> list = new List<long>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			long keyByIndex = OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (valueByIndex.HasOperationFlag(OperationFlag.ExpireOnWaveEnd))
			{
				list.Add(keyByIndex);
			}
			else if (flag && valueByIndex.HasOperationFlag(OperationFlag.ExpireOnBattleEnd))
			{
				list.Add(keyByIndex);
			}
		}
		foreach (long item in list)
		{
			RemoveOperationInfo(item);
		}
	}

	public List<OperationInfo> GetChildOperationList(long parentOperationUid)
	{
		List<OperationInfo> list = new List<OperationInfo>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (valueByIndex.Parent != null && valueByIndex.Parent.Uid == parentOperationUid)
			{
				list.Add(valueByIndex);
			}
		}
		return list;
	}

	public OperationResult Add(OperationInfo operationInfo)
	{
		if (operationInfo == null)
		{
			return OperationResult.Fail;
		}
		int beginTimeDelayMs = operationInfo.BeginTimeDelayMs;
		if (beginTimeDelayMs > 0)
		{
			return ReserveInternal(operationInfo, beginTimeDelayMs);
		}
		return AddInternal(operationInfo);
	}

	private OperationResult ReserveInternal(OperationInfo operationInfo, int delayMs)
	{
		DelayedOperationInfo delayedOperationInfo = SnapshotObjectPool<DelayedOperationInfo>.Alloc();
		delayedOperationInfo.OperationInfo = operationInfo;
		delayedOperationInfo.DelayMs = delayMs;
		delayedOperationInfo.ElapsedTimeMs = 0;
		DelayedOperationList.Add(delayedOperationInfo);
		return OperationResult.Success;
	}

	private OperationResult AddInternal(OperationInfo operationInfo)
	{
		if (operationInfo == null)
		{
			return OperationResult.Fail;
		}
		ISkillTriggerInfo triggerInfo = operationInfo.TriggerInfo;
		if ((triggerInfo == null || triggerInfo.TriggerType != SkillTriggerType.OnDie) && Owner.StatController.IsDead() && !OperationTypeAttribute.HasOperationFlag(operationInfo.Type, OperationApplyTargetFlag.Dead))
		{
			OnCancelOperation(operationInfo, CancelOperation.EType.DeadCreature);
			return OperationResult.Fail;
		}
		return Begin(operationInfo);
	}

	private OperationResult Begin(OperationInfo operationInfo)
	{
		if (operationInfo == null)
		{
			return OperationResult.Fail;
		}
		if (operationInfo.Uid == 0L)
		{
			VespaDebug.LogError("operation uid is zero!");
			return OperationResult.Fail;
		}
		if (operationInfo.Parent != null && !operationInfo.Parent.CanChildBeginOperation())
		{
			return OperationResult.ParentExpired;
		}
		if (operationInfo.DurationMs > 0)
		{
			operationInfo.InitFinalOperationDurationMs();
		}
		OperationPreBeginEvent operationPreBeginEvent = new OperationPreBeginEvent();
		operationPreBeginEvent.Owner = Owner;
		operationPreBeginEvent.OperationInfo = operationInfo;
		Owner.BattleInstance.Snapshot.TacticManager.SendEvent(operationPreBeginEvent);
		if (operationPreBeginEvent.TacticResult != OperationResult.Success)
		{
			return operationPreBeginEvent.TacticResult;
		}
		if (IsImmuneToCcOperation(operationInfo))
		{
			OnResistOperation(operationInfo);
			return OperationResult.Immune;
		}
		if (CheckResistCc(operationInfo, operationInfo.Runtime))
		{
			OnResistOperation(operationInfo);
			return OperationResult.Regist;
		}
		if (CheckResistDebuff(operationInfo, operationInfo.Type))
		{
			OnResistOperation(operationInfo);
			return OperationResult.Regist;
		}
		if (operationInfo.RangeCheck)
		{
			BattleCreature firedCreature = operationInfo.FiredCreature;
			int minRangeMm = operationInfo.SkillData.GetMinRangeMm(firedCreature);
			int maxRangeMm = operationInfo.SkillData.GetMaxRangeMm(firedCreature);
			if (BattleCreatureContainer.IsInSkillRange(firedCreature, Owner, minRangeMm, maxRangeMm) != SkillRangeResultType.Success)
			{
				OnCancelOperation(operationInfo, CancelOperation.EType.OutOfRange);
				return OperationResult.Fail;
			}
		}
		if (operationInfo.HasOperationFlag(OperationFlag.CheckConditionOnBegin))
		{
			FiredOperationInfo firedInfo = operationInfo.FiredInfo;
			if (!SkillController.CheckMeetCondition(firedInfo.FiredCreature.SkillController.ModifierContainer, operationInfo.OperationData, firedInfo, Owner))
			{
				OnCancelOperation(operationInfo, CancelOperation.EType.ConditionFail);
				return OperationResult.Fail;
			}
		}
		if (operationInfo.DurationMs > 0 || operationInfo.IsInfinitePassiveOperation())
		{
			if (!OperationDictionary.TryAdd(operationInfo.Uid, operationInfo))
			{
				VespaDebug.LogError("failed to add operation : no=" + operationInfo.Uid);
				return OperationResult.Fail;
			}
			OnBeginOperation(operationInfo);
		}
		else
		{
			OnBeginOperation(operationInfo);
			OnEndOperation(operationInfo);
		}
		return OperationResult.Success;
	}

	public bool IsImmuneToCcOperation(OperationInfo operationInfo)
	{
		if (operationInfo == null)
		{
			return false;
		}
		if (!(operationInfo.Runtime is IStateModifier stateModifier))
		{
			return false;
		}
		StateData stateData = stateModifier.GetStateData();
		if (stateData == null)
		{
			return false;
		}
		if (!StateController.IsCcState(stateModifier.GetFiredCreature(), stateData))
		{
			return false;
		}
		if (Owner.SkillController.IsUsingForceImmuneSkill())
		{
			return true;
		}
		if (operationInfo.SkillData.HasCcFlag(SkillCcFlag.IgnoreImmune))
		{
			return false;
		}
		if (operationInfo.HasOperationFlag(OperationFlag.IgnoreImmune))
		{
			return false;
		}
		SkillData skillData = operationInfo.SkillData;
		if (skillData != null)
		{
			SkillAttrType skillAttrType = skillData.AttrType;
			if (operationInfo.HasOperationFlag(OperationFlag.ForcePhysical))
			{
				skillAttrType = SkillAttrType.Physical;
			}
			else if (operationInfo.HasOperationFlag(OperationFlag.ForceMagical))
			{
				skillAttrType = SkillAttrType.Magical;
			}
			if (Owner.BattleInstance.DevParam_LegacySkillAttrCcCheck)
			{
				if (skillAttrType == SkillAttrType.Physical && Owner.StateController.IsImmuneToPhysicalCc())
				{
					return true;
				}
				if (skillAttrType == SkillAttrType.Magical && Owner.StateController.IsImmuneToMagicalCc())
				{
					return true;
				}
			}
			else
			{
				if ((skillAttrType == SkillAttrType.None || skillAttrType == SkillAttrType.Physical) && Owner.StateController.IsImmuneToPhysicalCc())
				{
					return true;
				}
				if ((skillAttrType == SkillAttrType.None || skillAttrType == SkillAttrType.Magical) && Owner.StateController.IsImmuneToMagicalCc())
				{
					return true;
				}
			}
		}
		string stateType = stateData.StateType;
		string stateType2 = stateData.Group;
		if (Owner.BattleCreatureData.CanIgnoreState(stateType) || Owner.BattleCreatureData.CanIgnoreState(stateType2))
		{
			return true;
		}
		if (Owner.StateController.IsImmuneToState(stateType) || Owner.StateController.IsImmuneToState(stateType2))
		{
			return true;
		}
		return false;
	}

	private bool CheckResistCc(OperationInfo operationInfo, IOperationRuntime operationRuntime)
	{
		if (operationInfo == null)
		{
			return false;
		}
		if (operationInfo.SkillData.HasCcFlag(SkillCcFlag.IgnoreResist))
		{
			return false;
		}
		if (operationInfo.HasOperationFlag(OperationFlag.IgnoreResist))
		{
			return false;
		}
		if (!(operationRuntime is IStateModifier stateModifier))
		{
			return false;
		}
		StateData stateData = stateModifier.GetStateData();
		if (stateData == null)
		{
			return false;
		}
		if (!StateController.IsCcState(stateModifier.GetFiredCreature(), stateData))
		{
			return false;
		}
		if (Owner.BattleInstance.AllResistCc)
		{
			return true;
		}
		long num = Owner.StatController.GetFinalStat(StatType.AntiCcChance);
		if (num <= 0)
		{
			return false;
		}
		int num2 = 1000;
		if (operationInfo.OperationData.HitChance > 0)
		{
			num2 = operationInfo.OperationData.HitChance;
		}
		if (operationInfo.FiredCreature != null)
		{
			long finalStat = operationInfo.FiredCreature.StatController.GetFinalStat(StatType.HitCcChance);
			num -= finalStat;
			num = System.Math.Max(num, 0L);
		}
		return !Owner.StatController.TestHitChanceRateResult(num2, num, operationInfo.SkillLevel, "CheckResistCc");
	}

	private bool CheckResistDebuff(OperationInfo operationInfo, OperationType operationType)
	{
		if (operationInfo != null && operationInfo.SkillData.HasCcFlag(SkillCcFlag.IgnoreResist))
		{
			return false;
		}
		if (operationInfo != null && operationInfo.HasOperationFlag(OperationFlag.IgnoreResist))
		{
			return false;
		}
		if (operationInfo == null || !operationInfo.HasOperationFlag(OperationFlag.Negative))
		{
			return false;
		}
		long num = Owner.StatController.GetFinalStat(StatType.AntiDebuffChance);
		if (num <= 0)
		{
			return false;
		}
		int num2 = 1000;
		if (operationInfo != null && operationInfo.OperationData.HitChance > 0)
		{
			num2 = operationInfo.OperationData.HitChance;
		}
		if (operationInfo.FiredCreature != null)
		{
			long finalStat = operationInfo.FiredCreature.StatController.GetFinalStat(StatType.HitDebuffChance);
			num -= finalStat;
			num = System.Math.Max(num, 0L);
		}
		return !Owner.StatController.TestHitChanceRateResult(num2, num, operationInfo.SkillLevel, "CheckRegistDebuff");
	}

	public void Update(int deltaTimeMs)
	{
		if (Owner.BattleInstance.IsBattleEnabled() && !Owner.IsResting)
		{
			UpdateDelayedOperation(deltaTimeMs);
			UpdateOperation(deltaTimeMs);
		}
	}

	private void UpdateOperation(int deltaTimeMs)
	{
		using TemporaryList<long> temporaryList = default(TemporaryList<long>);
		using TemporaryList<OperationInfo> temporaryList2 = default(TemporaryList<OperationInfo>);
		bool flag = Owner.BattleInstance.IsInSoloTime();
		if (BattleConstant.StopAllOperationInSoloTime && flag)
		{
			return;
		}
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			long keyByIndex = OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (Owner.BattleInstance.IsBattleClosingOrDisabled() && valueByIndex.FiredCreature != null && valueByIndex.FiredCreature.StatController.IsDead() && !valueByIndex.HasOperationFlag(OperationFlag.PersistOnWaveEnd))
			{
				temporaryList.Add(keyByIndex);
			}
			else
			{
				if ((flag && (!valueByIndex.IsInSoloTime() || valueByIndex.HasOperationFlag(OperationFlag.PauseOnSoloTime))) || deltaTimeMs == 0)
				{
					continue;
				}
				valueByIndex.Update(deltaTimeMs);
				if (valueByIndex.TickTimeMs > 0)
				{
					valueByIndex.TickedTimeMs += deltaTimeMs;
					if (valueByIndex.TickedTimeMs >= valueByIndex.TickTimeMs)
					{
						valueByIndex.TickedTimeMs = 0;
						temporaryList2.Add(valueByIndex);
						continue;
					}
				}
				if (!valueByIndex.IsInfinitePassiveOperation() && (valueByIndex.IsDurationEnded() || valueByIndex.Expired))
				{
					temporaryList.Add(keyByIndex);
					if (valueByIndex.State != null)
					{
						DurationExpired condition = new DurationExpired(valueByIndex.State);
						StateData data = GameRoot.Instance.StateDataContainer.GetData(valueByIndex.State);
						if (data != null)
						{
							DelegateStateModifier stateModifier = new DelegateStateModifier
							{
								StateData = data,
								FiredCreature = Owner
							};
							GameRoot.Instance.StateActionFactory.Process(Owner, condition, stateModifier);
						}
					}
				}
				else if (valueByIndex.Parent != null && valueByIndex.Parent.DurationMs > 0 && valueByIndex.Parent.Expired)
				{
					temporaryList.Add(keyByIndex);
				}
			}
		}
		for (int j = 0; j < temporaryList2.Count; j++)
		{
			OperationInfo operationInfo = temporaryList2[j];
			OnTickOperation(operationInfo);
		}
		for (int k = 0; k < temporaryList.Count; k++)
		{
			long operationUid = temporaryList[k];
			RemoveOperationInfo(operationUid);
		}
	}

	private void UpdateDelayedOperation(int deltaTimeMs)
	{
		using TemporaryList<DelayedOperationInfo> temporaryList = default(TemporaryList<DelayedOperationInfo>);
		for (int i = 0; i < DelayedOperationList.Count; i++)
		{
			DelayedOperationInfo delayedOperationInfo = DelayedOperationList[i];
			delayedOperationInfo.Update(deltaTimeMs);
			if (delayedOperationInfo.IsReady())
			{
				temporaryList.Add(delayedOperationInfo);
			}
		}
		for (int j = 0; j < temporaryList.Count; j++)
		{
			DelayedOperationInfo delayedOperationInfo2 = temporaryList[j];
			DelayedOperationList.Remove(delayedOperationInfo2);
			AddInternal(delayedOperationInfo2.OperationInfo);
		}
	}

	public List<OperationInfo> GetOperationListForBattleExplorer(int key)
	{
		List<OperationInfo> list = new List<OperationInfo>();
		List<OperationInfo> list2 = new List<OperationInfo>();
		List<OperationInfo> list3 = new List<OperationInfo>();
		bool flag = Owner.BattleInstance.IsInSoloTime();
		bool flag2 = Owner.BattleInstance.IsBattleClosingOrDisabled();
		IndexableSnapshotDictionary<long, OperationInfo> operationDictionary = OperationDictionary;
		int count = operationDictionary.Count;
		for (int i = 0; i < count; i++)
		{
			operationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = operationDictionary.GetValueByIndex(i);
			if ((flag2 && valueByIndex.FiredCreature != null && valueByIndex.FiredCreature.StatController.IsDead() && !valueByIndex.HasOperationFlag(OperationFlag.PersistOnWaveEnd)) || (flag && (!valueByIndex.IsInSoloTime() || valueByIndex.HasOperationFlag(OperationFlag.PauseOnSoloTime))))
			{
				continue;
			}
			if (valueByIndex.TickTimeMs > 0)
			{
				list.Add(valueByIndex);
				continue;
			}
			if (!valueByIndex.IsInfinitePassiveOperation())
			{
				if (valueByIndex.IsDurationEnded() || valueByIndex.Expired)
				{
					if (valueByIndex.State != null)
					{
						new DurationExpired(valueByIndex.State);
						if (GameRoot.Instance.StateDataContainer.GetData(valueByIndex.State) != null)
						{
						}
					}
					continue;
				}
				list3.Add(valueByIndex);
			}
			else
			{
				list2.Add(valueByIndex);
			}
			if (valueByIndex.Parent != null && valueByIndex.Parent.DurationMs > 0)
			{
				_ = valueByIndex.Parent.Expired;
			}
			else
				_ = 0;
		}
		return key switch
		{
			1 => list2, 
			2 => list3, 
			3 => list, 
			_ => list3, 
		};
	}

	public OperationInfo GetOperationInfo(long operationUid)
	{
		OperationDictionary.TryGetValue(operationUid, out var value);
		return value;
	}

	public OperationInfo FindOperationInfo(int skillIndex, int operationNo = 0)
	{
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (valueByIndex.SkillIndex == skillIndex && (operationNo == 0 || valueByIndex.OperationNo == operationNo))
			{
				return valueByIndex;
			}
		}
		return null;
	}

	public bool RemoveOperationInfo(long operationUid)
	{
		OperationInfo value = null;
		OperationDictionary.TryGetValue(operationUid, out value);
		if (value == null)
		{
			return false;
		}
		if (value.Runtime is IStateModifier stateModifier)
		{
			StateData stateData = stateModifier.GetStateData();
			if (stateData != null && stateData.IsSticky)
			{
				return false;
			}
		}
		if (OperationDictionary.TryRemove(operationUid, out value) && value != null)
		{
			OnEndOperation(value);
			return true;
		}
		return false;
	}

	public bool ForceRemoveOperationInfo(long operationUid)
	{
		OperationInfo value = null;
		OperationDictionary.TryGetValue(operationUid, out value);
		if (value == null)
		{
			return false;
		}
		if (value.Runtime is IStateModifier stateModifier && stateModifier.GetStateData() == null)
		{
			return false;
		}
		if (OperationDictionary.TryRemove(operationUid, out value) && value != null)
		{
			OnEndOperation(value);
			return true;
		}
		return false;
	}

	public int RemoveOperationOfFiredCreature(int firedCreatureUid, bool passiveOnly, int skillIndex = 0, int operationNo = 0, OperationType[] operationTypes = null)
	{
		List<long> list = new List<long>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			long keyByIndex = OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (CheckOperationConditionInternal(valueByIndex, firedCreatureUid, passiveOnly, skillIndex, operationNo, operationTypes))
			{
				list.Add(keyByIndex);
			}
		}
		int num = 0;
		foreach (long item in list)
		{
			RemoveOperationInfo(item);
			num++;
		}
		for (int num2 = DelayedOperationList.Count - 1; num2 >= 0; num2--)
		{
			OperationInfo operationInfo = DelayedOperationList[num2].OperationInfo;
			if (CheckOperationConditionInternal(operationInfo, firedCreatureUid, passiveOnly, skillIndex, operationNo, operationTypes))
			{
				DelayedOperationList.RemoveAt(num2);
			}
		}
		return num;
	}

	private bool CheckOperationConditionInternal(OperationInfo operationInfo, int firedCreatureUid, bool passiveOnly, int skillIndex, int operationNo, OperationType[] operationTypes)
	{
		if (operationInfo == null)
		{
			return false;
		}
		if (firedCreatureUid != 0 && (operationInfo.FiredCreature == null || operationInfo.FiredCreature.Uid != firedCreatureUid))
		{
			return false;
		}
		if (passiveOnly && !operationInfo.IsInfinitePassiveOperation())
		{
			return false;
		}
		if (skillIndex != 0 && operationInfo.SkillIndex != skillIndex)
		{
			return false;
		}
		if (operationNo != 0 && operationInfo.OperationNo != operationNo)
		{
			return false;
		}
		if (operationTypes != null && operationTypes.Length != 0 && !operationTypes.Contains(operationInfo.Type))
		{
			return false;
		}
		return true;
	}

	public int RemoveNegativeOperation()
	{
		List<long> list = new List<long>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			long keyByIndex = OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (!valueByIndex.IsInfinitePassiveOperation())
			{
				_ = valueByIndex.Type;
				if (valueByIndex.IsNegativeOperation())
				{
					list.Add(keyByIndex);
				}
			}
		}
		int num = 0;
		foreach (long item in list)
		{
			RemoveOperationInfo(item);
			num++;
		}
		return num;
	}

	public int GetNegativeOperationCount()
	{
		int num = 0;
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (!valueByIndex.IsInfinitePassiveOperation() && valueByIndex.IsNegativeOperation())
			{
				num++;
			}
		}
		return num;
	}

	public int RemovePositiveOperation()
	{
		List<long> list = new List<long>();
		for (int i = 0; i < OperationDictionary.Count; i++)
		{
			long keyByIndex = OperationDictionary.GetKeyByIndex(i);
			OperationInfo valueByIndex = OperationDictionary.GetValueByIndex(i);
			if (!valueByIndex.IsInfinitePassiveOperation() && valueByIndex.IsPositiveOperation())
			{
				list.Add(keyByIndex);
			}
		}
		int num = 0;
		foreach (long item in list)
		{
			RemoveOperationInfo(item);
			num++;
		}
		return num;
	}

	private void OnBeginOperation(OperationInfo operationInfo)
	{
		Owner.RaiseBattleEvent(new BeginOperation
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationInfo.Uid,
			DurationMs = operationInfo.DurationMs
		});
		if (operationInfo.Runtime != null)
		{
			operationInfo.Runtime.OnBegin();
		}
		OperationBeginEvent operationBeginEvent = new OperationBeginEvent();
		operationBeginEvent.Owner = Owner;
		operationBeginEvent.OperationInfo = operationInfo;
		Owner.BattleInstance.Snapshot.TacticManager.SendEvent(operationBeginEvent);
	}

	private void OnEndOperation(OperationInfo operationInfo)
	{
		operationInfo.Expired = true;
		if (operationInfo.Runtime != null)
		{
			operationInfo.Runtime.OnEnd();
			operationInfo.Runtime = null;
		}
		Owner.RaiseBattleEvent(new EndOperation
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationInfo.Uid
		});
		operationInfo.Close();
	}

	private void OnResistOperation(OperationInfo operationInfo)
	{
		StateData stateData = ((operationInfo.Runtime is IStateModifier stateModifier) ? stateModifier.GetStateData() : null);
		Owner.RaiseBattleEvent(new ResistOperation
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationInfo.Uid,
			StateType = ((stateData == null) ? string.Empty : stateData.StateType)
		});
	}

	private void OnCancelOperation(OperationInfo operationInfo, CancelOperation.EType cancelType)
	{
		Owner.RaiseBattleEvent(new CancelOperation
		{
			CreatureUid = Owner.Uid,
			OperationUid = operationInfo.Uid,
			CancelType = cancelType
		});
	}

	private void OnTickOperation(OperationInfo operationInfo)
	{
		if (operationInfo.Runtime != null)
		{
			operationInfo.Runtime.OnTick();
		}
	}

	bool IOperationController.HasOpearation(int skillIndex, int operationNo)
	{
		return FindOperationInfo(skillIndex, operationNo) != null;
	}
}
