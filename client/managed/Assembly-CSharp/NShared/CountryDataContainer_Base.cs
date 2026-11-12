using NShared.NJit;

namespace NShared;

public abstract class CountryDataContainer_Base : JitDataContainerDefault<string, CountryData>
{
	public string[] GetDefaultAgreement()
	{
		foreach (CountryData allDatum in GetAllData())
		{
			if (allDatum.AgreementCondition.Contains("Default"))
			{
				return allDatum.Agreement;
			}
		}
		return new string[2] { "US_PrivacyPolicy", "US_TermOfService" };
	}
}
