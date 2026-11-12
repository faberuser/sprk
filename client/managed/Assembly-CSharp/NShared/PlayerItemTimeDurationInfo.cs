namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerItemTimeDurationInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ItemIndex;

	public int ItemIndex { get; set; }

	public string StartTime { get; set; }

	public string EndTime { get; set; }
}
