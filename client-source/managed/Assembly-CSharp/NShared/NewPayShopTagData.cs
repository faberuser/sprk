using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class NewPayShopTagData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string PayShopCategoryType { get; set; }

	[Key(2)]
	public string Name { get; set; }

	[Key(3)]
	public PayShopIndexType IndexType { get; set; }

	[Key(4)]
	public int[] Indices { get; set; }

	[Key(5)]
	public int Sort { get; set; }

	[Key(6)]
	public bool IsEnable { get; set; }

	public override string ToString()
	{
		return "[" + Index + "]";
	}
}
