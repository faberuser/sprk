using NShared.NJit;

namespace NShared;

public class MemoryFragmentDataContainer : JitDataContainerDefault<int, MemoryFragmentData>
{
	public const string TABLENAME = "MemoryFragmentTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MemoryFragmentTable");
}
