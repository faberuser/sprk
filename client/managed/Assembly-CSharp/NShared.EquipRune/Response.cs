namespace NShared.EquipRune;

[Response]
public class Response : GenericResponse<ResultType>
{
	public HeroInfo HeroInfo { get; set; }

	public CurrencyResultInfo3 CurrencyResult { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public HeroRunePageInfo HeroRunePageInfo { get; set; }

	public int RemoveEquipItemSlotIndex { get; set; }
}
