namespace NShared.ConfirmValanceEnchant;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipItemInfo ResultEquipItem { get; set; }
}
