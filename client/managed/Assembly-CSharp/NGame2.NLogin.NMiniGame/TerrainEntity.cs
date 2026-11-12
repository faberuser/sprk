namespace NGame2.NLogin.NMiniGame;

public class TerrainEntity : MoveEntity
{
	public enum TerrainType
	{
		Ground,
		Environment
	}

	public TerrainType Type;

	private NGame2.NLogin.NMiniGame.PlayState _playState;

	public TerrainEntity(IGameGlobal gameGlobal, int id)
		: base(gameGlobal, id, applyGravity: false)
	{
	}

	protected override void Move()
	{
		base.Move();
		float num = Rect.position.x;
		if (num + Rect.width < -690f)
		{
			num = GetScrollRightX();
		}
		Rect.x = num;
		NGame2.NLogin.NMiniGame.MoveTerrain moveTerrain = base.GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.MoveTerrain>();
		moveTerrain.Set(base.Id, Rect.position);
		base.GameGlobal.OnEvent(moveTerrain);
	}

	private float GetScrollRightX()
	{
		if (Type != TerrainType.Ground)
		{
			return 690f;
		}
		if (_playState == null)
		{
			_playState = base.GameGlobal.GetCurrentState() as NGame2.NLogin.NMiniGame.PlayState;
		}
		if (_playState != null)
		{
			TerrainEntity terrainEntity = _playState.TerrainList.Find((TerrainEntity element) => element.Type == TerrainType.Ground && element.Id != base.Id);
			if (terrainEntity != null)
			{
				return terrainEntity.Rect.position.x + terrainEntity.Rect.width;
			}
		}
		return 690f;
	}
}
