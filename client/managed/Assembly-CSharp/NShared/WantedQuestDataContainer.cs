using NShared.NJit;

namespace NShared;

public class WantedQuestDataContainer : JitDataContainerDefault<int, WantedQuestData>
{
	public const string TABLENAME = "WantedQuestTable";

	public override TablePathDefine PathDefine => new TablePathDefine("WantedQuestTable");
}
