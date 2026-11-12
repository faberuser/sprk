using NShared.NJit;

namespace NShared;

public class BanRuleDataContainer : JitDataContainerDefault<int, BanRuleData>
{
	public const string TABLENAME = "BanRuleTable";

	public override TablePathDefine PathDefine => new TablePathDefine("BanRuleTable");
}
