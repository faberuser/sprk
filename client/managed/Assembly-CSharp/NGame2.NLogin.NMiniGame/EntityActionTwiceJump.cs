namespace NGame2.NLogin.NMiniGame;

internal class EntityActionTwiceJump : NGame2.NLogin.NMiniGame.EntityActionJump
{
	private float _twiceJumvel;

	private bool _twiceJump;

	public EntityActionTwiceJump(float jumpvel, float twiceJumpVel)
		: base(jumpvel)
	{
		_twiceJumvel = twiceJumpVel;
	}

	public EntityActionTwiceJump(float jumpvel)
		: base(jumpvel)
	{
		_twiceJumvel = jumpvel;
	}

	public override void Update()
	{
		base.Update();
		if (_gameGlobal.OnKeyDown() && !_entity.OnGround() && !_twiceJump)
		{
			_entity.VelY = _twiceJumvel;
			_twiceJump = true;
		}
		if (_entity.OnGround())
		{
			_twiceJump = false;
		}
	}
}
