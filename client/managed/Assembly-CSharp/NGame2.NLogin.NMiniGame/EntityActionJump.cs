namespace NGame2.NLogin.NMiniGame;

internal class EntityActionJump : NGame2.NLogin.NMiniGame.EntityActionBase
{
	protected float _jumpVel;

	public EntityActionJump(float jumpVel)
	{
		_jumpVel = jumpVel;
	}

	public override void Update()
	{
		if (DoJump(_entity.GameGlobal) && _entity.OnGround())
		{
			_entity.VelY = _jumpVel;
		}
	}

	protected bool DoJump(IGameGlobal GameGlobal)
	{
		return GameGlobal.OnKeyDown();
	}
}
