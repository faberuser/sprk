using System.Collections.Generic;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class DismantleConfirmPopup : OptionSellConfirmPopup
{
	public new const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/DismantleConfirmPopup";

	public UIScrollGrid ResultGrid;

	private List<DismantleResultComponent> ResultComponents = new List<DismantleResultComponent>();

	public override void Reset()
	{
		base.Reset();
		ClearGrid();
	}

	protected override void UpdateList(List<EquipItemInfo> equipItemInfos)
	{
		ClearGrid();
		SortedDictionary<int, EquipItemBreakData.BreakItemInfo> sortedDictionary = new SortedDictionary<int, EquipItemBreakData.BreakItemInfo>();
		foreach (EquipItemInfo equipItemInfo in equipItemInfos)
		{
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(equipItemInfo.ItemIndex);
			EquipItemData subData = data.GetSubData<EquipItemData>();
			EquipItemBreakData data2 = GameRoot.Instance.EquipItemBreakDataContainer.GetData(subData.Tier, subData.DetailIndex, data.Code, equipItemInfo.Star, subData.SetIndex);
			if (data2 == null)
			{
				continue;
			}
			foreach (EquipItemBreakData.BreakItemInfo item in data2.GetAllBreakItemInfo())
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode != null)
				{
					if (!sortedDictionary.ContainsKey(dataByCode.Index))
					{
						EquipItemBreakData.BreakItemInfo value = new EquipItemBreakData.BreakItemInfo
						{
							ItemCode = item.ItemCode,
							MaxItemCount = item.MaxItemCount,
							MinItemCount = item.MinItemCount
						};
						sortedDictionary.Add(dataByCode.Index, value);
					}
					else
					{
						EquipItemBreakData.BreakItemInfo breakItemInfo = sortedDictionary[dataByCode.Index];
						breakItemInfo.MinItemCount += item.MinItemCount;
						breakItemInfo.MaxItemCount += item.MaxItemCount;
					}
				}
			}
		}
		foreach (KeyValuePair<int, EquipItemBreakData.BreakItemInfo> item2 in sortedDictionary)
		{
			int key = item2.Key;
			EquipItemBreakData.BreakItemInfo value2 = item2.Value;
			ItemData data3 = GameRoot.Instance.ItemDataContainer.GetData(key);
			if (data3 != null)
			{
				int num = value2.MinItemCount;
				int num2 = value2.MaxItemCount;
				DropBonusEvent equipBreakEventBonus = GlobalAutoBehaviour<DropBonusEventManager>.instance.GetEquipBreakEventBonus(data3.Code);
				if (equipBreakEventBonus != null)
				{
					num = num * equipBreakEventBonus.BonusPercentage / 100;
					num2 = num2 * equipBreakEventBonus.BonusPercentage / 100;
				}
				DismantleResultComponent dismantleResultComponent = UIHelper.AddGridItem(ResultGrid, DismantleResultComponent.PrefabAuto);
				dismantleResultComponent.Open(data3, num, num2);
				dismantleResultComponent.ShowEventBonus(equipBreakEventBonus != null);
				ResultComponents.Add(dismantleResultComponent);
			}
		}
		UIHelper.RepositionGrid(ResultGrid);
	}

	private void ClearGrid()
	{
		FactoryBase.ReleaseAndClearList(ResultComponents);
	}
}
