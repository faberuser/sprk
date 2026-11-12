using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class DungeonHeroListSetData : IDataKeySelector<DungeonHeroListSetKey>
{
	[Key(0)]
	public int GroupIndex { get; set; }

	[Key(1)]
	public int HeroIndex { get; set; }

	[Key(2)]
	public int CostumeIndex { get; set; }

	DungeonHeroListSetKey IDataKeySelector<DungeonHeroListSetKey>.Key => new DungeonHeroListSetKey(GroupIndex, HeroIndex);
}
