using NShared.NJit;

namespace NShared;

public class CSMailDataContainer : JitDataContainerDefault<int, CSMailData>
{
	public const string TABLENAME = "CSMailTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CSMailTable");
}
