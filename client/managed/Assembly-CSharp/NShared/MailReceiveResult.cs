namespace NShared;

[JsonSerializable(Javascript = true)]
public class MailReceiveResult
{
	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public StaminaResultInfo[] StaminaResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemResults { get; set; }

	public HeroAddResultInfo HeroAddResult { get; set; }

	public EventGiftPointResultInfo EventGiftPointResult { get; set; }

	public GuildPointResultInfo GuildPointResult { get; set; }

	public PetInfo PetResult { get; set; }

	public PlayerAnyMisc[] PlayerAnyMiscs { get; set; }
}
