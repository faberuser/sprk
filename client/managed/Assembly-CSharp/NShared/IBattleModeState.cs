namespace NShared;

public interface IBattleModeState
{
	void Init();

	void Update();

	IBattleModeState GetNextState();
}
