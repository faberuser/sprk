using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SubQuestDataKey
{
	[Key(0)]
	public int QuestIndex;

	[Key(1)]
	public int Step;

	[IgnoreMember]
	public bool IsValid
	{
		get
		{
			if (QuestIndex != 0)
			{
				return Step != 0;
			}
			return false;
		}
	}

	public SubQuestDataKey(int chapterIndex, int dungeonIndex)
	{
		QuestIndex = chapterIndex;
		Step = dungeonIndex;
	}

	public bool Equals(SubQuestDataKey other)
	{
		if (QuestIndex == other.QuestIndex)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(QuestIndex, Step);
	}

	public override string ToString()
	{
		return $"QuestIndex={QuestIndex} Step={Step}";
	}
}
