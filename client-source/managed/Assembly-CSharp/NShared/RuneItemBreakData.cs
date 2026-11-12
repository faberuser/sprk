using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class RuneItemBreakData : IDataKeySelector<int>
{
	public class BreakItemInfo
	{
		public string ItemCode { get; set; }

		public int MinItemCount { get; set; }

		public int MaxItemCount { get; set; }
	}

	[Key(0)]
	public int Grade { get; set; }

	[Key(1)]
	public string ItemCode1 { get; set; }

	[Key(2)]
	public int MinItemCount1 { get; set; }

	[Key(3)]
	public int MaxItemCount1 { get; set; }

	[Key(4)]
	public string ItemCode2 { get; set; }

	[Key(5)]
	public int MinItemCount2 { get; set; }

	[Key(6)]
	public int MaxItemCount2 { get; set; }

	[Key(7)]
	public string ItemCode3 { get; set; }

	[Key(8)]
	public int MinItemCount3 { get; set; }

	[Key(9)]
	public int MaxItemCount3 { get; set; }

	[Key(10)]
	public string ItemCode4 { get; set; }

	[Key(11)]
	public int MinItemCount4 { get; set; }

	[Key(12)]
	public int MaxItemCount4 { get; set; }

	int IDataKeySelector<int>.Key => Grade;

	public override string ToString()
	{
		return "Grade=" + Grade;
	}

	public List<BreakItemInfo> GetAllBreakItemInfo()
	{
		List<BreakItemInfo> list = new List<BreakItemInfo>();
		if (!string.IsNullOrEmpty(ItemCode1))
		{
			list.Add(new BreakItemInfo
			{
				ItemCode = ItemCode1,
				MinItemCount = MinItemCount1,
				MaxItemCount = MaxItemCount1
			});
		}
		if (!string.IsNullOrEmpty(ItemCode2))
		{
			list.Add(new BreakItemInfo
			{
				ItemCode = ItemCode2,
				MinItemCount = MinItemCount2,
				MaxItemCount = MaxItemCount2
			});
		}
		if (!string.IsNullOrEmpty(ItemCode3))
		{
			list.Add(new BreakItemInfo
			{
				ItemCode = ItemCode3,
				MinItemCount = MinItemCount3,
				MaxItemCount = MaxItemCount3
			});
		}
		if (!string.IsNullOrEmpty(ItemCode4))
		{
			list.Add(new BreakItemInfo
			{
				ItemCode = ItemCode4,
				MinItemCount = MinItemCount4,
				MaxItemCount = MaxItemCount4
			});
		}
		return list;
	}
}
