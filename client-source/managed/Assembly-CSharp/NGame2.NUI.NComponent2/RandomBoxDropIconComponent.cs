using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class RandomBoxDropIconComponent : LinkFactoryComponent, InitOnCreate
{
	public static string PrefabPath = "Prefab2/Component/Icon/RandomBoxDropIconComponent";

	public static PrefabAuto<RandomBoxDropIconComponent> PrefabAuto = new PrefabAuto<RandomBoxDropIconComponent>(PrefabPath);

	public UIWidget FixedWidget;

	public Transform Root;

	private ItemIconComponent _itemIconComponent;

	private CurrencyIconComponent _currencyIconComponent;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseComponents();
	}

	public void Open(RewardElementData rewardData, bool disableFixed = false)
	{
		ReleaseComponents();
		UIHelper.EnableGameObject(FixedWidget, !disableFixed && rewardData.ItemRate == 1000);
		CurrencyType currencyType = EnumTools.Parse(rewardData.ItemCode, CurrencyType.None);
		if (currencyType != CurrencyType.None)
		{
			_currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(Root);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_currencyIconComponent.transform);
			CurrencyIconComponent currencyIconComponent = _currencyIconComponent;
			int itemMax = rewardData.ItemMax;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMax;
			currencyIconComponent.Open(currencyType, num, listener);
			if (currencyType == CurrencyType.Gold && rewardData.ItemMin != rewardData.ItemMax)
			{
				_currencyIconComponent.SetMinMaxCount(rewardData.ItemMin, rewardData.ItemMax);
			}
			return;
		}
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardData.ItemCode);
		if (dataByCode != null)
		{
			_itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(Root);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_itemIconComponent.transform);
			_itemIconComponent.Open(dataByCode);
			ItemIconComponent itemIconComponent = _itemIconComponent;
			int itemMax2 = rewardData.ItemMax;
			Color? color = null;
			int num2 = itemMax2;
			itemIconComponent.SetItemCount(num2, color);
			_itemIconComponent.SetStar(rewardData.ItemStar);
			_itemIconComponent.ApplyCustomOption(rewardData.ItemCustomOptionIndex);
		}
	}

	private void ReleaseComponents()
	{
		FactoryBase.SafeReleaseComponent(ref _itemIconComponent);
		FactoryBase.SafeReleaseComponent(ref _currencyIconComponent);
	}
}
