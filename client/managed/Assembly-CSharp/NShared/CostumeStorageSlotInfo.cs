using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class CostumeStorageSlotInfo : IAccountAttrKey<CostumeStorageSlotInfoKey>
{
	CostumeStorageSlotInfoKey IAccountAttrKey<CostumeStorageSlotInfoKey>.Key => new CostumeStorageSlotInfoKey(HeroIndex, CostumeStorageSlotIndex);

	public int HeroIndex { get; set; }

	public int CostumeStorageSlotIndex { get; set; }

	public string Name { get; set; }

	public int CostumeIndex { get; set; }

	public byte HideUniqueWeapon { get; set; }

	public int WeaponCostumeIndex { get; set; }

	public int HairCostumeIndex { get; set; }

	public int AccessoryCostumeIndex1 { get; set; }

	public string PositionInfo1 { get; set; }

	public int AccessoryCostumeIndex2 { get; set; }

	public string PositionInfo2 { get; set; }

	public int AccessoryCostumeIndex3 { get; set; }

	public string PositionInfo3 { get; set; }

	public int AccessoryCostumeIndex4 { get; set; }

	public string PositionInfo4 { get; set; }

	public int AccessoryCostumeIndex5 { get; set; }

	public string PositionInfo5 { get; set; }

	public int AccessoryCostumeIndex6 { get; set; }

	public string PositionInfo6 { get; set; }

	public void SetCostumeNameList(List<string> costumeNameList)
	{
		if (CostumeIndex != 0)
		{
			CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(CostumeIndex);
			if (data != null)
			{
				costumeNameList.Add(data.CostumeName);
			}
		}
		if (HairCostumeIndex != 0)
		{
			HairCostumeData data2 = GameRoot.Instance.HairCostumeDataContainer.GetData(HairCostumeIndex);
			if (data2 != null)
			{
				costumeNameList.Add(data2.Name);
			}
		}
		if (WeaponCostumeIndex != 0)
		{
			WeaponCostumeData data3 = GameRoot.Instance.WeaponCostumeDataContainer.GetData(WeaponCostumeIndex);
			if (data3 != null)
			{
				costumeNameList.Add(data3.Name);
			}
		}
		if (AccessoryCostumeIndex1 != 0)
		{
			AccessoryCostumeData data4 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(AccessoryCostumeIndex1);
			if (data4 != null)
			{
				costumeNameList.Add(data4.Name);
			}
		}
		if (AccessoryCostumeIndex2 != 0)
		{
			AccessoryCostumeData data5 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(AccessoryCostumeIndex2);
			if (data5 != null)
			{
				costumeNameList.Add(data5.Name);
			}
		}
		if (AccessoryCostumeIndex3 != 0)
		{
			AccessoryCostumeData data6 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(AccessoryCostumeIndex3);
			if (data6 != null)
			{
				costumeNameList.Add(data6.Name);
			}
		}
		if (AccessoryCostumeIndex4 != 0)
		{
			AccessoryCostumeData data7 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(AccessoryCostumeIndex4);
			if (data7 != null)
			{
				costumeNameList.Add(data7.Name);
			}
		}
		if (AccessoryCostumeIndex5 != 0)
		{
			AccessoryCostumeData data8 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(AccessoryCostumeIndex5);
			if (data8 != null)
			{
				costumeNameList.Add(data8.Name);
			}
		}
		if (AccessoryCostumeIndex6 != 0)
		{
			AccessoryCostumeData data9 = GameRoot.Instance.AccessoryCostumeDataContainer.GetData(AccessoryCostumeIndex6);
			if (data9 != null)
			{
				costumeNameList.Add(data9.Name);
			}
		}
	}
}
