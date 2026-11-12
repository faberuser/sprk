namespace NShared.VisitDungeon;

[Response]
public class Response : GenericResponse<ResultType>
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public HeroInfo[] HeroInfos { get; set; }

	public ExpResultInfo[] ExpResultInfos { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }

	public ChapterDungeonInfo DungeonInfo { get; set; }
}
