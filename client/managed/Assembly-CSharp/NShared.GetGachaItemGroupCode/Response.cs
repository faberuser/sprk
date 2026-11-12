namespace NShared.GetGachaItemGroupCode;

[Response]
public class Response : GenericResponse<ResultType>
{
	public string ItemGroupCode { get; set; }

	public string ItemGroupCodeNameKey { get; set; }

	public string BonusItemGroupCode { get; set; }

	public string BonusItemGroupCodeNameKey { get; set; }
}
