using NShared.NJit;

namespace NShared;

public class MonthlyHeroDataContainer : JitDataContainerDefault<int, MonthlyHeroData>
{
	public const string TABLENAME = "MonthlyHeroTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MonthlyHeroTable");
}
