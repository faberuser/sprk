using NShared.NJit;

namespace NShared;

public class StatAttributeDataContainer : JitDataContainerDefault<StatType, StatAttributeData>
{
	public const string TABLENAME = "StatAttributeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StatAttributeTable");
}
