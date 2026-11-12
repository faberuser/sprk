namespace NShared.RewardClearChapter;

[Response]
public class Response : GenericResponse<ResultType>
{
	public ChapterRewardInfo RewardInfo { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public HeroAddResultInfo HeroAddResult { get; set; }

	public ItemResultInfo ItemResult { get; set; }

	public EquipItemInfo EquipItemResult { get; set; }
}
