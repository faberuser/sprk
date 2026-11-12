namespace NShared;

[JsonSerializable]
public enum CurrencyChangeType
{
	ReplacePrice,
	ReplaceCurrencyCode,
	AddCurrencyUnitByCode,
	RemoveSymbol
}
