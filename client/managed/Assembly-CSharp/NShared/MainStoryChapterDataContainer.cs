using NShared.NJit;

namespace NShared;

public class MainStoryChapterDataContainer : JitDataContainerDefault<int, MainStoryChapterData>
{
	public const string TABLENAME = "MainStoryChapterTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MainStoryChapterTable");
}
