using NShared.NJit;

namespace NShared;

public class IconChatWindowDataContainer : JitDataContainerDefault<int, IconChatWindowData>
{
	public const string TABLENAME = "IconChatWindowTable";

	public override TablePathDefine PathDefine => new TablePathDefine("IconChatWindowTable");
}
