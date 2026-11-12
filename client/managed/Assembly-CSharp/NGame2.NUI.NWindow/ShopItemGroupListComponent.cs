using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ShopItemGroupListComponent : MonoBehaviour, ShopItemGroupComponent.Listener
{
	public interface Listener
	{
		void OnClickGroupComponent(ShopItemGroupComponent clicked);
	}

	public UILabel GroupLabel;

	public UISprite Sprite_Dir;

	public UISimpleGrid GroupListGrid;

	public List<ShopItemGroupComponent> ListShopItemGroups = new List<ShopItemGroupComponent>();

	private Listener _listener;

	public int SelectedGroupIndex { get; private set; }

	public void Init(Listener listener, int ShopIndex)
	{
		_listener = listener;
		SetupShopItemGroupList(ShopIndex);
	}

	private void ClearShopItemGroupList()
	{
		FactoryBase.ReleaseAndClearList(ListShopItemGroups);
	}

	private void SetupShopItemGroupList(int ShopIndex)
	{
		Debug.Log("### SetupShopItemGroupListSetupShopItemGroupListSetupShopItemGroupListSetupShopItemGroupList");
		ClearShopItemGroupList();
		List<ShopItemGroupData> groupDataList = GameRoot.Instance.ShopItemGroupDataContainer.GetGroupDataList(ShopIndex);
		if (groupDataList == null || __Extension_Method_ICollection__.COUNT(groupDataList) == 0)
		{
			VespaDebug.LogError("Group Data does not exist, shopIndex = " + ShopIndex);
			return;
		}
		groupDataList.Sort((ShopItemGroupData l, ShopItemGroupData r) => l.GroupIndex.CompareTo(r.GroupIndex));
		for (int num = 0; num < __Extension_Method_ICollection__.COUNT(groupDataList); num++)
		{
			ShopItemGroupData groupData = groupDataList[num];
			ShopItemGroupComponent shopItemGroupComponent = UIHelper.AddGridItem(GroupListGrid, ShopItemGroupComponent.PrefabAuto);
			shopItemGroupComponent.Open(groupData, this);
			ListShopItemGroups.Add(shopItemGroupComponent);
		}
		UIHelper.RepositionGrid(GroupListGrid);
		if (__Extension_Method_ICollection__.COUNT(ListShopItemGroups) <= 0)
		{
			return;
		}
		ShopItemGroupComponent shopItemGroupComponent2 = null;
		int? forceShopGroupIndex = SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex;
		int num2 = 0;
		for (int count = ListShopItemGroups.Count; num2 < count; num2++)
		{
			ShopItemGroupComponent shopItemGroupComponent3 = ListShopItemGroups[num2];
			if (shopItemGroupComponent3 == null || shopItemGroupComponent3.ShopItemGroupData == null)
			{
				continue;
			}
			if (SceneAutoBehaviour<EntryMenuManagement>.instance.ForceShopGroupIndex.HasValue && forceShopGroupIndex == shopItemGroupComponent3.ShopItemGroupData.GroupIndex)
			{
				shopItemGroupComponent2 = shopItemGroupComponent3;
				break;
			}
			if (shopItemGroupComponent3.ShopItemGroupData.IsDefault)
			{
				if (shopItemGroupComponent2 == null)
				{
					shopItemGroupComponent2 = shopItemGroupComponent3;
				}
				else if (shopItemGroupComponent2.ShopItemGroupData.GroupIndex > shopItemGroupComponent3.ShopItemGroupData.GroupIndex)
				{
					shopItemGroupComponent2 = shopItemGroupComponent3;
				}
			}
		}
		if (shopItemGroupComponent2 == null)
		{
			shopItemGroupComponent2 = ListShopItemGroups[0];
		}
		if (shopItemGroupComponent2 != null)
		{
			SelectedGroupIndex = shopItemGroupComponent2.ShopItemGroupData.GroupIndex;
			string stringDirect = LocalizationManager.GetStringDirect(shopItemGroupComponent2.ShopItemGroupData.GroupNameKey);
			UIHelper.SetLabelText(GroupLabel, stringDirect);
		}
		else
		{
			UIHelper.SetLabelText(GroupLabel, "");
		}
	}

	private void SelectShopItemGroup(ShopItemGroupComponent selectedItemGroup)
	{
		if (selectedItemGroup.ShopItemGroupData != null)
		{
			ShopItemGroupData shopItemGroupData = selectedItemGroup.ShopItemGroupData;
			SelectedGroupIndex = shopItemGroupData.GroupIndex;
			string stringDirect = LocalizationManager.GetStringDirect(shopItemGroupData.GroupNameKey);
			UIHelper.SetLabelText(GroupLabel, stringDirect);
			if (_listener != null)
			{
				_listener.OnClickGroupComponent(selectedItemGroup);
			}
		}
	}

	void ShopItemGroupComponent.Listener.OnClickComponent(ShopItemGroupComponent caller)
	{
		SelectShopItemGroup(caller);
	}
}
