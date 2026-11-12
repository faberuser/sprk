using NShared.NJit;

namespace NShared;

public class CampaignFieldDataContainer : JitDataContainerDefault<string, CampaignFieldData>
{
	public const string TABLENAME = "CampaignFieldTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CampaignFieldTable");
}
