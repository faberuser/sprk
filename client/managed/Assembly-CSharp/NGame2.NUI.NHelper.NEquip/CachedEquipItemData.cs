using NShared;

namespace NGame2.NUI.NHelper.NEquip;

public class CachedEquipItemData
{
	public EquipItemEquipType EquipType { get; private set; }

	public EquipItemEquipGrade EquipGrade { get; private set; }

	public EquipPartType PartType { get; private set; }

	public EquipItemDetailType EquipItemDetailType { get; private set; }

	public bool IsUnique { get; private set; }

	public void Init(int itemIndex)
	{
		ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex);
		if (data != null && data.SubData != null)
		{
			EquipItemData subData = data.SubData;
			EquipItemDetailData data2 = GameRoot.Instance.EquipItemDetailDataContainer.GetData(subData.DetailIndex);
			if (data2 != null)
			{
				EquipGrade = subData.EquipGrade;
				EquipType = subData.EquipType;
				PartType = subData.PartType;
				EquipItemDetailType = data2.EquipItemDetailType;
				IsUnique = data2.IsUnique;
			}
		}
	}
}
