using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CampaignMultiAvatarKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int LineIndex;

	public CampaignMultiAvatarKey(int chapterIndex, int lineIndex)
	{
		ChapterIndex = chapterIndex;
		LineIndex = lineIndex;
	}

	public bool Equals(CampaignMultiAvatarKey other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return LineIndex == other.LineIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, LineIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {LineIndex})";
	}
}
