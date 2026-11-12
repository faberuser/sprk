namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildSuppressPartyInfo
{
	public long AccountId { get; set; }

	public long HeroCount { get; set; }

	public int[] HeroIndices { get; set; }
}
