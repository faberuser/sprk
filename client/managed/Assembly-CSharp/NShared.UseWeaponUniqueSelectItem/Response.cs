namespace NShared.UseWeaponUniqueSelectItem;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ItemResultInfo ItemResult { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }
}
