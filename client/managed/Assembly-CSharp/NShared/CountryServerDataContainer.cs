using NShared.NJit;

namespace NShared;

public class CountryServerDataContainer : JitDataContainerDefault<string, CountryServerData>
{
	public const string TABLENAME = "CountryServerTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CountryServerTable");
}
