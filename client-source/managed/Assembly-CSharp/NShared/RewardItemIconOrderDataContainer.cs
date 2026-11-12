using NShared.NJit;

namespace NShared;

public class RewardItemIconOrderDataContainer : JitDataContainerDefault<RewardItemIconTypeDataKey, RewardItemIconOrderData>
{
	public const string TABLENAME = "RewardItemIconOrderTable";

	public override TablePathDefine PathDefine => new TablePathDefine("RewardItemIconOrderTable");
}
