namespace NShared;

public interface IBattleLogListener
{
	void OnProcessBattleLog(long globalTimeMs, BattleLog battleLog);
}
