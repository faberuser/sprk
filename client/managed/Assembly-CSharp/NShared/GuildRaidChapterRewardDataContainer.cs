using NShared.NJit;

namespace NShared;

public class GuildRaidChapterRewardDataContainer : JitDataContainerDefault<GuildRaidChapterRewardDataKey, GuildRaidChapterRewardData>
{
	public const string TABLENAME = "GuildRaidChapterRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("GuildRaidChapterRewardTable");

	public GuildRaidChapterRewardData GetDataOfScore(int chapterIndex, int difficulty, long score)
	{
		GuildRaidChapterRewardData result = null;
		foreach (GuildRaidChapterRewardData allDatum in GetAllData())
		{
			if (allDatum.ChapterIndex == chapterIndex && allDatum.Difficulty == difficulty && score >= allDatum.TotalDamage)
			{
				result = allDatum;
			}
		}
		return result;
	}
}
