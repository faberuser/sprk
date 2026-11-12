using NShared.NJit;

namespace NShared;

public class OrdealArenaTierDataContainer : JitDataContainerDefault<MatchTierType, OrdealArenaTierData>
{
	public const string TABLENAME = "OrdealArenaTierTable";

	public override TablePathDefine PathDefine => new TablePathDefine("OrdealArenaTierTable");
}
