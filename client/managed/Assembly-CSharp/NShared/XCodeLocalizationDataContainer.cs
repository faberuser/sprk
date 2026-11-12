using NShared.NJit;

namespace NShared;

public class XCodeLocalizationDataContainer : JitDataContainerDefault<int, XCodeLocalizationData>
{
	public const string TABLENAME = "DefaultTableData/XCodeLocalizationTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/XCodeLocalizationTable");
}
