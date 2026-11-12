namespace NShared.CompleteTutorial;

[Response]
public class Response : GenericResponse<ResultType>
{
	public TutorialInfo Info { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public HeroInfo[] HeroInfos { get; set; }

	public ExpResultInfo[] TeamExpResultInfos { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }

	public ChapterDungeonInfo[] DungeonInfos { get; set; }

	public FreeEquipGachaInfo FreeEquipGachaInfo { get; set; }

	public EquipGachaInfo EquipGachaInfo { get; set; }

	public MissionCategoryInfo[] OpendMissionCategories { get; set; }
}
