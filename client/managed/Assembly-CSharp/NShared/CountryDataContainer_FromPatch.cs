namespace NShared;

public class CountryDataContainer_FromPatch : CountryDataContainer_Base
{
	public const string TABLENAME = "CountryTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CountryTable");
}
