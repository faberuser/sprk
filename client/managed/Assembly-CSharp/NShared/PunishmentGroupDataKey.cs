using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct PunishmentGroupDataKey
{
	[Key(0)]
	public int GroupIndex;

	[Key(1)]
	public EPunishmentDungeonType DungeonType;

	public PunishmentGroupDataKey(int groupIndex, EPunishmentDungeonType dungeonType)
	{
		GroupIndex = groupIndex;
		DungeonType = dungeonType;
	}

	public bool Equals(PunishmentGroupDataKey other)
	{
		if (GroupIndex == other.GroupIndex)
		{
			return DungeonType == other.DungeonType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(GroupIndex, DungeonType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({GroupIndex}, {DungeonType})";
	}
}
