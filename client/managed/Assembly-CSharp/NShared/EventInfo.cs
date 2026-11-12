namespace NShared;

[JsonSerializable(Javascript = true)]
public class EventInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => EventIndex;

	public int EventIndex { get; set; }

	public int Value { get; set; }
}
