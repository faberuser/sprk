using System;
using System.Collections.Generic;
using NShared.NUtil;

namespace NShared;

public class TransformController : SnapshotObject
{
	private readonly SnapshotObjectMember<BattleCreature> _ownerHolder;

	private readonly SnapshotObjectMember<Vector3> _positionHolder;

	private readonly SnapshotObjectMember<Vector3> _directionHolder;

	private readonly SnapshotObjectMember<MoveHandle> _currentMoveHandleHolder;

	private readonly SnapshotObjectMember<MoveHandle> _cachedMoveHandleHolder;

	private readonly SnapshotObjectMember<int> _rangeBonusHolder;

	public BattleCreature Owner => _ownerHolder.Get();

	public Vector3 Position => _positionHolder.Get();

	public Vector3 Direction => _directionHolder.Get();

	public MoveHandle CurrentMoveHandle => _currentMoveHandleHolder.Get();

	public int RangeBonus => _rangeBonusHolder.Get();

	public TransformController()
	{
		_ownerHolder = new SnapshotObjectMember<BattleCreature>(this);
		_positionHolder = new SnapshotObjectMember<Vector3>(this);
		_directionHolder = new SnapshotObjectMember<Vector3>(this);
		_currentMoveHandleHolder = new SnapshotObjectMember<MoveHandle>(this);
		_cachedMoveHandleHolder = new SnapshotObjectMember<MoveHandle>(this);
		_rangeBonusHolder = new SnapshotObjectMember<int>(this, 0);
	}

	public void Init(BattleCreature owner)
	{
		_ownerHolder.Set(owner);
		_cachedMoveHandleHolder.Alloc();
	}

	public void InitDirectionByTeam()
	{
		SetDirectionInternal(Owner.TeamController.Team.Direction, checkStatic: false);
	}

	public int GetSpeedMms()
	{
		return (int)Owner.StatController.GetFinalStat(StatType.MoveSpeedMms);
	}

	public void SetRangeBonus(int rangeBonus)
	{
		_rangeBonusHolder.Set(rangeBonus);
	}

	public int GetRangeBonusMm()
	{
		return Math.Min(RangeBonus, 2) * BattleConstant.BattleRangeBonusMm;
	}

	public void StopMove(bool raiseEvent)
	{
		SetPosition(Position, raiseEvent, NShared.SetPosition.SetPositionMethodType.None, 0L);
	}

	private void SetPositionInternal(Vector3 position)
	{
		if (!Owner.CreatureData.IsStaticPosition())
		{
			Owner.BattleInstance.BattleArea.ValidateByGroupIndex(ref position, Owner.CreateInfo.GroupIndex);
		}
		_positionHolder.Set(position);
	}

	public void ViewPosition(Vector3 targetPosition)
	{
		Vector3 vector = targetPosition - Position;
		if (!vector.approximatelyZero)
		{
			SetDirectionInternal(vector.normalized);
		}
	}

	public void SetDirection(Vector3 direction, bool checkStatic = true)
	{
		SetDirectionInternal(direction.normalized, checkStatic);
	}

	private void SetDirectionInternal(Vector3 direction, bool checkStatic = true)
	{
		if (!checkStatic || (Owner.CreatureData.TransformType & CreatureTransformType.StaticRotation) == 0)
		{
			_directionHolder.Set(direction);
		}
	}

	public void SetPosition(Vector3 position, bool raiseEvent, SetPosition.SetPositionMethodType methodType = NShared.SetPosition.SetPositionMethodType.None, long operationUid = 0L)
	{
		if (CurrentMoveHandle != null)
		{
			_currentMoveHandleHolder.Set(null);
		}
		else if (position == Position)
		{
			return;
		}
		SetPositionInternal(position);
		if (raiseEvent)
		{
			SetPosition setPosition = new SetPosition();
			setPosition.CreatureUid = Owner.Uid;
			setPosition.Position = position;
			setPosition.MethodType = methodType;
			setPosition.OperationUid = operationUid;
			Owner.RaiseBattleEvent(setPosition);
		}
	}

	public void MoveToPosition(Vector3 targetPosition, int approachDistanceMm, int speedMms, long operationUid, bool isManualMove)
	{
		if (CurrentMoveHandle == null || CurrentMoveHandle.MoveTargetType != MoveTargetType.Position || !(CurrentMoveHandle.TargetPosition == targetPosition) || CurrentMoveHandle.ApproachDistanceMm != approachDistanceMm || CurrentMoveHandle.IsManualMove != isManualMove)
		{
			MoveTo(targetPosition, null, approachDistanceMm, speedMms, operationUid, isManualMove);
		}
	}

