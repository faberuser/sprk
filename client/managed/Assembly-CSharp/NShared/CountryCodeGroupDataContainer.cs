using NShared.NJit;

namespace NShared;

public class CountryCodeGroupDataContainer : JitDataContainerDefault<int, CountryCodeGroupData>
{
	public const string TABLENAME = "CountryCodeGroupTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CountryCodeGroupTable");
}
