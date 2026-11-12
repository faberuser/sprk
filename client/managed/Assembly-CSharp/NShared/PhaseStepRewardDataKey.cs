using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PhaseStepRewardDataKey
{
	[Key(0)]
	public int ChapterIndex;

	[Key(1)]
	public int DungeonIndex;

	[Key(2)]
	public PhaseStepRewardType Type;

	[Key(3)]
	public int Step;

	public PhaseStepRewardDataKey(int chapterIndex, int dungeonIndex, PhaseStepRewardType type, int step)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Type = type;
		Step = step;
	}

	public bool Equals(PhaseStepRewardDataKey other)
	{
		if (ChapterIndex == other.ChapterIndex && DungeonIndex == other.DungeonIndex && Type == other.Type)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ChapterIndex, DungeonIndex, Type, Step);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ChapterIndex}, {DungeonIndex}, {Type}, {Step})";
	}
}
