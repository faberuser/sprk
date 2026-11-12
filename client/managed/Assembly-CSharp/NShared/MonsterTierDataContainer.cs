using NShared.NJit;

namespace NShared;

public class MonsterTierDataContainer : JitDataContainerDefault<int, MonsterTierData>
{
	public const string TABLENAME = "MonsterTierTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MonsterTierTable");
}
