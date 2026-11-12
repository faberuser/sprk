using NShared.NJit;

namespace NShared;

public class CommunityWebViewDataContainer : JitDataContainerDefault<int, CommunityWebViewData>
{
	public const string TABLENAME = "CommunityWebViewTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CommunityWebViewTable");
}
