using NShared.NJit;

namespace NShared;

public class StaminaConsumePointDataContainer : JitDataContainerDefault<CurrencyType, StaminaConsumePointData>
{
	public const string TABLENAME = "StaminaConsumePointTable";

	public override TablePathDefine PathDefine => new TablePathDefine("StaminaConsumePointTable");
}
