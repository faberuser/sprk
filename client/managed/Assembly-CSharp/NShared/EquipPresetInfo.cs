using NVespa;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipPresetInfo : IAccountAttrKey<int>
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

	public int[] EquipItemSlotIndex { get; set; }

	public string[] EquipItemUid { get; set; }

	public int GetEquipItemSlotIndex(int partIndex)
	{
		if (EquipItemSlotIndex == null)
		{
			return 0;
		}
		switch (partIndex)
		{
		case 1:
			if (EquipItemSlotIndex.Length < 1)
			{
				return 0;
			}
			return EquipItemSlotIndex[0];
		case 2:
			if (EquipItemSlotIndex.Length < 2)
			{
				return 0;
			}
			return EquipItemSlotIndex[1];
		case 3:
			if (EquipItemSlotIndex.Length < 3)
			{
				return 0;
			}
			return EquipItemSlotIndex[2];
		case 4:
			if (EquipItemSlotIndex.Length < 4)
			{
				return 0;
			}
			return EquipItemSlotIndex[3];
		case 5:
			if (EquipItemSlotIndex.Length < 5)
			{
				return 0;
			}
			return EquipItemSlotIndex[4];
		case 6:
			if (EquipItemSlotIndex.Length < 6)
			{
				return 0;
			}
			return EquipItemSlotIndex[5];
		case 7:
			if (EquipItemSlotIndex.Length < 7)
			{
				return 0;
			}
			return EquipItemSlotIndex[6];
		case 8:
			if (EquipItemSlotIndex.Length < 8)
			{
				return 0;
			}
			return EquipItemSlotIndex[7];
		case 9:
			if (EquipItemSlotIndex.Length < 9)
			{
				return 0;
			}
			return EquipItemSlotIndex[8];
		case 10:
			if (EquipItemSlotIndex.Length < 10)
			{
				return 0;
			}
			return EquipItemSlotIndex[9];
		default:
			VespaDebug.LogError("GetEquipItemSlotIndex failed : partIndex=" + partIndex);
			return 0;
		}
	}

	public string GetEquipItemUid(int partIndex)
	{
		if (EquipItemUid == null)
		{
			return string.Empty;
		}
		switch (partIndex)
		{
		case 1:
			if (EquipItemUid.Length < 1)
			{
				return "";
			}
			return EquipItemUid[0];
		case 2:
			if (EquipItemUid.Length < 2)
			{
				return "";
			}
			return EquipItemUid[1];
		case 3:
			if (EquipItemUid.Length < 3)
			{
				return "";
			}
			return EquipItemUid[2];
		case 4:
			if (EquipItemUid.Length < 4)
			{
				return "";
			}
			return EquipItemUid[3];
		case 5:
			if (EquipItemUid.Length < 5)
			{
				return "";
			}
			return EquipItemUid[4];
		case 6:
			if (EquipItemUid.Length < 6)
			{
				return "";
			}
			return EquipItemUid[5];
		case 7:
			if (EquipItemUid.Length < 7)
			{
				return "";
			}
			return EquipItemUid[6];
		case 8:
			if (EquipItemUid.Length < 8)
			{
				return "";
			}
			return EquipItemUid[7];
		case 9:
			if (EquipItemUid.Length < 9)
			{
				return "";
			}
			return EquipItemUid[8];
		case 10:
			if (EquipItemUid.Length < 10)
			{
				return "";
			}
			return EquipItemUid[9];
		default:
			VespaDebug.LogError("GetEquipItemUid failed : partIndex=" + partIndex);
			return "";
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
