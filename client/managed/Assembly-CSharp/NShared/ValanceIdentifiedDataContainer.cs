using NShared.NJit;

namespace NShared;

public class ValanceIdentifiedDataContainer : JitDataContainerDefault<int, ValanceIdentifiedData>
{
	public const string TABLENAME = "ValanceIdentifiedTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ValanceIdentifiedTable");
}
