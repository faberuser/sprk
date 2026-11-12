namespace NShared.RenewConfirmEquipSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipItemInfo ResultEquipItem { get; set; }
}
