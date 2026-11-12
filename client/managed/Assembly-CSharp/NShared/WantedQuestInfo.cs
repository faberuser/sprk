namespace NShared;

[JsonSerializable(Javascript = true)]
public class WantedQuestInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => Index;

	public int Index { get; set; }

	public sbyte Cleared { get; set; }

	public string EndTime { get; set; }
}
