namespace NShared.RenewEquipSkill;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo ResultEquipItem { get; set; }

	public int NewSkillIndex { get; set; }

	public int NewSkillStep { get; set; }
}
