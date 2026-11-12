namespace NShared.CraftPunishmentRune;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo EquipItemResult { get; set; }
}
