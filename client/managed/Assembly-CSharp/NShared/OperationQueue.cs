using System.Collections.Generic;
using System.Linq;
using NShared.NUtil;

namespace NShared;

public class OperationQueue : SnapshotObject
{
	private readonly SnapshotObjectMember<SnapshotLinkedList<ReservedOperationInfo>> _reservedOperationListHolder;

	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	protected SnapshotObjectMember<int> _lastOperationUidHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public SnapshotLinkedList<ReservedOperationInfo> ReservedOperationList => _reservedOperationListHolder.Get();

	public OperationQueue()
	{
		_reservedOperationListHolder = new SnapshotObjectMember<SnapshotLinkedList<ReservedOperationInfo>>(this);
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_lastOperationUidHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Init(BattleCreature creature)
	{
		_reservedOperationListHolder.Alloc();
		_ownerHolder.Set(creature);
	}

	private long AllocOwnerOperationUid()
	{
		int num = _lastOperationUidHolder.Get() + 1;
		_lastOperationUidHolder.Set(num);
		return Owner.Uid * 1000000 + num;
	}

	public OperationInfo Add(FiredOperationInfo firedInfo, int fixedDelayMs, int distanceDelayMs, int distanceFixedDelayMs)
	{
		if (firedInfo == null)
		{
			return null;
		}
		if (firedInfo.OperationData.MaxAccCount > 0)
		{
			int num = 0;
			for (LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				ReservedOperationInfo value = linkedListNode.Value;
				if (value.OperationInfo.SkillIndex == firedInfo.SkillData.Index && value.OperationInfo.OperationNo == firedInfo.OperationNo)
				{
					num++;
				}
			}
			if (num >= firedInfo.OperationData.MaxAccCount)
			{
				return null;
			}
		}
		OperationInfo operationInfo = Create(firedInfo, null, 0L);
		if (operationInfo == null)
		{
			return null;
		}
		operationInfo.DistanceDelayMs = distanceDelayMs;
		operationInfo.DistanceFixedDelayMs = distanceFixedDelayMs;
		int num2 = fixedDelayMs;
		if (distanceDelayMs > 0 || distanceFixedDelayMs > 0)
		{
			Vector3 firedPosition = firedInfo.FiredPosition;
			BattleCreature targetCreature = firedInfo.TargetCreature;
			int distanceDelayOffsetMm = firedInfo.OperationData.GetDistanceDelayOffsetMm();
			if (distanceDelayOffsetMm != 0 && firedInfo.FiredCreature != null)
			{
				firedPosition += firedInfo.FiredCreature.TransformController.Direction * distanceDelayOffsetMm / 1000L;
			}
			int distanceDelayMs2 = GetDistanceDelayMs(firedPosition, targetCreature, distanceDelayMs, distanceFixedDelayMs);
			num2 += distanceDelayMs2;
		}
		bool num3 = operationInfo.HasOperationFlag(OperationFlag.HasParent);
		int parent = operationInfo.FiredInfo.OperationData.Parent;
		if (num3 && parent >= 0)
		{
			bool flag = operationInfo.HasOperationFlag(OperationFlag.SameTickParentOnly);
			for (LinkedListNode<ReservedOperationInfo> linkedListNode2 = ReservedOperationList.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
			{
				OperationInfo operationInfo2 = linkedListNode2.Value.OperationInfo;
				if (operationInfo2.OperationNo == parent && operationInfo2.SkillData == firedInfo.SkillData && operationInfo2.TargetCreature == operationInfo.TargetCreature && (!flag || operationInfo2.TickIndex == firedInfo.TickIndex))
				{
					operationInfo.Parent = operationInfo2;
					num2 = (fixedDelayMs = 100000000);
					break;
				}
			}
		}
		Reserve(operationInfo, num2, fixedDelayMs);
		return operationInfo;
	}

	private int GetDistanceDelayMs(BattleCreature firedCreature, BattleCreature targetCreature, int tableDistanceDelayMs, int tableDistanceFixedDelayMs)
	{
		return GetDistanceDelayMs(firedCreature.TransformController.Position, targetCreature, tableDistanceDelayMs, tableDistanceFixedDelayMs);
	}

	private int GetDistanceDelayMs(Vector3 firedPosition, BattleCreature targetCreature, int tableDistanceDelayMs, int tableDistanceFixedDelayMs)
	{
		return (int)(Vector3.Distance(firedPosition, targetCreature.TransformController.Position) * tableDistanceDelayMs / 1000) + tableDistanceFixedDelayMs;
	}

	public void Update(int deltaTimeMs)
	{
		if (Owner.BattleInstance.IsBattleClosingOrDisabled() && Owner.StatController.IsDead())
		{
			if (ReservedOperationList.Count > 0)
			{
				Clear();
			}
		}
		else
		{
			int attackSpeedAppliedDeltaTimeMs = Owner.StatController.MultiplyAttackSpeed(deltaTimeMs);
			UpdateReservedOperation(attackSpeedAppliedDeltaTimeMs, deltaTimeMs);
		}
	}

	private OperationInfo Create(FiredOperationInfo firedInfo, BattleCreature owner, long operationUid, OperationInfo parent = null)
	{
		if (firedInfo == null)
		{
			return null;
		}
		operationUid = ((operationUid != 0L) ? operationUid : AllocOwnerOperationUid());
		OperationInfo operationInfo = SnapshotObjectPool<OperationInfo>.Alloc();
		operationInfo.Uid = operationUid;
		operationInfo.Creature = owner;
		operationInfo.FiredInfo = firedInfo;
		operationInfo.Parent = parent;
		operationInfo.BeginTimeDelayMs = parent?.BeginTimeDelayMs ?? 0;
		operationInfo.Init();
		if (owner != null)
		{
			IOperationRuntime operationRuntime = GameRoot.Instance.OperationRuntimeFactory.Create(operationInfo.Type);
			if (operationRuntime != null)
			{
				operationInfo.Runtime = operationRuntime;
				operationRuntime.Init(operationInfo);
			}
		}
		return operationInfo;
	}

	private void Reserve(OperationInfo operationInfo, int delayMs, int fixedDelayMs)
	{
		ReservedOperationInfo reservedOperationInfo = SnapshotObjectPool<ReservedOperationInfo>.Alloc();
		reservedOperationInfo.OperationInfo = operationInfo;
		reservedOperationInfo.DelayTimeMs = delayMs;
		reservedOperationInfo.FixedDelayTimeMs = fixedDelayMs;
		ReservedOperationList.AddLast(reservedOperationInfo);
		OnReserveOperation(reservedOperationInfo);
		if (reservedOperationInfo.IsReadyToFly())
		{
			FlyOperation(reservedOperationInfo);
		}
	}

	public ReservedOperationInfo GetReservedOperationInfoOfSkill(int skillIndex)
	{
		for (LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			ReservedOperationInfo value = linkedListNode.Value;
			if (value.OperationInfo.FiredSkillIndex == skillIndex || value.OperationInfo.SkillIndex == skillIndex)
			{
				return value;
			}
		}
		return null;
	}

	public ReservedOperationInfo GetReservedOperationInfo(long operationUid)
	{
		for (LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			ReservedOperationInfo value = linkedListNode.Value;
			if (value.OperationInfo.Uid == operationUid)
			{
				return value;
			}
		}
		return null;
	}

	public bool CancelReserve(long operationUid)
	{
		long num = 0L;
		for (LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			ReservedOperationInfo value = linkedListNode.Value;
			if (value.OperationInfo.Uid == operationUid)
			{
				if (!value.OperationInfo.HasOperationFlag(OperationFlag.NoCancelReserve))
				{
					ReservedOperationList.Remove(value);
					num = operationUid;
				}
				break;
			}
		}
		if (num > 0)
		{
			CancelOperation cancelOperation = new CancelOperation();
			cancelOperation.CancelType = CancelOperation.EType.ReservedOperation;
			cancelOperation.CreatureUid = Owner.Uid;
			cancelOperation.OperationUid = num;
			Owner.RaiseBattleEvent(cancelOperation);
			return true;
		}
		return false;
	}

	public bool CancelReserve(int targetCreatureUid, int skillIndex = 0)
	{
		for (LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			OperationInfo operationInfo = linkedListNode.Value.OperationInfo;
			if (operationInfo != null && (targetCreatureUid == 0 || (operationInfo.TargetCreature != null && operationInfo.TargetCreature.Uid == targetCreatureUid)) && (skillIndex == 0 || operationInfo.SkillIndex == skillIndex))
			{
				return CancelReserve(operationInfo.Uid);
			}
		}
		return false;
	}

	public void Clear()
	{
		ReservedOperationInfo[] array = ReservedOperationList.ToArray();
		foreach (ReservedOperationInfo reservedOperationInfo in array)
		{
			CancelReserve(reservedOperationInfo.OperationInfo.Uid);
		}
	}

	public void ClearOnWaveEnd()
	{
		ReservedOperationInfo[] array = ReservedOperationList.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			OperationInfo operationInfo = array[i].OperationInfo;
			if (operationInfo != null && !operationInfo.HasOperationFlag(OperationFlag.AliveOnWaveEnd))
			{
				CancelReserve(operationInfo.Uid);
			}
		}
	}

	public void UpdateReservedOperation(int attackSpeedAppliedDeltaTimeMs, int originalDeltaTimeMs)
	{
		using TemporaryList<ReservedOperationInfo> temporaryList = default(TemporaryList<ReservedOperationInfo>);
		bool flag = Owner.BattleInstance.IsInSoloTime();
		LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First;
		while (linkedListNode != null)
		{
			LinkedListNode<ReservedOperationInfo> next = linkedListNode.Next;
			ReservedOperationInfo value = linkedListNode.Value;
			if (value.IsReadyToFly() && !value.Flied)
			{
				FlyOperation(value);
			}
			if (value.IsReadyToFire())
			{
				temporaryList.Add(value);
				ReservedOperationList.Remove(linkedListNode);
			}
			else if (value.IsExpired())
			{
				ReservedOperationList.Remove(linkedListNode);
				Owner.RaiseBattleEvent(new CancelOperation
				{
					CancelType = CancelOperation.EType.ReservedOperation,
					CreatureUid = Owner.Uid,
					OperationUid = value.OperationInfo.Uid
				});
			}
			else
			{
				bool isIgnoreSoloTime = value.OperationInfo.IsIgnoreSoloTime;
				if ((value.OperationInfo.SkillData != null && value.OperationInfo.SkillData.GetSoloTimeMs(isIgnoreSoloTime) > 0) || (value.OperationInfo.FiredSkillData != null && value.OperationInfo.FiredSkillData.GetSoloTimeMs(isIgnoreSoloTime) > 0) || !flag)
				{
					int num = (value.OperationInfo.HasOperationFlag(OperationFlag.DelayIgnoreAttackSpeed) ? originalDeltaTimeMs : attackSpeedAppliedDeltaTimeMs);
					value.OperationInfo.FiredInfo.SoloTimeMs -= num;
					value.Update(num);
				}
			}
			linkedListNode = next;
		}
		for (int i = 0; i < temporaryList.Count; i++)
		{
			ReservedOperationInfo reservedOperationInfo = temporaryList[i];
			OnOperationReady(reservedOperationInfo);
		}
	}

	private void OnOperationReady(ReservedOperationInfo reservedOperationInfo)
	{
		if (reservedOperationInfo == null)
		{
			return;
		}
		OperationInfo operationInfo = reservedOperationInfo.OperationInfo;
		if (operationInfo == null)
		{
			return;
		}
		if (operationInfo.HasOperationFlag(OperationFlag.CheckConditionOnReady))
		{
			FiredOperationInfo firedInfo = operationInfo.FiredInfo;
			if (operationInfo.OperationData != null && !SkillController.CheckMeetCondition(firedInfo.FiredCreature.SkillController.ModifierContainer, operationInfo.OperationData, firedInfo, firedInfo.TargetCreature))
			{
				return;
			}
		}
		if (operationInfo.MaxRangeMm > 0 && (operationInfo.TargetCreature == null || BattleCreatureContainer.IsInSkillRange(Owner, operationInfo.TargetCreature, 0, operationInfo.MaxRangeMm) != SkillRangeResultType.Success))
		{
			return;
		}
		SkillData skillData = operationInfo.SkillData;
		bool flag = skillData.GetSkillTargetType(Owner).IsFriendly();
		BattleCreatureList battleCreatureList = BattleCreatureList.Alloc();
		switch (operationInfo.TargetType)
		{
		case OperationTargetType.Myself:
			if (operationInfo.RadiusMm > 0)
			{
				Vector3 center2 = Owner.TransformController.Position + Owner.TransformController.Direction * operationInfo.AreaOffsetMm / 1000L;
				BattleCreatureList creatureListOfRange2 = Owner.BattleInstance.CreatureContainer.GetCreatureListOfRange(center2, operationInfo.RadiusMm);
				battleCreatureList.AddRange(creatureListOfRange2);
			}
			else
			{
				battleCreatureList.Add(Owner);
			}
			break;
		case OperationTargetType.Target:
		{
			if (operationInfo.RadiusMm > 0)
			{
				Vector3 center4 = operationInfo.TargetCreature.TransformController.Position + Owner.TransformController.Direction * operationInfo.AreaOffsetMm / 1000L;
				BattleCreatureList creatureListOfRange4 = Owner.BattleInstance.CreatureContainer.GetCreatureListOfRange(center4, operationInfo.RadiusMm);
				battleCreatureList.AddRange(creatureListOfRange4);
				break;
			}
			BattleCreature battleCreature = operationInfo.TargetCreature;
			bool flag2 = false;
			if (operationInfo.HasOperationFlag(OperationFlag.RetargetOnAdd))
			{
				flag2 = true;
			}
			else if (operationInfo.HasOperationFlag(OperationFlag.RetargetOnAddIfDied) && (battleCreature == null || battleCreature.StatController.IsDead()))
			{
				flag2 = true;
			}
			if (flag2)
			{
				BattleCreatureList battleCreatureList2 = Owner.BattleInstance.CreatureContainer.FindSkillTargetCreature(Owner, skillData, checkOnly: false, null);
				battleCreature = ((battleCreatureList2 != null && battleCreatureList2.Count > 0) ? battleCreatureList2[0] : null);
			}
			if (battleCreature != null)
			{
				battleCreatureList.Add(battleCreature);
			}
			break;
		}
		case OperationTargetType.FiredTargetGround:
			if (operationInfo.RadiusMm > 0)
			{
				Vector3 center = operationInfo.FiredInfo.FiredTargetPosition + Owner.TransformController.Direction * operationInfo.AreaOffsetMm / 1000L;
				BattleCreatureList creatureListOfRange = Owner.BattleInstance.CreatureContainer.GetCreatureListOfRange(center, operationInfo.RadiusMm);
				battleCreatureList.AddRange(creatureListOfRange);
			}
			break;
		case OperationTargetType.UsedTargetGround:
			if (operationInfo.RadiusMm > 0)
			{
				Vector3 center3 = operationInfo.FiredInfo.UsedTargetPosition + Owner.TransformController.Direction * operationInfo.AreaOffsetMm / 1000L;
				BattleCreatureList creatureListOfRange3 = Owner.BattleInstance.CreatureContainer.GetCreatureListOfRange(center3, operationInfo.RadiusMm);
				battleCreatureList.AddRange(creatureListOfRange3);
			}
			break;
		case OperationTargetType.Parent:
		{
			OperationInfo parent = operationInfo.Parent;
			if (parent != null && parent.Creature != null)
			{
				battleCreatureList.Add(parent.Creature);
			}
			break;
		}
		}
		if (operationInfo.RadiusMm > 0)
		{
			switch (operationInfo.TargetSelectType)
			{
			case OperationTargetSelectType.Same:
			{
				bool checkTeamFriendly2 = flag;
				if (operationInfo.IsTeamInverted)
				{
					checkTeamFriendly2 = !checkTeamFriendly2;
				}
				battleCreatureList.Remove((BattleCreature e) => e.TeamController.IsFriendly(Owner.TeamController) != checkTeamFriendly2);
				break;
			}
			case OperationTargetSelectType.Opponent:
			{
				bool checkTeamFriendly = flag;
				if (operationInfo.IsTeamInverted)
				{
					checkTeamFriendly = !checkTeamFriendly;
				}
				battleCreatureList.Remove((BattleCreature e) => e.TeamController.IsFriendly(Owner.TeamController) == checkTeamFriendly);
				break;
			}
			}
			int angle = operationInfo.Angle;
			if (angle != 0)
			{
				Vector3 position = operationInfo.FiredCreature.TransformController.Position;
				Vector3 direction = operationInfo.FiredCreature.TransformController.Direction;
				battleCreatureList = Owner.BattleInstance.CreatureContainer.GetCreatureListOfAngle(position, direction, battleCreatureList, angle);
			}
			int width = operationInfo.Width;
			if (width > 0)
			{
				Vector3 position2 = operationInfo.FiredCreature.TransformController.Position;
				Vector3 direction2 = operationInfo.FiredCreature.TransformController.Direction;
				battleCreatureList = Owner.BattleInstance.CreatureContainer.GetCreatureListOfWidth(position2, direction2, battleCreatureList, width);
			}
		}
		using TemporaryList<ReservedOperationInfo> temporaryList = PopReservedOperationOfParent(operationInfo);
		for (int num = 0; num < battleCreatureList.Count; num++)
		{
			BattleCreature battleCreature2 = battleCreatureList[num];
			bool flag3 = battleCreature2 == operationInfo.TargetCreature;
			if (flag3 && operationInfo.ExcludeTarget)
			{
				continue;
			}
			OperationInfo operationInfo2 = CloneAndAddOperation(operationInfo, operationInfo.Parent, battleCreature2);
			if (operationInfo2 == null || !operationInfo2.CanChildBeginOperation())
			{
				continue;
			}
			for (int num2 = 0; num2 < temporaryList.Count; num2++)
			{
				ReservedOperationInfo reservedOperationInfo2 = temporaryList[num2];
				OperationInfo operationInfo3 = reservedOperationInfo2.OperationInfo;
				if (operationInfo3 == null)
				{
					continue;
				}
				OperationData operationData = operationInfo3.OperationData;
				if (operationData == null || (operationInfo3.OnlyTarget && !flag3))
				{
					continue;
				}
				OperationConditionType conditionType = operationData.ConditionType;
				if (conditionType != OperationConditionType.None && conditionType != OperationConditionType.Random && !operationInfo3.HasDelayedConditionCheckFlag())
				{
					FiredOperationInfo firedInfo2 = operationInfo3.FiredInfo;
					string[] conditionValues = operationData.ConditionValues;
					bool reverseCheck = operationData.ReverseCheck;
					if (!GameRoot.Instance.OperationConditionFactory.MeetCondition(conditionType, firedInfo2, firedInfo2.TargetCreature, conditionValues, reverseCheck))
					{
						continue;
					}
				}
				FlyOperation(reservedOperationInfo2);
				CloneAndAddOperation(operationInfo3, operationInfo2, battleCreature2);
			}
		}
	}

	private OperationInfo CloneAndAddOperation(OperationInfo operationInfo, OperationInfo parentOperationInfo, BattleCreature targetCreature)
	{
		OperationInfo operationInfo2 = Create(operationInfo.FiredInfo, targetCreature, operationInfo.Uid, parentOperationInfo);
		if (operationInfo2 == null)
		{
			return null;
		}
		if (operationInfo.AreaDelayMss != null && operationInfo.AreaDelayMss.Length != 0)
		{
			Vector3 firedPosition = operationInfo.TargetCreature.TransformController.Position;
			switch (operationInfo.TargetType)
			{
			case OperationTargetType.FiredTargetGround:
				firedPosition = operationInfo.FiredTargetPosition;
				break;
			case OperationTargetType.UsedTargetGround:
				firedPosition = operationInfo.UsedTargetPosition;
				break;
			case OperationTargetType.Myself:
				firedPosition = operationInfo.FiredPosition;
				break;
			}
			int tableDistanceDelayMs = operationInfo.AreaDelayMss[0];
			int tableDistanceFixedDelayMs = ((operationInfo.AreaDelayMss.Length >= 2) ? operationInfo.AreaDelayMss[1] : 0);
			operationInfo2.BeginTimeDelayMs += GetDistanceDelayMs(firedPosition, targetCreature, tableDistanceDelayMs, tableDistanceFixedDelayMs);
		}
		if (targetCreature.OperationController.Add(operationInfo2) != OperationResult.Success)
		{
			operationInfo2.Close();
			return null;
		}
		if (operationInfo2.WhileSkillUse)
		{
			UsingSkillInfo usingSkillInfo = Owner.SkillController.UsingSkillInfo;
			if (usingSkillInfo == null)
			{
				operationInfo2.Expired = true;
				return null;
			}
			if (!usingSkillInfo.OperationInfos.Contains(operationInfo))
			{
				operationInfo2.Expired = true;
				return null;
			}
			usingSkillInfo.OperationInfos.Add(operationInfo2);
		}
		return operationInfo2;
	}

	private TemporaryList<ReservedOperationInfo> PopReservedOperationOfParent(OperationInfo parentOperationInfo)
	{
		TemporaryList<ReservedOperationInfo> result = default(TemporaryList<ReservedOperationInfo>);
		LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First;
		while (linkedListNode != null)
		{
			LinkedListNode<ReservedOperationInfo> next = linkedListNode.Next;
			ReservedOperationInfo value = linkedListNode.Value;
			if (value.OperationInfo.Parent == parentOperationInfo)
			{
				result.Add(value);
				ReservedOperationList.Remove(linkedListNode);
			}
			linkedListNode = next;
		}
		return result;
	}

	private void OnReserveOperation(ReservedOperationInfo reservedOperationInfo)
	{
		if (reservedOperationInfo != null)
		{
			Owner.RaiseBattleEvent(new ReserveOperation
			{
				OperationUid = reservedOperationInfo.OperationInfo.Uid,
				FixedDelayTimeMs = reservedOperationInfo.FixedDelayTimeMs,
				DurationTimeMs = reservedOperationInfo.DelayTimeMs
			});
		}
	}

	private void FlyOperation(ReservedOperationInfo reservedOperationInfo)
	{
		if (reservedOperationInfo != null && !reservedOperationInfo.Flied)
		{
			reservedOperationInfo.Flied = true;
			BattleCreature firedCreature = reservedOperationInfo.OperationInfo.FiredCreature;
			BattleCreature targetCreature = reservedOperationInfo.OperationInfo.TargetCreature;
			int distanceDelayMs = reservedOperationInfo.OperationInfo.DistanceDelayMs;
			int distanceFixedDelayMs = reservedOperationInfo.OperationInfo.DistanceFixedDelayMs;
			int distanceDelayMs2 = GetDistanceDelayMs(firedCreature, targetCreature, distanceDelayMs, distanceFixedDelayMs);
			long finalStat = firedCreature.StatController.GetFinalStat(StatType.AttackSpeed);
			int flyingDurationTimeMs = (int)(distanceDelayMs2 * 1000 / ((finalStat > 0) ? finalStat : 1));
			reservedOperationInfo.DelayTimeMs = reservedOperationInfo.ElapsedTimeMs + distanceDelayMs2;
			Owner.RaiseBattleEvent(new FlyingOperation
			{
				OperationUid = reservedOperationInfo.OperationInfo.Uid,
				ReserveDurationTimeMs = reservedOperationInfo.DelayTimeMs,
				FlyingDurationTimeMs = flyingDurationTimeMs,
				TargetCreatureUid = targetCreature.Uid
			});
		}
	}

	public void FlyOperation(int skillIndex, int operationNo, int flyCount, BattleCreature firstTargetCreature, BattleCreature[] targetCreatures)
	{
		int num = 0;
		LinkedListNode<ReservedOperationInfo> linkedListNode = ReservedOperationList.First;
		while (linkedListNode != null)
		{
			ReservedOperationInfo value = linkedListNode.Value;
			linkedListNode = linkedListNode.Next;
			if (value.OperationInfo.SkillIndex == skillIndex && value.OperationInfo.OperationNo == operationNo && !value.Flied)
			{
				if (firstTargetCreature != null && targetCreatures != null && targetCreatures.Length != 0)
				{
					int distanceDelayTimeMs = value.DistanceDelayTimeMs;
					int num2 = (value.FixedDelayTimeMs = value.ElapsedTimeMs);
					value.DelayTimeMs = num2 + distanceDelayTimeMs;
					value.OperationInfo.FiredInfo.TargetCreature = firstTargetCreature;
					value.OperationInfo.FiredInfo.TargetCreatures = targetCreatures;
					value.OperationInfo.Creature = firstTargetCreature;
				}
				FlyOperation(value);
				if (flyCount > 0 && ++num >= flyCount)
				{
					break;
				}
			}
		}
	}
}
