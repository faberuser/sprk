namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class MemberResultsRes
{
	public bool UseStamina { get; set; }

	public HeroExpResultInfo[] HeroExpResults { get; set; }

	public FlaskEndCampaignResultInfo[] FlaskResults { get; set; }

	public ItemResultInfo[] FlaskItemResults { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos { get; set; }

	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public ItemResultInfo[] ItemResults2 { get; set; }

	public EquipItemInfo[] EquipItemInfos2 { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos2 { get; set; }
}
