using NShared.NJit;

namespace NShared;

public class CodecSupportListDataContainer : JitDataContainerDefault<int, CodecSupportListData>, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/CodecSupportListTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/CodecSupportListTable");
}
