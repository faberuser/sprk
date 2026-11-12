using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

internal class ObstacleBase : IGameEvent
{
	public int ObstacleID;

	public Vector2 Pos;

	public virtual void Set(int id, Vector2 pos)
	{
		ObstacleID = id;
		Pos = pos;
	}

	public virtual void Reset()
	{
		ObstacleID = 0;
		Pos = Vector2.zero;
	}
}
