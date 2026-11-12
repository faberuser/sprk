namespace NShared.GetGuildRaidAllBootyItems;

[Response]
public class Response : GenericResponse<ResultType>
{
	public GuildRaidBootyEquipItemInfo[] EquipItemInfos { get; set; }

	public GuildRaidBootyItemInfo[] ItemInfos { get; set; }
}
