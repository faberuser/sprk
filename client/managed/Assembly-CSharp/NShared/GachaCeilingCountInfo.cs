namespace NShared;

[JsonSerializable(Javascript = true)]
public class GachaCeilingCountInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => GachaIndex;

	public int GachaIndex { get; set; }

	public int GachaCount { get; set; }
}
