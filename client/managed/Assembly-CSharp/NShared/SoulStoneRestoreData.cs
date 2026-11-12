using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulStoneRestoreData : IDataKeySelector<string>
{
	[Key(0)]
	public string MaterialItemCode { get; set; }

	[Key(1)]
	public int MaterialItemCount { get; set; }

	[Key(2)]
	public string SoulStoneItemGroup { get; set; }

	string IDataKeySelector<string>.Key => MaterialItemCode;
}