	public void MoveToCreature(BattleCreature targetCreature, int approachDistanceMm, int speedMms, long operationUid)
	{
		if (targetCreature != null && (CurrentMoveHandle == null || CurrentMoveHandle.MoveTargetType != MoveTargetType.Creature || CurrentMoveHandle.TargetCreature != targetCreature || CurrentMoveHandle.ApproachDistanceMm != approachDistanceMm))
		{
			Vector3 position = targetCreature.TransformController.Position;
			MoveTo(position, targetCreature, approachDistanceMm, speedMms, operationUid, isManualMove: false);
		}
	}

	private void MoveTo(Vector3 targetPosition, BattleCreature targetCreature, int approachDistanceMm, int speedMms, long operationUid, bool isManualMove)
	{
		if (Owner.StateController.CannotMoveState() || Owner.CreatureData.IsStaticPosition())
		{
			return;
		}
		Vector3 vector = targetPosition - Position;
		long magnitudeSqr = vector.MagnitudeSqr;
		long num = (long)approachDistanceMm * (long)approachDistanceMm;
		if (magnitudeSqr > num)
		{
			long currentTimeMs = Owner.BattleInstance.MainTimer.GetCurrentTimeMs();
			MoveHandle moveHandle = _cachedMoveHandleHolder.Get();
			if (targetCreature != null)
			{
				moveHandle.Init(currentTimeMs, Position, targetCreature, approachDistanceMm, isManualMove);
			}
			else
			{
				moveHandle.Init(currentTimeMs, Position, targetPosition, approachDistanceMm, isManualMove);
			}
			moveHandle.SetMoveSpeed(speedMms);
			moveHandle.SetAvailableMoveTargetOffset(Owner, targetCreature);
			_currentMoveHandleHolder.Set(moveHandle);
			SetDirectionInternal(vector.normalized);
			BeginMove beginMove = new BeginMove();
			beginMove.CreatureUid = Owner.Uid;
			beginMove.StartPosition = Position;
			beginMove.TargetPosition = targetPosition;
			beginMove.TargetOffsetIndex = moveHandle.MoveTargetOffsetIndex;
			beginMove.SpeedMms = ((speedMms > 0) ? speedMms : GetSpeedMms());
			beginMove.TargetCreatureUid = targetCreature?.Uid ?? 0;
			beginMove.ApproachDistanceMm = approachDistanceMm;
			beginMove.OperationUid = operationUid;
			Owner.RaiseBattleEvent(beginMove);
		}
	}

	public void MoveBy(Vector3 offset, bool sendPacket, SetPosition.SetPositionMethodType methodType, long operationUid)
	{
		if (!Owner.CreatureData.IsStaticPosition())
		{
			Vector3 position = Position + offset;
			SetPosition(position, sendPacket, methodType, operationUid);
		}
	}

	public bool IsManualMoving()
	{
		return _currentMoveHandleHolder.Get()?.IsManualMove ?? false;
	}

	public void Update(long currentTimeMs, int deltaTimeMs)
	{
		if (Owner.SkillController.CanUpdateTimer() && !Owner.IsResting)
		{
			UpdateMoveHandle(currentTimeMs, deltaTimeMs);
			UpdatePositionSeparation(deltaTimeMs);
		}
	}

	private void UpdateMoveHandle(long currentTimeMs, int deltaTimeMs)
	{
		if (CurrentMoveHandle == null)
		{
			return;
		}
		if (Owner.StatController.IsDead())
		{
			StopMove(raiseEvent: false);
		}
		else
		{
			if (Owner.StateController.CannotMoveState() || Owner.CreatureData.IsStaticPosition())
			{
				return;
			}
			Vector3 vector = CurrentMoveHandle.TargetPosition;
			Vector3 vector2 = CurrentMoveHandle.TargetPosition;
			if (CurrentMoveHandle.TargetCreature != null)
			{
				if (CurrentMoveHandle.TargetCreature.StatController.IsDead())
				{
					StopMove(raiseEvent: true);
					return;
				}
				vector = CurrentMoveHandle.TargetCreature.TransformController.Position;
				int num = Math.Min(BattleConstant.MoveTargetOffsetMm, Math.Abs(CurrentMoveHandle.ApproachDistanceMm));
				int num2 = CurrentMoveHandle.TargetCreature.CreatureData.SizeMm / 2 + num;
				int moveTargetOffsetIndex = CurrentMoveHandle.MoveTargetOffsetIndex;
				Vector3 vector3 = MoveHandle.MoveTargetOffsets[moveTargetOffsetIndex] * num2 / 1000L;
				vector2 = vector + vector3;
			}
			Vector3 v = vector2 - Position;
			if (v.approximatelyZero)
			{
				StopMove(raiseEvent: true);
				return;
			}
			Vector3 normalized = v.normalized;
			int num3 = ((CurrentMoveHandle.SpeedMms > 0) ? CurrentMoveHandle.SpeedMms : GetSpeedMms());
			SetDirectionInternal(normalized);
			Vector3 vector4 = normalized * num3 / 1000L * deltaTimeMs / 1000L;
			Vector3 vector5 = Position + vector4;
			Vector3 v2 = vector2 - vector5;
			if (Vector3.DotProduct(v, v2) <= 0)
			{
				Vector3 vector6 = vector - Position;
				SetPositionInternal(vector2);
				if (!vector6.approximatelyZero)
				{
					SetDirectionInternal(vector6.normalized);
				}
				StopMove(raiseEvent: true);
				return;
			}
			SetPositionInternal(vector5);
			long num4 = Vector3.DistanceSqrWithoutY(Position, vector);
			long num5 = (long)CurrentMoveHandle.ApproachDistanceMm * (long)CurrentMoveHandle.ApproachDistanceMm;
			if (num4 <= num5)
			{
				SetDirectionInternal((vector - Position).normalized);
				StopMove(raiseEvent: true);
			}
		}
	}

