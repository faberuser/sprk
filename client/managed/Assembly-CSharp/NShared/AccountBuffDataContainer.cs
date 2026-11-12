using NShared.NJit;

namespace NShared;

public class AccountBuffDataContainer : JitDataContainerDefault<int, AccountBuffData>
{
	public const string TABLENAME = "AccountBuffTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AccountBuffTable");
}
