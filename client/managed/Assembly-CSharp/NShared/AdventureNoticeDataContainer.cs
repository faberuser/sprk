using NShared.NJit;

namespace NShared;

public class AdventureNoticeDataContainer : JitDataContainerDefault<string, AdventureNoticeData>
{
	public const string TABLENAME = "AdventureNoticeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AdventureNoticeTable");
}
