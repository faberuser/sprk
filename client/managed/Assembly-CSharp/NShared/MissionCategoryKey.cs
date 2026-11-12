using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct MissionCategoryKey
{
	[Key(0)]
	public int MainCategoryIndex;

	[Key(1)]
	public int SubCategoryIndex;

	public MissionCategoryKey(int mainCategoryIndex, int subCategoryIndex)
	{
		MainCategoryIndex = mainCategoryIndex;
		SubCategoryIndex = subCategoryIndex;
	}

	public bool Equals(MissionCategoryKey other)
	{
		if (MainCategoryIndex == other.MainCategoryIndex)
		{
			return SubCategoryIndex == other.SubCategoryIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(MainCategoryIndex, SubCategoryIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({MainCategoryIndex}, {SubCategoryIndex})";
	}
}
