using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CountryServerData : IDataKeySelector<string>
{
	[Key(0)]
	public string ServerName { get; set; }

	[Key(1)]
	public string[] Law { get; set; }

	[Key(2)]
	public bool IsPaidRubyPrint { get; set; }

	[Key(3)]
	public bool IsAdultCheck { get; set; }

	string IDataKeySelector<string>.Key => ServerName;
}
