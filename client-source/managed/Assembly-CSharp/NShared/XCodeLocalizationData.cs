using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class XCodeLocalizationData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Country { get; set; }

	[Key(2)]
	public string CountryCode { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
