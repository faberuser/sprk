using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildRaidChapterRewardData : IDataKeySelector<GuildRaidChapterRewardDataKey>
{
	[Key(0)]
	public int ChapterIndex { get; set; }

	[Key(1)]
	public int Difficulty { get; set; }

	[Key(2)]
	public long TotalDamage { get; set; }

	[Key(3)]
	public string Credit { get; set; }

	[Key(4)]
	public int RewardIndex { get; set; }

	[Key(5)]
	public string MailTitleKey { get; set; }

	[Key(6)]
	public string MailContentKey { get; set; }

	GuildRaidChapterRewardDataKey IDataKeySelector<GuildRaidChapterRewardDataKey>.Key => new GuildRaidChapterRewardDataKey(ChapterIndex, Difficulty, TotalDamage);

	public override string ToString()
	{
		return "GuildRaidChapterIndex=" + ChapterIndex;
	}
}
