using System.Collections.Generic;
using System.Linq;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class SoulBreakData : IDataKeySelector<SoulBreakKey>
{
	[MessagePackObject(false)]
	public class ItemCodeMinMaxData
	{
		[Key(0)]
		public string ItemCode { get; set; }

		[Key(1)]
		public int MinItemCount { get; set; }

		[Key(2)]
		public int MaxItemCount { get; set; }
	}

	SoulBreakKey IDataKeySelector<SoulBreakKey>.Key => new SoulBreakKey(TagType, Grade, ReinforceLevel);

	[Key(0)]
	public CreatureTagType TagType { get; set; }

	[Key(1)]
	public int Grade { get; set; }

	[Key(2)]
	public int ReinforceLevel { get; set; }

	[Key(3)]
	public List<ItemCodeMinMaxData> ItemMinMaxList { get; set; }

	public List<string> GetItemCodeList()
	{
		return ItemMinMaxList.Select((ItemCodeMinMaxData v) => v.ItemCode).ToList();
	}

	public List<Tuple<int, int>> GetMinMaxItemCountList()
	{
		return ItemMinMaxList.Select((ItemCodeMinMaxData v) => Tuple.Create(v.MinItemCount, v.MaxItemCount)).ToList();
	}
}
