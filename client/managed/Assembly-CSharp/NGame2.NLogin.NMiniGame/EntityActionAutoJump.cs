using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

internal class EntityActionAutoJump : NGame2.NLogin.NMiniGame.EntityActionBase
{
	private float _jumpDisFromEntityToObstacle;

	private float _jumpVel;

	public EntityActionAutoJump(float jumpVel, float distanceFromEntityToObstacle)
	{
		_jumpVel = jumpVel;
		_jumpDisFromEntityToObstacle = distanceFromEntityToObstacle;
	}

	public override void Update()
	{
		if (DoJump() && _entity.OnGround())
		{
			_entity.VelY = _jumpVel;
		}
	}

	protected bool DoJump()
	{
		if (!(_gameGlobal.GetCurrentState() is NGame2.NLogin.NMiniGame.PlayState playState))
		{
			return false;
		}
		float num = float.MaxValue;
		foreach (NGame2.NLogin.NMiniGame.ObstacleEntity item in playState.ObstacleContainer)
		{
			if (!(item.Rect.position.x < _entity.Rect.position.x))
			{
				float b = Vector2.Distance(_entity.Rect.position, item.Rect.position);
				num = Mathf.Min(num, b);
			}
		}
		if (num > _jumpDisFromEntityToObstacle)
		{
			return false;
		}
		return true;
	}
}
