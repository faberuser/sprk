using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class DbRewardComponent : LinkFactoryComponent, InitOnCreate, CurrencyIconComponent.Listener, ItemIconComponent.Listener
{
	public Transform GoldIconRoot;

	public Transform GemIconRoot;

	public Transform ItemIconRoot;

	private Component _component;

	public DbRewardData DbRewardData { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		DestroyComponent();
		DbRewardData = null;
	}

	public void Init(DbRewardData dbRewardData)
	{
		if (dbRewardData != null)
		{
			DestroyComponent();
			DbRewardData = dbRewardData;
			RewardType type = dbRewardData.Type;
			_ = dbRewardData.Value;
			_ = dbRewardData.Count;
			_ = dbRewardData.Star;
			switch (type)
			{
			case RewardType.Gold:
				_component = CreateGoldIconComponent(dbRewardData);
				break;
			case RewardType.Gem:
				_component = CreateGemIconComponent(dbRewardData);
				break;
			case RewardType.Item:
				_component = CreateItemIconComponent(dbRewardData);
				break;
			}
			_ = _component == null;
		}
	}

	protected virtual void DestroyComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _component);
	}

	private Component CreateGoldIconComponent(DbRewardData data)
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
		int value = data.Value;
		currencyIconComponent.Open(CurrencyType.Gold, value, this);
		return currencyIconComponent;
	}

	private Component CreateGemIconComponent(DbRewardData data)
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
		int value = data.Value;
		currencyIconComponent.Open(CurrencyType.Gem, value, this);
		return currencyIconComponent;
	}

	private Component CreateItemIconComponent(DbRewardData data)
	{
		if (ItemIconRoot == null)
		{
			return null;
		}
		ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(data.Value);
		if (data2 == null)
		{
			return null;
		}
		ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(ItemIconRoot);
		if (itemIconComponent == null)
		{
			return null;
		}
		Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
		if (data2.Type == ItemType.Equip)
		{
			EquipItemInfo equipItemInfo = new EquipItemInfo();
			equipItemInfo.ItemIndex = data.Value;
			equipItemInfo.Star = data.Star;
			if (!itemIconComponent.Open(equipItemInfo, this))
			{
				FactoryBase.ReleaseComponent(itemIconComponent);
				return null;
			}
			return itemIconComponent;
		}
		itemIconComponent.Open(data2, this);
		int count = data.Count;
		Color? color = null;
		int num = count;
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
