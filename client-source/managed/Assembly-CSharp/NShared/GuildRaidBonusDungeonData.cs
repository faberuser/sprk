using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class GuildRaidBonusDungeonData : GuildRaidDungeonData, IDataKeySelector<GuildRaidBonusDungeonDataKey>, IRaidData
{
	[Key(20)]
	public int BonusGroup { get; set; }

	[Key(21)]
	public int Rate { get; set; }

	GuildRaidBonusDungeonDataKey IDataKeySelector<GuildRaidBonusDungeonDataKey>.Key => new GuildRaidBonusDungeonDataKey(base.ChapterIndex, base.DungeonIndex, BonusGroup);

	public override string ToString()
	{
		return "BonusGroup =" + BonusGroup;
	}
}
