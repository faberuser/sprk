namespace NShared;

[JsonSerializable(Javascript = true)]
public class SubQuestInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SubQuestIndex;

	public int SubQuestIndex { get; set; }

	public int LastStep { get; set; }

	public long Progress { get; set; }
}
