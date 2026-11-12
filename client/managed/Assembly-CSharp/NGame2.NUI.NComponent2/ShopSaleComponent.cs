using NNguiExtension;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class ShopSaleComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/PayShop/ShopSaleComponent";

	public static PrefabAuto<ShopSaleComponent> PrefabAuto = new PrefabAuto<ShopSaleComponent>("Resources/Prefab2/Component/PayShop/ShopSaleComponent");

	public UISpriteSelector SpriteSelector;

	void InitOnCreate.OnCreate()
	{
		SetEnable(isOn: true);
	}

	void InitOnCreate.OnRelease()
	{
		SetEnable(isOn: false);
	}

	public void SetSaleSprite(int saleValue)
	{
		if (SpriteSelector != null)
		{
			SpriteSelector.SelectInt(saleValue);
		}
	}

	public void SetEnable(bool isOn)
	{
		UIHelper.EnableGameObject(SpriteSelector, isOn);
	}
}
