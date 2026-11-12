using NVespa;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipStorageSlotInfo : IAccountAttrKey<int>
{
	public static readonly EquipPartType[] SaveEquipParts = new EquipPartType[5]
	{
		EquipPartType.Armor,
		EquipPartType.SecondGear,
		EquipPartType.Accessory,
		EquipPartType.Orb,
		EquipPartType.Artifact
	};

	int IAccountAttrKey<int>.Key => EquipStorageSlotIndex;

	public virtual int EquipStorageSlotIndex { get; set; }

	public virtual string Name { get; set; }

	public virtual int EquipItemSlotIndex1 { get; set; }

	public virtual int EquipItemSlotIndex2 { get; set; }

	public virtual int EquipItemSlotIndex3 { get; set; }

	public virtual int EquipItemSlotIndex4 { get; set; }

	public virtual int EquipItemSlotIndex5 { get; set; }

	public virtual int EquipItemSlotIndex6 { get; set; }

	public virtual int EquipItemSlotIndex7 { get; set; }

	public virtual int EquipItemSlotIndex8 { get; set; }

	public virtual int EquipItemSlotIndex9 { get; set; }

	public virtual int EquipItemSlotIndex10 { get; set; }

	public string LinkInfo { get; set; }

	public int GetEquipItemSlotIndex(int partIndex)
	{
		switch (partIndex)
		{
		case 1:
			return EquipItemSlotIndex1;
		case 2:
			return EquipItemSlotIndex2;
		case 3:
			return EquipItemSlotIndex3;
		case 4:
			return EquipItemSlotIndex4;
		case 5:
			return EquipItemSlotIndex5;
		case 6:
			return EquipItemSlotIndex6;
		case 7:
			return EquipItemSlotIndex7;
		case 8:
			return EquipItemSlotIndex8;
		case 9:
			return EquipItemSlotIndex9;
		case 10:
			return EquipItemSlotIndex10;
		default:
			VespaDebug.LogError("GetEquipItemSlotIndex failed : partIndex=" + partIndex);
			return 0;
		}
	}

	public bool IsEmptySlot()
	{
		for (int i = 1; i <= 10; i++)
		{
			if (GetEquipItemSlotIndex(i) != 0)
			{
				return false;
			}
		}
		return true;
	}
}
