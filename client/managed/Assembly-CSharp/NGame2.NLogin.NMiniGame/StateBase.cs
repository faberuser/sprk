namespace NGame2.NLogin.NMiniGame;

public class StateBase : IState
{
	public static State State;

	public IGameGlobal GameGlobal;

	public virtual void OnBegin()
	{
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnEnd()
	{
	}
}
