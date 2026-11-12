namespace NShared;

[JsonSerializable(Javascript = true)]
public class MonthlyHeroInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => MonthlyHeroIndex;

	public int MonthlyHeroIndex { get; set; }

	public string CompletedTime { get; set; }
}
