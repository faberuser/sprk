using NShared.NJit;

namespace NShared;

public class ConstantDataContainer_InApp : ConstantDataContainer_Base, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/ConstantTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/ConstantTable");
}
