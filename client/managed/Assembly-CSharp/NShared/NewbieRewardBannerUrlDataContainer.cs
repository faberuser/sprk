using NShared.NJit;

namespace NShared;

public class NewbieRewardBannerUrlDataContainer : JitDataContainerDefault<NewbieRewardBannerUrlKey, NewbieRewardBannerUrlData>
{
	public const string TABLENAME = "NewbieRewardBannerUrlTable";

	public override TablePathDefine PathDefine => new TablePathDefine("NewbieRewardBannerUrlTable");

	public NewbieRewardBannerUrlData GetData(NewbieRewardType rewardType, int day, string language)
	{
		return GetData(new NewbieRewardBannerUrlKey(rewardType, day, language));
	}
}
