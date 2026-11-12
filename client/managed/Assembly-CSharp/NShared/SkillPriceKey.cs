using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SkillPriceKey
{
	[Key(0)]
	public int SlotIndex;

	[Key(1)]
	public int Level;

	public SkillPriceKey(int slotIndex, int level)
	{
		SlotIndex = slotIndex;
		Level = level;
	}

	public bool Equals(SkillPriceKey other)
	{
		if (SlotIndex == other.SlotIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(SlotIndex, Level);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({SlotIndex}, {Level})";
	}
}
