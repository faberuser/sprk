namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerBookMarkHeroInfo : IAccountAttrKey<long>
{
	long IAccountAttrKey<long>.Key => AccountId;

	public long AccountId { get; set; }

	public string BookMarkHeroIndices { get; set; }
}
