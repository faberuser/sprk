using NShared.NJit;

namespace NShared;

public class StaminaDataContainer : JitDataContainerDefault<StaminaType, StaminaData>
{
	public const string TABLENAME = "StaminaTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StaminaTable");
}
