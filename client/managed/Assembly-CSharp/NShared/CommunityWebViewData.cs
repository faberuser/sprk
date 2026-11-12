using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CommunityWebViewData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Language { get; set; }

	[Key(2)]
	public string IconName { get; set; }

	[Key(3)]
	public string Url { get; set; }

	[Key(4)]
	public bool IsDefault { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
