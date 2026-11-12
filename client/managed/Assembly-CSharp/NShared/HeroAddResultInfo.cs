namespace NShared;

[JsonSerializable(Javascript = true)]
public class HeroAddResultInfo
{
	public HeroInfo HeroInfo { get; set; }

	public ExpResultInfo TeamExpResult { get; set; }

	public StaminaResultInfo StaminaResult { get; set; }

	public PlayerHeroFriendlyInfo HeroFriendlyInfo { get; set; }
}
