using NShared.NJit;

namespace NShared;

public class CountryCodeDataContainer : JitDataContainerDefault<string, CountryCodeData>
{
	public const string TABLENAME = "CountryCodeTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CountryCodeTable");

	public CountryCodeData GetDataByCode(string countryCode)
	{
		string text = countryCode;
		if (text == null || text.Length == 0)
		{
			text = "VESPA";
		}
		CountryCodeData data = GetData(text);
		if (data == null)
		{
			data = GetData("VESPA");
		}
		return data;
	}
}
