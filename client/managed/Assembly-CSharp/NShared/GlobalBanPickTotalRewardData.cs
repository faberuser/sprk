using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class GlobalBanPickTotalRewardData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string ServerBuffCode { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
