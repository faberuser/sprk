using NNguiExtension;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopStepUpGachaRewardPopupComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PayShop2/PayShopStepUpGachaRewardPopupComponent";

	public static PrefabAuto<PayShopStepUpGachaRewardPopupComponent> PrefabAuto = new PrefabAuto<PayShopStepUpGachaRewardPopupComponent>("Resources/Prefab2/Component/PayShop2/PayShopStepUpGachaRewardPopupComponent");

	public Transform IconRoot;

	public UISpriteSelector SpriteSelectorSale;

	public GameObject GoSaleBg;

	public UILabel LabelDesc;

	private Component _iconComponent;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseItemIcon();
	}

	public void Open(int discount)
	{
		ShowSale(show: true);
		if (SpriteSelectorSale != null)
		{
			SpriteSelectorSale.Select(discount.ToString());
		}
		string saleLabelText = GetSaleLabelText(discount);
		SetLabel(saleLabelText);
	}

	public void Open(RewardElementData data)
	{
		ShowSale(show: false);
		CreateIcon(data, IconRoot);
	}

	public void Open(string buffName)
	{
	}

	private void ShowSale(bool show)
	{
		UIHelper.EnableGameObject(SpriteSelectorSale, show);
		UIHelper.EnableGameObject(GoSaleBg, show);
	}

	private void CreateIcon(RewardElementData rewardElement, Transform root)
	{
		if (rewardElement != null && !(root == null))
		{
			if (rewardElement.GetCurrencyType() != CurrencyType.None)
			{
				CreateCurrencyItemIcon(rewardElement, root);
			}
			else
			{
				CreateItemIcon(rewardElement, root);
			}
		}
	}

	private void CreateCurrencyItemIcon(RewardElementData rewardElement, Transform root)
	{
		CurrencyIconComponent currencyIconComponent = CurrencyIconComponent.PrefabAuto.Factory.Create(root);
		if (!(currencyIconComponent == null))
		{
			CurrencyType currencyType = rewardElement.GetCurrencyType();
			int itemMin = rewardElement.ItemMin;
			CurrencyIconComponent.Listener listener = null;
			int num = itemMin;
			currencyIconComponent.Open(currencyType, num, listener);
			_iconComponent = currencyIconComponent;
			string stringDirect = LocalizationManager.GetStringDirect($"TOOLTIP_INFO_NAME_{rewardElement.GetCurrencyType().ToString()}");
			string rewardDataLabelText = GetRewardDataLabelText(stringDirect, rewardElement.ItemMin);
			SetLabel(rewardDataLabelText);
		}
	}

	private void CreateItemIcon(RewardElementData rewardElement, Transform root)
	{
		ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(rewardElement.ItemCode);
		if (dataByCode != null)
		{
			ItemIconComponent itemIconComponent = ItemIconComponent.PrefabAuto.Factory.Create(root);
			if (!(itemIconComponent == null))
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(itemIconComponent.transform);
				itemIconComponent.Open(dataByCode);
				_iconComponent = itemIconComponent;
				string rewardDataLabelText = GetRewardDataLabelText(dataByCode.Name, rewardElement.ItemMin);
				SetLabel(rewardDataLabelText);
			}
		}
	}

	private void SetLabel(string name)
	{
		UIHelper.SetLabelText(LabelDesc, name);
	}

	private string GetRewardDataLabelText(string itemName, int count)
	{
		return $"{itemName} X {count}";
	}

	private string GetSaleLabelText(int discount)
	{
		return LocalizationManager.GetStringDirect("STEPUP_STEP_SALE_TEXT", discount);
	}

	public void ReleaseItemIcon()
	{
		FactoryBase.SafeReleaseComponent(ref _iconComponent);
	}
}
