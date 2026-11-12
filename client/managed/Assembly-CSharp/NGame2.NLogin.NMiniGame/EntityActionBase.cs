namespace NGame2.NLogin.NMiniGame;

internal class EntityActionBase : IEntityAction
{
	protected MoveEntity _entity;

	protected IGameGlobal _gameGlobal;

	public virtual void Assign(MoveEntity entity)
	{
		_entity = entity;
		_gameGlobal = entity.GameGlobal;
	}

	public virtual void Update()
	{
	}
}
