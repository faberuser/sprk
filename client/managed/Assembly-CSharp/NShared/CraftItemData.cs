using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CraftItemData : IItemSubData, IDataKeySelector<int>
{
	public class MaterialItemInfo
	{
		public string ItemCode { get; set; }

		public int ItemCount { get; set; }
	}

	int IDataKeySelector<int>.Key => CraftIndex;

	[Key(0)]
	public int CraftIndex { get; set; }

	[Key(1)]
	public string ItemCode { get; set; }

	[Key(2)]
	public int ResultItemCount { get; set; }

	[Key(3)]
	public int ReqGold { get; set; }

	[Key(4)]
	public int ReqTime { get; set; }

	[Key(5)]
	public bool Countable { get; set; }

	[Key(6)]
	public bool IsOpen { get; set; }

	[Key(7)]
	public bool IsRefund { get; set; }

	[Key(8)]
	public ItemType ItemType { get; set; }

	[Key(9)]
	public int ShowIndex { get; set; }

	[Key(10)]
	public string MaterialItemCode1 { get; set; }

	[Key(11)]
	public int MaterialItemCount1 { get; set; }

	[Key(12)]
	public string MaterialItemCode2 { get; set; }

	[Key(13)]
	public int MaterialItemCount2 { get; set; }

	[Key(14)]
	public string MaterialItemCode3 { get; set; }

	[Key(15)]
	public int MaterialItemCount3 { get; set; }

	[Key(16)]
	public string MaterialItemCode4 { get; set; }

	[Key(17)]
	public int MaterialItemCount4 { get; set; }

	[Key(18)]
	public string MaterialItemCode5 { get; set; }

	[Key(19)]
	public int MaterialItemCount5 { get; set; }

	[Key(20)]
	public string MaterialItemCode6 { get; set; }

	[Key(21)]
	public int MaterialItemCount6 { get; set; }

	[Key(22)]
	public string MaterialItemCode7 { get; set; }

	[Key(23)]
	public int MaterialItemCount7 { get; set; }

	[Key(24)]
	public string MaterialItemCode8 { get; set; }

	[Key(25)]
	public int MaterialItemCount8 { get; set; }

	public override string ToString()
	{
		return "CraftIndex =" + CraftIndex;
	}

	public List<MaterialItemInfo> GetMaterialItemList()
	{
		List<MaterialItemInfo> list = new List<MaterialItemInfo>();
		if (MaterialItemCode1 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode1,
				ItemCount = MaterialItemCount1
			});
		}
		if (MaterialItemCode2 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode2,
				ItemCount = MaterialItemCount2
			});
		}
		if (MaterialItemCode3 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode3,
				ItemCount = MaterialItemCount3
			});
		}
		if (MaterialItemCode4 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode4,
				ItemCount = MaterialItemCount4
			});
		}
		if (MaterialItemCode5 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode5,
				ItemCount = MaterialItemCount5
			});
		}
		if (MaterialItemCode6 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode6,
				ItemCount = MaterialItemCount6
			});
		}
		if (MaterialItemCode7 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode7,
				ItemCount = MaterialItemCount7
			});
		}
		if (MaterialItemCode8 != null)
		{
			list.Add(new MaterialItemInfo
			{
				ItemCode = MaterialItemCode8,
				ItemCount = MaterialItemCount8
			});
		}
		return list;
	}
}
