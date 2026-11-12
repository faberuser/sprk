using System;

namespace NGame2.NLogin.NMiniGame;

internal class ObstacleEntity : MoveEntity
{
	private Action<int> _obstacleRemoveCallback;

	public ObstacleEntity(IGameGlobal gameGlobal, int id, Action<int> obstacleRemoveCallback)
		: base(gameGlobal, id, applyGravity: false)
	{
		_obstacleRemoveCallback = obstacleRemoveCallback;
	}

	protected override void Move()
	{
		base.Move();
		if (Rect.position.x + Rect.width < -690f)
		{
			NGame2.NLogin.NMiniGame.RemoveObstacle removeObstacle = base.GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.RemoveObstacle>();
			removeObstacle.Set(base.Id);
			base.GameGlobal.OnEvent(removeObstacle);
			_obstacleRemoveCallback(base.Id);
		}
		else
		{
			NGame2.NLogin.NMiniGame.MoveObstacle moveObstacle = base.GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.MoveObstacle>();
			moveObstacle.Set(base.Id, Rect.position);
			base.GameGlobal.OnEvent(moveObstacle);
		}
	}
}
