namespace NShared;

public interface ICurrencyResultInfo
{
	CurrencyType CurrencyType { get; }

	long AddValue { get; }

	long NewValue { get; }
}
