using NGame2.NBattleField;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class SyncPosition : BaseHandler
{
	private enum EMoveState
	{
		Idle,
		SyncPosition
	}

	public const float PositionThreshold = 0.2f;

	public int DisableLock;

	private EMoveState _moveState;

	public bool IsEnabled
	{
		get
		{
			if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance && !BattleViewGenericSingleton<CreatureManager>.instance.SyncPositionEnabled)
			{
				return false;
			}
			return DisableLock <= 0;
		}
	}

	public SyncPosition(Creature creature)
		: base(creature)
	{
	}

	public void Reset()
	{
		if (Owner.Action.IsMove())
		{
			_moveState = EMoveState.SyncPosition;
		}
		else
		{
			_moveState = EMoveState.Idle;
		}
	}

	public void Sync()
	{
		if (!IsEnabled)
		{
			return;
		}
		BattleFacade.CreatureCacheData creatureCache = Owner.CreatureCache;
		if (creatureCache.Uid == 0 || creatureCache.IsDeadOrResting())
		{
			return;
		}
		if (_moveState == EMoveState.Idle)
		{
			if ((!Owner.Action.IsIdle() && !Owner.Action.IsMove() && !Owner.Action.IsActionEnd()) || !SingletonAuto<BattleFacade>.isValidInstance || !SingletonAuto<BattleFacade>.instance.IsBattleEnabled())
			{
				return;
			}
			Vector3 position = Owner.Position;
			Vector3 vector = creatureCache.Position;
			Vector3 i_src = creatureCache.Direction;
			VectorXZ vectorXZ = Vespa_Unity_Vector3_Extension_Method.VecXZ(vector - position);
			if (vectorXZ.magnitude > 0.2f)
			{
				_moveState = EMoveState.SyncPosition;
				return;
			}
			if (vectorXZ.magnitude > Mathf.Epsilon)
			{
				Vector3 position2 = Vector3.MoveTowards(Owner.Position, vector, 0.3f * Time.deltaTime);
				Owner.SetBattlePos(position2);
			}
			Owner.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src), blendMotion: true);
			Owner.Action.Idle();
		}
		else if (_moveState == EMoveState.SyncPosition)
		{
			Vector3 i_src2 = creatureCache.Position;
			MoveTo(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src2), base.Speed, idleActionIfEnd: true, out var endMove);
			if (endMove)
			{
				_moveState = EMoveState.Idle;
				Owner.Action.Idle();
				Vector3 i_src3 = creatureCache.Direction;
				Owner.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src3), blendMotion: true);
			}
		}
		else
		{
			_moveState = EMoveState.Idle;
		}
	}
}
