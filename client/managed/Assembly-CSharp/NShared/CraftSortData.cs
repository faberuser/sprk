using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class CraftSortData : IDataKeySelector<CraftSortKey>
{
	[Key(0)]
	public CraftSortCategory Category { get; set; }

	[Key(1)]
	public int Value { get; set; }

	[Key(2)]
	public int BackGroundIndex { get; set; }

	[Key(3)]
	public string StringKey { get; set; }

	CraftSortKey IDataKeySelector<CraftSortKey>.Key => new CraftSortKey(Category, Value);

	public override string ToString()
	{
		return "CraftSortData = " + Category.ToString() + "," + Value;
	}
}
