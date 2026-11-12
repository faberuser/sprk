namespace NShared;

[JsonSerializable(Javascript = true)]
public class KingsPassSeaonInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SeasonIndex;

	public int SeasonIndex { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public bool ShowYN { get; set; }

	public int CurrentLevel { get; set; }

	public int LastLevel { get; set; }
}
