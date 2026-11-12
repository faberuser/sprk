namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipGachaInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => GachaIndex;

	public int GachaIndex { get; set; }

	public string BeginTime { get; set; }

	public string EndTime { get; set; }

	public bool OnSale { get; set; }

	public int Discount { get; set; }

	public int Durational { get; set; }

	public string SaleResetTime { get; set; }

	public string ProgressResetTime { get; set; }

	public int IsUTCBeginTime { get; set; }

	public int IsUTCEndTime { get; set; }

	public int IsUTCProgressResetTime { get; set; }

	public string LastDiscountedTime { get; set; }

	public int RemainNumberOfDiscount { get; set; }
}
