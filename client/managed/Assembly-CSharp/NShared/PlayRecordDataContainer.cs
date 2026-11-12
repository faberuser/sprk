using NShared.NJit;

namespace NShared;

public class PlayRecordDataContainer : JitDataContainerDefault<int, PlayRecordData>
{
	public const string TABLENAME = "PlayRecordTable";

	public override TablePathDefine PathDefine => new TablePathDefine("PlayRecordTable");
}
