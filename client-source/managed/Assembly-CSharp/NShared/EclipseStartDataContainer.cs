using NShared.NJit;

namespace NShared;

public class EclipseStartDataContainer : JitDataContainerDefault<int, EclipseStartData>
{
	public const string TABLENAME = "EclipseStartTable";

	public override TablePathDefine PathDefine => new TablePathDefine("EclipseStartTable");
}
