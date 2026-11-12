using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct MissionVisualKey
{
	[Key(0)]
	public int MainCategoryIndex;

	[Key(1)]
	public int SubCategoryIndex;

	[Key(2)]
	public int MainStep;

	[Key(3)]
	public int SubStep;

	public MissionVisualKey(int mainCategory, int subCategory, int mainStep, int subStep)
	{
		MainCategoryIndex = mainCategory;
		SubCategoryIndex = subCategory;
		MainStep = mainStep;
		SubStep = subStep;
	}

	public bool Equals(MissionVisualKey other)
	{
		if (MainCategoryIndex == other.MainCategoryIndex && SubCategoryIndex == other.SubCategoryIndex && MainStep == other.MainStep)
		{
			return SubStep == other.SubStep;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(MainCategoryIndex, SubCategoryIndex, MainStep, SubStep);
	}

	public override string ToString()
	{
		return $"MainCategoryIndex={MainCategoryIndex} SubCategoryIndex={SubCategoryIndex} MainStep={MainStep} SubStep={SubStep}";
	}
}
