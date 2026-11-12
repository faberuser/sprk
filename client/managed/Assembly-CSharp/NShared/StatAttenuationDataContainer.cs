using NShared.NJit;

namespace NShared;

public class StatAttenuationDataContainer : JitDataContainerDefault<StatType, StatAttenuationData>
{
	public const string TABLENAME = "StatAttenuationTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StatAttenuationTable");
}
