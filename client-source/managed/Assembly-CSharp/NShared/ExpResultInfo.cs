namespace NShared;

[JsonSerializable(Javascript = true)]
public class ExpResultInfo
{
	public int AddValue { get; set; }

	public int NewValue { get; set; }

	public int OldLevel { get; set; }

	public int NewLevel { get; set; }

	public int OldValue => NewValue - AddValue;
}
