namespace NShared;

[JsonSerializable(Javascript = true)]
public class StarterPickupEquipGachaInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => GachaIndex;

	public int GachaIndex { get; set; }

	public int GachaCount { get; set; }

	public bool IsTakeLastReward { get; set; }
}
