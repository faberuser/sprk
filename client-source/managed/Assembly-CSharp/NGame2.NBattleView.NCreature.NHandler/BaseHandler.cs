using NVespa;
using UnityEngine;

namespace NGame2.NBattleView.NCreature.NHandler;

public abstract class BaseHandler
{
	public Creature Owner;

	protected float Speed => (float)Owner.CreatureCache.MoveSpeedMms * 0.001f;

	protected BaseHandler(Creature creature)
	{
		Owner = creature;
	}

	protected void MoveTo(VectorXZ dest, float moveSpeed, bool idleActionIfEnd, out bool endMove)
	{
		MoveTo(Owner.MoveLocalTimer, dest, moveSpeed, idleActionIfEnd, out endMove);
	}

	protected void MoveTo(ILocalTimer timer, VectorXZ dest, float moveSpeed, bool idleActionIfEnd, out bool endMove)
	{
		Vector3 position = Owner.Position;
		VectorXZ vectorXZ = dest - Vespa_Unity_Vector3_Extension_Method.VecXZ(position);
		if (vectorXZ.magnitude < Mathf.Epsilon)
		{
			Owner.Action.Idle();
			endMove = true;
			return;
		}
		float num = (float)timer.deltaTick * 0.001f * moveSpeed;
		if (vectorXZ.magnitude < num)
		{
			Owner.SetBattlePos(dest.ToVec3(Owner.Position.y));
			endMove = true;
			Owner.Action.Idle();
		}
		else if (moveSpeed < Mathf.Epsilon)
		{
			endMove = false;
		}
		else
		{
			VectorXZ vectorXZ2 = vectorXZ.normalized * num;
			Owner.SetBattlePos(Owner.Position + vectorXZ2.ToVec3());
			endMove = false;
			Owner.Action.Run(vectorXZ.normalized, moveSpeed);
		}
	}
}
