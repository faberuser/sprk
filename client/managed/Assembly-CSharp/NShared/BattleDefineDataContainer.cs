using NShared.NJit;

namespace NShared;

public class BattleDefineDataContainer : JitDataContainerDefault<string, BattleDefineData>
{
	public const string TABLENAME = "BattleDefineTable";

	public override TablePathDefine PathDefine => new TablePathDefine("BattleDefineTable");

	public int GetMaxBattleTime(string code)
	{
		return GetData(code)?.MaxTime ?? 0;
	}

	public int GetExtraBattleTime(string code)
	{
		return GetData(code)?.ExtraTime ?? 0;
	}

	public EBattleMenu GetReplayBattleMenu()
	{
		return GetData("Replay_BattleMenu")?.BattleMenu ?? (EBattleMenu.TimeStatus | EBattleMenu.SystemButton);
	}
}
