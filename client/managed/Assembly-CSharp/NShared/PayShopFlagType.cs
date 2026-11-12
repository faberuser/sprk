namespace NShared;

[JsonSerializable(Javascript = true)]
public enum PayShopFlagType
{
	None = 0,
	Free = 2,
	Sale = 4,
	Hot = 8,
	New = 16,
	NewNSale = 20
}
