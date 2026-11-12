using NShared.NJit;

namespace NShared;

public class NewPayShopOpenConditionDataContainer : JitDataContainerDefault<PayShopUserGroup, NewPayShopOpenConditionData>
{
	public const string TABLENAME = "NewPayShopOpenConditionTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewPayShopOpenConditionTable");
}
