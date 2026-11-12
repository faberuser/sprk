namespace NGame2.NLogin.NMiniGame;

internal class ReadyState : StateBase
{
	public ReadyState()
	{
		StateBase.State = State.ReadyState;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		GameGlobal.OnEvent(GameGlobal.CreateEventInstance<NGame2.NLogin.NMiniGame.ReadyToPlay>());
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
