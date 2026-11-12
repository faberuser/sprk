namespace NGame2.NLogin.NMiniGame;

internal class RemoveObstacle : IGameEvent
{
	public int ObstacleID;

	public virtual void Set(int id)
	{
		ObstacleID = id;
	}

	public virtual void Reset()
	{
		ObstacleID = 0;
	}
}
