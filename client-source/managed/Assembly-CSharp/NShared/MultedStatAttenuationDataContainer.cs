using NShared.NJit;

namespace NShared;

public class MultedStatAttenuationDataContainer : JitDataContainerDefault<StatType, StatAttenuationData>
{
	public const string TABLENAME = "MultedStatAttenuationTable";

	public override TablePathDefine PathDefine => new TablePathDefine("MultedStatAttenuationTable");
}
