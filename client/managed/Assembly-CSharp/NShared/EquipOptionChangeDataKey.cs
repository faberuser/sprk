using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipOptionChangeDataKey
{
	[Key(0)]
	public int OptionIndex;

	[Key(1)]
	public int BeforeTier;

	[Key(2)]
	public int AfterTier;

	public EquipOptionChangeDataKey(int optionIndex, int beforeTier, int afterTier)
	{
		OptionIndex = optionIndex;
		BeforeTier = beforeTier;
		AfterTier = afterTier;
	}

	public bool Equals(EquipOptionChangeDataKey other)
	{
		if (OptionIndex == other.OptionIndex && BeforeTier == other.BeforeTier)
		{
			return AfterTier == other.AfterTier;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(OptionIndex, BeforeTier, AfterTier);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({OptionIndex}, {BeforeTier}, {AfterTier})";
	}
}
