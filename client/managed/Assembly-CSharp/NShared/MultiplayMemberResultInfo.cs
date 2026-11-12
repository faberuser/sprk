namespace NShared;

[JsonSerializable(Javascript = true)]
public class MultiplayMemberResultInfo
{
	public long AccountId { get; set; }

	public string Nick { get; set; }

	public int MemberIndex { get; set; }

	public bool UseStamina { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public ItemResultInfo[] ItemResults2 { get; set; }

	public EquipItemInfo[] EquipItemInfos2 { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos2 { get; set; }

	public HeroExpResultInfo[] HeroExpResults { get; set; }

	public FlaskEndCampaignResultInfo[] FlaskResults { get; set; }

	public ItemResultInfo[] FlaskItemResults { get; set; }
}
