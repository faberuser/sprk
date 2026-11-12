using System.Collections.Generic;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NMonthlyHero;

public class RewardComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/MonthlyHero/RewardComponent";

	public static PrefabAuto<RewardComponent> PrefabAuto = new PrefabAuto<RewardComponent>("Prefab2/Component/MonthlyHero/RewardComponent");

	public UISprite SpriteCheck;

	private List<Component> _componentList = new List<Component>();

	public void Open(RewardElementData rewardElemData, bool alreadyRewarded)
	{
		CreateCurrency(rewardElemData);
		CreateItem(rewardElemData);
		ComponentResetTM();
		SetCheckSprite(alreadyRewarded);
	}

	private void CreateItem(RewardElementData rewardElemData)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElemData.ItemCode);
		if (dataByCode != null)
		{
			int itemMin = rewardElemData.ItemMin;
			long count = itemMin;
			int itemMax = rewardElemData.ItemMax;
			long maxCount = itemMax;
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(base.transform);
			itemIconComponent.Open(dataByCode);
			if (dataByCode.Type != ItemType.Equip)
			{
				itemIconComponent.SetItemMinMax(count, maxCount);
			}
			_componentList.Add(itemIconComponent);
		}
	}

	private void CreateCurrency(RewardElementData rewardElemData)
	{
		CurrencyType currencyType = rewardElemData.GetCurrencyType();
		if (currencyType != CurrencyType.None)
		{
			CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(base.transform);
			int itemMin = rewardElemData.ItemMin;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMin;
			currencyIconComponent.Open(currencyType, num, listener);
			_componentList.Add(currencyIconComponent);
		}
	}

	private void ComponentResetTM()
	{
		_componentList.ForEach(delegate(Component e)
		{
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(e.transform);
		});
	}

	private void SetCheckSprite(bool active)
	{
		UIHelper.EnableGameObject(SpriteCheck, active);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}
}
