using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CountryCodeData : IDataKeySelector<string>
{
	[Key(0)]
	public string Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public string CountryName { get; set; }

	[Key(3)]
	public string IconPath { get; set; }

	string IDataKeySelector<string>.Key => Index;
}
