using System;
using NShared;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class DungeonChangeMove : BaseHandler, IHandler
{
	private enum State
	{
		RunToForward,
		Return
	}

	private NShared.SetPosition _packet;

	private State _state;

	private VectorXZ _forwardPositionXZ;

	private const float _moveSecond = 2f;

	private DateTime _moveBeginTime;

	public DungeonChangeMove(Creature creature, NShared.SetPosition packet)
		: base(creature)
	{
		_packet = packet;
	}

	private void Close()
	{
		_packet = null;
	}

	void IHandler.Begin()
	{
		_state = State.RunToForward;
		_moveBeginTime = DateTime.Now;
		Vector3 position = Owner.Position;
		position += Vector3.right * 50f;
		_forwardPositionXZ = Vespa_Unity_Vector3_Extension_Method.VecXZ(position);
	}

	bool IHandler.IsEnd()
	{
		return _packet == null;
	}

	void IHandler.Cancel()
	{
		Close();
	}

	void IHandler.ChainUpdate()
	{
		if (Vespa_Common_Class_Extension_Method.Interface((IHandler)this).IsEnd())
		{
			return;
		}
		if (_state == State.RunToForward)
		{
			MoveTo(_forwardPositionXZ, base.Speed, idleActionIfEnd: true, out var _);
			if ((DateTime.Now - _moveBeginTime).TotalSeconds >= 2.0)
			{
				_state = State.Return;
			}
		}
		else if (_state == State.Return)
		{
			Owner.SetBattlePos(_packet.Position);
			Owner.Action.Idle(Vespa_Unity_Vector3_Extension_Method.VecXZ(Vector3.right));
			Owner.SetCreateWaveIndex(0);
			Close();
		}
	}
}
