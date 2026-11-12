namespace NShared;

[JsonSerializable(Javascript = true)]
public class RouletteInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => RouletteInfoIndex;

	public int RouletteInfoIndex { get; set; }

	public int RemainCount { get; set; }

	public int MaxCount { get; set; }
}
