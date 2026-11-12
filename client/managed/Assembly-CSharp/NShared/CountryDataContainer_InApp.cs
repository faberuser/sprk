using NShared.NJit;

namespace NShared;

public class CountryDataContainer_InApp : CountryDataContainer_Base, IData_from_PreloadInAppBytes
{
	public const string TABLENAME = "DefaultTableData/CountryTable";

	public override TablePathDefine PathDefine => new TablePathDefine("DefaultTableData/CountryTable");
}
