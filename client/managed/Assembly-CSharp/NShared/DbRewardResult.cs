namespace NShared;

[JsonSerializable(Javascript = true)]
public class DbRewardResult
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public HeroInfo[] HeroInfos { get; set; }

	public ExpResultInfo[] ExpResultInfos { get; set; }

	public StaminaResultInfo[] StaminaResultInfos { get; set; }

	public PetInfo[] PetInfos { get; set; }
}
