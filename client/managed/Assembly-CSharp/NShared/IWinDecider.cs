namespace NShared;

public interface IWinDecider
{
	void Init(WinDeciderType type, string[] values);

	bool Decide(BattleInstance battleInstance, out TeamType teamType);

	WinDeciderType GetWinDeciderType();
}
