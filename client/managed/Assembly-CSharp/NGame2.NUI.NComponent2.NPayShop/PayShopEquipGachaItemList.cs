using System.Collections;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NHelper;
using NShared;
using NVespa;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaItemList : MonoBehaviour
{
	public UISimpleGrid ItemGrid;

	public UIWidget BonusPrefixWidget;

	public UISimpleGrid BonusItemGrid;

	public UIWidget WidgetDisableTouch;

	private List<ItemIconComponent> _itemIconList = new List<ItemIconComponent>();

	private List<CurrencyIconComponent> _currencyIconList = new List<CurrencyIconComponent>();

	private GachaItemResultInfo[] _gachaItemResults;

	public float BonusItemPopDelay = 0.2f;

	private ItemAnimationContainer _itemAnimationContainer;

	public bool IsAnimatingItemGet { get; private set; }

	public bool IsOpenItem => __Extension_Method_ICollection__.COUNT(_itemIconList) > 0;

	public void Open()
	{
		UIHelper.EnableGameObject(WidgetDisableTouch, enable: false);
		UIHelper.EnableGameObject(BonusPrefixWidget, enable: false);
	}

	public void Clear()
	{
		ClearItemList();
		UIHelper.EnableGameObject(BonusPrefixWidget, enable: false);
	}

	private void ClearItemList()
	{
		FactoryBase.ReleaseAndClearList(_itemIconList);
		FactoryBase.ReleaseAndClearList(_currencyIconList);
		if (_itemAnimationContainer != null)
		{
			_itemAnimationContainer.Reset();
			_itemAnimationContainer = null;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_gachaItemResults))
		{
			_gachaItemResults = null;
		}
	}

	public void SetItem(GachaItemResultInfo[] gachaItemResults)
	{
		ClearItemList();
		_itemAnimationContainer = CreateItemComponents(gachaItemResults);
		_gachaItemResults = gachaItemResults;
	}

	public void Play()
	{
		VespaCoroutine.Start(coGacha());
	}

	private IEnumerator coGacha()
	{
		IsAnimatingItemGet = true;
		UIHelper.EnableGameObject(BonusPrefixWidget, enable: false);
		UIHelper.EnableGameObject(WidgetDisableTouch, enable: true);
		while (!_itemAnimationContainer.IsEmpty)
		{
			yield return null;
			_itemAnimationContainer.ChainUpdate();
		}
		if (__Extension_Method_ICollection__.COUNT(_gachaItemResults) > 10)
		{
			UIHelper.EnableGameObject(BonusPrefixWidget, enable: true);
			yield return new WaitForSeconds(BonusItemPopDelay);
		}
		UIHelper.EnableGameObject(WidgetDisableTouch, enable: false);
		IsAnimatingItemGet = false;
	}

	private ItemAnimationContainer CreateItemComponents(GachaItemResultInfo[] gachaItemResults)
	{
		ItemAnimationContainer itemAnimationContainer = new ItemAnimationContainer(open3dModelPopup: true);
		for (int i = 0; i < gachaItemResults.Length; i++)
		{
			GachaItemResultInfo gachaItemResultInfo = gachaItemResults[i];
			GetItemAnimationData getItemAnimationData = null;
			if (gachaItemResultInfo.EquipItemResult != null)
			{
				getItemAnimationData = AddEquipItemResult(i, gachaItemResultInfo.EquipItemResult);
			}
			else if (gachaItemResultInfo.ItemResult != null)
			{
				ItemData data = GameRoot.Instance.ItemDataContainer.GetData(gachaItemResultInfo.ItemResult.ItemIndex);
				if (data == null)
				{
					continue;
				}
				getItemAnimationData = AddItemResult(i, data, gachaItemResultInfo.ItemResult.AddCount);
			}
			else if (gachaItemResultInfo.PetItemResult != null)
			{
				int petIndex = gachaItemResultInfo.PetItemResult.PetIndex;
				ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(petIndex);
				if (data2 == null)
				{
					continue;
				}
				getItemAnimationData = AddItemResult(i, data2, 1);
			}
			else
			{
				if (gachaItemResultInfo.StaminaResult == null)
				{
					VespaDebug.LogError("Invalid GachaResult: " + JsonMarshaler.EncodeJsonString(gachaItemResultInfo));
					continue;
				}
				getItemAnimationData = AddStaminaReslut(i, gachaItemResultInfo.StaminaResult);
			}
			if (getItemAnimationData != null)
			{
				if (i >= 10)
				{
					getItemAnimationData.Delay(BonusItemPopDelay);
				}
				itemAnimationContainer.Add(getItemAnimationData);
			}
		}
		UIHelper.RepositionGrid(ItemGrid);
		UIHelper.RepositionGrid(BonusItemGrid);
		return itemAnimationContainer;
	}

	private GetItemAnimationData AddEquipItemResult(int index, EquipItemInfo equipItemInfo)
	{
		ItemIconComponent itemIconComponent = AddItemIconComponent(index);
		if (!itemIconComponent.Open(equipItemInfo, null))
		{
			FactoryBase.ReleaseComponent(itemIconComponent);
			return null;
		}
		_itemIconList.Add(itemIconComponent);
		return itemIconComponent.CreateGetAnimation();
	}

	private GetItemAnimationData AddItemResult(int index, ItemData itemData, int addCount)
	{
		ItemIconComponent itemIconComponent = AddItemIconComponent(index);
		itemIconComponent.Open(itemData);
		itemIconComponent.SetItemCount(addCount);
		_itemIconList.Add(itemIconComponent);
		return itemIconComponent.CreateGetAnimation();
	}

	private GetItemAnimationData AddStaminaReslut(int index, StaminaResultInfo staminaResultInfo)
	{
		CurrencyType currencyType = EnumAttributeContainer<StaminaType, StaminaTypeAttribute>.Get(staminaResultInfo.Type)?.CurrencyType ?? CurrencyType.None;
		if (currencyType == CurrencyType.None)
		{
			return null;
		}
		CurrencyIconComponent currencyIconComponent = AddCurrencyIconComponent(index);
		currencyIconComponent.Open(currencyType, staminaResultInfo.AddValue, null);
		currencyIconComponent.SetSkipIncrease(skip: true);
		_currencyIconList.Add(currencyIconComponent);
		return currencyIconComponent.CreateGetAnimation();
	}

	private ItemIconComponent AddItemIconComponent(int index)
	{
		return UIHelper.AddGridItem((index < 10) ? ItemGrid : BonusItemGrid, ItemIconComponent.PrefabAuto);
	}

	private CurrencyIconComponent AddCurrencyIconComponent(int index)
	{
		return UIHelper.AddGridItem((index < 10) ? ItemGrid : BonusItemGrid, CurrencyIconComponent.PrefabAuto);
	}
}
