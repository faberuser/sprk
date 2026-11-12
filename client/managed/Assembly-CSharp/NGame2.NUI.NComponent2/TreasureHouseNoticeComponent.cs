using NGame2.NUI.NWindow;
using NVespa.NGlobal;
using NVespa.NPrefab2;

namespace NGame2.NUI.NComponent2;

public class TreasureHouseNoticeComponent : LinkFactoryComponent
{
	public const string PrefabPath = "Prefab2/Component/TreasureHouse/TreasureHouseNoticeComponent";

	public static PrefabAuto<TreasureHouseNoticeComponent> PrefabAuto = new PrefabAuto<TreasureHouseNoticeComponent>("Prefab2/Component/TreasureHouse/TreasureHouseNoticeComponent");

	public void OnClickButton()
	{
		CenterMessage.Open(LocalizationManager.GetStringDirect("TREASUREHOUSE_OPEN_NOTICE"));
	}
}
