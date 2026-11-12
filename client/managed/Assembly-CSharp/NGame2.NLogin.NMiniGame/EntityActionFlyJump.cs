namespace NGame2.NLogin.NMiniGame;

internal class EntityActionFlyJump : NGame2.NLogin.NMiniGame.EntityActionJump
{
	private float _flyJumvel;

	public EntityActionFlyJump(float jumpvel, float flyJumvel)
		: base(jumpvel)
	{
		_flyJumvel = flyJumvel;
	}

	public EntityActionFlyJump(float jumpvel)
		: base(jumpvel)
	{
		_flyJumvel = jumpvel;
	}

	public override void Update()
	{
		base.Update();
		if (_gameGlobal.OnKeyDown() && !_entity.OnGround())
		{
			_entity.VelY = _flyJumvel;
		}
	}
}
