using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipUpgradeTierData : IDataKeySelector<string>
{
	public class Material
	{
		public string ItemCode { get; set; }

		public int ItemCount { get; set; }
	}

	private List<Material> _materialList;

	string IDataKeySelector<string>.Key => ItemCode;

	[Key(0)]
	public string ItemCode { get; set; }

	[Key(1)]
	public string ResultItemCode { get; set; }

	[Key(2)]
	public string MaterialItemCode1 { get; set; }

	[Key(3)]
	public int MaterialItemCount1 { get; set; }

	[Key(4)]
	public string MaterialItemCode2 { get; set; }

	[Key(5)]
	public int MaterialItemCount2 { get; set; }

	[Key(6)]
	public string MaterialItemCode3 { get; set; }

	[Key(7)]
	public int MaterialItemCount3 { get; set; }

	[Key(8)]
	public string MaterialItemCode4 { get; set; }

	[Key(9)]
	public int MaterialItemCount4 { get; set; }

	[Key(10)]
	public string MaterialItemCode5 { get; set; }

	[Key(11)]
	public int MaterialItemCount5 { get; set; }

	[Key(12)]
	public ShopCostType CostType { get; set; }

	[Key(13)]
	public int CostValue { get; set; }

	public override string ToString()
	{
		return "ItemCode=" + ItemCode + " ResultItemCode = " + ResultItemCode;
	}

	public List<Material> GetMaterialList()
	{
		if (_materialList == null)
		{
			_materialList = new List<Material>();
			_materialList.Add(new Material
			{
				ItemCode = MaterialItemCode1,
				ItemCount = MaterialItemCount1
			});
			_materialList.Add(new Material
			{
				ItemCode = MaterialItemCode2,
				ItemCount = MaterialItemCount2
			});
			_materialList.Add(new Material
			{
				ItemCode = MaterialItemCode3,
				ItemCount = MaterialItemCount3
			});
			_materialList.Add(new Material
			{
				ItemCode = MaterialItemCode4,
				ItemCount = MaterialItemCount4
			});
			_materialList.Add(new Material
			{
				ItemCode = MaterialItemCode5,
				ItemCount = MaterialItemCount5
			});
		}
		return _materialList;
	}
}
