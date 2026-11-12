using NShared.NJit;

namespace NShared;

public class CreatureLevelStatDataContainer2 : JitDataContainerDefault<int, CreatureLevelStatData2>
{
	public const string TABLENAME = "CreatureLevelStatTable2";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureLevelStatTable2");
}
