using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildRaidDummyDungeonData : GuildRaidDungeonData, IDataKeySelector<ChapterDungeonKey2>, IRaidData
{
	ChapterDungeonKey2 IDataKeySelector<ChapterDungeonKey2>.Key => new ChapterDungeonKey2(base.ChapterIndex, base.DungeonIndex);
}
