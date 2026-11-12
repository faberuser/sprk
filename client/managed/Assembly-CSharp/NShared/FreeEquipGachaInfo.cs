namespace NShared;

[JsonSerializable(Javascript = true)]
public class FreeEquipGachaInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => GachaIndex;

	public int GachaIndex { get; set; }

	public string LastGachaTime { get; set; }

	public string LastGachaResetTime { get; set; }

	public byte RemainDayGachaChance { get; set; }

	[NonDbSchema]
	public int ResetRemainTime { get; set; }
}
