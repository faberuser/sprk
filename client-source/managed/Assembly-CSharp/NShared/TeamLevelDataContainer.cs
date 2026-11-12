using NShared.NJit;

namespace NShared;

public class TeamLevelDataContainer : JitDataContainerDefault<int, TeamLevelData>, IExpGetter
{
	public const string TABLENAME = "TeamLevelTable";

	public override TablePathDefine PathDefine => new TablePathDefine("TeamLevelTable");

	protected override void OnEndLoadFromJsonData()
	{
		long num = 0L;
		foreach (TeamLevelData item in base.BaseList.JsonLoad_GetOrNew())
		{
			item.TotalExp = num;
			num += item.LocalExp;
		}
	}

	public long GetTotalExp(int level, long localExp)
	{
		return (GetData(level)?.TotalExp ?? 0) + localExp;
	}

	long IExpGetter.GetLocalExp(int level)
	{
		return GetData(level)?.LocalExp ?? 0;
	}
}
