namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerItemUseInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ItemIndex;

	public int ItemIndex { get; set; }

	public int OutItemIndex { get; set; }

	public int UseCount { get; set; }

	public string CompletedTime { get; set; }
}
