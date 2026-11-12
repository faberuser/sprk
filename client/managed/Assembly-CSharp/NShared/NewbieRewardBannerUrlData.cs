using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewbieRewardBannerUrlData : IDataKeySelector<NewbieRewardBannerUrlKey>
{
	[Key(0)]
	public NewbieRewardType RewardType { get; set; }

	[Key(1)]
	public int ShowIndex { get; set; }

	[Key(2)]
	public string Language { get; set; }

	[Key(3)]
	public string Url { get; set; }

	NewbieRewardBannerUrlKey IDataKeySelector<NewbieRewardBannerUrlKey>.Key => new NewbieRewardBannerUrlKey(RewardType, ShowIndex, Language);

	public override string ToString()
	{
		return "NewbieRewardType=" + RewardType;
	}
}
