using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class PetEggData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int ReqTime { get; set; }

	[Key(2)]
	public string ItemGroupCode { get; set; }

	int IDataKeySelector<int>.Key => Index;
}
