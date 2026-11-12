using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

internal class EntityMove : IGameEvent
{
	public enum EState
	{
		Run,
		Jump,
		Stop
	}

	public int ID;

	public Vector2 MovePos;

	public EState State;

	public virtual void Set(int id, Vector3 movePos, EState state)
	{
		ID = id;
		MovePos = movePos;
		State = state;
	}

	public virtual void Reset()
	{
		ID = 0;
		MovePos = Vector2.zero;
		State = EState.Run;
	}
}
