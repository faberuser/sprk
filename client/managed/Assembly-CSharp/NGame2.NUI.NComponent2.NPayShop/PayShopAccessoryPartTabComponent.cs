using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopAccessoryPartTabComponent : PayShopBaseTabComponent<string>
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopAccessoryPartTabComponent";

	public static PrefabAuto<PayShopAccessoryPartTabComponent> PrefabAuto = new PrefabAuto<PayShopAccessoryPartTabComponent>("Prefab2/Component/PayShop2/PayShopAccessoryPartTabComponent");

	public UILabel Label_Name;

	public override void Open(IListener listener, string accessoryCategory)
	{
		base.Open(listener, accessoryCategory);
		SetClass(accessoryCategory);
	}

	public void SetClass(string accessoryCategory)
	{
		UIHelper.SetLabelText(Label_Name, accessoryCategory);
	}
}
