using NShared;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopEquipGachaFrameComponent : PayShopBaseFrameComponent<PayShopEquipGachaComponent, NewEquipGachaData>, PayShopBaseComponent.IListener, InitOnCreate
{
	public const string PrefabPath = "Prefab2/Component/PayShop2/PayShopEquipGachaFrameComponent";

	public static PrefabAuto<PayShopEquipGachaFrameComponent> PrefabAuto = new PrefabAuto<PayShopEquipGachaFrameComponent>("Prefab2/Component/PayShop2/PayShopEquipGachaFrameComponent");

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ReleaseComponent();
	}

	protected override PrefabAuto<PayShopEquipGachaComponent> GetPrefabAuto(int cardType)
	{
		return PayShopEquipGachaComponent.GetPrefabAuto(cardType);
	}
}
