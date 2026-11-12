namespace NShared;

[JsonSerializable(Javascript = true)]
public class TowerInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => TowerIndex;

	public int TowerIndex { get; set; }

	public int CompletedFloor { get; set; }

	public string CompletedTime { get; set; }

	public int ResetCount { get; set; }

	public string ResetTime { get; set; }

	public string ReceiveRewardTime { get; set; }
}
