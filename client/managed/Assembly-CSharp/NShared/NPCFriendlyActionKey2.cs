using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct NPCFriendlyActionKey2
{
	[Key(0)]
	public int HeroIndex;

	[Key(1)]
	public int ItemGrade;

	public NPCFriendlyActionKey2(int heroIndex, int itemGrade)
	{
		HeroIndex = heroIndex;
		ItemGrade = itemGrade;
	}

	public bool Equals(NPCFriendlyActionKey2 other)
	{
		if (HeroIndex == other.HeroIndex)
		{
			return ItemGrade == other.ItemGrade;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(HeroIndex, ItemGrade);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({HeroIndex}, {ItemGrade})";
	}
}
