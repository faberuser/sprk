using NShared;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class SetPosition : BaseHandler, IHandler
{
	public const float PositionThreshold = 0.2f;

	private NShared.SetPosition _packet;

	public SetPosition(Creature creature, NShared.SetPosition packet)
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
		if (_packet.MethodType == NShared.SetPosition.SetPositionMethodType.Spawn)
		{
			Owner.SetBattlePos(_packet.Position);
			Close();
			return;
		}
		Vector3 position = Owner.Position;
		Vector3 b = _packet.Position;
		if (Vector3.Distance(position, b) < 0.2f)
		{
			Close();
		}
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
		if (!Vespa_Common_Class_Extension_Method.Interface((IHandler)this).IsEnd())
		{
			VectorXZ dest = Vespa_Unity_Vector3_Extension_Method.VecXZ(GetDestinationFromPacket());
			MoveTo(dest, base.Speed, idleActionIfEnd: true, out var endMove);
			if (endMove)
			{
				Owner.Action.Idle();
				Close();
			}
		}
	}

	private Vector3 GetDestinationFromPacket()
	{
		return _packet.Position;
	}
}
