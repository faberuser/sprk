using NShared.NJit;

namespace NShared;

public class KarmaRewardDataContainer : JitDataContainerDefault<KarmaRewardKey, KarmaRewardData>
{
	public const string TABLENAME = "KarmaRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("KarmaRewardTable");
}
