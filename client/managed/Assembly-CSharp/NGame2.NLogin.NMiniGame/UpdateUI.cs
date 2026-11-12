namespace NGame2.NLogin.NMiniGame;

internal class UpdateUI : IGameEvent
{
	public int RunDistance;

	public virtual void Set(int runDistance)
	{
		RunDistance = runDistance;
	}

	public void Reset()
	{
		RunDistance = 0;
	}
}
