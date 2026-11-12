namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildRaidBootyEquipItemInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => Id;

	public long Id { get; set; }

	public string CreatedTime { get; set; }

	public int ItemIndex { get; set; }

	public int Star { get; set; }
}