	private void UpdatePositionSeparationOriginal(int deltaTimeMs)
	{
		if (CurrentMoveHandle != null || Owner.CreatureData.IsStaticPosition() || Owner.StatController.IsDead())
		{
			return;
		}
		int sizeMm = Owner.CreatureData.SizeMm;
		BattleCreatureList creatureListOfRange = Owner.BattleInstance.CreatureContainer.GetCreatureListOfRange(Position, sizeMm);
		if (creatureListOfRange.Count > 1)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < creatureListOfRange.Count; i++)
			{
				BattleCreature battleCreature = creatureListOfRange[i];
				zero += battleCreature.TransformController.Position;
			}
			zero /= (long)creatureListOfRange.Count;
			Vector3 vector = Position - zero;
			if (vector.approximatelyZero)
			{
				vector = Vector3.forward;
			}
			Vector3 vector2 = vector.normalized * deltaTimeMs / 1000L;
			Vector3 positionInternal = Position + vector2;
			SetPositionInternal(positionInternal);
		}
	}

	private void UpdatePositionSeparation(int deltaTimeMs)
	{
		int positionSeperationWeight = BattleConstant.PositionSeperationWeight;
		if (positionSeperationWeight <= 0 || CurrentMoveHandle != null || Owner.StatController.IsDead() || Owner.CreatureData.IsStaticPosition() || Owner.SkillController.UsingSkillInfo != null)
		{
			return;
		}
		int sizeMm = Owner.CreatureData.SizeMm;
		BattleCreatureList creatureListOfRange = Owner.BattleInstance.CreatureContainer.GetCreatureListOfRange(Position, sizeMm);
		if (creatureListOfRange.Count <= 1)
		{
			return;
		}
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < creatureListOfRange.Count; i++)
		{
			BattleCreature battleCreature = creatureListOfRange[i];
			int num = sizeMm + battleCreature.CreatureData.SizeMm;
			Vector3 vector = battleCreature.TransformController.Position - Position;
			if (!vector.approximatelyZero)
			{
				Vector3 vector2 = DirectionToRight(Direction);
				bool flag = vector.DotProduct(vector2) >= 0;
				long magnitude = vector.Magnitude;
				if (num >= magnitude)
				{
					Vector3 vector3 = (flag ? (-vector2) : vector2);
					long num2 = 1000 - magnitude * 1000 / num;
					zero += vector3 * num2 / 1000L;
				}
			}
		}
		Vector3 vector4 = zero * deltaTimeMs / 1000L;
		Vector3 positionInternal = Position + vector4 * positionSeperationWeight / 1000L;
		SetPositionInternal(positionInternal);
	}

	private Vector3 DirectionToRight(Vector3 direction)
	{
		return new Vector3(direction.mz, direction.my, -direction.mx);
	}

	public bool Dash(long operationUid, Vector3 targetPosition, int targetSizeMm, int approachMm, int positionOffsetMm, int durationMs, bool allowBackward, int maxMoveDistanceMm, bool ignoreBlock)
	{
		Vector3 ownerPosition = Position;
		Vector3 vector = targetPosition - ownerPosition;
		if (vector.approximatelyZero)
		{
			vector = Direction;
		}
		long magnitude = vector.Magnitude;
		Vector3 vector2 = vector * 1000L / magnitude;
		bool flag = positionOffsetMm < 0;
		int num = Math.Abs(positionOffsetMm) + approachMm;
		if (!allowBackward && !flag && magnitude <= num)
		{
			return false;
		}
		Vector3 vector3 = targetPosition + vector2 * (flag ? num : (-num)) / 1000L;
		bool flag2 = false;
		Vector3 v = vector3 - ownerPosition;
		if (v.MagnitudeSqr > 0)
		{
			BattleCreatureList aliveCreatures = Owner.BattleInstance.CreatureContainer.GetAliveCreatures(BattleCreatureList.AllocWithCreature(Owner));
			aliveCreatures.Remove((BattleCreature e) => !e.CreatureData.BlockDash);
			int testSizeMm = Owner.CreatureData.SizeMm + targetSizeMm;
			long minDashMx = Math.Min(ownerPosition.Mx, vector3.Mx);
			long maxDashMx = Math.Max(ownerPosition.Mx, vector3.Mx);
			aliveCreatures.Remove(delegate(BattleCreature e)
			{
				Vector3 position2 = e.TransformController.Position;
				return position2.Mx < minDashMx - testSizeMm || position2.Mx > maxDashMx + testSizeMm;
			});
			aliveCreatures.Sort(delegate(BattleCreature l, BattleCreature r)
			{
				long num6 = Math.Abs(l.TransformController.Position.Mx - ownerPosition.Mx);
				long value = Math.Abs(r.TransformController.Position.Mx - ownerPosition.Mx);
				return num6.CompareTo(value);
			});
			for (int num2 = 0; num2 < aliveCreatures.Count; num2++)
			{
				BattleCreature battleCreature = aliveCreatures[num2];
				bool flag3 = (battleCreature.CreatureData.TransformType & CreatureTransformType.Wall) != 0;
				if (!flag3 && ignoreBlock)
				{
					continue;
				}
				Vector3 position = battleCreature.TransformController.Position;
				Vector3 v2 = position - ownerPosition;
				if (Vector3.DotProduct(v2, v) >= 0)
				{
					int num3 = testSizeMm;
					if (Owner.BattleInstance.DevParam_LegacyBlockDashSizeCheck && !flag3)
					{
						num3 = Owner.CreatureData.SizeMm;
					}
					if (num3 >= Math.Abs(v2.Mx))
					{
						vector3 = ownerPosition;
						flag2 = true;
					}
					else
					{
						vector3 = position - v2.normalized * num3 / 1000L;
						flag2 = true;
					}
					break;
				}
			}
		}
		if (!flag2)
		{
			List<Vector3> list = new List<Vector3>();
			list.Add(vector3);
			Vector3 normalized = Vector3.CrossProduct(Vector3.up, vector).normalized;
			Vector3 vector4 = -normalized;
			Vector3 vector5 = normalized * 2L;
			Vector3 vector6 = vector4 * 2L;
			Vector3[] array = new Vector3[4] { normalized, vector4, vector5, vector6 };
			foreach (Vector3 vector7 in array)
			{
				Vector3 vector8 = vector3 + vector7 * Owner.CreatureData.SizeMm / 1000L;
				long magnitude2 = (vector8 - targetPosition).Magnitude;
				vector8 = ((vector8 != targetPosition) ? (targetPosition + (vector8 - targetPosition) * num / magnitude2) : vector8);
				list.Add(vector8);
			}
			vector3 = SelectBestPosition(Owner, list);
		}
		long num5 = Vector3.Distance(ownerPosition, vector3);
		if (maxMoveDistanceMm > 0 && num5 > maxMoveDistanceMm)
		{
			num5 = maxMoveDistanceMm;
			vector3 = ownerPosition + vector * num5 / 1000L;
		}
		if (durationMs > 0)
		{
			int speedMms = (int)(num5 * 1000 / durationMs);
			MoveToPosition(vector3, 10, speedMms, operationUid, isManualMove: false);
			return true;
		}
		SetPosition(vector3, raiseEvent: true, NShared.SetPosition.SetPositionMethodType.Warp, 0L);
		return true;
	}

	public static Vector3 SelectBestPosition(BattleCreature creature, IList<Vector3> positions)
	{
		if (positions == null || positions.Count == 0)
		{
			return Vector3.zero;
		}
		long num = long.MaxValue;
		Vector3 result = positions[0];
		for (int i = 0; i < positions.Count; i++)
		{
			Vector3 vector = positions[i];
			long positionBusyScore = GetPositionBusyScore(creature, vector);
			if (positionBusyScore < num)
			{
				num = positionBusyScore;
				result = vector;
			}
		}
		return result;
	}

	public static long GetPositionBusyScore(BattleCreature creature, Vector3 position)
	{
		int num = 2;
		long num2 = 0L;
		BattleCreatureList aliveCreatures = creature.BattleInstance.CreatureContainer.GetAliveCreatures(BattleCreatureList.AllocWithCreature(creature));
		for (int i = 0; i < aliveCreatures.Count; i++)
		{
			BattleCreature battleCreature = aliveCreatures[i];
			long num3 = Vector3.DistanceSqrWithoutY(position, battleCreature.TransformController.Position);
			int num4 = creature.CreatureData.SizeMm + battleCreature.CreatureData.SizeMm;
			int num5 = num4 * num4;
			if (num3 < num5 * num)
			{
				num2 += 100000000000L / Math.Max(1L, num3);
			}
		}
		return num2;
	}
}
