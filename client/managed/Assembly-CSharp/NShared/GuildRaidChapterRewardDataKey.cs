using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GuildRaidChapterRewardDataKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int Difficulty;

	[Key(2)]
	public long TotalDamage;

	public GuildRaidChapterRewardDataKey(int chapterIndex, int difficulty, long totalDamage)
	{
		ChapterIndex = chapterIndex;
		Difficulty = difficulty;
		TotalDamage = totalDamage;
	}

	public bool Equals(GuildRaidChapterRewardDataKey other)
	{
		if (ChapterIndex == other.ChapterIndex && Difficulty == other.Difficulty)
		{
			return TotalDamage == other.TotalDamage;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, Difficulty, TotalDamage);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {Difficulty}, {TotalDamage})";
	}
}
