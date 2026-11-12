using System;

namespace NShared;

public class BattleLog
{
	public BaseBattleLog BaseBattleLog;

	public bool Processed;

	public Action OnProcessed;

	public int No => BaseBattleLog.No;

	public long GlobalTimeMs => BaseBattleLog.GlobalTimeMs;

	public BattleLogType Type => BaseBattleLog.Type;

	public string Data => BaseBattleLog.Data;
}
