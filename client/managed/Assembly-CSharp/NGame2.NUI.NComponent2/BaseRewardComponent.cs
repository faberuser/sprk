using System.Collections.Generic;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class BaseRewardComponent : LinkFactoryComponent, InitOnCreate, CurrencyIconComponent.Listener, ItemIconComponent.Listener
{
	public Transform GoldIconRoot;

	public Transform GemIconRoot;

	public Transform ItemIconRoot;

	private Component _component;

	public RewardData RewardData { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		DestroyComponent();
		RewardData = null;
	}

	public void Init(RewardData rewardData)
	{
		if (rewardData != null)
		{
			DestroyComponent();
			RewardData = rewardData;
			List<RewardElementData> rewardItemList = RewardData.GetRewardItemList();
			if (__Extension_Method_ICollection__.COUNT(rewardItemList) > 0)
			{
				RewardElementData data = rewardItemList[0];
				_component = CreateItemIconComponent(data);
			}
			else if (RewardData.GoldMax > 0)
			{
				_component = CreateGoldIconComponent(RewardData.GoldMax);
			}
			else if (RewardData.GemMax > 0)
			{
				_component = CreateGemIconComponent(RewardData.GemMax);
			}
		}
	}

	protected virtual void DestroyComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _component);
	}

	private Component CreateGoldIconComponent(int value)
	{
		if (GoldIconRoot == null)
		{
			return null;
		}
		CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(GoldIconRoot);
		if (currencyIconComponent == null)
		{
			return null;
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent.transform);
		currencyIconComponent.Open(CurrencyType.Gold, value, this);
		return currencyIconComponent;
	}

	private Component CreateGemIconComponent(int value)
	{
		if (GemIconRoot == null)
		{
			return null;
		}
		CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(GemIconRoot);
		if (currencyIconComponent == null)
		{
			return null;
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(currencyIconComponent.transform);
		currencyIconComponent.Open(CurrencyType.Gem, value, this);
		return currencyIconComponent;
	}

	private Component CreateItemIconComponent(RewardElementData data)
	{
		if (data.IsGold())
		{
			return CreateGoldIconComponent(data.ItemMax);
		}
		if (data.IsGem())
		{
			return CreateGemIconComponent(data.ItemMax);
		}
		if (ItemIconRoot == null)
		{
			return null;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(data.ItemCode);
		if (dataByCode == null)
		{
			return null;
		}
		ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		if (itemIconComponent == null)
		{
			return null;
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
		if (dataByCode.Type == ItemType.Equip)
		{
			EquipItemInfo equipItemInfo = new EquipItemInfo();
			equipItemInfo.ItemIndex = dataByCode.Index;
			equipItemInfo.Star = data.ItemStarMax;
			if (!itemIconComponent.Open(equipItemInfo, this))
			{
				FactoryBase.ReleaseComponent(itemIconComponent);
				return null;
			}
			return itemIconComponent;
		}
		itemIconComponent.Open(dataByCode, this);
		int itemMax = data.ItemMax;
		Color? color = null;
		int num = itemMax;
		itemIconComponent.SetItemCount(num, color);
		return itemIconComponent;
	}

	void CurrencyIconComponent.Listener.OnClick(CurrencyIconComponent component)
	{
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent component)
	{
	}
}
