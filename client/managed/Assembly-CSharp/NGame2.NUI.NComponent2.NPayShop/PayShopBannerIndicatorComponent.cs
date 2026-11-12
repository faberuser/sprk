using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopBannerIndicatorComponent : MonoBehaviour
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopBannerIndicatorComponent";

	public static PrefabAuto<PayShopBannerIndicatorComponent> PrefabAuto = new PrefabAuto<PayShopBannerIndicatorComponent>("Prefab2/Component/PayShop2/PayShopBannerIndicatorComponent");

	public UISprite SpriteOn;

	public UISprite SpriteOff;

	public void Open()
	{
		SetOn(On: false);
	}

	public void SetOn(bool On)
	{
		UIHelper.EnableGameObject(SpriteOn, On);
		UIHelper.EnableGameObject(SpriteOff, !On);
	}
}
