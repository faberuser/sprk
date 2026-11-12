namespace NShared;

public interface IBattleLogRuntime
{
	BattleLogType GetBattleLogType();

	void Run(BattleInstance battleInstance, BattleLog battleLog);
}
