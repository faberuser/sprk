using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CreatureLevelFactorDataKey
{
	[Key(0)]
	public int FactorIndex;

	[Key(1)]
	public int Level;

	[IgnoreMember]
	public bool IsValid
	{
		get
		{
			if (FactorIndex != 0)
			{
				return Level != 0;
			}
			return false;
		}
	}

	public CreatureLevelFactorDataKey(int factorIndex, int level)
	{
		FactorIndex = factorIndex;
		Level = level;
	}

	public bool Equals(CreatureLevelFactorDataKey other)
	{
		if (FactorIndex == other.FactorIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(FactorIndex, Level);
	}

	public override string ToString()
	{
		return $"FactorIndex={FactorIndex} Level={Level}";
	}
}
