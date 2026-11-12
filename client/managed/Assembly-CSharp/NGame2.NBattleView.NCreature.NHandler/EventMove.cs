using NGame2.NBattleField;
using NVespa;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class EventMove : BaseHandler, IHandler
{
	private bool _endMove;

	private Vector3 _position;

	public EventMove(Creature creature, Vector3 position)
		: base(creature)
	{
		_position = position;
	}

	private void Close()
	{
		_endMove = true;
	}

	void IHandler.Begin()
	{
		SingletonAuto<BattleFacade>.instance.CreatureSetPosition(Owner.Uid, _position, null);
	}

	bool IHandler.IsEnd()
	{
		return _endMove;
	}

	void IHandler.Cancel()
	{
		Close();
	}

	void IHandler.ChainUpdate()
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((IHandler)this).IsEnd())
		{
			MoveTo(Vespa_Unity_Vector3_Extension_Method.VecXZ(_position), base.Speed, idleActionIfEnd: true, out _endMove);
		}
	}
}
