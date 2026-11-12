namespace NShared;

[JsonSerializable(Javascript = true)]
public class LoginDailyInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => LoginDailyIndex;

	public int LoginDailyIndex { get; set; }

	public int CompletedCount { get; set; }

	public string CompletedTime { get; set; }
}
