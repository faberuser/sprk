namespace NShared;

[JsonSerializable(Javascript = true)]
public class ClearPartyHeroInfo : HeroInfo
{
	public PunishmentRuneOptionInfo[] PunishmentRuneOptionInfos { get; set; }
}
