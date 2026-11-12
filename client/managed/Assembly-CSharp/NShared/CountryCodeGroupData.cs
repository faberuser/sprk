using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CountryCodeGroupData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string LocalTextPath { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
