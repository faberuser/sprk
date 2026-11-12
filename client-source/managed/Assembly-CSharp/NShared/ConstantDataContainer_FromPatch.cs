namespace NShared;

public class ConstantDataContainer_FromPatch : ConstantDataContainer_Base
{
	public const string TABLENAME = "ConstantTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ConstantTable");
}
