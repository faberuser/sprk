namespace NGame2.NLogin.NMiniGame;

public interface IState
{
	void OnBegin();

	void OnUpdate();

	void OnEnd();
}
