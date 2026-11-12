namespace NGame2.NLogin.NMiniGame;

public interface IGameEventPool
{
	IGameEvent Pop(string typeName);

	void Push(IGameEvent gameEvent);

	void Clear();
}
