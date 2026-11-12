using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CountryData : IDataKeySelector<string>
{
	[Key(0)]
	public string Code { get; set; }

	[Key(1)]
	public string Name { get; set; }

	[Key(2)]
	public string[] Agreement { get; set; }

	[Key(3)]
	public string AgreementCondition { get; set; }

	[Key(4)]
	public int Version { get; set; }

	[Key(5)]
	public string PrivacyURL { get; set; }

	[Key(6)]
	public string TeamOfServiceURL { get; set; }

	string IDataKeySelector<string>.Key => Code;
}
