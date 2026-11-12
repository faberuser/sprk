namespace NShared.BuyGuildRaidBootyItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public GuildPointResultInfo GuildPointResult { get; set; }

	public EquipItemInfo EquipItemInfo { get; set; }
}
