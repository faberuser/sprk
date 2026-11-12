using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

internal class CreateObstacle : NGame2.NLogin.NMiniGame.ObstacleBase
{
	public enum EObstacleType
	{
		Long,
		Short,
		Orc
	}

	public EObstacleType Type;

	public void Set(int id, Vector2 pos, EObstacleType type)
	{
		base.Set(id, pos);
		Type = type;
	}

	public override void Reset()
	{
		base.Reset();
		Type = EObstacleType.Long;
	}
}
