namespace NShared.ValanceIdentified;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public EquipItemIdentifiedInfo[] EquipItemIdentifiedInfos { get; set; }
}
