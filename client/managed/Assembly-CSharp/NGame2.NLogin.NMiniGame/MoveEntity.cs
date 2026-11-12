using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

public class MoveEntity : EntityBase
{
	public float VelX;

	public float VelY;

	private bool _applyGraivity;

	private List<IEntityAction> _entityActionList = new List<IEntityAction>();

	public MoveEntity(IGameGlobal gameGlobal)
		: base(gameGlobal)
	{
		_applyGraivity = true;
	}

	public MoveEntity(IGameGlobal gameGlobal, int id, bool applyGravity)
		: base(gameGlobal)
	{
		SetId(id);
		_applyGraivity = applyGravity;
	}

	public void AddAction(IEntityAction action)
	{
		action.Assign(this);
		_entityActionList.Add(action);
	}

	public void ClearAction()
	{
		_entityActionList.Clear();
	}

	public virtual void Update()
	{
		Move();
		ApplyGravity();
		ApplyMaxY();
		NotificationMoveEvent();
	}

	protected virtual void Move()
	{
		Vector2 vector = Velocity() * base.GameGlobal.TimeDelta();
		Rect.position += vector;
	}

	protected virtual void NotificationMoveEvent()
	{
		NGame2.NLogin.NMiniGame.EntityMove entityMove = base.GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.EntityMove>();
		entityMove.Set(base.Id, Rect.position, (!OnGround()) ? NGame2.NLogin.NMiniGame.EntityMove.EState.Jump : NGame2.NLogin.NMiniGame.EntityMove.EState.Run);
		base.GameGlobal.OnEvent(entityMove);
	}

	public virtual Vector2 Velocity()
	{
		foreach (IEntityAction entityAction in _entityActionList)
		{
			entityAction.Update();
		}
		return new Vector2(VelX, VelY);
	}

	private void ApplyGravity()
	{
		if (_applyGraivity)
		{
			if (!OnGround())
			{
				VelY += base.GameGlobal.TimeDelta() * -1300f;
			}
			if (Rect.position.y < -248f)
			{
				VelY = 0f;
				Rect.y = -248f;
			}
		}
	}

	private void ApplyMaxY()
	{
		if (Rect.position.y > 156f)
		{
			Rect.y = 156f;
		}
	}

	public bool OnGround()
	{
		return Mathf.Abs(Rect.y - -248f) < float.Epsilon;
	}
}
