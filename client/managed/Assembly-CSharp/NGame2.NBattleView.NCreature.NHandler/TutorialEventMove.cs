using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public class TutorialEventMove : BaseHandler, IHandler
{
	private float _xOffset;

	public TutorialEventMove(Creature creature, float xOffset)
		: base(creature)
	{
		_xOffset = xOffset;
	}

	private void Close()
	{
	}

	void IHandler.Begin()
	{
	}

	bool IHandler.IsEnd()
	{
		return false;
	}

	void IHandler.Cancel()
	{
		Close();
	}

	void IHandler.ChainUpdate()
	{
		if (!Vespa_Common_Class_Extension_Method.Interface((IHandler)this).IsEnd())
		{
			Vector3 i_src = Owner.Position + Vector3.right * _xOffset;
			MoveTo(Vespa_Unity_Vector3_Extension_Method.VecXZ(i_src), base.Speed, idleActionIfEnd: true, out var _);
		}
	}
}
