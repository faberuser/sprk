using UnityEngine;

namespace NGame2.NLogin.NMiniGame;

public interface IGameGlobal
{
	StateBase GetCurrentState();

	float TimeDelta();

	void SetNextState(State state);

	bool OnKeyDown();

	KeyCode GeyKeyCoden();

	T CreateEventInstance<T>() where T : IGameEvent, new();

	void OnEvent(IGameEvent gameEvent);
}
