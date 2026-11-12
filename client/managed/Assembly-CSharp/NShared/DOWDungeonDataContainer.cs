using NShared.NJit;

namespace NShared;

public class DOWDungeonDataContainer : JitDataContainerDefault<int, DOWDungeonData>
{
	public const string TABLENAME = "DOWDungeonTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DOWDungeonTable");
}
