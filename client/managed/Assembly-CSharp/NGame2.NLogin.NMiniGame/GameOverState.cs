namespace NGame2.NLogin.NMiniGame;

internal class GameOverState : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (GameGlobal.OnKeyDown())
		{
			GameGlobal.SetNextState(State.PlayState);
		}
	}
}
