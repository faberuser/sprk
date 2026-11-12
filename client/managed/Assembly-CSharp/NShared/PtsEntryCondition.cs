namespace NShared;

[JsonSerializable(Javascript = true)]
public class PtsEntryCondition
{
	public string CCULimit { get; set; }

	public string TeamLevelLocalization { get; set; }

	public int TeamLevelValue { get; set; }
}
