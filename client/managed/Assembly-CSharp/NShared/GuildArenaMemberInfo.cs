namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaMemberInfo
{
	public long AccountId { get; set; }

	public int SessionIndex { get; set; }

	public string BattledHeroIndices { get; set; }
}
