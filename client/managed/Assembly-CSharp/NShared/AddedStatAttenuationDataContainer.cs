using NShared.NJit;

namespace NShared;

public class AddedStatAttenuationDataContainer : JitDataContainerDefault<StatType, StatAttenuationData>
{
	public const string TABLENAME = "AddedStatAttenuationTable";

	public override TablePathDefine PathDefine => new TablePathDefine("AddedStatAttenuationTable");
}
