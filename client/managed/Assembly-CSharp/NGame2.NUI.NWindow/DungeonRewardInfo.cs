using System.Collections.Generic;
using System.Linq;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DungeonRewardInfo : BaseWindow
{
	public const string PrefabPathPet = "Resources/Prefab2/Window/Pet/Popup/PetExploreRewardInfoPopup";

	public const string PREFAB_PATH = "Resources/Prefab2/Window/Dungeon/DungeonRewardInfo";

	public UIWidget WidgetRewardItemList;

	public UIScrollView ScrollView;

	private List<RewardItemListComponent> _ListRewardItemLists = new List<RewardItemListComponent>();

	public static void OpenPet(RewardData rewardData, BattleType battleType, List<RewardData> additionalReward = null)
	{
		WindowPrefab<DungeonRewardInfo> windowPrefab = new WindowPrefab<DungeonRewardInfo>("Resources/Prefab2/Window/Pet/Popup/PetExploreRewardInfoPopup", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			windowPrefab.instance.Open(rewardData, battleType, additionalReward);
		}
	}

	public static void OpenGlobal(RewardData rewardData, BattleType battleType, List<RewardData> additionalReward = null)
	{
		WindowPrefab<DungeonRewardInfo> windowPrefab = new WindowPrefab<DungeonRewardInfo>("Resources/Prefab2/Window/Dungeon/DungeonRewardInfo", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			windowPrefab.instance.Open(rewardData, battleType, additionalReward);
		}
	}

	public void Open(RewardData rewardData, BattleType battleType, List<RewardData> additionalReward)
	{
		base.Open();
		SetupListOfRewardItemList(rewardData, battleType, additionalReward);
	}

	public override void Reset()
	{
		base.Reset();
		_ListRewardItemLists.ForEach(delegate(RewardItemListComponent comp)
		{
			comp.Close();
		});
		FactoryBase.ReleaseAndClearList(_ListRewardItemLists);
	}

	public void OnClickExitButton()
	{
		Close();
	}

	private void SetupListOfRewardItemList(RewardData rewardData, BattleType battleType, List<RewardData> additionalReward)
	{
		if (rewardData == null)
		{
			return;
		}
		int GroupCount;
		List<RewardElementData> rewardItemListWithGroupCount = rewardData.GetRewardItemListWithGroupCount(out GroupCount, ignoreRate: true);
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(additionalReward); i < num; i++)
		{
			int GroupCount2 = 0;
			RewardData rewardData2 = additionalReward[i];
			rewardItemListWithGroupCount.AddRange(rewardData2.GetRewardItemListWithGroupCount(out GroupCount2, ignoreRate: true));
		}
		SortedDictionary<int, List<ItemData>> equipItemDic = new SortedDictionary<int, List<ItemData>>();
		SortedDictionary<int, List<Tuple<ItemData, int, int>>> itemDic = new SortedDictionary<int, List<Tuple<ItemData, int, int>>>();
		rewardItemListWithGroupCount.ForEach(delegate(RewardElementData e)
		{
			if (e != null)
			{
				ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(e.ItemCode);
				if (dataByCode != null)
				{
					if (dataByCode.Type == ItemType.Equip)
					{
						List<ItemData> list = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(equipItemDic, dataByCode.Grade, null);
						if (list == null)
						{
							list = new List<ItemData>();
							equipItemDic.Add(dataByCode.Grade, list);
						}
						list.Add(dataByCode);
					}
					else
					{
						int type = (int)dataByCode.Type;
						List<Tuple<ItemData, int, int>> list2 = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(itemDic, type, null);
						if (list2 == null)
						{
							list2 = new List<Tuple<ItemData, int, int>>();
							itemDic.Add(type, list2);
						}
						list2.Add(new Tuple<ItemData, int, int>(dataByCode, e.ItemMin, e.ItemMax));
					}
				}
			}
		});
		Vector3 localPosition = WidgetRewardItemList.transform.localPosition;
		foreach (KeyValuePair<int, List<ItemData>> item in equipItemDic.Reverse())
		{
			List<ItemData> value = item.Value;
			RewardItemListComponent rewardItemListComponent = RewardItemListComponent.PrefabAuto.Value.Factory.Create(WidgetRewardItemList.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(rewardItemListComponent.transform);
			rewardItemListComponent.transform.localScale = Vector3.one;
			SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.GetAuto(rewardItemListComponent.transform).scrollView = ScrollView;
			rewardItemListComponent.Open(ItemType.Equip, item.Key, value);
			rewardItemListComponent.transform.localPosition = localPosition;
			localPosition.y -= rewardItemListComponent.GetComponentHeight();
			_ListRewardItemLists.Add(rewardItemListComponent);
		}
		foreach (KeyValuePair<int, List<Tuple<ItemData, int, int>>> item2 in itemDic)
		{
			List<Tuple<ItemData, int, int>> value2 = item2.Value;
			RewardItemListComponent rewardItemListComponent2 = RewardItemListComponent.PrefabAuto.Value.Factory.Create(WidgetRewardItemList.transform);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(rewardItemListComponent2.transform);
			rewardItemListComponent2.transform.localScale = Vector3.one;
			SingletonAuto<LinkedTransform<UIDragScrollView>>.instance.GetAuto(rewardItemListComponent2.transform).scrollView = ScrollView;
			rewardItemListComponent2.Open((ItemType)item2.Key, 0, value2);
			rewardItemListComponent2.ShowBonusAssignedItem(value2, battleType);
			rewardItemListComponent2.transform.localPosition = localPosition;
			localPosition.y -= rewardItemListComponent2.GetComponentHeight();
			_ListRewardItemLists.Add(rewardItemListComponent2);
		}
		ScrollView.ResetPosition();
	}
}
