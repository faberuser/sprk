using NGame2.NBattleField;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class WaveReady : BaseHandler, IHandler, ILooting
{
	public enum EState
	{
		Looting,
		Waiting,
		MoveToDestination,
		End
	}

	private NShared.SetPosition _packet;

	private float _maxTimeLength;

	private float _beginTime;

	private EState _state = EState.MoveToDestination;

	private float _stateTime;

	private Looting _looting;

	public float MoveSpeed => GameSetting.data.BattleField.WaveMove_MoveSpeed;

	public bool IsEnd => _state == EState.End;

	public float TimeLength { get; private set; }

	public float RemainedTime => _beginTime + _maxTimeLength - MainTimer.time;

	public WaveReady(Creature creature, NShared.SetPosition packet)
		: base(creature)
	{
		_packet = packet;
		_looting = new Looting(creature);
	}

	public float CalculateIfAdd(DropItem dropItem)
	{
		float result = _looting.CalculateIfAdd(dropItem);
		Vespa_Unity_Vector3_Extension_Method.VecXZ(dropItem.transform.position);
		Vespa_Unity_Vector3_Extension_Method.VecXZ(GetDestinationFromPacket());
		return result;
	}

	public void AddDropItem(DropItem dropItem)
	{
		_looting.AddDropItem(dropItem);
	}

	public bool IsLootingEnd()
	{
		return _state != EState.Looting;
	}

	public void CalculateTimeLength()
	{
		VectorXZ lastPosition = _looting.LastPosition;
		VectorXZ b = Vespa_Unity_Vector3_Extension_Method.VecXZ(GetDestinationFromPacket());
		TimeLength = _looting.TimeLength + VectorXZ.Distance(lastPosition, b) / MoveSpeed;
	}

	public void SetMaxTimeLength(float maxTimeLength)
	{
		_maxTimeLength = maxTimeLength;
	}

	void IHandler.Begin()
	{
		_beginTime = MainTimer.time;
		if (_looting.IsEmpty)
		{
			State_Waiting();
			return;
		}
		Vespa_Common_Class_Extension_Method.Interface((IHandler)_looting).Begin();
		State_Looting();
	}

	bool IHandler.IsEnd()
	{
		return IsEnd;
	}

	void IHandler.Cancel()
	{
		_packet = null;
		_state = EState.End;
	}

	void IHandler.ChainUpdate()
	{
		if (_state == EState.Looting)
		{
			Vespa_Common_Class_Extension_Method.Interface((IHandler)_looting).ChainUpdate();
			if (_looting.IsEnd)
			{
				State_Waiting();
			}
		}
		else if (_state == EState.Waiting)
		{
			VectorXZ a = Vespa_Unity_Vector3_Extension_Method.VecXZ(Owner.Position);
			VectorXZ b = Vespa_Unity_Vector3_Extension_Method.VecXZ(GetDestinationFromPacket());
			if (VectorXZ.Distance(a, b) / MoveSpeed >= RemainedTime)
			{
				State_MoveToDestination();
			}
		}
		else if (_state == EState.MoveToDestination)
		{
			VectorXZ dest = Vespa_Unity_Vector3_Extension_Method.VecXZ(GetDestinationFromPacket());
			MoveTo(MainTimer.DefaultLocal, dest, MoveSpeed, idleActionIfEnd: true, out var endMove);
			if (endMove)
			{
				State_End();
			}
		}
	}

	private void State_Looting()
	{
		_state = EState.Looting;
		_stateTime = MainTimer.time;
	}

	private void State_Waiting()
	{
		_state = EState.Waiting;
		_stateTime = MainTimer.time;
		BattleFacade.CreatureCacheData creatureCache = Owner.CreatureCache;
		if (creatureCache.Uid != 0)
		{
			Vector3 i_src = ((creatureCache.TeamType == TeamType.Left) ? Vector3.right : Vector3.left);
			Owner.Action.Idle(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src));
		}
	}

	private void State_MoveToDestination()
	{
		_state = EState.MoveToDestination;
		_stateTime = MainTimer.time;
	}

	private void State_End()
	{
		_state = EState.End;
		Vector3 i_src = Owner.CreatureCache.Direction;
		Owner.Action.Idle();
		Owner.SetBattlePos(GetDestinationFromPacket());
		Owner.Action.SetDirection(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src), blendMotion: false);
	}

	private Vector3 GetDestinationFromPacket()
	{
		return _packet.Position;
	}
}
