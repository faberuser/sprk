using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct TutorialKey
{
	[Key(0)]
	public int Index;

	[Key(1)]
	public int Sequence;

	[IgnoreMember]
	public bool IsValid
	{
		get
		{
			if (Index != 0)
			{
				return Sequence != 0;
			}
			return false;
		}
	}

	public TutorialKey(int index, int sequence)
	{
		Index = index;
		Sequence = sequence;
	}

	public bool Equals(TutorialKey other)
	{
		if (Index == other.Index)
		{
			return Sequence == other.Sequence;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Index, Sequence);
	}

	public override string ToString()
	{
		return $"Index={Index} Sequence={Sequence}";
	}
}
