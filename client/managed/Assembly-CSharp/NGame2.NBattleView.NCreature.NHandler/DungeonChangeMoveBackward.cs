using System;
using NShared;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class DungeonChangeMoveBackward : BaseHandler, IHandler
{
	private enum State
	{
		RunToBackward,
		Return
	}

	private NShared.SetPosition _packet;

	private State _state;

	private VectorXZ _bakcwoardPositionXZ;

	private const float _moveSecond = 2f;

	private DateTime _moveBeginTime;

	public DungeonChangeMoveBackward(Creature creature, NShared.SetPosition packet)
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
		_state = State.RunToBackward;
		_moveBeginTime = DateTime.Now;
		Vector3 position = Owner.Position;
		position += Vector3.left * 50f;
		_bakcwoardPositionXZ = Vespa_Unity_Vector3_Extension_Method.VecXZ(position);
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
		if (_state == State.RunToBackward)
		{
			MoveTo(_bakcwoardPositionXZ, base.Speed, idleActionIfEnd: true, out var _);
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
