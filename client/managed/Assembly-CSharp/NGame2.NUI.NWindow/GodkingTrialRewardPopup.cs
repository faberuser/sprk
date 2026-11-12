using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NHelper.NItem;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NWindow;

public class GodkingTrialRewardPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/GodkingTrial/GodkingTrialRewardPopup";

	public UILabel Name;

	public UILabel Desc;

	public UIScrollGrid Grid_RewardItem;

	private List<int> _rewardIndices = new List<int>();

	private List<ItemIconComponent> _itemIconComponentList = new List<ItemIconComponent>();

	public override void Open()
	{
		base.Open();
		Init();
	}

	public override void Reset()
	{
		ClearItemIconList();
	}

	private void Init()
	{
		InitRewardItem();
	}

	private void InitPopupText()
	{
		string stringDirect = LocalizationManager.GetStringDirect("");
		UIHelper.SetLabelText(Name, stringDirect);
		string stringDirect2 = LocalizationManager.GetStringDirect("");
		UIHelper.SetLabelText(Desc, stringDirect2);
	}

	private void InitRewardItem()
	{
		ClearItemIconList();
		IList<GodkingTrialGroupData> allData = GameRoot.Instance.GodkingTrialGroupDataContainer.GetAllData();
		List<ItemData> list = new List<ItemData>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
		{
			GodkingTrialGroupData godkingTrialGroupData = allData[i];
			if (godkingTrialGroupData == null)
			{
				continue;
			}
			_rewardIndices.Add(godkingTrialGroupData.RewardIndex);
			int maxDifficulty = GameRoot.Instance.GodkingTrialDataContainer.GetMaxDifficulty(godkingTrialGroupData.ChapterIndex);
			SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(godkingTrialGroupData.ChapterIndex, maxDifficulty);
			if (data != null)
			{
				List<SelectRewardData.SelectRewardItemInfo> selectRewardItemList = data.GetSelectRewardItemList();
				for (int j = 0; j < __Extension_Method_ICollection__.COUNT(selectRewardItemList); j++)
				{
					SelectRewardData.SelectRewardItemInfo selectRewardItemInfo = selectRewardItemList[j];
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(selectRewardItemInfo.Code);
					list.Add(dataByCode);
				}
			}
		}
		for (int k = 0; k < __Extension_Method_ICollection__.COUNT(_rewardIndices); k++)
		{
			List<RewardElementData> rewardItemList = GameRoot.Instance.RewardDataContainer.GetData(_rewardIndices[k]).GetRewardItemList();
			if (rewardItemList == null)
			{
				continue;
			}
			foreach (RewardElementData item in rewardItemList)
			{
				ItemData dataByCode2 = GameRoot.Instance.ItemDataContainer.GetDataByCode(item.ItemCode);
				if (dataByCode2 != null && !list.Contains(dataByCode2))
				{
					list.Add(dataByCode2);
				}
			}
		}
		list.Sort((ItemData l, ItemData r) => SortItem(l, r));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(list); num++)
		{
			ItemData itemData = list[num];
			if (itemData != null)
			{
				ItemIconComponent itemIconComponent = UIHelper.AddGridItem(Grid_RewardItem, ItemIconComponent.PrefabAuto);
				itemIconComponent.Open(itemData);
				_itemIconComponentList.Add(itemIconComponent);
			}
		}
		UIHelper.RepositionGrid(Grid_RewardItem);
	}

	private int SortItem(ItemData itemData1, ItemData itemData2)
	{
		ItemComparer.SortClass sortClass = new ItemComparer.SortClass();
		sortClass.Add(ItemComparer.Compare, itemData1.Tier, itemData1.Tier, isAscending: false).Add(ItemComparer.Compare_ItemGrade, itemData1.Grade, itemData2.Grade, isAscending: false).Add(ItemComparer.Compare, itemData1.Index, itemData2.Index, isAscending: true);
		return sortClass.Sort();
	}

	private void ClearItemIconList()
	{
		_rewardIndices.Clear();
		FactoryBase.ReleaseAndClearList(_itemIconComponentList);
	}
}
