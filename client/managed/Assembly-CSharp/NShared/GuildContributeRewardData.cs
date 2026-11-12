using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildContributeRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int ContributedGem { get; set; }

	[Key(1)]
	public int RewardGuildActivityPoint { get; set; }

	int IDataKeySelector<int>.Key => ContributedGem;

	public override string ToString()
	{
		return "ContributedGem=" + ContributedGem;
	}
}
