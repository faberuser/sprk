using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ChapterDungeonKey2
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	[IgnoreMember]
	public bool IsValid
	{
		get
		{
			if (ChapterIndex != 0)
			{
				return DungeonIndex != 0;
			}
			return false;
		}
	}

	public ChapterDungeonKey2(int chapterIndex, int dungeonIndex)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
	}

	public bool Equals(ChapterDungeonKey2 other)
	{
		if (ChapterIndex == other.ChapterIndex)
		{
			return DungeonIndex == other.DungeonIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex);
	}

	public override string ToString()
	{
		return $"ChapterIndex={ChapterIndex} DungeonIndex={DungeonIndex}";
	}
}
