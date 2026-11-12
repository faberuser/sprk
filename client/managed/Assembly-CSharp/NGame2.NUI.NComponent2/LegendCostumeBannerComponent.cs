using System;
using NGame2.NAccount;
using NGame2.NDropBonusEvent;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NComponent2;

public class LegendCostumeBannerComponent : LinkFactoryComponent, LobbyRightGridWindow.IRightGridSort
{
	public const string PrefabPath = "Resources/Prefab2/Lobby/LegendCostumeBannerComponent";

	public static PrefabAuto<LegendCostumeBannerComponent> PrefabAuto = new PrefabAuto<LegendCostumeBannerComponent>("Resources/Prefab2/Lobby/LegendCostumeBannerComponent");

	public const string BannerPrefix = "LobbyBanner1/";

	public UIBundleLoader Texture_Banner;

	private Action _onClick;

	public void Open(Action onClick)
	{
		_onClick = onClick;
		Init();
	}

	public void Init()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.PayShopDiscountProductManager.Count() == 0)
		{
			SceneAutoBehaviour<CostumeManagement>.instance.RequestRefreshPayshopDiscountInfo();
		}
		SetBannerImage();
	}

	public void SetBannerImage()
	{
		string value = GameRoot.Instance.ConstantDataContainer.GetValue("PayshopDicountBannerName", "LIMITED_LEGENDCOSTUME_BANNER_211221");
		string assetPath = string.Format("{0}{1}", "LobbyBanner1/", value);
		UIHelper.SetBundleTexture(Texture_Banner, assetPath);
	}

	public void OnClickBanner()
	{
		Vespa_System_Action_Extension_Method.fire(_onClick);
	}

	bool LobbyRightGridWindow.IRightGridSort.IsDestroy()
	{
		if (!GlobalAutoBehaviour<DropBonusEventManager>.isValidInstance)
		{
			return true;
		}
		EventBonusType eventBonusType = EventBonusType.PayShopDiscount;
		return !GlobalAutoBehaviour<DropBonusEventManager>.instance.AnyEffectiveEventByEventBonusType(eventBonusType);
	}

	void LobbyRightGridWindow.IRightGridSort.ChainUpdate()
	{
	}

	int LobbyRightGridWindow.IRightGridSort.GetSortIndex()
	{
		return 0;
	}
}
