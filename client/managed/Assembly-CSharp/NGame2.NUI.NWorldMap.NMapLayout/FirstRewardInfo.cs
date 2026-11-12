using NVespa.NPrefab2;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class FirstRewardInfo : LinkFactoryComponent, InitOnCreate
{
	public static PrefabAuto<FirstRewardInfo> PrefabAuto = new PrefabAuto<FirstRewardInfo>("Prefab2/WorldMap/FirstRewardInfo");

	public UILabel AmountLabel;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
	}

	public void Open(int gem)
	{
		UIHelper.SetLabelText(AmountLabel, gem);
	}
}
