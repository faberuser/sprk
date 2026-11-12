using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CSMailData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string CSMailTitle { get; set; }

	[Key(2)]
	public string CSMailDesc { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
