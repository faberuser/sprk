namespace NGame2.NLogin.NMiniGame;

internal class EntityActionIntervalJump : NGame2.NLogin.NMiniGame.EntityActionBase
{
	private float _jumpVel;

	private float _jumpInteval;

	private float _timer;

	public EntityActionIntervalJump(float jumpVel, float jumpInterval)
	{
		_jumpVel = jumpVel;
		_jumpInteval = jumpInterval;
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
		_timer += GameGlobal.TimeDelta();
		if (_timer < _jumpInteval)
		{
			return false;
		}
		_timer = 0f;
		return true;
	}
}
