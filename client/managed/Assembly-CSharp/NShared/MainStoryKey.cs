using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct MainStoryKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int StoryNo;

	[IgnoreMember]
	public bool IsValid
	{
		get
		{
			if (ChapterIndex != 0)
			{
				return StoryNo != 0;
			}
			return false;
		}
	}

	public MainStoryKey(int chapterIndex, int storyNo)
	{
		ChapterIndex = chapterIndex;
		StoryNo = storyNo;
	}

	public bool Equals(MainStoryKey other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return StoryNo == other.StoryNo;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, StoryNo);
	}

	public override string ToString()
	{
		return $"ChapterIndex={ChapterIndex} StoryNo={StoryNo}";
	}
}
